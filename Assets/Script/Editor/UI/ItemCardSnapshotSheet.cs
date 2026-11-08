using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.UI.Inventory.Utility;
using UnityEditor;
using UnityEngine;
using static CoreSystem.Editor.UI.EditorUIFactory;

namespace CoreSystem.Editor.UI
{
    public static class ItemCardSnapshotSheet
    {
        #region [Variables/Fields]
        public const string FileName = "itemcard_sheet";
        public static readonly Color Backdrop = new Color(0.263f, 0.263f, 0.263f, 1f);
        private const float ColumnPitch = 536f;
        private const float RowPitch = 112f;
        private static readonly ItemCardState[] States = { ItemCardState.Default, ItemCardState.Hover, ItemCardState.Focus, ItemCardState.Pressed, ItemCardState.Disabled };
        #endregion

        #region [Public Methods]
        public static GameObject Create()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ItemCardPrefabBuilder.CardPrefabPath);
            if (prefab == null) return null;

            var root = new GameObject("ItemCard Sheet", typeof(RectTransform));
            ((RectTransform)root.transform).sizeDelta = new Vector2(24f * 2f + ColumnPitch + ItemCardView.CardWidth, 24f + States.Length * RowPitch);

            ItemCardDisplay display = Sample();
            for (int column = 0; column < 2; column++)
            {
                for (int row = 0; row < States.Length; row++)
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
                    instance.name = $"{(column == 0 ? "White" : "Dark")} {States[row]}";
                    SetTopLeft((RectTransform)instance.transform, 24f + column * ColumnPitch, 24f + row * RowPitch, ItemCardView.CardWidth, ItemCardView.CardHeight);
                    var view = instance.GetComponent<ItemCardView>();
                    view.SetTheme(column == 0 ? ItemCardTheme.White : ItemCardTheme.Dark);
                    view.Fit();
                    view.Show(display);
                    view.ForceState(States[row]);
                }
            }
            return root;
        }
        #endregion

        #region [Private Methods]
        private static ItemCardDisplay Sample()
        {
            Sprite icon = null;
            foreach (Item item in Database.GetItems)
            {
                if (item is Equipment && item.Icon != null) { icon = item.Icon; break; }
            }
            return new ItemCardDisplay
            {
                HasItem = true,
                Icon = icon,
                Rarity = ItemRarity.Uncommon,
                Name = "Ashen Blade",
                ShowRefine = true,
                Stars = 3,
                SocketSlots = 3,
                SocketFilled = 2,
                Badge = "+7",
                Label = ItemCardDisplay.EquippedLabel
            };
        }
        #endregion
    }
}
