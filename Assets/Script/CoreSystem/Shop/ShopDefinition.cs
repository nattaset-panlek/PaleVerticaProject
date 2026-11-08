using System;
using System.Collections.Generic;

namespace CoreSystem.Shop
{
    [Serializable]
    public class ShopDefinition
    {
        #region [Variables/Fields]
        public string Id;
        public string Name;
        public List<ShopEntry> Items = new List<ShopEntry>();
        #endregion
    }
}
