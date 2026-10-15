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
}
