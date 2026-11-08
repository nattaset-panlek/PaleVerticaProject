using CoreSystem.Data.Items;
using CoreSystem.Utility;
using UnityEngine;

namespace CoreSystem.UI.Inventory.Utility
{
    public struct ItemCardDisplay
    {
        #region [Variables/Fields]
        public const string EquippedLabel = "Equipped";

        public bool HasItem;
        public Sprite Icon;
        public ItemRarity Rarity;
        public string Name;
        public string Meta;
        public bool ShowRefine;
        public int Stars;
        public int SocketSlots;
        public int SocketFilled;
        public string Badge;
        public string Label;
        #endregion

        #region [Public Methods]
        public static ItemCardDisplay From(ItemInstance instance)
        {
            if (instance == null || instance.Item == null) return default;
            ItemSlotDisplay slot = ItemSlotDisplay.From(instance);
            Item item = instance.Item;
            var card = new ItemCardDisplay
            {
                HasItem = true,
                Icon = item.Icon,
                Rarity = item.Rarity,
                Name = instance.Name,
                Meta = item.Type.ToString(),
                ShowRefine = slot.Kind == ItemSlotKind.Instance && instance is EquipmentInstance,
                Stars = slot.Stars,
                SocketSlots = slot.SocketSlots,
                SocketFilled = slot.SocketFilled,
                Label = slot.Equipped ? EquippedLabel : null
            };
            if (slot.ShowAdvancement) card.Badge = "+" + NumberFormatter.Format(slot.Advancement);
            else if (slot.ShowQuantity) card.Badge = "x" + NumberFormatter.Format(slot.Quantity);
            return card;
        }
        #endregion
    }
}
