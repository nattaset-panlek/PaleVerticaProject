using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Pathfinding
{
    public static class GridPathFormula
    {
        #region [Variables/Fields]
        public const int DefaultMaxExpansions = 20000;
        public const int DefaultSnapCells = 4;

        private const float DiagonalCost = 1.41421356f;
        private const float TieBreak = 1.001f;
        private const float EdgeInset = 0.002f;

        private static readonly int[] StepX = { 1, -1, 0, 0, 1, 1, -1, -1 };
        private static readonly int[] StepY = { 0, 0, 1, -1, 1, -1, 1, -1 };

        private static readonly List<int> _cells = new List<int>(128);
        private static readonly List<Vector2> _points = new List<Vector2>(128);

        private static float[] _g = Array.Empty<float>();
        private static int[] _parent = Array.Empty<int>();
        private static int[] _seen = Array.Empty<int>();
        private static int[] _closed = Array.Empty<int>();
        private static float[] _heapKeys = new float[256];
        private static int[] _heapCells = new int[256];
        private static int _heapCount;
        private static int _search;
        #endregion

        #region [Public Methods]
        public static bool TryFindPath(WalkableGrid grid, Vector2 from, Vector2 to, List<Vector2> result, int maxExpansions = DefaultMaxExpansions, int snapCells = DefaultSnapCells)
        {
            return Found(FindPath(grid, from, to, result, maxExpansions, snapCells));
        }

        public static PathOutcome FindPath(WalkableGrid grid, Vector2 from, Vector2 to, List<Vector2> result, int maxExpansions = DefaultMaxExpansions, int snapCells = DefaultSnapCells)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            result.Clear();

            PathOutcome ends = ResolveEnds(grid, from, to, snapCells, out Vector2Int start, out Vector2Int end, out bool startSnapped);
            if (!Found(ends)) return ends;

            int startIndex = grid.IndexOf(start.x, start.y);
            int endIndex = grid.IndexOf(end.x, end.y);
            if (!Search(grid, startIndex, endIndex, maxExpansions)) return PathOutcome.NoRoute;

            bool endSnapped = ends == PathOutcome.StopsShort;
            CollectPoints(grid, startIndex, endIndex, startSnapped ? grid.CellCenter(start.x, start.y) : from, endSnapped ? grid.CellCenter(end.x, end.y) : to);
            Smooth(grid, startSnapped, result);
            return result.Count > 0 ? ends : PathOutcome.NoRoute;
        }

        public static PathOutcome SnapPoint(WalkableGrid grid, Vector2 from, Vector2 point, out Vector2 snapped, int snapCells = DefaultSnapCells)
        {
            snapped = point;
            PathOutcome ends = ResolveEnds(grid, from, point, snapCells, out _, out Vector2Int end, out _);
            if (ends == PathOutcome.StopsShort) snapped = grid.CellCenter(end.x, end.y);
            return ends;
        }

        public static bool Found(PathOutcome outcome)
        {
            return outcome == PathOutcome.ReachesTarget || outcome == PathOutcome.StopsShort;
        }

        public static bool HasLineOfSight(WalkableGrid grid, Vector2 from, Vector2 to)
        {
            if (grid == null || !IsSegmentClear(grid, from, to, false)) return false;
            if (grid.Clearance <= 0f) return true;

            Vector2 delta = to - from;
            if (delta.sqrMagnitude < 1e-10f) return true;

            Vector2 normal = new Vector2(-delta.y, delta.x).normalized;
            float reach = Mathf.Max(0f, grid.Clearance - EdgeInset * grid.CellSize);
            int lanes = Mathf.Max(1, Mathf.CeilToInt(reach / grid.CellSize));
            for (int lane = 1; lane <= lanes; lane++)
            {
                Vector2 offset = normal * (reach * lane / lanes);
                if (!IsSegmentClear(grid, from + offset, to + offset, true)) return false;
                if (!IsSegmentClear(grid, from - offset, to - offset, true)) return false;
            }

            return true;
        }
        #endregion

        #region [Private Methods]
        private static PathOutcome ResolveEnds(WalkableGrid grid, Vector2 from, Vector2 to, int snapCells, out Vector2Int start, out Vector2Int end, out bool startSnapped)
        {
            end = default;
            startSnapped = false;
            start = grid != null ? grid.WorldToCell(from) : default;
            if (grid == null || !grid.InGrid(start.x, start.y)) return PathOutcome.NoGrid;

            startSnapped = !grid.IsWalkable(start.x, start.y);
            if (startSnapped && !TryFindNearest(grid, start, from, to, 0, snapCells, out start)) return PathOutcome.NoRoute;

            int region = grid.RegionOf(start.x, start.y);
            end = grid.WorldToCell(to);
            if (grid.RegionOf(end.x, end.y) == region) return PathOutcome.ReachesTarget;

            end = grid.ClampCell(end);
            if (grid.RegionOf(end.x, end.y) != region && !TryFindNearest(grid, end, to, from, region, snapCells, out end)) return PathOutcome.NoRoute;
            return PathOutcome.StopsShort;
        }

        private static bool Search(WalkableGrid grid, int start, int goal, int maxExpansions)
        {
            EnsureCapacity(grid.CellCount);
            int search = NextSearch();
            int columns = grid.Columns;
            int goalX = goal % columns;
            int goalY = goal / columns;

            _heapCount = 0;
            _g[start] = 0f;
            _parent[start] = -1;
            _seen[start] = search;
            Push(Octile(start % columns - goalX, start / columns - goalY) * TieBreak, start);

            int expansions = 0;
            while (_heapCount > 0)
            {
                int current = Pop();
                if (_closed[current] == search) continue;
                if (current == goal) return true;

                _closed[current] = search;
                if (++expansions > maxExpansions) return false;

                int x = current % columns;
                int y = current / columns;
                for (int dir = 0; dir < StepX.Length; dir++)
                {
                    int dx = StepX[dir];
                    int dy = StepY[dir];
                    int nx = x + dx;
                    int ny = y + dy;
                    if (!grid.IsWalkable(nx, ny)) continue;

                    bool diagonal = dx != 0 && dy != 0;
                    if (diagonal && (!grid.IsWalkable(x + dx, y) || !grid.IsWalkable(x, y + dy))) continue;

                    int next = ny * columns + nx;
                    if (_closed[next] == search) continue;

                    float g = _g[current] + (diagonal ? DiagonalCost : 1f);
                    if (_seen[next] == search && g >= _g[next]) continue;

                    _seen[next] = search;
                    _g[next] = g;
                    _parent[next] = current;
                    Push(g + Octile(nx - goalX, ny - goalY) * TieBreak, next);
                }
            }

            return false;
        }

        private static void CollectPoints(WalkableGrid grid, int start, int goal, Vector2 first, Vector2 last)
        {
            _cells.Clear();
            for (int index = goal; index != -1; index = _parent[index]) _cells.Add(index);
            _cells.Reverse();

            _points.Clear();
            _points.Add(first);
            for (int i = 1; i < _cells.Count - 1; i++)
            {
                int index = _cells[i];
                _points.Add(grid.CellCenter(index % grid.Columns, index / grid.Columns));
            }
            _points.Add(last);
        }

        private static void Smooth(WalkableGrid grid, bool keepFirst, List<Vector2> result)
        {
            if (keepFirst) result.Add(_points[0]);

            int anchor = 0;
            int probe = 2;
            while (probe < _points.Count)
            {
                if (HasLineOfSight(grid, _points[anchor], _points[probe]))
                {
                    probe++;
                    continue;
                }

                anchor = probe - 1;
                result.Add(_points[anchor]);
                probe = anchor + 2;
            }

            Vector2 final = _points[_points.Count - 1];
            if (result.Count == 0 || (result[result.Count - 1] - final).sqrMagnitude > 1e-10f) result.Add(final);
        }

        private static bool TryFindNearest(WalkableGrid grid, Vector2Int center, Vector2 target, Vector2 bias, int region, int maxRing, out Vector2Int found)
        {
            found = center;
            for (int ring = 0; ring <= maxRing; ring++)
            {
                float best = float.MaxValue;
                float bestBias = float.MaxValue;
                for (int dy = -ring; dy <= ring; dy++)
                {
                    for (int dx = -ring; dx <= ring; dx++)
                    {
                        if (Mathf.Abs(dx) != ring && Mathf.Abs(dy) != ring) continue;

                        int x = center.x + dx;
                        int y = center.y + dy;
                        if (!grid.IsWalkable(x, y)) continue;
                        if (region != 0 && grid.RegionOf(x, y) != region) continue;

                        Vector2 cellCenter = grid.CellCenter(x, y);
                        float distance = (cellCenter - target).sqrMagnitude;
                        float biasDistance = (cellCenter - bias).sqrMagnitude;
                        if (distance > best + 1e-6f) continue;
                        if (distance > best - 1e-6f && biasDistance >= bestBias) continue;
                        best = distance;
                        bestBias = biasDistance;
                        found = new Vector2Int(x, y);
                    }
                }

                if (best < float.MaxValue) return true;
            }

            return false;
        }

        private static bool IsSegmentClear(WalkableGrid grid, Vector2 from, Vector2 to, bool bodyLane)
        {
            float ax = (from.x - grid.Bounds.x) / grid.CellSize;
            float ay = (from.y - grid.Bounds.y) / grid.CellSize;
            float bx = (to.x - grid.Bounds.x) / grid.CellSize;
            float by = (to.y - grid.Bounds.y) / grid.CellSize;

            int x = Mathf.FloorToInt(ax);
            int y = Mathf.FloorToInt(ay);
            int endX = Mathf.FloorToInt(bx);
            int endY = Mathf.FloorToInt(by);
            if (!IsCellClear(grid, x, y, bodyLane)) return false;

            float dx = bx - ax;
            float dy = by - ay;
            int stepX = Math.Sign(dx);
            int stepY = Math.Sign(dy);
            float deltaX = stepX != 0 ? Mathf.Abs(1f / dx) : float.PositiveInfinity;
            float deltaY = stepY != 0 ? Mathf.Abs(1f / dy) : float.PositiveInfinity;
            float nextX = stepX > 0 ? (x + 1 - ax) * deltaX : stepX < 0 ? (ax - x) * deltaX : float.PositiveInfinity;
            float nextY = stepY > 0 ? (y + 1 - ay) * deltaY : stepY < 0 ? (ay - y) * deltaY : float.PositiveInfinity;

            int guard = Mathf.Abs(endX - x) + Mathf.Abs(endY - y) + 2;
            while ((x != endX || y != endY) && guard-- > 0)
            {
                if (stepX != 0 && stepY != 0 && Mathf.Abs(nextX - nextY) < 1e-5f)
                {
                    if (!IsCellClear(grid, x + stepX, y, bodyLane) || !IsCellClear(grid, x, y + stepY, bodyLane)) return false;
                    x += stepX;
                    y += stepY;
                    nextX += deltaX;
                    nextY += deltaY;
                }
                else if (nextX < nextY)
                {
                    x += stepX;
                    nextX += deltaX;
                }
                else
                {
                    y += stepY;
                    nextY += deltaY;
                }

                if (!IsCellClear(grid, x, y, bodyLane)) return false;
            }

            return IsCellClear(grid, endX, endY, bodyLane);
        }

        private static bool IsCellClear(WalkableGrid grid, int column, int row, bool bodyLane)
        {
            if (!bodyLane) return grid.IsWalkable(column, row);
            return !grid.InGrid(column, row) || grid.IsBakedWalkable(column, row);
        }

        private static float Octile(int dx, int dy)
        {
            dx = Mathf.Abs(dx);
            dy = Mathf.Abs(dy);
            return dx > dy ? dx + (DiagonalCost - 1f) * dy : dy + (DiagonalCost - 1f) * dx;
        }

        private static void EnsureCapacity(int cellCount)
        {
            if (_g.Length >= cellCount) return;
            _g = new float[cellCount];
            _parent = new int[cellCount];
            _seen = new int[cellCount];
            _closed = new int[cellCount];
            _search = 0;
        }

        private static int NextSearch()
        {
            if (_search == int.MaxValue)
            {
                Array.Clear(_seen, 0, _seen.Length);
                Array.Clear(_closed, 0, _closed.Length);
                _search = 0;
            }

            return ++_search;
        }

        private static void Push(float key, int cell)
        {
            if (_heapCount == _heapKeys.Length)
            {
                Array.Resize(ref _heapKeys, _heapCount * 2);
                Array.Resize(ref _heapCells, _heapCount * 2);
            }

            int i = _heapCount++;
            while (i > 0)
            {
                int parent = (i - 1) >> 1;
                if (_heapKeys[parent] <= key) break;
                _heapKeys[i] = _heapKeys[parent];
                _heapCells[i] = _heapCells[parent];
                i = parent;
            }

            _heapKeys[i] = key;
            _heapCells[i] = cell;
        }

        private static int Pop()
        {
            int top = _heapCells[0];
            _heapCount--;
            if (_heapCount == 0) return top;

            float key = _heapKeys[_heapCount];
            int cell = _heapCells[_heapCount];
            int i = 0;
            while (true)
            {
                int child = i * 2 + 1;
                if (child >= _heapCount) break;
                if (child + 1 < _heapCount && _heapKeys[child + 1] < _heapKeys[child]) child++;
                if (_heapKeys[child] >= key) break;
                _heapKeys[i] = _heapKeys[child];
                _heapCells[i] = _heapCells[child];
                i = child;
            }

            _heapKeys[i] = key;
            _heapCells[i] = cell;
            return top;
        }
        #endregion
    }
}

