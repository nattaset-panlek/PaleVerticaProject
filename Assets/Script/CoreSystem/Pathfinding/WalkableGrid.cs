using System;
using UnityEngine;

namespace CoreSystem.Pathfinding
{
    public sealed class WalkableGrid
    {
        #region [Variables/Fields]
        private readonly bool[] _baked;
        private readonly bool[] _walkable;
        private readonly int[] _regions;
        #endregion

        #region [Properties]
        public Rect Bounds { get; }
        public float CellSize { get; }
        public int Columns { get; }
        public int Rows { get; }
        public float Clearance { get; }
        public int CellCount => Columns * Rows;
        public int WalkableCount { get; private set; }
        public int RegionCount { get; private set; }
        #endregion

        #region [Public Methods]
        public WalkableGrid(Rect bounds, float cellSize, int columns, int rows, bool[] bakedWalkable, float clearance = 0f)
        {
            if (cellSize <= 0f) throw new ArgumentOutOfRangeException(nameof(cellSize));
            if (columns <= 0 || rows <= 0) throw new ArgumentOutOfRangeException(nameof(columns));
            if (bakedWalkable == null || bakedWalkable.Length < (long)columns * rows) throw new ArgumentException("Not enough cells for the grid size.", nameof(bakedWalkable));

            Bounds = bounds;
            CellSize = cellSize;
            Columns = columns;
            Rows = rows;
            Clearance = Mathf.Max(0f, clearance);

            _baked = new bool[CellCount];
            Array.Copy(bakedWalkable, _baked, CellCount);
            _walkable = BuildWalkable();
            _regions = new int[CellCount];
            LabelRegions();
        }

        public bool InGrid(int column, int row)
        {
            return column >= 0 && row >= 0 && column < Columns && row < Rows;
        }

        public int IndexOf(int column, int row)
        {
            return row * Columns + column;
        }

        public bool IsWalkable(int column, int row)
        {
            return InGrid(column, row) && _walkable[IndexOf(column, row)];
        }

        public bool IsWalkable(Vector2 position)
        {
            var cell = WorldToCell(position);
            return IsWalkable(cell.x, cell.y);
        }

        public bool IsBakedWalkable(int column, int row)
        {
            return InGrid(column, row) && _baked[IndexOf(column, row)];
        }

        public int RegionOf(int column, int row)
        {
            return InGrid(column, row) ? _regions[IndexOf(column, row)] : 0;
        }

        public bool Contains(Vector2 position)
        {
            var cell = WorldToCell(position);
            return InGrid(cell.x, cell.y);
        }

        public Vector2Int WorldToCell(Vector2 position)
        {
            return new Vector2Int(
                Mathf.FloorToInt((position.x - Bounds.x) / CellSize),
                Mathf.FloorToInt((position.y - Bounds.y) / CellSize));
        }

        public Vector2Int ClampCell(Vector2Int cell)
        {
            return new Vector2Int(Mathf.Clamp(cell.x, 0, Columns - 1), Mathf.Clamp(cell.y, 0, Rows - 1));
        }

        public Vector2 CellCenter(int column, int row)
        {
            return new Vector2(Bounds.x + (column + 0.5f) * CellSize, Bounds.y + (row + 0.5f) * CellSize);
        }
        #endregion

        #region [Private Methods]
        private bool[] BuildWalkable()
        {
            var walkable = new bool[CellCount];
            Array.Copy(_baked, walkable, CellCount);
            if (Clearance <= 0f) return walkable;

            float reachCells = Clearance / CellSize;
            int reach = Mathf.CeilToInt(reachCells + 0.5f);
            float reachSqr = reachCells * reachCells;

            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    if (_baked[IndexOf(column, row)]) continue;

                    for (int dy = -reach; dy <= reach; dy++)
                    {
                        float gapY = Mathf.Max(0f, Mathf.Abs(dy) - 0.5f);
                        for (int dx = -reach; dx <= reach; dx++)
                        {
                            int x = column + dx;
                            int y = row + dy;
                            if (!InGrid(x, y)) continue;

                            float gapX = Mathf.Max(0f, Mathf.Abs(dx) - 0.5f);
                            if (gapX * gapX + gapY * gapY < reachSqr) walkable[IndexOf(x, y)] = false;
                        }
                    }
                }
            }

            return walkable;
        }

        private void LabelRegions()
        {
            var queue = new int[CellCount];
            int region = 0;
            int walkableCount = 0;

            for (int start = 0; start < CellCount; start++)
            {
                if (!_walkable[start] || _regions[start] != 0) continue;

                region++;
                int head = 0;
                int tail = 0;
                queue[tail++] = start;
                _regions[start] = region;

                while (head < tail)
                {
                    int index = queue[head++];
                    walkableCount++;
                    int column = index % Columns;
                    int row = index / Columns;
                    Visit(column + 1, row, region, queue, ref tail);
                    Visit(column - 1, row, region, queue, ref tail);
                    Visit(column, row + 1, region, queue, ref tail);
                    Visit(column, row - 1, region, queue, ref tail);
                }
            }

            RegionCount = region;
            WalkableCount = walkableCount;
        }

        private void Visit(int column, int row, int region, int[] queue, ref int tail)
        {
            if (!InGrid(column, row)) return;
            int index = IndexOf(column, row);
            if (!_walkable[index] || _regions[index] != 0) return;
            _regions[index] = region;
            queue[tail++] = index;
        }
        #endregion
    }
}

