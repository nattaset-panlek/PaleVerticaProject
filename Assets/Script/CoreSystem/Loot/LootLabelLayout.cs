using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Loot
{
    public static class LootLabelLayout
    {
        #region [Public Methods]
        public static void Resolve(IReadOnlyList<Vector2> desired, IReadOnlyList<Vector2> sizes, float gap, List<Vector2> results)
        {
            results.Clear();
            int count = desired.Count;
            for (int i = 0; i < count; i++) results.Add(desired[i]);

            var order = new List<int>(count);
            for (int i = 0; i < count; i++) order.Add(i);
            order.Sort((a, b) =>
            {
                int byHeight = desired[a].y.CompareTo(desired[b].y);
                return byHeight != 0 ? byHeight : a.CompareTo(b);
            });

            for (int placed = 1; placed < count; placed++)
            {
                int index = order[placed];
                bool moved = true;
                while (moved)
                {
                    moved = false;
                    for (int previous = 0; previous < placed; previous++)
                    {
                        int other = order[previous];
                        if (!Overlaps(results[index], sizes[index], results[other], sizes[other], gap)) continue;

                        float y = results[other].y + (sizes[other].y + sizes[index].y) * 0.5f + gap;
                        results[index] = new Vector2(results[index].x, y);
                        moved = true;
                    }
                }
            }
        }
        #endregion

        #region [Private Methods]
        private static bool Overlaps(Vector2 a, Vector2 sizeA, Vector2 b, Vector2 sizeB, float gap)
        {
            bool xApart = Mathf.Abs(a.x - b.x) >= (sizeA.x + sizeB.x) * 0.5f + gap;
            bool yApart = Mathf.Abs(a.y - b.y) >= (sizeA.y + sizeB.y) * 0.5f + gap - 0.001f;
            return !xApart && !yApart;
        }
        #endregion
    }
}
