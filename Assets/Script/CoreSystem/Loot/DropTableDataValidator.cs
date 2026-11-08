using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSystem.Loot
{
    public static class DropTableDataValidator
    {
        #region [Public Methods]
        public static List<string> Validate(DropTableModel table, IReadOnlyList<DropTableModel> all, Func<int, bool> itemExists)
        {
            var problems = new List<string>();
            if (table == null) return problems;

            if (all.Count(other => other.Id == table.Id) > 1) problems.Add($"Drop table id {table.Id} is used more than once.");
            if (table.Id <= 0) problems.Add("Id must be 1 or more, because 0 means no drops.");
            if (table.Entries == null || table.Entries.Count == 0)
            {
                problems.Add("The table has no items.");
                return problems;
            }

            foreach (DropEntry entry in table.Entries)
            {
                if (!itemExists(entry.ItemId)) problems.Add($"Item {entry.ItemId} does not exist.");
                if (entry.Chance <= 0f) problems.Add($"Item {entry.ItemId} has a drop chance of 0, so it never drops.");
                if (entry.MinQty < 1) problems.Add($"Item {entry.ItemId}: Min must be 1 or more.");
                if (entry.MaxQty < entry.MinQty) problems.Add($"Item {entry.ItemId}: Max is below Min.");
            }
            return problems;
        }
        #endregion
    }
}
