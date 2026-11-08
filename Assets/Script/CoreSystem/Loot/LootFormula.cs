using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Loot
{
    public static class LootFormula
    {
        #region [Public Methods]
        public static List<LootRoll> Roll(DropTableModel table, Func<float> random01)
        {
            var result = new List<LootRoll>();
            if (table == null || table.Entries == null || random01 == null) return result;

            foreach (DropEntry entry in table.Entries)
            {
                if (entry == null || entry.ItemId <= 0 || entry.MaxQty <= 0) continue;
                if (random01() >= entry.Chance) continue;

                int min = Mathf.Max(1, entry.MinQty);
                int max = Mathf.Max(min, entry.MaxQty);
                int quantity = Mathf.Min(max, min + Mathf.FloorToInt(random01() * (max - min + 1)));
                result.Add(new LootRoll(entry.ItemId, quantity));
            }
            return result;
        }

        public static bool CanPickUp(object owner, object picker, float age, float ownerLockSeconds)
        {
            if (picker == null) return false;
            if (owner == null || age >= ownerLockSeconds) return true;
            return ReferenceEquals(owner, picker);
        }
        #endregion
    }
}
