using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSystem.Shop
{
    public static class ShopDataValidator
    {
        #region [Public Methods]
        public static List<string> Validate(ShopDefinition shop, IReadOnlyList<ShopDefinition> all, Func<int, bool> itemExists)
        {
            var problems = new List<string>();
            if (shop == null) return problems;

            if (string.IsNullOrEmpty(shop.Id)) problems.Add("Shop Id is empty, so the game ignores this shop.");
            else if (all.Count(other => other.Id == shop.Id) > 1) problems.Add($"Shop Id '{shop.Id}' is used more than once.");

            if (shop.Items == null || shop.Items.Count == 0)
            {
                problems.Add("The shop sells nothing.");
                return problems;
            }

            foreach (ShopEntry entry in shop.Items)
            {
                if (!itemExists(entry.ItemId)) problems.Add($"Item {entry.ItemId} does not exist.");
                if (entry.Price < 0) problems.Add($"Item {entry.ItemId} has a negative price, so the game removes it.");
                if (entry.Stock == 0) problems.Add($"Item {entry.ItemId} has 0 stock, so it is always sold out.");
            }
            return problems;
        }
        #endregion
    }
}
