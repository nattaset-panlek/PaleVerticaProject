using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using CoreSystem.UI;

namespace CoreSystem.Editor.UI
{
    public static class LightUIKitGenerator
    {
        #region [Variables/Fields]
        public const string KitFolder = "Assets/Prefab/Game Page/Global/UIElement/Light";
        private const string RegistryPath = "Assets/Prefab/UIElementRegistry.asset";
        private const string ModalPrefabPath = "Assets/Prefab/Game Page/Global/Global Modal.prefab";

        private const string ArtRoot = "Assets/Art/UI/New/";
        private const string LightWindowArt = ArtRoot + "Modal/UI_Modal_Skill_Window.png";
        private const string DarkWindowArt = ArtRoot + "Modal/UI_Modal_Window.png";
        private const string ItemCardArt = ArtRoot + "Modal/UI_Modal_ItemInfomation.png";
        private const string ButtonArt = ArtRoot + "Button/UI_Primary_Button.png";
        private const string RoundedArt = ArtRoot + "Rect/rounded.png";
        private const string DecoLineArt = ArtRoot + "Page/Skill Manager Page/UI_Deco_Line.png";
        private const string StarIconArt = ArtRoot + "Icon/UI_StarGylp_Icon.png";
        private const string CubeIconArt = ArtRoot + "Icon/item.png";
        private const string NumFrameArt = ArtRoot + "Frame/UI_Num_Frame.png";

        private const string FontRoot = "Assets/Art/Front/";
        private const string KanitRegularFont = FontRoot + "Kanit/Kanit-Regular SDF.asset";
        private const string KanitMediumFont = FontRoot + "Kanit/Kanit-Medium SDF.asset";
        private const string KanitSemiBoldFont = FontRoot + "Kanit/Kanit-SemiBold SDF.asset";
        private const string KanitBoldFont = FontRoot + "Kanit/Kanit-Bold SDF.asset";
        private const string NovecentoFont = FontRoot + "Novecento Sans/Novecentosanswide-DemiBold SDF.asset";

        private const float RoundedBorder = 21f;
        private const float HeaderHeight = 94f;
        private static readonly Vector2 ButtonSize = new Vector2(181f, 55f);
        private static readonly RectOffset ContentInsets = new RectOffset(16, 16, 100, 16);

        private static readonly Color Ink = Hex("141313");
        private static readonly Color Muted = Hex("616161");
        private static readonly Color Dark = Hex("353535");
        private static readonly Color Paper = Hex("F2F2F2");
        private static readonly Color PanelArt = Hex("E6E6E6");
        private static readonly Color Line = Hex("C4C4C4");
        private static readonly Color Soft = Hex("BDBDBD");

        private static Sprite _rounded;
        private static TMP_FontAsset _regular;
        private static TMP_FontAsset _medium;
        private static TMP_FontAsset _semiBold;
        private static TMP_FontAsset _bold;
        private static TMP_FontAsset _novecento;
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/UI/Build Light UI Kit")]
        public static void BuildFromMenu()
        {
            bool exists = Directory.Exists(KitFolder) && Directory.GetFiles(KitFolder, "*.prefab").Length > 0;
            if (exists && !EditorUtility.DisplayDialog("Build Light UI Kit", $"Prefabs in {KitFolder} will be overwritten and the registry entries, theme and Global Modal will be updated.", "Build", "Cancel")) return;
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

        public static void ResizeArtButtonsBatch()
        {
            int exitCode = 0;
            try
            {
                Debug.Log(ResizeArtButtons());
                AssetDatabase.SaveAssets();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                exitCode = 1;
            }
            EditorApplication.Exit(exitCode);
        }

        public static string ResizeArtButtons()
        {
            var log = new StringBuilder("[LightUIKit] Art button size:").AppendLine();
            foreach (string name in new[] { "Art Primary Button", "Art Secondary Button" })
            {
                string path = $"{KitFolder}/{name}.prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                {
                    log.AppendLine($"  {path}: not found");
                    continue;
                }
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var rect = (RectTransform)root.transform;
                    Vector2 before = rect.sizeDelta;
                    rect.sizeDelta = ButtonSize;
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    log.AppendLine($"  {path}: {before} -> {ButtonSize}");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
            return log.ToString();
        }

        public static string Build()
        {
            LoadShared();
            if (!Directory.Exists(KitFolder))
            {
                Directory.CreateDirectory(KitFolder);
                AssetDatabase.Refresh();
            }

            var registry = AssetDatabase.LoadAssetAtPath<UIElementRegistrySO>(RegistryPath);
            if (registry == null) return $"[LightUIKit] Registry not found at {RegistryPath}.";

            var log = new StringBuilder("[LightUIKit] Built:\n");
            var primary = Save(BuildArtButton("Art Primary Button", Color.white, Hex("676363")), log);
            var secondary = Save(BuildArtButton("Art Secondary Button", Hex("1E1E1E"), Hex("C9C9C9")), log);
            var chip = Save(BuildChip(), log);
            var section = Save(BuildSectionHeader(), log);
            var keyValue = Save(BuildKeyValue(), log);
            var itemRow = Save(BuildItemRow(), log);
            var attrCell = Save(BuildAttrCell(), log);
            var levelFrame = Save(BuildLevelFrame(), log);
            var hpBar = Save(BuildStatusBar("HP Bar", Hex("00FF99")), log);
            var mpBar = Save(BuildStatusBar("MP Bar", Hex("7B00FF")), log);
            var expBar = Save(BuildStatusBar("EXP Bar", Ink), log);
            var itemCard = Save(BuildItemCard(), log);
            var searchField = Save(BuildSearchField(), log);

            Register(registry, UIElementType.Button, "Primary Button", primary, ButtonSize, true);
            Register(registry, UIElementType.Button, "Button", secondary, ButtonSize, true);
            Register(registry, UIElementType.Button, "Nav Button", chip, new Vector2(0f, 34f), true);
            Register(registry, UIElementType.Button, "Chip", chip, new Vector2(0f, 34f), true);
            Register(registry, UIElementType.Button, UIBuilderContext.FoldoutHeaderStyle, section, Vector2.zero, true);
            Register(registry, UIElementType.Custom, UIBuilderContext.SectionHeaderStyle, section, Vector2.zero, false);
            Register(registry, UIElementType.Custom, UIBuilderContext.KeyValueStyle, keyValue, Vector2.zero, false);
            Register(registry, UIElementType.Custom, "Item Row", itemRow, Vector2.zero, false);
            Register(registry, UIElementType.Custom, "Attr Cell", attrCell, Vector2.zero, false);
            Register(registry, UIElementType.Custom, "Level Frame", levelFrame, Vector2.zero, false);
            Register(registry, UIElementType.ProgressBar, "HP Bar", hpBar, Vector2.zero, false);
            Register(registry, UIElementType.ProgressBar, "MP Bar", mpBar, Vector2.zero, false);
            Register(registry, UIElementType.ProgressBar, "EXP Bar", expBar, Vector2.zero, false);
            Register(registry, UIElementType.Container, "Item Card", itemCard, Vector2.zero, false);
            Register(registry, UIElementType.InputField, UIBuilderContext.DefaultInputStyle, searchField, Vector2.zero, false);
            ApplyLightTheme(registry);
            EditorUtility.SetDirty(registry);
            log.AppendLine("Registry entries and light theme updated.");

            log.AppendLine(SkinGlobalModal());
            AssetDatabase.SaveAssets();
            return log.ToString();
        }
        #endregion

        #region [Private Methods]
        private static void LoadShared()
        {
            _rounded = LoadSprite(RoundedArt);
            _regular = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KanitRegularFont);
            _medium = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KanitMediumFont);
            _semiBold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KanitSemiBoldFont);
            _bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KanitBoldFont);
            _novecento = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NovecentoFont);
        }

        private static GameObject BuildArtButton(string name, Color tint, Color ink)
        {
            var go = NewRoot(name, ButtonSize);
            var image = AddImage(go, LoadSprite(ButtonArt), tint, Image.Type.Sliced, 1f, true);
            AddButton(go, image, Selectable.Transition.ColorTint);
            AddRow(go, new RectOffset(14, 14, 6, 6), 0f, TextAnchor.MiddleCenter, false);
            go.AddComponent<LayoutElement>();
            AddText(go.transform, "Text", "Button", _medium, 18f, ink, TextAlignmentOptions.Center);
            return go;
        }

        private static GameObject BuildChip()
        {
            var go = NewRoot("Nav Chip", new Vector2(150f, 34f));
            var image = AddImage(go, _rounded, Hex("D9D9D9"), Image.Type.Sliced, RoundedBorder / 6f, true);
            AddButton(go, image, Selectable.Transition.ColorTint);
            AddRow(go, new RectOffset(14, 14, 6, 6), 0f, TextAnchor.MiddleCenter, false);
            go.AddComponent<LayoutElement>();
            AddText(go.transform, "Text", "Chip", _semiBold, 12f, Dark, TextAlignmentOptions.Center, true, 6f);
            return go;
        }

        private static GameObject BuildSectionHeader()
        {
            var go = NewRoot("Section Header", new Vector2(400f, 34f));
            var hit = AddImage(go, null, Color.clear, Image.Type.Simple, 1f, true);
            AddButton(go, hit, Selectable.Transition.None);
            AddRow(go, new RectOffset(0, 0, 0, 0), 0f, TextAnchor.MiddleCenter, false);
            SetLayout(go, -1f, 34f, 1f);

            var line = NewChild(go.transform, "Line");
            AddImage(line, LoadSprite(DecoLineArt), Color.white, Image.Type.Sliced);
            line.AddComponent<LayoutElement>().ignoreLayout = true;
            var lineRect = (RectTransform)line.transform;
            lineRect.anchorMin = new Vector2(0f, 0.5f);
            lineRect.anchorMax = new Vector2(1f, 0.5f);
            lineRect.sizeDelta = new Vector2(0f, 18f);

            var pill = NewChild(go.transform, "Pill");
            AddImage(pill, _rounded, Ink, Image.Type.Sliced, RoundedBorder / 14f);
            AddRow(pill, new RectOffset(2, 2, 2, 2), 0f, TextAnchor.MiddleCenter, false);
            var pillLayout = SetLayout(pill, -1f, 28f, 0f);
            pillLayout.minHeight = 28f;

            var fill = NewChild(pill.transform, "Fill");
            AddImage(fill, _rounded, PanelArt, Image.Type.Sliced, RoundedBorder / 12f);
            AddRow(fill, new RectOffset(16, 16, 2, 2), 0f, TextAnchor.MiddleCenter, false);
            SetLayout(fill, -1f, 24f, 0f).minHeight = 24f;
            var title = AddText(fill.transform, "Title", "Section", _semiBold, 13f, Ink, TextAlignmentOptions.Center, true, 8f);

            Map(go, ("Title", title.gameObject));
            return go;
        }

        private static GameObject BuildKeyValue()
        {
            var go = NewRoot("Key Value", new Vector2(260f, 36f));
            AddImage(go, _rounded, Line, Image.Type.Sliced, RoundedBorder / 7f);
            AddRow(go, new RectOffset(1, 1, 1, 1), 0f, TextAnchor.MiddleCenter, true);
            var le = SetLayout(go, 0f, 36f, 1f);
            le.minHeight = 36f;

            var body = NewChild(go.transform, "Body");
            AddImage(body, _rounded, Paper, Image.Type.Sliced, RoundedBorder / 6f);
            AddRow(body, new RectOffset(12, 12, 0, 0), 8f, TextAnchor.MiddleLeft, false);

            var key = AddText(body.transform, "Key", "Key", _regular, 12f, Muted, TextAlignmentOptions.MidlineLeft);
            SetLayout(key.gameObject, -1f, -1f, 1f);
            var value = AddText(body.transform, "Value", "Value", _semiBold, 12f, Ink, TextAlignmentOptions.MidlineRight);

            Map(go, ("Key", key.gameObject), ("Value", value.gameObject));
            return go;
        }

        private static GameObject BuildItemRow()
        {
            var go = NewRoot("Item Row", new Vector2(440f, 76f));
            AddImage(go, _rounded, Dark, Image.Type.Sliced, RoundedBorder / 10f, true);
            AddRow(go, new RectOffset(2, 2, 2, 2), 0f, TextAnchor.MiddleCenter, true);
            var le = SetLayout(go, -1f, 76f, 1f);
            le.minHeight = 76f;

            var body = NewChild(go.transform, "Body");
            AddImage(body, _rounded, Paper, Image.Type.Sliced, RoundedBorder / 8f);
            AddRow(body, new RectOffset(8, 10, 8, 8), 14f, TextAnchor.MiddleLeft, false);

            var iconBox = NewChild(body.transform, "Icon Box");
            AddImage(iconBox, _rounded, Dark, Image.Type.Sliced, RoundedBorder / 8f);
            FixedSize(iconBox, 56f, 56f);
            var icon = NewChild(iconBox.transform, "Icon");
            AddImage(icon, LoadSprite(CubeIconArt), Color.white, Image.Type.Simple).preserveAspect = true;
            ((RectTransform)icon.transform).sizeDelta = new Vector2(30f, 30f);

            var texts = NewChild(body.transform, "Text");
            AddColumn(texts, new RectOffset(0, 0, 0, 0), 2f, TextAnchor.MiddleLeft);
            SetLayout(texts, -1f, -1f, 1f).minWidth = 0f;
            var name = AddText(texts.transform, "Name", "Item Name", _semiBold, 15f, Ink, TextAlignmentOptions.MidlineLeft, true, 2f);
            var sub = AddText(texts.transform, "Sub", "Type  ·  Rarity", _regular, 12f, Muted, TextAlignmentOptions.MidlineLeft);

            var valueBox = NewChild(body.transform, "Value Box");
            AddImage(valueBox, _rounded, Dark, Image.Type.Sliced, RoundedBorder / 8f);
            AddColumn(valueBox, new RectOffset(2, 2, 4, 4), 0f, TextAnchor.MiddleCenter);
            FixedSize(valueBox, 56f, 56f);
            var valueLabel = AddText(valueBox.transform, "Value Label", "ID", _medium, 9f, Soft, TextAlignmentOptions.Center, true, 6f);
            var value = AddText(valueBox.transform, "Value", "0000", _bold, 17f, Color.white, TextAlignmentOptions.Center);
            value.enableAutoSizing = true;
            value.fontSizeMin = 10f;
            value.fontSizeMax = 17f;

            Map(go, ("Outline", go), ("Body", body), ("Icon", icon), ("Name", name.gameObject), ("Sub", sub.gameObject), ("ValueLabel", valueLabel.gameObject), ("Value", value.gameObject));
            return go;
        }

        private static GameObject BuildAttrCell()
        {
            var go = NewRoot("Attr Cell", new Vector2(72f, 72f));
            AddImage(go, _rounded, Dark, Image.Type.Sliced, RoundedBorder / 8f);
            AddColumn(go, new RectOffset(4, 4, 6, 6), 0f, TextAnchor.MiddleCenter);
            FixedSize(go, 72f, 72f);
            var label = AddText(go.transform, "Label", "STR", _semiBold, 10f, Soft, TextAlignmentOptions.Center, true, 6f);
            var value = AddText(go.transform, "Value", "0", _bold, 24f, Color.white, TextAlignmentOptions.Center);
            Map(go, ("Label", label.gameObject), ("Value", value.gameObject));
            return go;
        }

        private static GameObject BuildLevelFrame()
        {
            var go = NewRoot("Level Frame", new Vector2(168f, 168f));
            AddImage(go, LoadSprite(NumFrameArt), Color.white, Image.Type.Sliced);
            AddColumn(go, new RectOffset(8, 8, 6, 6), 0f, TextAnchor.MiddleCenter);
            FixedSize(go, 168f, 168f);
            AddText(go.transform, "Base Label", "Base", _semiBold, 11f, Hex("8C8C8C"), TextAlignmentOptions.Center, true, 10f);
            var baseLevel = AddText(go.transform, "Base", "1", _novecento != null ? _novecento : _bold, 40f, Ink, TextAlignmentOptions.Center);
            AddText(go.transform, "Job Label", "Job", _semiBold, 11f, Hex("8C8C8C"), TextAlignmentOptions.Center, true, 10f);
            var jobLevel = AddText(go.transform, "Job", "01", _novecento != null ? _novecento : _bold, 40f, Muted, TextAlignmentOptions.Center);
            Map(go, ("Base", baseLevel.gameObject), ("Job", jobLevel.gameObject));
            return go;
        }

        private static GameObject BuildStatusBar(string name, Color fillColor)
        {
            var go = NewRoot(name, new Vector2(280f, 14f));
            AddImage(go, _rounded, Ink, Image.Type.Sliced, RoundedBorder / 7f);
            var le = SetLayout(go, -1f, 14f, 1f);
            le.minHeight = 14f;

            var background = NewChild(go.transform, "Background");
            AddImage(background, _rounded, Paper, Image.Type.Sliced, RoundedBorder / 5f);
            Stretch(background, 2f);

            var area = NewChild(go.transform, "Fill Area");
            Stretch(area, 3f);
            var fill = NewChild(area.transform, "Fill");
            AddImage(fill, _rounded, fillColor, Image.Type.Sliced, RoundedBorder / 4f);
            ((RectTransform)fill.transform).sizeDelta = Vector2.zero;

            var slider = go.AddComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            slider.fillRect = (RectTransform)fill.transform;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.6f;
            return go;
        }

        private static GameObject BuildItemCard()
        {
            var go = NewRoot("Item Card", new Vector2(300f, 560f));
            AddImage(go, LoadSprite(ItemCardArt), Color.white, Image.Type.Tiled);
            AddColumn(go, new RectOffset(18, 18, 70, 18), 10f, TextAnchor.UpperLeft);
            return go;
        }

        private static GameObject BuildSearchField()
        {
            var go = NewRoot("Search Field", new Vector2(240f, 36f));
            var frame = AddImage(go, _rounded, Dark, Image.Type.Sliced, RoundedBorder / 8f, true);

            var fill = NewChild(go.transform, "Fill");
            AddImage(fill, _rounded, Hex("F7F7F7"), Image.Type.Sliced, RoundedBorder / 6f);
            Stretch(fill, 2f);

            var area = NewChild(go.transform, "Text Area");
            area.AddComponent<RectMask2D>();
            var areaRect = (RectTransform)area.transform;
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(12f, 2f);
            areaRect.offsetMax = new Vector2(-12f, -2f);

            var placeholder = AddText(area.transform, "Placeholder", "search...", _regular, 13f, Hex("9E9E9E"), TextAlignmentOptions.MidlineLeft);
            Stretch(placeholder.gameObject, 0f);
            var text = AddText(area.transform, "Text", "", _regular, 14f, Ink, TextAlignmentOptions.MidlineLeft);
            Stretch(text.gameObject, 0f);

            var input = go.AddComponent<TMP_InputField>();
            input.targetGraphic = frame;
            input.textViewport = areaRect;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.fontAsset = _regular;
            input.pointSize = 14f;
            input.customCaretColor = true;
            input.caretColor = Ink;
            input.selectionColor = new Color(0.21f, 0.21f, 0.21f, 0.25f);
            return go;
        }

        private static string SkinGlobalModal()
        {
            var root = PrefabUtility.LoadPrefabContents(ModalPrefabPath);
            try
            {
                var modal = root.GetComponent<UIGlobalModal>();
                if (modal == null) return "Global Modal: UIGlobalModal component not found.";

                var so = new SerializedObject(modal);
                var title = so.FindProperty("_titleText").objectReferenceValue as TMP_Text;
                var close = so.FindProperty("_closeButton").objectReferenceValue as Button;
                var window = so.FindProperty("_windowImage").objectReferenceValue as Image;
                var closeIcon = so.FindProperty("_closeIcon").objectReferenceValue as Image;
                var content = so.FindProperty("_contentContainer").objectReferenceValue as RectTransform;
                if (title == null || close == null || content == null) return "Global Modal: title, close or content reference missing.";

                var lightWindow = LoadSprite(LightWindowArt);
                var darkWindow = LoadSprite(DarkWindowArt);
                SetSkin(so.FindProperty("_lightSkin"), lightWindow, Color.white, Ink);
                SetSkin(so.FindProperty("_darkSkin"), darkWindow, Color.white, PanelArt);
                so.ApplyModifiedPropertiesWithoutUndo();

                if (window != null)
                {
                    window.sprite = lightWindow;
                    window.type = Image.Type.Tiled;
                    window.pixelsPerUnitMultiplier = 1f;
                    window.color = Color.white;
                }

                var tag = title.transform.parent != null && title.transform.parent.name == "Title Tag"
                    ? (RectTransform)title.transform.parent
                    : null;
                var nav = (RectTransform)(tag != null ? tag.parent : title.transform.parent);
                nav.anchorMin = new Vector2(0f, 1f);
                nav.anchorMax = new Vector2(1f, 1f);
                nav.pivot = new Vector2(0.5f, 1f);
                nav.anchoredPosition = Vector2.zero;
                nav.sizeDelta = new Vector2(0f, HeaderHeight);

                if (tag == null)
                {
                    var tagGo = NewChild(nav, "Title Tag");
                    tag = (RectTransform)tagGo.transform;
                    AddImage(tagGo, _rounded, Ink, Image.Type.Sliced, RoundedBorder / 4f);
                    AddRow(tagGo, new RectOffset(12, 16, 6, 6), 10f, TextAnchor.MiddleLeft, false);
                    var fitter = tagGo.AddComponent<ContentSizeFitter>();
                    fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    var star = NewChild(tag, "Icon");
                    AddImage(star, LoadSprite(StarIconArt), Color.white, Image.Type.Simple).preserveAspect = true;
                    FixedSize(star, 26f, 26f);
                    title.transform.SetParent(tag, false);
                }
                tag.anchorMin = new Vector2(0f, 0.5f);
                tag.anchorMax = new Vector2(0f, 0.5f);
                tag.pivot = new Vector2(0f, 0.5f);
                tag.anchoredPosition = new Vector2(30f, 2f);

                if (_novecento != null) title.font = _novecento;
                title.fontSize = 24f;
                title.fontStyle = FontStyles.UpperCase;
                title.characterSpacing = 2f;
                title.alignment = TextAlignmentOptions.MidlineLeft;
                title.textWrappingMode = TextWrappingModes.NoWrap;
                title.overflowMode = TextOverflowModes.Overflow;
                title.color = Color.white;
                title.raycastTarget = false;

                var closeRect = (RectTransform)close.transform;
                closeRect.anchorMin = new Vector2(1f, 0.5f);
                closeRect.anchorMax = new Vector2(1f, 0.5f);
                closeRect.pivot = new Vector2(1f, 0.5f);
                closeRect.sizeDelta = new Vector2(56f, 56f);
                closeRect.anchoredPosition = new Vector2(-20f, 2f);
                if (closeIcon != null)
                {
                    ((RectTransform)closeIcon.transform).sizeDelta = new Vector2(26f, 26f);
                    closeIcon.color = Ink;
                }

                content.anchorMin = Vector2.zero;
                content.anchorMax = Vector2.one;
                content.offsetMin = new Vector2(ContentInsets.left, ContentInsets.bottom);
                content.offsetMax = new Vector2(-ContentInsets.right, -ContentInsets.top);

                PrefabUtility.SaveAsPrefabAsset(root, ModalPrefabPath);
                return "Global Modal: light skin, header band, title tag and content insets applied.";
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void SetSkin(SerializedProperty skin, Sprite sprite, Color title, Color icon)
        {
            if (skin == null) return;
            skin.FindPropertyRelative("WindowSprite").objectReferenceValue = sprite;
            skin.FindPropertyRelative("WindowColor").colorValue = Color.white;
            skin.FindPropertyRelative("TitleColor").colorValue = title;
            skin.FindPropertyRelative("IconColor").colorValue = icon;
        }

        private static void ApplyLightTheme(UIElementRegistrySO registry)
        {
            var so = new SerializedObject(registry);
            var theme = so.FindProperty("_theme");
            SetColor(theme, "Panel", Paper);
            SetColor(theme, "PanelAlt", Hex("D9D9D9"));
            SetColor(theme, "Divider", new Color(Ink.r, Ink.g, Ink.b, 0.14f));
            SetColor(theme, "Text", Ink);
            SetColor(theme, "TextMuted", Muted);
            SetColor(theme, "TextOnAccent", Color.white);
            SetColor(theme, "Primary", Ink);
            SetColor(theme, "Secondary", Soft);
            SetColor(theme, "Danger", Hex("D93636"));
            SetColor(theme, "Success", Hex("12A06A"));
            SetColor(theme, "Warning", Hex("FFBB00"));
            SetColor(theme, "Selected", Dark);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetColor(SerializedProperty theme, string field, Color color)
        {
            var property = theme.FindPropertyRelative(field);
            if (property != null) property.colorValue = color;
        }

        private static void Register(UIElementRegistrySO registry, UIElementType type, string styleName, GameObject prefab, Vector2 minSize, bool keepLook)
        {
            var entry = registry.GetEntry(type, styleName);
            if (entry == null)
            {
                entry = new UIElementEntry { StyleName = styleName, Type = type };
                registry.Entries.Add(entry);
            }
            entry.Prefab = prefab;
            entry.TargetComponentPath = string.Empty;
            entry.MinSize = minSize;
            entry.FixedSize = Vector2.zero;
            entry.KeepPrefabLook = keepLook;
        }

        private static GameObject Save(GameObject go, StringBuilder log)
        {
            string path = $"{KitFolder}/{go.name}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            log.AppendLine($"  {path}");
            return prefab;
        }

        private static void Map(GameObject go, params (string Key, GameObject Target)[] pairs)
        {
            var custom = go.AddComponent<UICustomElement>();
            var so = new SerializedObject(custom);
            var list = so.FindProperty("_elements");
            list.arraySize = pairs.Length;
            for (int i = 0; i < pairs.Length; i++)
            {
                var element = list.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("Key").stringValue = pairs[i].Key;
                element.FindPropertyRelative("Target").objectReferenceValue = pairs[i].Target;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject NewRoot(string name, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            ((RectTransform)go.transform).sizeDelta = size;
            return go;
        }

        private static GameObject NewChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Image AddImage(GameObject go, Sprite sprite, Color color, Image.Type type, float ppuMultiplier = 1f, bool raycast = false)
        {
            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = sprite != null ? type : Image.Type.Simple;
            image.pixelsPerUnitMultiplier = ppuMultiplier;
            image.raycastTarget = raycast;
            return image;
        }

        private static void AddButton(GameObject go, Graphic target, Selectable.Transition transition)
        {
            var button = go.AddComponent<Button>();
            button.targetGraphic = target;
            button.transition = transition;
        }

        private static void AddRow(GameObject go, RectOffset padding, float spacing, TextAnchor alignment, bool expand)
        {
            var group = go.AddComponent<HorizontalLayoutGroup>();
            ConfigureGroup(group, padding, spacing, alignment, expand);
        }

        private static void AddColumn(GameObject go, RectOffset padding, float spacing, TextAnchor alignment)
        {
            var group = go.AddComponent<VerticalLayoutGroup>();
            ConfigureGroup(group, padding, spacing, alignment, false);
        }

        private static void ConfigureGroup(HorizontalOrVerticalLayoutGroup group, RectOffset padding, float spacing, TextAnchor alignment, bool expand)
        {
            group.padding = padding;
            group.spacing = spacing;
            group.childAlignment = alignment;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = expand;
            group.childForceExpandHeight = expand;
        }

        private static LayoutElement SetLayout(GameObject go, float preferredWidth, float preferredHeight, float flexibleWidth)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            le.preferredWidth = preferredWidth;
            le.preferredHeight = preferredHeight;
            le.flexibleWidth = flexibleWidth;
            return le;
        }

        private static void FixedSize(GameObject go, float width, float height)
        {
            var le = SetLayout(go, width, height, 0f);
            le.minWidth = width;
            le.minHeight = height;
            ((RectTransform)go.transform).sizeDelta = new Vector2(width, height);
        }

        private static void Stretch(GameObject go, float inset)
        {
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        private static TextMeshProUGUI AddText(Transform parent, string name, string content, TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment, bool upper = false, float spacing = 0f)
        {
            var go = NewChild(parent, name);
            var text = go.AddComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = upper ? FontStyles.UpperCase : FontStyles.Normal;
            text.characterSpacing = spacing;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            return text;
        }

        private static Sprite LoadSprite(string path)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite) return sprite;
            }
            Debug.LogWarning($"[LightUIKit] Sprite not found: {path}");
            return null;
        }

        private static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString("#" + hex, out Color color) ? color : Color.magenta;
        }
        #endregion
    }
}
