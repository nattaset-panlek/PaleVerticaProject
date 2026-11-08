using System;
using System.Collections.Generic;
using System.Linq;
using CoreSystem.Data.Items;

namespace CoreSystem.UI.DevTools
{
    public readonly struct DevToolDatabaseRow
    {
        #region [Properties]
        public string Group { get; }
        public string Title { get; }
        public bool IsOpen { get; }
        public Item Item { get; }
        public bool IsHeader => Item == null;
        #endregion

        #region [Public Methods]
        public DevToolDatabaseRow(string group, string title, bool isOpen, Item item)
        {
            Group = group;
            Title = title;
            IsOpen = isOpen;
            Item = item;
        }

        public static string StateKey(string group)
        {
            return $"db.{group}.open";
        }

        public static void Fill(List<DevToolDatabaseRow> rows, IEnumerable<Item> items, Func<string, bool> matches, Func<string, bool> isOpen)
        {
            rows.Clear();
            if (items == null) return;
            foreach (IGrouping<string, Item> group in items.GroupBy(item => item.Type.ToString()))
            {
                List<Item> shown = group.Where(item => matches == null || matches(item.Name)).ToList();
                if (shown.Count == 0) continue;

                bool open = isOpen != null && isOpen(group.Key);
                rows.Add(new DevToolDatabaseRow(group.Key, $"{group.Key.ToUpperInvariant()}  ·  {shown.Count}", open, null));
                if (!open) continue;
                for (int i = 0; i < shown.Count; i++) rows.Add(new DevToolDatabaseRow(group.Key, null, true, shown[i]));
            }
        }

        public static int IndexOfHeader(List<DevToolDatabaseRow> rows, string group)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].IsHeader && rows[i].Group == group) return i;
            }
            return -1;
        }
        #endregion
    }
}
