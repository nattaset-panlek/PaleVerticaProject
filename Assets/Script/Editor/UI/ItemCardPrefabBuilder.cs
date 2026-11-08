using System.Collections.Generic;
using System.Text;
using CoreSystem.UI.Inventory.Utility;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static CoreSystem.Editor.UI.EditorUIFactory;

namespace CoreSystem.Editor.UI
{
    public static class ItemCardPrefabBuilder
    {
        #region [Variables/Fields]
        public const string CardPrefabPath = "Assets/Prefab/UI/ItemCard.prefab";
        public const string WidePrefabPath = "Assets/Prefab/UI/ItemSlotWide.prefab";
        private const string ArtFolder = "Assets/Art/UI/New/ItemCard/";
        private const string BodyWhitePath = ArtFolder + "UI_ItemCard_Body_White.png";
        private const string BodyDarkPath = ArtFolder + "UI_ItemCard_Body_Dark.png";
        private const string RingHoverPath = ArtFolder + "UI_ItemCard_Ring_Hover.png";
        private const string RingFocusPath = ArtFolder + "UI_ItemCard_Ring_Focus.png";
        private const string RingPressedPath = ArtFolder + "UI_ItemCard_Ring_Pressed.png";
        private const string IconFramePath = ArtFolder + "UI_ItemCard_IconFrame.png";
        private const string SocketPath = ArtFolder + "UI_ItemCard_Socket.png";
        private const string Pill6Path = ArtFolder + "UI_ItemCard_Pill6.png";
        private const string Pill4Path = ArtFolder + "UI_ItemCard_Pill4.png";
        private const string Pill4OutlinePath = ArtFolder + "UI_ItemCard_Pill4_Outline.png";
        private const string StripePath = ArtFolder + "UI_ItemCard_Stripe.png";
        private const string StarPath = "Assets/Art/UI/New/Icon/UI_StarGylp_Icon.png";
        private static readonly Vector2 WideSize = new Vector2(420f, 61f);
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/UI/Build ItemCard")]
        public static void BuildFromMenu()
        {
            if (!EditorUtility.DisplayDialog("Build ItemCard", $"Rebuilds\n{CardPrefabPath}\nand the children of\n{WidePrefabPath}\n(the root, its GUID and its ItemSlotWide stay).", "Build", "Cancel")) return;
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
            var log = new StringBuilder("[ItemCardPrefabBuilder]");
            PrepareSprites();
            BuildCardPrefab();
            log.Append($"\n- {CardPrefabPath}");
            BuildWidePrefab();
            log.Append($"\n- {WidePrefabPath}");
            AssetDatabase.SaveAssets();
            return log.ToString();
        }

        public static ItemCardView AddCard(Transform parent, ItemCardTheme theme, string name = "ItemCard")
        {
            var prefab = Load<GameObject>(CardPrefabPath);
            if (prefab == null) throw new System.InvalidOperationException($"Missing {CardPrefabPath}. Run Pale Vertica > UI > Build ItemCard first.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            StretchFull((RectTransform)instance.transform);
            var view = instance.GetComponent<ItemCardView>();
            var so = new SerializedObject(view);
            so.FindProperty("_theme").enumValueIndex = (int)theme;
            so.ApplyModifiedPropertiesWithoutUndo();
            view.Fit();
            return view;
        }
        #endregion

        #region [Private Methods]
        private static void PrepareSprites()
        {
            foreach (string path in new[] { BodyWhitePath, BodyDarkPath, RingHoverPath, RingFocusPath, RingPressedPath, IconFramePath, SocketPath, StripePath }) PrepareSprite(path, Vector4.zero);
            PrepareSprite(Pill6Path, new Vector4(8f, 8f, 8f, 8f));
            PrepareSprite(Pill4Path, new Vector4(6f, 6f, 6f, 6f));
            PrepareSprite(Pill4OutlinePath, new Vector4(6f, 6f, 6f, 6f));
        }

        private static void BuildCardPrefab()
        {
            bool exists = Load<GameObject>(CardPrefabPath) != null;
            GameObject root = exists ? PrefabUtility.LoadPrefabContents(CardPrefabPath) : new GameObject("ItemCard", typeof(RectTransform));
            try
            {
                root.layer = 5;
                ((RectTransform)root.transform).sizeDelta = new Vector2(ItemCardView.CardWidth, ItemCardView.CardHeight);
                ClearChildren(root.transform);
                var hit = root.GetComponent<Image>() ?? root.AddComponent<Image>();
                hit.sprite = null;
                hit.color = new Color(0f, 0f, 0f, 0f);
                hit.raycastTarget = true;
                var view = root.GetComponent<ItemCardView>() ?? root.AddComponent<ItemCardView>();
                FillCard(view, root.transform);
                PrefabUtility.SaveAsPrefabAsset(root, CardPrefabPath);
            }
            finally
            {
                if (exists) PrefabUtility.UnloadPrefabContents(root);
                else Object.DestroyImmediate(root);
            }
        }

        private static void FillCard(ItemCardView view, Transform root)
        {
            TMP_FontAsset semiBold = Load<TMP_FontAsset>(KanitSemiBoldPath);
            TMP_FontAsset regular = Load<TMP_FontAsset>(KanitRegularPath);

            RectTransform card = NewChild(root, "Card");
            card.anchorMin = card.anchorMax = card.pivot = new Vector2(0.5f, 0.5f);
            card.anchoredPosition = Vector2.zero;
            card.sizeDelta = new Vector2(ItemCardView.CardWidth, ItemCardView.CardHeight);
            var group = card.gameObject.AddComponent<CanvasGroup>();

            Image body = AddImage(card, "Body", LoadSprite(BodyDarkPath), Color.white, -8f, -28f, 512f, 128f);
            Image bodyFade = AddImage(card, "Body Fade", null, Color.white, -8f, -28f, 512f, 128f);
            bodyFade.enabled = false;
            Image ringFade = AddImage(card, "Ring Fade", null, Color.white, -8f, -28f, 512f, 128f);
            ringFade.enabled = false;
            Image ring = AddImage(card, "Ring", null, Color.white, -8f, -28f, 512f, 128f);
            ring.enabled = false;
            Image stripe = AddImage(card, "Rarity Stripe", LoadSprite(StripePath), Color.white, 3f, 4f, 16f, 64f);
            AddImage(card, "Icon Frame", LoadSprite(IconFramePath), Color.white, 18f, 4f, 64f, 64f);
            Image icon = AddImage(card, "Icon", null, Color.white, 37f, 23f, 26f, 26f);
            icon.preserveAspect = true;

            TextMeshProUGUI name = AddText(card, "Name", "Item Name", semiBold, 17f, Color.white, TextAlignmentOptions.TopLeft, 88f, 10f, 232f, 26f);
            name.characterSpacing = 1f / 17f * 100f;
            name.overflowMode = TextOverflowModes.Ellipsis;
            name.fontStyle = FontStyles.UpperCase;
            TextMeshProUGUI meta = AddText(card, "Meta", "Material", regular, 13f, Color.white, TextAlignmentOptions.TopLeft, 88f, 39f, 200f, 19f);
            meta.characterSpacing = 0.8f / 13f * 100f;
            meta.fontStyle = FontStyles.UpperCase;

            RectTransform starRow = NewChild(card, "Refine");
            SetTopLeft(starRow, 88f, 42f, 82f, 14f);
            Sprite star = LoadSprite(StarPath);
            var stars = new List<Image>();
            for (int i = 0; i < ItemSlotDisplay.MaxStars; i++) stars.Add(AddImage(starRow, $"Star {i + 1}", star, Color.white, i * 17f, 0f, 14f, 14f));

            RectTransform socketRow = NewChild(card, "Sockets");
            SetTopLeft(socketRow, 186f, 41f, 70f, 16f);
            Sprite socket = LoadSprite(SocketPath);
            var sockets = new List<Image>();
            for (int i = 0; i < ItemSlotDisplay.MaxSockets; i++) sockets.Add(AddImage(socketRow, $"Socket {i + 1}", socket, Color.white, i * 18f, 0f, 16f, 16f));

            Image badgeFill = AddImage(card, "Badge", LoadSprite(Pill6Path), Color.white, 449f, 8f, 31f, 22f);
            badgeFill.type = Image.Type.Sliced;
            TextMeshProUGUI badgeText = AddText(badgeFill.transform, "Badge Text", "+7", semiBold, 13f, Color.black, TextAlignmentOptions.Center, 0f, 0f, 31f, 22f);
            StretchFull((RectTransform)badgeText.transform);

            Image labelFill = AddImage(card, "Label", LoadSprite(Pill4Path), Color.white, 384f, 38f, 96f, 26f);
            labelFill.type = Image.Type.Sliced;
            Image labelOutline = AddImage(labelFill.transform, "Outline", LoadSprite(Pill4OutlinePath), Color.black, 0f, 0f, 96f, 26f);
            labelOutline.type = Image.Type.Sliced;
            StretchFull((RectTransform)labelOutline.transform);
            TextMeshProUGUI labelText = AddText(labelFill.transform, "Label Text", "Equipped", semiBold, 15f, Color.black, TextAlignmentOptions.Center, 0f, 0f, 96f, 26f);
            labelText.characterSpacing = 0.8f / 15f * 100f;
            labelText.fontStyle = FontStyles.UpperCase;
            StretchFull((RectTransform)labelText.transform);

            SetObject(view, "_card", card);
            SetObject(view, "_group", group);
            SetObject(view, "_body", body);
            SetObject(view, "_bodyFade", bodyFade);
            SetObject(view, "_ring", ring);
            SetObject(view, "_ringFade", ringFade);
            SetObject(view, "_stripe", stripe);
            SetObject(view, "_icon", icon);
            SetObject(view, "_name", name);
            SetObject(view, "_meta", meta);
            SetObject(view, "_starRow", starRow.gameObject);
            SetObjects(view, "_stars", stars.ToArray());
            SetObject(view, "_socketRow", socketRow.gameObject);
            SetObjects(view, "_sockets", sockets.ToArray());
            SetObject(view, "_badge", badgeFill.rectTransform);
            SetObject(view, "_badgeFill", badgeFill);
            SetObject(view, "_badgeText", badgeText);
            SetObject(view, "_label", labelFill.rectTransform);
            SetObject(view, "_labelFill", labelFill);
            SetObject(view, "_labelOutline", labelOutline);
            SetObject(view, "_labelText", labelText);
            SetObject(view, "_ringHover", LoadSprite(RingHoverPath));
            SetObject(view, "_ringFocus", LoadSprite(RingFocusPath));
            SetObject(view, "_ringPressed", LoadSprite(RingPressedPath));
            SetPalette(view, "_white", BodyWhitePath, "141313", "6b6b6b", "141313", "c6c6c6", "141414", "ffffff", "f2f2f2", "141313", true);
            SetPalette(view, "_dark", BodyDarkPath, "ffffff", "8a8a8a", "ffffff", "3a3a3a", "ffffff", "141313", "ffffff", "141313", false);
        }

        private static void SetPalette(ItemCardView view, string field, string bodyPath, string name, string meta, string on, string off, string badgeFill, string badgeText, string labelFill, string labelText, bool outline)
        {
            var so = new SerializedObject(view);
            SerializedProperty palette = so.FindProperty(field);
            palette.FindPropertyRelative("Body").objectReferenceValue = LoadSprite(bodyPath);
            palette.FindPropertyRelative("Name").colorValue = Hex(name);
            palette.FindPropertyRelative("Meta").colorValue = Hex(meta);
            palette.FindPropertyRelative("On").colorValue = Hex(on);
            palette.FindPropertyRelative("Off").colorValue = Hex(off);
            palette.FindPropertyRelative("BadgeFill").colorValue = Hex(badgeFill);
            palette.FindPropertyRelative("BadgeText").colorValue = Hex(badgeText);
            palette.FindPropertyRelative("LabelFill").colorValue = Hex(labelFill);
            palette.FindPropertyRelative("LabelText").colorValue = Hex(labelText);
            palette.FindPropertyRelative("LabelOutline").boolValue = outline;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildWidePrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(WidePrefabPath);
            try
            {
                ClearChildren(root.transform);
                ((RectTransform)root.transform).sizeDelta = WideSize;
                var image = root.GetComponent<Image>() ?? root.AddComponent<Image>();
                image.sprite = null;
                image.color = new Color(0f, 0f, 0f, 0f);
                image.raycastTarget = true;

                ItemCardView card = AddCard(root.transform, ItemCardTheme.Dark);
                var slot = root.GetComponent<ItemSlotWide>() ?? root.AddComponent<ItemSlotWide>();
                SetObject(slot, "_card", card);
                SetObject(slot, "ItemIcon", (Object)FindImage(card.transform, "Card/Icon"));
                SetObject(slot, "RarityFrame", (Object)FindImage(card.transform, "Card/Rarity Stripe"));
                PrefabUtility.SaveAsPrefabAsset(root, WidePrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Image FindImage(Transform root, string path)
        {
            Transform child = root.Find(path);
            return child != null ? child.GetComponent<Image>() : null;
        }
        #endregion
    }
}
