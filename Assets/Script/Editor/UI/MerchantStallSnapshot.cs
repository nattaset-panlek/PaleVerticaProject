using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Shop;
using CoreSystem.UI.Shop;
using UnityEngine;

namespace CoreSystem.Editor.UI
{
    public static class MerchantStallSnapshot
    {
        #region [Variables/Fields]
        public const string ShopId = "general_merchant";
        public const int SampleCredits = 2450000;
        public static readonly Color Backdrop = new Color(0.553f, 0.553f, 0.553f, 1f);
        public static readonly string[] States = { "a", "b" };
        #endregion

        #region [Public Methods]
        public static void Setup(GameObject instance, string state)
        {
            var window = instance.GetComponent<MerchantStallWindow>();
            ShopDatabase.Reload();
            ShopDefinition shop = ShopDatabase.Get(ShopId);
            if (window == null || shop == null) return;

            var inventory = new PlayerInventory();
            inventory.AddCredit(SampleCredits);
            var session = new ShopSession(shop);
            List<int> stackables = Stackables(session);
            for (int i = 0; i < stackables.Count && i < 3; i++) session.AddToCart(stackables[i], 10 * (i + 1));

            window.Bind(session, inventory);
            if (window.WareOrder.Count > 0) window.FocusWare(0);

            if (state == "a")
            {
                if (window.CartSlots.Count > 0) window.CartSlots[0].ForceHover(true);
                return;
            }

            if (window.CartSlots.Count > 3) window.CartSlots[3].ForceDropTarget();
            if (stackables.Count > 0) window.RequestQuantity(stackables[0]);
        }
        #endregion

        #region [Private Methods]
        private static List<int> Stackables(ShopSession session)
        {
            var list = new List<int>();
            for (int i = 0; i < session.Entries.Count; i++)
            {
                if (session.Entries[i].Item != null && session.Entries[i].Item.IsStackable) list.Add(i);
            }
            return list;
        }
        #endregion
    }
}
