using System;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;

namespace CoreSystem.Shop
{
    [Serializable]
    public class ShopEntry
    {
        #region [Variables/Fields]
        public const int Unlimited = -1;

        public int ItemId;
        public int Price;
        public int Stock = Unlimited;
        #endregion

        #region [Properties]
        public Item Item => Database.GetItem(ItemId);
        public bool IsUnlimited => Stock < 0;
        public bool IsEquipment => Item is Equipment;
        #endregion
    }
}
