using System.Collections.Generic;
using System.Linq;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.UI.DevTools;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class DevToolDatabaseRowTests
    {
        #region [Public Methods]
        [Test]
        public void Fill_ClosedGroupsShowOnlyHeaders()
        {
            List<Item> items = Database.GetItems;
            var rows = new List<DevToolDatabaseRow>();

            DevToolDatabaseRow.Fill(rows, items, null, group => false);

            int types = items.Select(item => item.Type).Distinct().Count();
            Assert.AreEqual(types, rows.Count);
            Assert.IsTrue(rows.All(row => row.IsHeader && !row.IsOpen));
            string first = items[0].Type.ToString();
            int firstCount = items.Count(item => item.Type.ToString() == first);
            Assert.AreEqual($"{first.ToUpperInvariant()}  ·  {firstCount}", rows[0].Title);
        }

        [Test]
        public void Fill_OpenGroupListsItsItemsAfterItsHeader()
        {
            List<Item> items = Database.GetItems;
            string group = items[items.Count - 1].Type.ToString();
            List<Item> expected = items.Where(item => item.Type.ToString() == group).ToList();
            var rows = new List<DevToolDatabaseRow>();

            DevToolDatabaseRow.Fill(rows, items, null, key => key == group);

            int header = DevToolDatabaseRow.IndexOfHeader(rows, group);
            Assert.GreaterOrEqual(header, 0);
            Assert.IsTrue(rows[header].IsOpen);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.IsFalse(rows[header + 1 + i].IsHeader);
                Assert.AreSame(expected[i], rows[header + 1 + i].Item);
            }
            Assert.AreEqual(items.Select(item => item.Type).Distinct().Count() + expected.Count, rows.Count);
        }

        [Test]
        public void Fill_SearchHidesGroupsWithoutMatches()
        {
            List<Item> items = Database.GetItems;
            string needle = items[0].Name.Substring(0, System.Math.Min(4, items[0].Name.Length));
            System.Func<string, bool> matches = name => name != null && name.IndexOf(needle, System.StringComparison.OrdinalIgnoreCase) >= 0;
            var rows = new List<DevToolDatabaseRow>();

            DevToolDatabaseRow.Fill(rows, items, matches, group => true);

            int groups = items.Where(item => matches(item.Name)).Select(item => item.Type).Distinct().Count();
            Assert.AreEqual(groups, rows.Count(row => row.IsHeader));
            Assert.IsTrue(rows.Where(row => !row.IsHeader).All(row => matches(row.Item.Name)));
            Assert.AreEqual(-1, DevToolDatabaseRow.IndexOfHeader(rows, "NoSuchGroup"));
        }

        [Test]
        public void StateKey_MatchesTheFoldoutKeys()
        {
            Assert.AreEqual("db.Consumable.open", DevToolDatabaseRow.StateKey("Consumable"));
        }
        #endregion
    }
}
