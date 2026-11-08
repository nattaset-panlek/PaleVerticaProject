using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace CoreSystem.Shop
{
    public static class ShopLoader
    {
        #region [Variables/Fields]
        public const string ResourcePath = "Data/Shop";

        [Serializable]
        private class Root
        {
            public List<ShopDefinition> Shops = new List<ShopDefinition>();
        }
        #endregion

        #region [Public Methods]
        public static List<ShopDefinition> LoadFromResources()
        {
            TextAsset file = Resources.Load<TextAsset>(ResourcePath);
            if (file == null)
            {
                Debug.LogWarning($"[ShopLoader] Resources/{ResourcePath}.json not found.");
                return new List<ShopDefinition>();
            }
            return Parse(file.text);
        }

        public static List<ShopDefinition> Parse(string json)
        {
            var shops = new List<ShopDefinition>();
            if (string.IsNullOrWhiteSpace(json)) return shops;
            try
            {
                Root root = JsonConvert.DeserializeObject<Root>(json);
                if (root?.Shops == null) return shops;
                foreach (ShopDefinition shop in root.Shops)
                {
                    if (shop == null || string.IsNullOrEmpty(shop.Id)) continue;
                    shop.Items ??= new List<ShopEntry>();
                    shop.Items.RemoveAll(entry => entry == null || entry.Price < 0);
                    shops.Add(shop);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ShopLoader] Shop.json parse failed: {e.Message}");
            }
            return shops;
        }

        public static string ToJson(List<ShopDefinition> shops)
        {
            return JsonConvert.SerializeObject(new Root { Shops = shops ?? new List<ShopDefinition>() }, Formatting.Indented);
        }
        #endregion
    }
}
