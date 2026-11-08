using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.UI.Inventory.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static CoreSystem.Editor.UI.EditorUIFactory;

namespace CoreSystem.Editor.UI
{
    public static class ItemSlotSnapshotSheet
    {
        #region [Variables/Fields]
        public const string FileName = "itemslot_sheet";
        public static readonly Color Backdrop = new Color(0.263f, 0.263f, 0.263f, 1f);
        private static readonly Vector2 SheetSize = new Vector2(816f, 336f);
        private static readonly ItemRarity[] Rarities = { ItemRarity.Legendary, ItemRarity.Epic, ItemRarity.Rare, ItemRarity.Uncommon, ItemRarity.Common };
        #endregion

        #region [Public Methods]
        public static GameObject Create()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ItemSlotPrefabBuilder.ArtPrefabPath);
            if (prefab == null) return null;

            var root = new GameObject("ItemSlot Sheet", typeof(RectTransform));
            var rect = (RectTransform)root.transform;
            rect.sizeDelta = SheetSize;
            var background = root.AddComponent<Image>();
            background.color = Hex("2a2a2a");
            background.raycastTarget = false;

            Sprite icon = PickIcon();
            for (int row = 0; row < 2; row++)
            {
                for (int column = 0; column < Rarities.Length; column++)
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
                    instance.name = $"{(row == 0 ? "Instance" : "Stack")} {Rarities[column]}";
                    SetTopLeft((RectTransform)instance.transform, 24f + column * 160f, 24f + row * 160f, 128f, 128f);
                    var view = instance.GetComponent<ItemSlotArtView>();
                    view.Fit();
                    view.Show(Sample(row == 0, Rarities[column], icon));
                }
            }
            return root;
        }
        #endregion

        #region [Private Methods]
        private static ItemSlotDisplay Sample(bool instance, ItemRarity rarity, Sprite icon)
        {
            if (!instance) return new ItemSlotDisplay { Kind = ItemSlotKind.Stack, Rarity = rarity, Icon = icon, Quantity = 999 };
            return new ItemSlotDisplay { Kind = ItemSlotKind.Instance, Rarity = rarity, Icon = icon, Stars = 2, Advancement = 999, SocketSlots = 4, SocketFilled = 2 };
        }

        private static Sprite PickIcon()
        {
            Item fallback = null;
            foreach (Item item in Database.GetItems)
            {
                if (item == null || item.Icon == null) continue;
                if (item is Equipment equipment && equipment.Slot.ToString().Contains("Head")) return item.Icon;
                if (fallback == null && item is Equipment) fallback = item;
            }
            return fallback != null ? fallback.Icon : null;
        }
        #endregion
    }
}
