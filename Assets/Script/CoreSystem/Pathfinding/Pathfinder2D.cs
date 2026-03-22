using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Pathfinding
{
    /// <summary>
    /// A* pathfinder for 2D grids.
    /// Uses PathfinderGrid.Instance as its data source.
    /// Thread-safety: call only from main thread.
    /// </summary>
    public static class Pathfinder2D
    {
        private static readonly List<PathNode> _openList   = new List<PathNode>(64);
        private static readonly HashSet<PathNode> _closed  = new HashSet<PathNode>();

        /// <summary>
        /// Find a path from worldStart to worldEnd.
        /// Returns a list of world-space positions from start+1 to end (inclusive).
        /// Returns null if no grid or no path found.
        /// </summary>
        public static List<Vector2> FindPath(Vector2 worldStart, Vector2 worldEnd)
        {
            var grid = PathfinderGrid.Instance;
            if (grid == null) return null;

            // Start a new search to reset node states efficiently
            int searchID = grid.StartNewSearch();

            if (!grid.WorldToGrid(worldStart, out int sx, out int sy)) return null;
            if (!grid.WorldToGrid(worldEnd,   out int ex, out int ey)) return null;

            PathNode startNode = grid.GetNode(sx, sy, searchID);
            PathNode endNode   = grid.GetNode(ex, ey, searchID);

            if (startNode == null || endNode == null) return null;
            if (!endNode.Walkable)
            {
                // Try nearest walkable around destination
                endNode = FindNearestWalkable(grid, ex, ey, searchID, 5);
                if (endNode == null) return null;
            }

            // Reset local lists
            _openList.Clear();
            _closed.Clear();

            startNode.GCost = 0;
            startNode.HCost = Heuristic(startNode, endNode);
            _openList.Add(startNode);

            // Since the grid is now global/infinite, we use a sensible max iteration limit
            const int maxIterations = 5000;
            int iterations    = 0;

            while (_openList.Count > 0 && iterations++ < maxIterations)
            {
                PathNode current = GetLowestF();

                if (current == endNode)
                    return BuildPath(endNode, grid);

                _openList.Remove(current);
                _closed.Add(current);

                foreach (var neighbor in grid.GetNeighbors(current, searchID))
                {
                    if (_closed.Contains(neighbor)) continue;

                    float tentativeG = current.GCost + Distance(current, neighbor);
                    bool inOpen = _openList.Contains(neighbor);

                    if (!inOpen || tentativeG < neighbor.GCost)
                    {
                        neighbor.GCost  = tentativeG;
                        neighbor.HCost  = Heuristic(neighbor, endNode);
                        neighbor.Parent = current;

                        if (!inOpen) _openList.Add(neighbor);
                    }
                }
            }

            return null; // No path
        }

        private static List<Vector2> BuildPath(PathNode endNode, PathfinderGrid grid)
        {
            var path = new List<Vector2>();
            PathNode current = endNode;
            while (current != null)
            {
                path.Add(grid.GridToWorld(current.X, current.Y));
                current = current.Parent;
            }
            path.Reverse();
            // Remove start node pos (index 0) so we only return waypoints ahead
            if (path.Count > 1) path.RemoveAt(0);
            return path;
        }

        private static PathNode GetLowestF()
        {
            PathNode best = _openList[0];
            for (int i = 1; i < _openList.Count; i++)
                if (_openList[i].FCost < best.FCost ||
                    (_openList[i].FCost == best.FCost && _openList[i].HCost < best.HCost))
                    best = _openList[i];
            return best;
        }

        private static float Heuristic(PathNode a, PathNode b)
            => Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y); // Manhattan

        private static float Distance(PathNode a, PathNode b)
            => (Mathf.Abs(a.X - b.X) == 1 && Mathf.Abs(a.Y - b.Y) == 1) ? 1.414f : 1f;

        private static PathNode FindNearestWalkable(PathfinderGrid grid, int cx, int cy, int searchID, int searchRadius)
        {
            for (int r = 1; r <= searchRadius; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue;
                        var n = grid.GetNode(cx + dx, cy + dy, searchID);
                        if (n != null && n.Walkable) return n;
                    }
                }
            }
            return null;
        }
    }
}
