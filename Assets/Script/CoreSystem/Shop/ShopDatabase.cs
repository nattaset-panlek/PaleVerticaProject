using System.Collections.Generic;

namespace CoreSystem.Shop
{
    public static class ShopDatabase
    {
        #region [Variables/Fields]
        private static Dictionary<string, ShopDefinition> _shops;
        #endregion

        #region [Properties]
        public static IReadOnlyCollection<ShopDefinition> All
        {
            get
            {
                EnsureLoaded();
                return _shops.Values;
            }
        }
        #endregion

        #region [Public Methods]
        public static ShopDefinition Get(string shopId)
        {
            if (string.IsNullOrEmpty(shopId)) return null;
            EnsureLoaded();
            return _shops.TryGetValue(shopId, out ShopDefinition shop) ? shop : null;
        }

        public static void Register(ShopDefinition shop)
        {
            if (shop == null || string.IsNullOrEmpty(shop.Id)) return;
            EnsureLoaded();
            _shops[shop.Id] = shop;
        }

        public static void Reload()
        {
            Set(ShopLoader.LoadFromResources());
        }

        public static void Set(IEnumerable<ShopDefinition> shops)
        {
            _shops = new Dictionary<string, ShopDefinition>();
            if (shops == null) return;
            foreach (ShopDefinition shop in shops)
            {
                if (shop != null && !string.IsNullOrEmpty(shop.Id)) _shops[shop.Id] = shop;
            }
        }
        #endregion

        #region [Private Methods]
        private static void EnsureLoaded()
        {
            if (_shops == null) Reload();
        }
        #endregion
    }
}
