using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using UnityEngine;

namespace CoreSystem.UI.Inventory.Utility
{
    public struct ItemSlotDisplay
    {
        #region [Variables/Fields]
        public const int MaxStars = 5;
        public const int MaxSockets = 4;
        public const int RefinePerStar = 4;

        public ItemSlotKind Kind;
        public Sprite Icon;
        public ItemRarity Rarity;
        public int SocketSlots;
        public int SocketFilled;
        public int Stars;
        public int Advancement;
        public int Quantity;
        public bool Equipped;
        #endregion

        #region [Properties]
        public static ItemSlotDisplay Empty => new ItemSlotDisplay { Kind = ItemSlotKind.Empty };
        public bool ShowStars => Kind == ItemSlotKind.Instance && Stars > 0;
        public bool ShowSockets => Kind == ItemSlotKind.Instance && SocketSlots > 0;
        public bool ShowAdvancement => Kind == ItemSlotKind.Instance && Advancement > 0;
        public bool ShowQuantity => Kind == ItemSlotKind.Stack && Quantity > 1;
        #endregion

        #region [Public Methods]
        public static ItemSlotDisplay From(ItemInstance instance)
        {
            Item item = instance != null ? instance.Item : null;
            if (item == null) return Empty;

            var display = new ItemSlotDisplay
            {
                Kind = item is Equipment ? ItemSlotKind.Instance : ItemSlotKind.Stack,
                Icon = item.Icon,
                Rarity = item.Rarity,
                Quantity = instance.Quantity
            };

            if (instance is EquipmentInstance equipment)
            {
                display.Kind = ItemSlotKind.Instance;
                display.Stars = StarsForRefine(equipment.Refine);
                display.Advancement = Mathf.Max(0, equipment.Reforge);
                display.Equipped = equipment.IsEquipped;
                int sockets = item is Equipment model ? model.MaxSockets : 0;
                display.SocketSlots = Mathf.Clamp(sockets, 0, MaxSockets);
                int filled = equipment.SocketedFragments != null ? equipment.SocketedFragments.Count : 0;
                display.SocketFilled = Mathf.Clamp(filled, 0, display.SocketSlots);
            }
            else if (instance is CostumeInstance costume)
            {
                display.Equipped = costume.IsEquipped;
            }

            return display;
        }

        public static ItemSlotDisplay FromItem(Item item, int quantity)
        {
            if (item == null) return Empty;
            return new ItemSlotDisplay
            {
                Kind = item is Equipment ? ItemSlotKind.Instance : ItemSlotKind.Stack,
                Icon = item.Icon,
                Rarity = item.Rarity,
                Quantity = quantity
            };
        }

        public static int StarsForRefine(int refine)
        {
            if (refine <= 0) return 0;
            return Mathf.Clamp(Mathf.CeilToInt(refine / (float)RefinePerStar), 0, MaxStars);
        }
        #endregion
    }
}
