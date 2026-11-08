using System.Collections.Generic;
using System.Text;
using CoreSystem.UI;
using CoreSystem.UI.Inventory.Utility;
using CoreSystem.UI.Shop;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static CoreSystem.Editor.UI.EditorUIFactory;

namespace CoreSystem.Editor.UI
{
    public static class MerchantStallPrefabBuilder
    {
        #region [Variables/Fields]
        public const string WindowPrefabPath = "Assets/Prefab/Game Page/Merchant Stall Window.prefab";
        public const string WarePrefabPath = "Assets/Prefab/UI/Shop/Ware Card.prefab";
        public const string CartSlotPrefabPath = "Assets/Prefab/UI/Shop/Cart Slot.prefab";
        public const string QuantityModalPrefabPath = "Assets/Prefab/UI/Shop/Quantity Modal.prefab";
        public const string WindowName = "Merchant Stall";
        private const string RegistryPath = "Assets/Prefab/UIWindowRegistry.asset";
        private const string ShopArt = "Assets/Art/UI/New/Shop/";
        private const string WindowArt = ShopArt + "UI_Stall_Window.png";
        private const string TagArt = ShopArt + "UI_Stall_WindowTag.png";
        private const string HeaderArt = ShopArt + "UI_Stall_ColumnHeader.png";
        private const string QuantityArt = ShopArt + "UI_Stall_QuantityModal.png";
        private const string HoverRingArt = ShopArt + "UI_CartSlot_HoverRing.png";
        private const string RemoveArt = ShopArt + "UI_Stall_RemoveButton.png";
        private const string Pill8Art = ShopArt + "UI_Stall_Pill8.png";
        private const string Outline7Art = ShopArt + "UI_Stall_Outline7.png";
        private const string Outline8Art = ShopArt + "UI_Stall_Outline8.png";
        private const string Pill25Art = ShopArt + "UI_Stall_Pill25.png";
        private const string BuyButtonArt = ShopArt + "UI_Stall_BuyButton.png";
        private const string CancelButtonArt = ShopArt + "UI_Stall_CancelButton.png";
        private const string AddButtonArt = ShopArt + "UI_Stall_AddButton.png";
        private const string Pill4Art = "Assets/Art/UI/New/ItemCard/UI_ItemCard_Pill4.png";
        private const string CloseIconArt = "Assets/Art/UI/New/Icon/UI_Close_Secon_Icon.png";
        private const string StarIconArt = "Assets/Art/UI/New/Icon/UI_StarGylp_Icon.png";
        private const string NotoSemiBoldPath = "Assets/Art/Front/Noto Sans Thai/NotoSansThai-SemiBold SDF.asset";
        private const string KanitBoldPath = "Assets/Art/Front/Kanit/Kanit-Bold SDF.asset";
        private static readonly Vector2 WindowSize = new Vector2(1160f, 780f);
        private static readonly float[] CartColumns = { 618f, 708f, 797f, 887f, 976f, 1066f };
        private static readonly float[] CartRows = { 184f, 268f, 351f, 435f, 518f, 602f };
        private static readonly Color Ink = new Color(0.078f, 0.075f, 0.075f, 1f);
        private static readonly Color Rule = new Color(0.078f, 0.075f, 0.075f, 0.12f);
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/UI/Build Merchant Stall")]
        public static void BuildFromMenu()
        {
            if (!EditorUtility.DisplayDialog("Build Merchant Stall", $"Rebuilds\n{WindowPrefabPath}\n{WarePrefabPath}\n{CartSlotPrefabPath}\n{QuantityModalPrefabPath}\nand registers '{WindowName}' in the UI window registry.", "Build", "Cancel")) return;
            Debug.Log(Build());
        }

        public static void BuildBatch()
        {
            int exitCode = 0;
            try
            {
                Debug.Log(Build());
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                exitCode = 1;
            }
            EditorApplication.Exit(exitCode);
        }

        public static string Build()
        {
            var log = new StringBuilder("[MerchantStallPrefabBuilder]");
            PrepareSprites();
            EnsureFolder("Assets/Prefab/UI/Shop");
            Save(WarePrefabPath, "Ware Card", FillWare);
            Save(CartSlotPrefabPath, "Cart Slot", FillCartSlot);
            Save(QuantityModalPrefabPath, "Quantity Modal", FillQuantityModal);
            Save(WindowPrefabPath, "Merchant Stall Window", FillWindow);
            Register();
            AssetDatabase.SaveAssets();
            log.Append($"\n- {WarePrefabPath}\n- {CartSlotPrefabPath}\n- {QuantityModalPrefabPath}\n- {WindowPrefabPath}\n- registry entry '{WindowName}'");
            return log.ToString();
        }
        #endregion

        #region [Private Methods]
        private static void PrepareSprites()
        {
            foreach (string path in new[] { WindowArt, TagArt, HeaderArt, QuantityArt, HoverRingArt, RemoveArt, BuyButtonArt, CancelButtonArt, AddButtonArt }) PrepareSprite(path, Vector4.zero);
            PrepareSprite(Pill8Art, new Vector4(10f, 10f, 10f, 10f));
            PrepareSprite(Outline7Art, new Vector4(10f, 10f, 10f, 10f));
            PrepareSprite(Outline8Art, new Vector4(10f, 10f, 10f, 10f));
            PrepareSprite(Pill25Art, new Vector4(26f, 26f, 26f, 26f));
        }

        private static void Save(string path, string name, System.Action<GameObject> fill)
        {
            bool exists = Load<GameObject>(path) != null;
            GameObject root = exists ? PrefabUtility.LoadPrefabContents(path) : new GameObject(name, typeof(RectTransform));
            try
            {
                root.layer = 5;
                ClearChildren(root.transform);
                fill(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                if (exists) PrefabUtility.UnloadPrefabContents(root);
                else Object.DestroyImmediate(root);
            }
        }

        private static T Ensure<T>(GameObject go) where T : Component => go.GetComponent<T>() ?? go.AddComponent<T>();

        private static void FillWare(GameObject root)
        {
            ((RectTransform)root.transform).sizeDelta = new Vector2(ItemCardView.CardWidth, ItemCardView.CardHeight);
            ItemCardView card = ItemCardPrefabBuilder.AddCard(root.transform, ItemCardTheme.White);
            var slot = Ensure<MerchantWareSlot>(root);
            SetObject(slot, "_card", card);
        }

        private static void FillCartSlot(GameObject root)
        {
            ((RectTransform)root.transform).sizeDelta = new Vector2(64f, 64f);
            var hit = Ensure<Image>(root);
            hit.sprite = null;
            hit.color = new Color(0f, 0f, 0f, 0f);
            hit.raycastTarget = true;

            Image empty = AddImage(root.transform, "Empty Body", LoadSprite(Pill4Art), new Color(0.102f, 0.102f, 0.102f, 0.18f), 2f, 2f, 60f, 60f);
            empty.type = Image.Type.Sliced;
            RectTransform artHolder = NewChild(root.transform, "Slot");
            SetTopLeft(artHolder, 0f, 0f, 64f, 64f);
            ItemSlotArtView art = ItemSlotPrefabBuilder.AddArt(artHolder, "ItemSlot Art");
            Image ring = AddImage(root.transform, "Hover Ring", LoadSprite(HoverRingArt), Color.white, -32f, -32f, 128f, 128f);
            Image outline = AddImage(root.transform, "Drop Outline", LoadSprite(Outline7Art), Ink, -2f, -2f, 68f, 68f);
            outline.type = Image.Type.Sliced;
            Image remove = AddImage(root.transform, "Remove Button", LoadSprite(RemoveArt), Color.white, 49.2f, -10.8f, 25.6f, 25.6f);
            remove.raycastTarget = true;
            var removeButton = remove.gameObject.AddComponent<Button>();
            removeButton.transition = Selectable.Transition.None;

            ring.gameObject.SetActive(false);
            outline.gameObject.SetActive(false);
            remove.gameObject.SetActive(false);
            artHolder.gameObject.SetActive(true);

            var view = Ensure<CartSlotView>(root);
            SetObject(view, "_art", art);
            SetObject(view, "_emptyBody", empty.gameObject);
            SetObject(view, "_dropOutline", outline.gameObject);
            SetObject(view, "_hoverRing", ring.gameObject);
            SetObject(view, "_removeButton", removeButton);
        }

        private static void FillQuantityModal(GameObject root)
        {
            var rect = (RectTransform)root.transform;
            StretchFull(rect);
            rect.sizeDelta = Vector2.zero;
            TMP_FontAsset semiBold = Load<TMP_FontAsset>(KanitSemiBoldPath);

            Image dim = AddImage(root.transform, "Dim", null, new Color(0f, 0f, 0f, 0.25f), 0f, 0f, 0f, 0f);
            dim.raycastTarget = true;
            RectTransform dimRect = dim.rectTransform;
            dimRect.anchorMin = dimRect.anchorMax = dimRect.pivot = new Vector2(0.5f, 0.5f);
            dimRect.anchoredPosition = Vector2.zero;
            dimRect.sizeDelta = new Vector2(7680f, 4320f);

            RectTransform panel = NewChild(root.transform, "Panel");
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = new Vector2(528f, 268f);
            Image bg = AddImage(panel, "Background", LoadSprite(QuantityArt), Color.white, 0f, 0f, 1024f, 512f);
            bg.raycastTarget = true;

            TextMeshProUGUI title = AddText(panel, "Title", "Item", semiBold, 24f, Ink, TextAlignmentOptions.MidlineLeft, 24f, 16f, 440f, 36f);
            title.overflowMode = TextOverflowModes.Ellipsis;
            Button close = IconButton(panel, "Close", LoadSprite(CloseIconArt), Ink, 480f, 21f, 24f, 24f);

            Button minus = OutlineButton(panel, "Minus Button", 24f, 72f, 96f, 44f);
            AddImage(minus.transform, "Icon", null, Ink, 38f, 20f, 20f, 3.5f);
            TextMeshProUGUI value = AddText(panel, "Value", "1", semiBold, 40f, Ink, TextAlignmentOptions.Center, 120f, 68f, 288f, 52f);
            Button plus = OutlineButton(panel, "Plus Button", 408f, 72f, 96f, 44f);
            AddImage(plus.transform, "Icon H", null, Ink, 38f, 20f, 20f, 3.5f);
            AddImage(plus.transform, "Icon V", null, Ink, 46.3f, 11.8f, 3.5f, 20f);

            Image frame = AddImage(panel, "Preset Frame", LoadSprite(Outline8Art), Ink, 24f, 132f, 480f, 44f);
            frame.type = Image.Type.Sliced;
            Image active = AddImage(panel, "Preset Active", null, Ink, 184f, 132f, 160f, 44f);
            var presets = new List<Button>();
            var labels = new List<TMP_Text>();
            string[] names = { "One", "Half", "Max" };
            for (int i = 0; i < names.Length; i++)
            {
                Image hit = AddImage(panel, $"Preset {names[i]}", null, new Color(0f, 0f, 0f, 0f), 24f + i * 160f, 132f, 160f, 44f);
                hit.raycastTarget = true;
                var button = hit.gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                presets.Add(button);
                TextMeshProUGUI label = AddText(hit.transform, "Label", names[i], semiBold, 18f, Ink, TextAlignmentOptions.Center, 0f, 0f, 160f, 44f);
                labels.Add(label);
            }

            Button cancel = ArtButton(panel, "Cancel Button", LoadSprite(CancelButtonArt), 24f, 192f, 225f, 50f, "Cancel", semiBold, 18f, Ink);
            Button confirm = ArtButton(panel, "Add Button", LoadSprite(AddButtonArt), 279f, 192f, 225f, 50f, "Add x1", semiBold, 18f, Color.white);

            var view = Ensure<QuantityModalView>(root);
            SetObject(view, "_title", title);
            SetObject(view, "_value", value);
            SetObject(view, "_minus", minus);
            SetObject(view, "_plus", plus);
            SetObjects(view, "_presets", presets.ToArray());
            SetObjects(view, "_presetLabels", labels.ToArray());
            SetObject(view, "_presetActive", active.rectTransform);
            SetObject(view, "_cancel", cancel);
            SetObject(view, "_close", close);
            SetObject(view, "_confirm", confirm);
            SetObject(view, "_confirmLabel", confirm.GetComponentInChildren<TMP_Text>());
        }

        private static void FillWindow(GameObject root)
        {
            var rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = WindowSize;
            TMP_FontAsset semiBold = Load<TMP_FontAsset>(KanitSemiBoldPath);
            TMP_FontAsset medium = Load<TMP_FontAsset>(KanitMediumPath);
            TMP_FontAsset bold = Load<TMP_FontAsset>(KanitBoldPath);
            TMP_FontAsset novecento = Load<TMP_FontAsset>(NovecentoPath);
            TMP_FontAsset noto = Load<TMP_FontAsset>(NotoSemiBoldPath);

            Image background = AddImage(root.transform, "Background", LoadSprite(WindowArt), Color.white, 0f, 0f, 2048f, 1024f);
            background.raycastTarget = true;
            RectTransform header = NewChild(root.transform, "Header");
            SetTopLeft(header, 0f, 0f, WindowSize.x, 104f);

            AddImage(root.transform, "Window Tag", LoadSprite(TagArt), Color.white, 32f, 32f, 256f, 64f);
            TextMeshProUGUI title = AddText(root.transform, "Title", "Merchant", bold, 24f, Color.white, TextAlignmentOptions.MidlineLeft, 85f, 32f, 144f, 41f);
            title.overflowMode = TextOverflowModes.Ellipsis;
            Button close = IconButton(root.transform, "Close", LoadSprite(CloseIconArt), Ink, 1106f, 40f, 24f, 24f);

            ColumnHeader(root.transform, "Wares Header", "Wares", novecento, 32f);
            ColumnHeader(root.transform, "Cart Header", "Cart", novecento, 618f);

            Transform waresContent = BuildWaresScroll(root.transform);
            AddImage(root.transform, "Column Divider", null, Rule, 581f, 128f, 1f, 560f);

            var cartSlots = new List<CartSlotView>();
            var cartPrefab = Load<GameObject>(CartSlotPrefabPath);
            RectTransform cart = NewChild(root.transform, "Cart Grid");
            SetTopLeft(cart, 0f, 0f, WindowSize.x, WindowSize.y);
            for (int row = 0; row < CartRows.Length; row++)
            {
                for (int column = 0; column < CartColumns.Length; column++)
                {
                    var slot = (GameObject)PrefabUtility.InstantiatePrefab(cartPrefab, cart);
                    slot.name = $"Cart Slot {row * CartColumns.Length + column + 1:00}";
                    SetTopLeft((RectTransform)slot.transform, CartColumns[column], CartRows[row], 64f, 64f);
                    cartSlots.Add(slot.GetComponent<CartSlotView>());
                }
            }

            AddImage(root.transform, "Footer Rule", null, Rule, 32f, 683f, 1098f, 1f);
            Image wallet = AddImage(root.transform, "Wallet Badge", LoadSprite(Pill8Art), EditorUIFactory.Hex("141414"), 32f, 704f, 200f, 42f);
            wallet.type = Image.Type.Sliced;
            AddImage(wallet.transform, "Icon", LoadSprite(StarIconArt), Color.white, 14f, 8f, 26f, 26f);
            TextMeshProUGUI walletAmount = AddText(wallet.transform, "Amount", "0", semiBold, 22f, Color.white, TextAlignmentOptions.MidlineLeft, 50f, 4.5f, 146f, 33f);
            TextMeshProUGUI totalLabel = AddText(root.transform, "Total Label", "Total", medium, 16f, EditorUIFactory.Hex("6b6b6b"), TextAlignmentOptions.MidlineLeft, 618f, 718f, 60f, 24f);
            totalLabel.characterSpacing = 1f / 16f * 100f;
            totalLabel.fontStyle = FontStyles.UpperCase;
            TextMeshProUGUI total = AddText(root.transform, "Total", "0", semiBold, 30f, Ink, TextAlignmentOptions.MidlineLeft, 684f, 703f, 240f, 45f);
            Button buy = ArtButton(root.transform, "Buy All Button", LoadSprite(BuyButtonArt), 949f, 700f, 181f, 50.2f, "Buy All", noto, 18f, EditorUIFactory.Hex("c9c9c9"));

            var modalPrefab = Load<GameObject>(QuantityModalPrefabPath);
            var modal = (GameObject)PrefabUtility.InstantiatePrefab(modalPrefab, root.transform);
            modal.name = "Quantity Modal";
            StretchFull((RectTransform)modal.transform);
            modal.SetActive(false);

            var window = Ensure<MerchantStallWindow>(root);
            SetObject(window, "_closeButton", close);
            SetObject(window, "_waresContent", waresContent);
            SetObject(window, "_wareItemPrefab", Load<GameObject>(WarePrefabPath));
            SetObjects(window, "_cartSlots", cartSlots.ToArray());
            SetObject(window, "_walletAmount", walletAmount);
            SetObject(window, "_total", total);
            SetObject(window, "_buyAllButton", buy);
            SetObject(window, "_quantityModal", modal.GetComponent<QuantityModalView>());
            var so = new SerializedObject(window);
            so.FindProperty("_headerRect").objectReferenceValue = header;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform BuildWaresScroll(Transform root)
        {
            RectTransform scroll = NewChild(root, "Wares Scroll");
            SetTopLeft(scroll, 26f, 178f, 524f, 494f);
            var scrollRect = scroll.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 30f;

            RectTransform viewport = NewChild(scroll, "Viewport");
            SetTopLeft(viewport, 0f, 0f, 508f, 494f);
            viewport.gameObject.AddComponent<RectMask2D>();
            var viewportHit = viewport.gameObject.AddComponent<Image>();
            viewportHit.color = new Color(0f, 0f, 0f, 0f);

            RectTransform content = NewChild(viewport, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 494f);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            Image track = AddImage(scroll, "Scrollbar", LoadSprite(Pill25Art), EditorUIFactory.Hex("4b4b4b"), 514f, 6f, 4f, 482f);
            track.type = Image.Type.Sliced;
            track.pixelsPerUnitMultiplier = 6.5f;
            track.raycastTarget = true;
            var scrollbar = track.gameObject.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.transition = Selectable.Transition.None;
            RectTransform area = NewChild(track.transform, "Sliding Area");
            StretchFull(area);
            Image handle = AddImage(area, "Handle", LoadSprite(Pill25Art), Color.white, 0f, 0f, 4f, 265f);
            handle.type = Image.Type.Sliced;
            handle.pixelsPerUnitMultiplier = 6.5f;
            StretchFull(handle.rectTransform);
            scrollbar.handleRect = handle.rectTransform;
            scrollbar.targetGraphic = handle;

            scrollRect.viewport = viewport;
            scrollRect.content = content;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            return content;
        }

        private static void ColumnHeader(Transform root, string name, string title, TMP_FontAsset font, float x)
        {
            RectTransform header = NewChild(root, name);
            SetTopLeft(header, x, 128f, 512f, 40f);
            AddImage(header, "Shape", LoadSprite(HeaderArt), Color.white, 0f, 0f, 512f, 64f);
            TextMeshProUGUI label = AddText(header, "Title", title, font, 16f, Color.white, TextAlignmentOptions.MidlineLeft, 16f, 0f, 300f, 40f);
            label.characterSpacing = 1.5f / 16f * 100f;
            label.fontStyle = FontStyles.UpperCase;
        }

        private static Button IconButton(Transform parent, string name, Sprite icon, Color color, float x, float y, float width, float height)
        {
            Image hit = AddImage(parent, name, null, new Color(0f, 0f, 0f, 0f), x - 8f, y - 8f, width + 16f, height + 16f);
            hit.raycastTarget = true;
            AddImage(hit.transform, "Icon", icon, color, 8f, 8f, width, height).preserveAspect = true;
            var button = hit.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            return button;
        }

        private static Button OutlineButton(Transform parent, string name, float x, float y, float width, float height)
        {
            Image image = AddImage(parent, name, LoadSprite(Outline8Art), Ink, x, y, width, height);
            image.type = Image.Type.Sliced;
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.35f);
            button.colors = colors;
            return button;
        }

        private static Button ArtButton(Transform parent, string name, Sprite sprite, float x, float y, float width, float height, string label, TMP_FontAsset font, float size, Color ink)
        {
            Image hit = AddImage(parent, name, null, new Color(0f, 0f, 0f, 0f), x, y, width, height);
            hit.raycastTarget = true;
            Image art = AddImage(hit.transform, "Art", sprite, Color.white, 0f, 0f, 256f, 64f);
            AddText(hit.transform, "Label", label, font, size, ink, TextAlignmentOptions.Center, 0f, 0f, width, height);
            var button = hit.gameObject.AddComponent<Button>();
            button.targetGraphic = art;
            ColorBlock colors = button.colors;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.5f);
            button.colors = colors;
            return button;
        }

        private static void Register()
        {
            var registry = AssetDatabase.LoadMainAssetAtPath(RegistryPath);
            if (registry == null) throw new System.InvalidOperationException($"Missing {RegistryPath}.");
            var so = new SerializedObject(registry);
            SerializedProperty entries = so.FindProperty("_entries") ?? so.FindProperty("Entries");
            if (entries == null) throw new System.InvalidOperationException("UIWindowRegistry has no entries list.");
            SerializedProperty entry = null;
            for (int i = 0; i < entries.arraySize; i++)
            {
                SerializedProperty candidate = entries.GetArrayElementAtIndex(i);
                if (candidate.FindPropertyRelative("WindowName").stringValue == WindowName) entry = candidate;
            }
            if (entry == null)
            {
                entries.arraySize++;
                entry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                entry.FindPropertyRelative("Keys").arraySize = 0;
            }
            entry.FindPropertyRelative("WindowName").stringValue = WindowName;
            entry.FindPropertyRelative("Type").enumValueIndex = 0;
            entry.FindPropertyRelative("Prefab").objectReferenceValue = Load<GameObject>(WindowPrefabPath);
            entry.FindPropertyRelative("DestroyOnClose").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(registry);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }
        #endregion
    }
}
