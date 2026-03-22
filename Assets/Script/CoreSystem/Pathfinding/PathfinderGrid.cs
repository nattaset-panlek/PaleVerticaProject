using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Pathfinding
{
    /// <summary>
    /// A single node on the pathfinding grid.
    /// </summary>
    public class PathNode
    {
        public int X, Y;
        public bool Walkable;

        // Search tracking for efficient resetting
        public int LastSearchID;

        // A* costs
        public float GCost;   // distance from start
        public float HCost;   // estimated distance to end
        public float FCost => GCost + HCost;

        public PathNode Parent;

        public PathNode(int x, int y, bool walkable)
        {
            X = x;
            Y = y;
            Walkable = walkable;
        }

        /// <summary>
        /// Resets cost data if the search ID has changed.
        /// </summary>
        public void CheckReset(int searchID)
        {
            if (LastSearchID != searchID)
            {
                LastSearchID = searchID;
                GCost = 0;
                HCost = 0;
                Parent = null;
            }
        }
    }

    /// <summary>
    /// Generates and caches the walkable grid based on Physics2D overlap checks.
    /// Attach to a single GameObject per scene (or let PathfinderGrid.Instance auto-create).
    /// </summary>
    public class PathfinderGrid : MonoBehaviour
    {
        public static PathfinderGrid Instance { get; private set; }

        [Header("Grid Settings")]
        public float NodeSize    = 1f;
        public Vector2 GridOrigin = Vector2.zero;

        [Header("Obstacle Detection")]
        [Tooltip("Layers treated as solid obstacles.")]
        public LayerMask ObstacleLayers;

        [Tooltip("Radius used for overlap check per cell.")]
        public float ObstacleCheckRadius = 0.4f;

        private Dictionary<Vector2Int, PathNode> _nodeCache = new Dictionary<Vector2Int, PathNode>();
        private int _currentSearchID = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Clears the generated node cache. Call this if static obstacles change.
        /// </summary>
        public void ClearCache()
        {
            _nodeCache.Clear();
        }

        /// <summary>
        /// Prepares for a new pathfinding request and returns a unique ID to track node states.
        /// </summary>
        public int StartNewSearch() => ++_currentSearchID;

        // ─── Coordinate helpers ─────────────────────────────────────

        public Vector2 GridToWorld(int x, int y)
            => GridOrigin + new Vector2(x * NodeSize, y * NodeSize);

        public bool WorldToGrid(Vector2 worldPos, out int gx, out int gy)
        {
            gx = Mathf.RoundToInt((worldPos.x - GridOrigin.x) / NodeSize);
            gy = Mathf.RoundToInt((worldPos.y - GridOrigin.y) / NodeSize);
            return true; // Dynamic grid is always in bounds
        }

        /// <summary>
        /// Retrieves a node from cache or generates it if it doesn't exist.
        /// </summary>
        public PathNode GetNode(int x, int y, int searchID = -1)
        {
            var key = new Vector2Int(x, y);
            if (!_nodeCache.TryGetValue(key, out var node))
            {
                Vector2 worldPos = GridToWorld(x, y);
                bool walkable = Physics2D.OverlapCircle(worldPos, ObstacleCheckRadius, ObstacleLayers) == null;
                node = new PathNode(x, y, walkable);
                _nodeCache[key] = node;
            }

            if (searchID != -1)
            {
                node.CheckReset(searchID);
            }

            return node;
        }

        public PathNode GetNodeFromWorld(Vector2 worldPos, int searchID = -1)
        {
            WorldToGrid(worldPos, out int x, out int y);
            return GetNode(x, y, searchID);
        }

        public List<PathNode> GetNeighbors(PathNode node, int searchID, bool allowDiagonal = true)
        {
            var neighbors = new List<PathNode>();
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    if (!allowDiagonal && Mathf.Abs(dx) == 1 && Mathf.Abs(dy) == 1) continue;
                    
                    var n = GetNode(node.X + dx, node.Y + dy, searchID);
                    if (n != null && n.Walkable) neighbors.Add(n);
                }
            }
            return neighbors;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_nodeCache == null) return;
            
            // Only draw nodes near the grid origin or any recent search if needed
            // For simplicity, we draw the first 1000 nodes in cache to avoid CPU lag
            int count = 0;
            foreach (var kvp in _nodeCache)
            {
                if (count++ > 2000) break;
                
                var node = kvp.Value;
                Gizmos.color = node.Walkable
                    ? new Color(0f, 1f, 0f, 0.08f)
                    : new Color(1f, 0f, 0f, 0.18f);
                Gizmos.DrawCube(GridToWorld(node.X, node.Y), Vector3.one * (NodeSize * 0.9f));
            }
        }
#endif
    }
}
