using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using CoreSystem.UI;
using CoreSystem.UI.Components;
using S = CoreSystem.UI.UIMiniHeaderStyleRegistry;

namespace CoreSystem.Editor.UI
{
    public static class MiniHeaderUIKitGenerator
    {
        #region [Variables/Fields]
        public const string KitFolder = "Assets/Prefab/Game Page/Global/UIElement/MiniHeader";
        public const string ModalPrefabPath = "Assets/Prefab/Game Page/Global/Global Modal.prefab";
        public const string MiniHeaderName = "Mini Header";
        public static float HeaderHeight => S.HeaderHeight;
        public const string RegistryPath = "Assets/Prefab/UIElementRegistry.asset";
        public const string ButtonPrimaryStyle = S.ButtonPrimary;
        public const string ButtonSecondaryStyle = S.ButtonSecondary;
        public const string ButtonDangerStyle = S.ButtonDanger;
        public const string MenuRowStyle = S.MenuRow;
        public const string TabButtonStyle = S.TabButton;
        public const string SwitchStyle = S.Switch;
        public const string CheckboxStyle = S.Checkbox;
        public const string SliderStyle = S.Slider;
        public const string DropdownStyle = S.Dropdown;
        public const string InputStyle = S.Input;
        public const string SeparatorStyle = S.Separator;

        private const string ArtRoot = "Assets/Art/UI/New/";
        private const string WindowArt = ArtRoot + "Modal/Global/UI_Global_Window_Dark.png";
        private const string ButtonArt = ArtRoot + "Button/UI_Primary_Button.png";
        private const string CloseArt = ArtRoot + "Icon/UI_Close_Secon_Icon.png";
        private const string RoundedArt = ArtRoot + "Rect/rounded.png";
        private const string RoundedLineArt = ArtRoot + "Rect/rounded_line.png";

        private const string FontRoot = "Assets/Art/Front/";
        private const string KanitBoldFont = FontRoot + "Kanit/Kanit-Bold SDF.asset";
        private const string NovecentoNormalFont = FontRoot + "Novecento Sans/Novecentosanswide-Normal SDF.asset";
        private const string NovecentoDemiFont = FontRoot + "Novecento Sans/Novecentosanswide-DemiBold SDF.asset";
        private const string NotoSemiBoldFont = FontRoot + "Noto Sans Thai/NotoSansThai-SemiBold SDF.asset";

        private const float RoundedBorder = 21f;
        private static Vector2 ButtonSize => S.ButtonSize;
        private static Color Body => S.Body;
        private static Color Text => S.Text;
        private static Color Sub => S.Sub;
        private static Color Accent => S.Accent;
        private static Color Field => S.Field;
        private static Color FieldLine => S.FieldLine;
        private static Color Raised => S.Raised;
        private static Color Inner => S.Inner;
        private static Color InnerLine => S.InnerLine;
        private static Color Track => S.Track;

        private static Sprite _rounded;
        private static Sprite _roundedLine;
        private static Sprite _circle;
        private static Sprite _checkmark;
        private static Sprite _arrow;
        private static Sprite _mask;
        private static TMP_FontAsset _title;
        private static TMP_FontAsset _label;
        private static TMP_FontAsset _demi;
        private static TMP_FontAsset _button;
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/UI/Build MiniHeader Window Style")]
        public static void BuildFromMenu()
        {
            if (!EditorUtility.DisplayDialog("Build MiniHeader Window Style", $"Adds or replaces the '{MiniHeaderName}' parts in Global Modal (the default style is not changed) and rebuilds the prefabs in {KitFolder}.", "Build", "Cancel")) return;
            Debug.Log(Build());
        }

        [MenuItem("Pale Vertica/UI/Build MiniHeader Kit Only")]
        public static void BuildKitFromMenu()
        {
            Debug.Log(BuildKit());
            Debug.Log(RegisterKit());
            AssetDatabase.SaveAssets();
        }

        public static string Build()
        {
            var log = new StringBuilder("[MiniHeaderUIKit] ");
            log.AppendLine(AddMiniHeaderToGlobalModal());
            log.Append(BuildKit());
            AssetDatabase.SaveAssets();
            return log.ToString();
        }

        public static string AddMiniHeaderToGlobalModal()
        {
            LoadShared();
            var root = PrefabUtility.LoadPrefabContents(ModalPrefabPath);
            try
            {
                var modal = root.GetComponent<UIGlobalModal>();
                if (modal == null) return "Global Modal: UIGlobalModal component not found.";

                var so = new SerializedObject(modal);
                var window = so.FindProperty("_windowImage").objectReferenceValue as Image;
                var content = so.FindProperty("_contentContainer").objectReferenceValue as RectTransform;
                if (window == null)
                {
                    var background = root.transform.Find("Background");
                    if (background != null) window = background.GetComponent<Image>();
                }
                var topNavigator = root.transform.Find("Top Navigator");
                if (window == null || content == null || topNavigator == null) return "Global Modal: Background, Content Container or Top Navigator not found.";

                var old = root.transform.Find(MiniHeaderName);
                if (old != null) Object.DestroyImmediate(old.gameObject);

                var header = NewChild(root.transform, MiniHeaderName);
                var headerRect = (RectTransform)header.transform;
                headerRect.anchorMin = new Vector2(0f, 1f);
                headerRect.anchorMax = new Vector2(1f, 1f);
                headerRect.pivot = new Vector2(0.5f, 1f);
                headerRect.sizeDelta = new Vector2(0f, HeaderHeight);
                headerRect.anchoredPosition = Vector2.zero;
                header.transform.SetSiblingIndex(topNavigator.GetSiblingIndex() + 1);

                var title = AddText(header.transform, "Title", "Window Title", _title, 16f, Color.white, TextAlignmentOptions.MidlineLeft, true);
                var titleRect = title.rectTransform;
                titleRect.anchorMin = Vector2.zero;
                titleRect.anchorMax = Vector2.one;
                titleRect.offsetMin = new Vector2(14f, 0f);
                titleRect.offsetMax = new Vector2(-44f, 0f);

                var close = NewChild(header.transform, "Close");
                var closeRect = (RectTransform)close.transform;
                closeRect.anchorMin = new Vector2(1f, 0.5f);
                closeRect.anchorMax = new Vector2(1f, 0.5f);
                closeRect.pivot = new Vector2(1f, 0.5f);
                closeRect.sizeDelta = new Vector2(HeaderHeight, HeaderHeight);
                closeRect.anchoredPosition = new Vector2(-2f, 0f);
                var hit = AddImage(close, null, new Color(1f, 1f, 1f, 0f), Image.Type.Simple, 1f, true);
                var closeIcon = NewChild(close.transform, "Icon");
                var iconImage = AddImage(closeIcon, LoadSprite(CloseArt), Color.white, Image.Type.Simple);
                iconImage.preserveAspect = true;
                ((RectTransform)closeIcon.transform).sizeDelta = new Vector2(18f, 18f);
                var closeButton = close.AddComponent<Button>();
                closeButton.targetGraphic = iconImage;
                closeButton.transition = Selectable.Transition.ColorTint;
                var colors = closeButton.colors;
                colors.highlightedColor = Accent;
                colors.pressedColor = new Color(Accent.r, Accent.g, Accent.b, 0.6f);
                closeButton.colors = colors;
                hit.raycastTarget = true;

                header.SetActive(false);

                var view = root.GetComponent<UIModalStyleView>();
                if (view == null) view = root.AddComponent<UIModalStyleView>();
                var vso = new SerializedObject(view);
                var parts = vso.FindProperty("_defaultParts");
                parts.arraySize = 1;
                parts.GetArrayElementAtIndex(0).objectReferenceValue = topNavigator.gameObject;
                vso.FindProperty("_miniHeaderRoot").objectReferenceValue = header;
                vso.FindProperty("_miniTitle").objectReferenceValue = title;
                vso.FindProperty("_miniCloseButton").objectReferenceValue = closeButton;
                vso.FindProperty("_miniWindowSprite").objectReferenceValue = LoadSprite(WindowArt);
                vso.FindProperty("_miniWindowColor").colorValue = Color.white;
                vso.FindProperty("_windowImage").objectReferenceValue = window;
                vso.FindProperty("_contentContainer").objectReferenceValue = content;
                vso.FindProperty("_miniContentInsets").vector4Value = S.ContentInsets;
                vso.ApplyModifiedPropertiesWithoutUndo();

                so.FindProperty("_styleView").objectReferenceValue = view;
                so.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, ModalPrefabPath);
                return "Global Modal: Mini Header parts and UIModalStyleView added (style stays Default).";
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        public static string BuildKit()
        {
            LoadShared();
            if (!Directory.Exists(KitFolder))
            {
                Directory.CreateDirectory(KitFolder);
                AssetDatabase.Refresh();
            }

            var log = new StringBuilder("Kit prefabs:\n");
            Save(BuildButton(ButtonPrimaryStyle, S.ButtonPrimaryTint, S.ButtonPrimaryInk), log);
            Save(BuildButton(ButtonSecondaryStyle, S.ButtonSecondaryTint, S.ButtonSecondaryInk), log);
            Save(BuildButton(ButtonDangerStyle, S.ButtonDangerTint, S.ButtonDangerInk), log);
            Save(BuildMenuRow(), log);
            Save(BuildSwitch(), log);
            Save(BuildCheckbox(), log);
            Save(BuildSlider(), log);
            Save(BuildDropdown(), log);
            Save(BuildInput(), log);
            Save(BuildScrollbar(), log);
            Save(BuildKeyCap(), log);
            Save(BuildKeyButton(), log);
            Save(BuildTab(), log);
            Save(BuildTabButton(), log);
            Save(BuildFoldoutHeader(), log);
            Save(BuildContainer(), log);
            Save(BuildTooltip(), log);
            Save(BuildSeparator(), log);
            Save(BuildSettingRow(), log);
            return log.ToString();
        }

        public static void BuildKitBatch()
        {
            int exitCode = 0;
            try
            {
                Debug.Log(BuildKit());
                Debug.Log(RegisterKit());
                AssetDatabase.SaveAssets();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                exitCode = 1;
            }
            EditorApplication.Exit(exitCode);
        }

        public static string RegisterKit()
        {
            var registry = AssetDatabase.LoadAssetAtPath<UIElementRegistrySO>(RegistryPath);
            if (registry == null) return $"[MiniHeaderUIKit] Registry not found at {RegistryPath}.";
            var log = new StringBuilder("[MiniHeaderUIKit] Registry styles:").AppendLine();
            Register(registry, UIElementType.Button, ButtonPrimaryStyle, ButtonSize, Vector2.zero, log);
            Register(registry, UIElementType.Button, ButtonSecondaryStyle, ButtonSize, Vector2.zero, log);
            Register(registry, UIElementType.Button, ButtonDangerStyle, ButtonSize, Vector2.zero, log);
            Register(registry, UIElementType.Button, MenuRowStyle, Vector2.zero, new Vector2(0f, S.MenuRowHeight), log);
            Register(registry, UIElementType.Button, TabButtonStyle, Vector2.zero, new Vector2(0f, S.TabHeight), log);
            Register(registry, UIElementType.Toggle, SwitchStyle, new Vector2(36f, 18f), Vector2.zero, log);
            Register(registry, UIElementType.Toggle, CheckboxStyle, new Vector2(18f, 18f), Vector2.zero, log);
            Register(registry, UIElementType.Slider, SliderStyle, new Vector2(0f, 18f), Vector2.zero, log);
            Register(registry, UIElementType.Dropdown, DropdownStyle, new Vector2(0f, 30f), Vector2.zero, log);
            Register(registry, UIElementType.InputField, InputStyle, new Vector2(0f, 30f), Vector2.zero, log);
            Register(registry, UIElementType.Divider, SeparatorStyle, Vector2.zero, Vector2.zero, log);
            Register(registry, UIElementType.Button, S.FoldoutHeader, Vector2.zero, new Vector2(0f, 32f), log);
            Register(registry, UIElementType.Button, S.KeyButton, S.KeyButtonSize, new Vector2(0f, S.KeyButtonSize.y), log);
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            return log.ToString();
        }

        public static GameObject LoadKitPrefab(string name)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>($"{KitFolder}/{name}.prefab");
        }

        public static void BuildSampleSettings(UIGlobalModal modal)
        {
            LoadShared();
            modal.SetStyle(UIModalStyle.MiniHeader).SetThemeMode(UIThemeMode.Dark).SetTitle("Settings");
            var content = modal.ContentContainer;
            var group = content.GetComponent<VerticalLayoutGroup>();
            if (group != null)
            {
                group.padding = new RectOffset(10, 10, 0, 0);
                group.spacing = 6f;
                group.childForceExpandWidth = true;
            }

            var tabs = NewChild(content, "Tabs");
            var tabRow = tabs.AddComponent<HorizontalLayoutGroup>();
            tabRow.childControlWidth = true;
            tabRow.childControlHeight = true;
            tabRow.childForceExpandWidth = true;
            SetLayout(tabs, -1f, 34f, 1f);
            var toggleGroup = tabs.AddComponent<ToggleGroup>();
            string[] tabNames = { "Graphics", "Audio", "Gameplay", "Controls" };
            for (int i = 0; i < tabNames.Length; i++)
            {
                var tab = Spawn("MH Tab", tabs.transform);
                var label = tab.GetComponentInChildren<TMP_Text>();
                label.text = tabNames[i];
                label.color = i == 0 ? Color.white : Sub;
                var toggle = tab.GetComponent<Toggle>();
                toggle.group = toggleGroup;
                toggle.isOn = i == 0;
            }

            AddRow(content, "Resolution", "MH Dropdown", go => SetDropdown(go, "1920 x 1080"));
            AddRow(content, "Display Mode", "MH Dropdown", go => SetDropdown(go, "Fullscreen"));
            AddRow(content, "Vertical Sync", "MH Switch Toggle", go =>
            {
                go.GetComponent<Toggle>().isOn = true;
                go.GetComponent<Image>().color = S.SwitchOn;
                ((RectTransform)go.transform.Find("Handle")).anchoredPosition = new Vector2(9f, 0f);
            });
            AddRow(content, "Frame Rate Limit", "MH Slider", go => go.GetComponent<Slider>().value = 0.6f);
            AddRow(content, "Brightness", "MH Slider", go => go.GetComponent<Slider>().value = 0.6f);
            AddRow(content, "Quality Preset", "MH Dropdown", go => SetDropdown(go, "High"));

            var spacer = NewChild(content, "Spacer");
            SetLayout(spacer, -1f, 30f, 1f).flexibleHeight = 1f;
            Spawn("MH Separator", content);
            var footer = NewChild(content, "Footer");
            var footerRow = footer.AddComponent<HorizontalLayoutGroup>();
            footerRow.spacing = 12f;
            footerRow.childControlWidth = true;
            footerRow.childControlHeight = true;
            footerRow.childForceExpandWidth = false;
            footerRow.childAlignment = TextAnchor.MiddleLeft;
            SetLayout(footer, -1f, 40f, 1f);
            SetButton(Spawn("MH Button Secondary", footer.transform), "Default");
            SetLayout(NewChild(footer.transform, "Gap"), -1f, 1f, 1f);
            SetButton(Spawn("MH Button Secondary", footer.transform), "Cancel");
            SetButton(Spawn("MH Button Primary", footer.transform), "Apply");
        }
        #endregion

        #region [Private Methods]
        private static void LoadShared()
        {
            _rounded = LoadSprite(RoundedArt);
            _roundedLine = LoadSprite(RoundedLineArt);
            _circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            _checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
            _arrow = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");
            _mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
            _title = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KanitBoldFont);
            _label = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NovecentoNormalFont);
            _demi = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NovecentoDemiFont);
            _button = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NotoSemiBoldFont);
        }

        private static GameObject Spawn(string prefabName, Transform parent)
        {
            var prefab = LoadKitPrefab(prefabName);
            return prefab != null ? (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent) : NewChild(parent, prefabName + " (missing)");
        }

        private static void AddRow(Transform parent, string label, string controlPrefab, System.Action<GameObject> setup)
        {
            var row = Spawn("MH Setting Row", parent);
            var text = row.transform.Find("Label");
            if (text != null) text.GetComponent<TMP_Text>().text = label;
            var control = row.transform.Find("Control");
            if (control == null) return;
            var go = Spawn(controlPrefab, control);
            setup?.Invoke(go);
        }

        private static void SetDropdown(GameObject go, string value)
        {
            var dropdown = go.GetComponent<TMP_Dropdown>();
            if (dropdown == null) return;
            dropdown.options.Clear();
            dropdown.options.Add(new TMP_Dropdown.OptionData(value));
            dropdown.value = 0;
            dropdown.RefreshShownValue();
        }

        private static void SetButton(GameObject go, string label)
        {
            var text = go.GetComponentInChildren<TMP_Text>();
            if (text != null) text.text = label;
        }

        private static GameObject BuildButton(string name, Color tint, Color ink)
        {
            var go = NewRoot(name, ButtonSize);
            var image = AddImage(go, LoadSprite(ButtonArt), tint, Image.Type.Sliced, 1f, true);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.4f);
            button.colors = colors;
            var layout = go.AddComponent<LayoutElement>();
            layout.minWidth = ButtonSize.x;
            layout.minHeight = ButtonSize.y;
            layout.preferredHeight = ButtonSize.y;
            var text = AddText(go.transform, "Text", "Button", _button, 16f, ink, TextAlignmentOptions.Center);
            Stretch(text.gameObject, 0f);
            return go;
        }

        private static GameObject BuildMenuRow()
        {
            var go = NewRoot("MH Menu Row", new Vector2(288f, 30f));
            var image = AddImage(go, _rounded, Color.white, Image.Type.Sliced, RoundedBorder / 4f, true);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.06f);
            colors.selectedColor = new Color(1f, 1f, 1f, 0.06f);
            colors.pressedColor = new Color(Accent.r, Accent.g, Accent.b, 0.2f);
            colors.disabledColor = new Color(1f, 1f, 1f, 0f);
            button.colors = colors;
            SetLayout(go, -1f, 30f, 1f);

            var selected = NewChild(go.transform, "Selected");
            AddImage(selected, _roundedLine, Accent, Image.Type.Sliced, RoundedBorder / 4f);
            Stretch(selected, 0f);
            selected.SetActive(false);

            var text = AddText(go.transform, "Text", "Menu Item", _label, 17f, Text, TextAlignmentOptions.MidlineLeft, true, 2f);
            Stretch(text.gameObject, 0f);
            text.rectTransform.offsetMin = new Vector2(12f, 0f);
            text.rectTransform.offsetMax = new Vector2(-12f, 0f);
            return go;
        }

        private static GameObject BuildSwitch()
        {
            var go = NewRoot("MH Switch Toggle", new Vector2(36f, 18f));
            var background = AddImage(go, _rounded, Track, Image.Type.Sliced, RoundedBorder / 9f, true);
            var handle = NewChild(go.transform, "Handle");
            AddImage(handle, _circle, Color.white, Image.Type.Simple).preserveAspect = true;
            var handleRect = (RectTransform)handle.transform;
            handleRect.sizeDelta = new Vector2(12f, 12f);
            handleRect.anchoredPosition = new Vector2(-9f, 0f);
            var toggle = go.AddComponent<Toggle>();
            toggle.transition = Selectable.Transition.None;
            toggle.targetGraphic = background;
            toggle.graphic = null;
            toggle.isOn = false;
            var sw = go.AddComponent<SwitchToggle>();
            var so = new SerializedObject(sw);
            so.FindProperty("handleTransform").objectReferenceValue = handleRect;
            so.FindProperty("background").objectReferenceValue = background;
            so.FindProperty("onColor").colorValue = S.SwitchOn;
            so.FindProperty("offColor").colorValue = Track;
            so.ApplyModifiedPropertiesWithoutUndo();
            FixedSize(go, 36f, 18f);
            return go;
        }

        private static GameObject BuildCheckbox()
        {
            var go = NewRoot("MH Checkbox", new Vector2(18f, 18f));
            var box = AddImage(go, _rounded, Field, Image.Type.Sliced, RoundedBorder / 3f, true);
            AddOutline(go, S.BoxLine, 3f);
            var check = NewChild(go.transform, "Checkmark");
            var mark = AddImage(check, _checkmark, Accent, Image.Type.Simple);
            mark.preserveAspect = true;
            Stretch(check, 1f);
            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = box;
            toggle.graphic = mark;
            toggle.isOn = true;
            FixedSize(go, 18f, 18f);
            return go;
        }

        private static GameObject BuildSlider()
        {
            var res = DefaultResources();
            var go = DefaultControls.CreateSlider(res);
            go.name = "MH Slider";
            ((RectTransform)go.transform).sizeDelta = new Vector2(260f, 18f);
            var background = go.transform.Find("Background").GetComponent<Image>();
            background.sprite = _rounded;
            background.type = Image.Type.Sliced;
            background.pixelsPerUnitMultiplier = RoundedBorder / 2f;
            background.color = Track;
            var bgRect = background.rectTransform;
            bgRect.anchorMin = new Vector2(0f, 0.5f);
            bgRect.anchorMax = new Vector2(1f, 0.5f);
            bgRect.sizeDelta = new Vector2(0f, 4f);

            var fillArea = (RectTransform)go.transform.Find("Fill Area");
            fillArea.anchorMin = new Vector2(0f, 0.5f);
            fillArea.anchorMax = new Vector2(1f, 0.5f);
            fillArea.sizeDelta = new Vector2(-14f, 4f);
            var fill = fillArea.Find("Fill").GetComponent<Image>();
            fill.sprite = _rounded;
            fill.type = Image.Type.Sliced;
            fill.pixelsPerUnitMultiplier = RoundedBorder / 2f;
            fill.color = Accent;
            fill.rectTransform.sizeDelta = new Vector2(7f, 0f);

            var handleArea = (RectTransform)go.transform.Find("Handle Slide Area");
            handleArea.sizeDelta = new Vector2(-14f, 0f);
            var handle = handleArea.Find("Handle").GetComponent<Image>();
            handle.sprite = _circle;
            handle.type = Image.Type.Simple;
            handle.color = Accent;
            handle.preserveAspect = true;
            handle.rectTransform.sizeDelta = new Vector2(14f, 0f);
            var inner = NewChild(handle.transform, "Inner");
            AddImage(inner, _circle, Color.white, Image.Type.Simple).preserveAspect = true;
            Stretch(inner, 2f);

            var slider = go.GetComponent<Slider>();
            slider.value = 0.8f;
            SetLayout(go, 260f, 18f, 0f);
            return go;
        }

        private static GameObject BuildDropdown()
        {
            var go = TMP_DefaultControls.CreateDropdown(TmpResources());
            go.name = "MH Dropdown";
            ((RectTransform)go.transform).sizeDelta = new Vector2(220f, 30f);
            StyleField(go.GetComponent<Image>(), 4f);
            AddOutline(go, FieldLine, 4f);

            var dropdown = go.GetComponent<TMP_Dropdown>();
            StyleLabel(dropdown.captionText, 15f, Text);
            dropdown.captionText.rectTransform.offsetMin = new Vector2(10f, 0f);
            dropdown.captionText.rectTransform.offsetMax = new Vector2(-30f, 0f);
            var arrow = go.transform.Find("Arrow").GetComponent<Image>();
            arrow.sprite = _arrow;
            arrow.color = Sub;
            arrow.rectTransform.sizeDelta = new Vector2(14f, 14f);

            var template = dropdown.template;
            var templateImage = template.GetComponent<Image>();
            StyleField(templateImage, 4f);
            templateImage.color = S.ListBackground;
            AddOutline(template.gameObject, FieldLine, 4f);
            var item = template.Find("Viewport/Content/Item");
            var itemBg = item.Find("Item Background").GetComponent<Image>();
            itemBg.sprite = _rounded;
            itemBg.type = Image.Type.Sliced;
            itemBg.pixelsPerUnitMultiplier = RoundedBorder / 3f;
            itemBg.color = new Color(1f, 1f, 1f, 0f);
            var itemToggle = item.GetComponent<Toggle>();
            var itemColors = itemToggle.colors;
            itemColors.normalColor = new Color(1f, 1f, 1f, 0f);
            itemColors.highlightedColor = new Color(Accent.r, Accent.g, Accent.b, 0.25f);
            itemColors.selectedColor = new Color(Accent.r, Accent.g, Accent.b, 0.25f);
            itemToggle.colors = itemColors;
            var itemCheck = item.Find("Item Checkmark").GetComponent<Image>();
            itemCheck.color = Accent;
            StyleLabel(dropdown.itemText, 14f, Text);

            var scrollbar = template.Find("Scrollbar");
            if (scrollbar != null) StyleVerticalScrollbar(scrollbar, template.GetComponent<ScrollRect>());

            dropdown.options.Clear();
            foreach (var option in new[] { "Low", "Medium", "High", "Ultra" }) dropdown.options.Add(new TMP_Dropdown.OptionData(option));
            dropdown.value = 2;
            dropdown.RefreshShownValue();
            SetLayout(go, 220f, 30f, 0f);
            return go;
        }

        private static GameObject BuildInput()
        {
            var go = TMP_DefaultControls.CreateInputField(TmpResources());
            go.name = "MH Input Field";
            ((RectTransform)go.transform).sizeDelta = new Vector2(240f, 30f);
            StyleField(go.GetComponent<Image>(), 4f);
            AddOutline(go, FieldLine, 4f);
            var input = go.GetComponent<TMP_InputField>();
            StyleLabel(input.textComponent, 14f, Text);
            if (input.placeholder is TMP_Text placeholder)
            {
                StyleLabel(placeholder, 14f, S.Placeholder);
                placeholder.text = "Search...";
                placeholder.fontStyle = FontStyles.Normal;
            }
            input.caretColor = Accent;
            input.selectionColor = new Color(Accent.r, Accent.g, Accent.b, 0.3f);
            SetLayout(go, 240f, 30f, 0f);
            return go;
        }

        private static GameObject BuildScrollbar()
        {
            var go = DefaultControls.CreateScrollbar(DefaultResources());
            go.name = "MH Scrollbar";
            var scrollbar = go.GetComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            ((RectTransform)go.transform).sizeDelta = new Vector2(6f, 160f);
            var track = go.GetComponent<Image>();
            track.sprite = _rounded;
            track.type = Image.Type.Sliced;
            track.pixelsPerUnitMultiplier = RoundedBorder / 3f;
            track.color = S.ScrollTrack;
            var handle = scrollbar.handleRect.GetComponent<Image>();
            handle.sprite = _rounded;
            handle.type = Image.Type.Sliced;
            handle.pixelsPerUnitMultiplier = RoundedBorder / 3f;
            handle.color = S.ScrollThumb;
            var area = (RectTransform)scrollbar.handleRect.parent;
            area.offsetMin = Vector2.zero;
            area.offsetMax = Vector2.zero;
            scrollbar.handleRect.offsetMin = Vector2.zero;
            scrollbar.handleRect.offsetMax = Vector2.zero;
            scrollbar.size = 0.35f;
            return go;
        }

        private static GameObject BuildKeyCap()
        {
            var go = NewRoot("MH Key Cap", new Vector2(34f, 26f));
            AddImage(go, _rounded, S.KeyCap, Image.Type.Sliced, RoundedBorder / 4f);
            AddOutline(go, S.KeyCapLine, 4f);
            var text = AddText(go.transform, "Text", "E", _demi, 13f, Color.white, TextAlignmentOptions.Center);
            Stretch(text.gameObject, 0f);
            FixedSize(go, 34f, 26f);
            return go;
        }

        private static GameObject BuildKeyButton()
        {
            var go = NewRoot(S.KeyButton, S.KeyButtonSize);
            var image = AddImage(go, _rounded, S.KeyCap, Image.Type.Sliced, RoundedBorder / 4f, true);
            AddOutline(go, S.KeyCapLine, 4f);
            var shade = NewChild(go.transform, "Shade");
            AddImage(shade, null, new Color(0f, 0f, 0f, 0.5f), Image.Type.Simple);
            var shadeRect = (RectTransform)shade.transform;
            shadeRect.anchorMin = new Vector2(0f, 0f);
            shadeRect.anchorMax = new Vector2(1f, 0f);
            shadeRect.pivot = new Vector2(0.5f, 0f);
            shadeRect.sizeDelta = new Vector2(-4f, 2f);
            shadeRect.anchoredPosition = new Vector2(0f, 1f);
            shade.AddComponent<LayoutElement>().ignoreLayout = true;
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(1.3f, 1.3f, 1.3f, 1f);
            colors.pressedColor = S.Accent;
            colors.selectedColor = Color.white;
            button.colors = colors;
            var text = AddText(go.transform, "Text", "E", _demi, 13f, Color.white, TextAlignmentOptions.Center, true, 2f);
            Stretch(text.gameObject, 0f);
            SetLayout(go, S.KeyButtonSize.x, S.KeyButtonSize.y, 0f);
            return go;
        }

        private static GameObject BuildTab()
        {
            var go = NewRoot("MH Tab", new Vector2(120f, 34f));
            var hit = AddImage(go, null, new Color(Accent.r, Accent.g, Accent.b, 0f), Image.Type.Simple, 1f, true);
            var line = NewChild(go.transform, "Active Line");
            var lineImage = AddImage(line, null, Accent, Image.Type.Simple);
            var lineRect = (RectTransform)line.transform;
            lineRect.anchorMin = new Vector2(0f, 0f);
            lineRect.anchorMax = new Vector2(1f, 0f);
            lineRect.pivot = new Vector2(0.5f, 0f);
            lineRect.sizeDelta = new Vector2(0f, 2f);
            lineRect.anchoredPosition = new Vector2(0f, 1f);
            var text = AddText(go.transform, "Text", "Tab", _demi, 14f, Text, TextAlignmentOptions.Center, true, 4f);
            Stretch(text.gameObject, 0f);
            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = hit;
            toggle.graphic = lineImage;
            var colors = toggle.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.06f);
            colors.selectedColor = new Color(1f, 1f, 1f, 0f);
            toggle.colors = colors;
            SetLayout(go, -1f, 34f, 1f);
            return go;
        }

        private static GameObject BuildTabButton()
        {
            var go = NewRoot(TabButtonStyle, new Vector2(120f, 34f));
            var hit = AddImage(go, null, Color.white, Image.Type.Simple, 1f, true);
            var button = go.AddComponent<Button>();
            button.targetGraphic = hit;
            var colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.06f);
            colors.pressedColor = new Color(Accent.r, Accent.g, Accent.b, 0.15f);
            colors.selectedColor = new Color(1f, 1f, 1f, 0f);
            colors.disabledColor = new Color(1f, 1f, 1f, 0f);
            button.colors = colors;

            var fill = NewChild(go.transform, "Active Fill");
            AddImage(fill, null, new Color(Accent.r, Accent.g, Accent.b, 0.08f), Image.Type.Simple);
            Stretch(fill, 0f);
            fill.SetActive(false);

            var line = NewChild(go.transform, "Active Line");
            AddImage(line, null, Accent, Image.Type.Simple);
            var lineRect = (RectTransform)line.transform;
            lineRect.anchorMin = new Vector2(0f, 0f);
            lineRect.anchorMax = new Vector2(1f, 0f);
            lineRect.pivot = new Vector2(0.5f, 0f);
            lineRect.sizeDelta = new Vector2(0f, 2f);
            lineRect.anchoredPosition = new Vector2(0f, 1f);
            line.SetActive(false);

            var text = AddText(go.transform, "Text", "Tab", _demi, 14f, Sub, TextAlignmentOptions.Center, true, 4f);
            Stretch(text.gameObject, 0f);
            SetLayout(go, -1f, 34f, 1f);
            return go;
        }

        private static void Register(UIElementRegistrySO registry, UIElementType type, string styleName, Vector2 minSize, Vector2 fixedSize, StringBuilder log)
        {
            var prefab = LoadKitPrefab(styleName);
            if (prefab == null)
            {
                log.AppendLine($"  {styleName}: prefab missing, skipped");
                return;
            }
            var entry = registry.GetEntry(type, styleName);
            if (entry == null)
            {
                entry = new UIElementEntry { StyleName = styleName, Type = type };
                registry.Entries.Add(entry);
            }
            entry.Prefab = prefab;
            entry.TargetComponentPath = string.Empty;
            entry.MinSize = minSize;
            entry.FixedSize = fixedSize;
            entry.KeepPrefabLook = true;
            log.AppendLine($"  {type} / {styleName}");
        }

        private static GameObject BuildFoldoutHeader()
        {
            var go = NewRoot("MH Foldout Header", new Vector2(400f, 32f));
            var bg = AddImage(go, _rounded, Raised, Image.Type.Sliced, RoundedBorder / 4f, true);
            AddOutline(go, InnerLine, 4f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = bg;
            var arrow = NewChild(go.transform, "Arrow");
            AddImage(arrow, _arrow, Accent, Image.Type.Simple).preserveAspect = true;
            var arrowRect = (RectTransform)arrow.transform;
            arrowRect.anchorMin = new Vector2(0f, 0.5f);
            arrowRect.anchorMax = new Vector2(0f, 0.5f);
            arrowRect.sizeDelta = new Vector2(14f, 14f);
            arrowRect.anchoredPosition = new Vector2(16f, 0f);
            var text = AddText(go.transform, "Text", "Section", _demi, 14f, Color.white, TextAlignmentOptions.MidlineLeft, true, 4f);
            Stretch(text.gameObject, 0f);
            text.rectTransform.offsetMin = new Vector2(30f, 0f);
            SetLayout(go, -1f, 32f, 1f);
            return go;
        }

        private static GameObject BuildContainer()
        {
            var go = NewRoot("MH Container", new Vector2(340f, 140f));
            AddImage(go, _rounded, Inner, Image.Type.Sliced, RoundedBorder / 6f);
            AddOutline(go, InnerLine, 6f);
            var group = go.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(12, 12, 8, 12);
            group.spacing = 8f;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;
            var title = AddText(go.transform, "Title", "Group Title", _demi, 12f, Sub, TextAlignmentOptions.MidlineLeft, true, 6f);
            SetLayout(title.gameObject, -1f, 18f, 1f);
            var sep = NewChild(go.transform, "Separator");
            AddImage(sep, null, new Color(1f, 1f, 1f, 0.06f), Image.Type.Simple);
            SetLayout(sep, -1f, 1f, 1f);
            var body = NewChild(go.transform, "Body");
            var bodyGroup = body.AddComponent<VerticalLayoutGroup>();
            bodyGroup.spacing = 6f;
            bodyGroup.childControlWidth = true;
            bodyGroup.childControlHeight = true;
            bodyGroup.childForceExpandWidth = true;
            bodyGroup.childForceExpandHeight = false;
            SetLayout(body, -1f, -1f, 1f);
            return go;
        }

        private static GameObject BuildTooltip()
        {
            var go = NewRoot("MH Tooltip", new Vector2(250f, 64f));
            AddImage(go, _rounded, S.TooltipBackground, Image.Type.Sliced, RoundedBorder / 4f);
            AddOutline(go, FieldLine, 4f);
            var accent = NewChild(go.transform, "Accent");
            AddImage(accent, null, Accent, Image.Type.Simple);
            var accentRect = (RectTransform)accent.transform;
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(0f, 1f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.sizeDelta = new Vector2(3f, 0f);
            accent.AddComponent<LayoutElement>().ignoreLayout = true;
            var group = go.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(12, 10, 8, 8);
            group.spacing = 2f;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;
            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var title = AddText(go.transform, "Title", "Tooltip Title", _demi, 13f, Color.white, TextAlignmentOptions.TopLeft, true, 2f);
            var desc = AddText(go.transform, "Description", "Short description of the setting.", _label, 12f, Sub, TextAlignmentOptions.TopLeft);
            desc.textWrappingMode = TextWrappingModes.Normal;
            desc.overflowMode = TextOverflowModes.Overflow;
            title.overflowMode = TextOverflowModes.Overflow;
            return go;
        }

        private static GameObject BuildSeparator()
        {
            var go = NewRoot("MH Separator", new Vector2(300f, 1f));
            AddImage(go, null, new Color(1f, 1f, 1f, 0.08f), Image.Type.Simple);
            SetLayout(go, -1f, 1f, 1f);
            return go;
        }

        private static GameObject BuildSettingRow()
        {
            var go = NewRoot("MH Setting Row", new Vector2(420f, 36f));
            var row = go.AddComponent<HorizontalLayoutGroup>();
            row.childAlignment = TextAnchor.MiddleLeft;
            row.spacing = 16f;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;
            SetLayout(go, -1f, 36f, 1f);
            var label = AddText(go.transform, "Label", "Setting", _label, 15f, Text, TextAlignmentOptions.MidlineLeft, true, 2f);
            SetLayout(label.gameObject, -1f, 36f, 1f);
            var control = NewChild(go.transform, "Control");
            var controlRow = control.AddComponent<HorizontalLayoutGroup>();
            controlRow.childAlignment = TextAnchor.MiddleRight;
            controlRow.spacing = 8f;
            controlRow.childControlWidth = true;
            controlRow.childControlHeight = true;
            controlRow.childForceExpandWidth = false;
            controlRow.childForceExpandHeight = false;
            var divider = NewChild(go.transform, "Divider");
            AddImage(divider, null, new Color(1f, 1f, 1f, 0.05f), Image.Type.Simple);
            divider.AddComponent<LayoutElement>().ignoreLayout = true;
            var dividerRect = (RectTransform)divider.transform;
            dividerRect.anchorMin = new Vector2(0f, 0f);
            dividerRect.anchorMax = new Vector2(1f, 0f);
            dividerRect.pivot = new Vector2(0.5f, 0f);
            dividerRect.sizeDelta = new Vector2(0f, 1f);
            return go;
        }

        private static DefaultControls.Resources DefaultResources()
        {
            return new DefaultControls.Resources
            {
                standard = _rounded,
                background = _rounded,
                inputField = _rounded,
                knob = _circle,
                checkmark = _checkmark,
                dropdown = _arrow,
                mask = _mask
            };
        }

        private static TMP_DefaultControls.Resources TmpResources()
        {
            return new TMP_DefaultControls.Resources
            {
                standard = _rounded,
                background = _rounded,
                inputField = _rounded,
                knob = _circle,
                checkmark = _checkmark,
                dropdown = _arrow,
                mask = _mask
            };
        }

        private static void StyleVerticalScrollbar(Transform scrollbar, ScrollRect scrollRect)
        {
            var rect = (RectTransform)scrollbar;
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(S.ScrollbarWidth, -S.ScrollbarInset * 2f);
            rect.anchoredPosition = new Vector2(-S.ScrollbarInset, -S.ScrollbarInset);

            var track = scrollbar.GetComponent<Image>();
            track.sprite = _rounded;
            track.type = Image.Type.Sliced;
            track.pixelsPerUnitMultiplier = RoundedBorder / (S.ScrollbarWidth * 0.5f);
            track.color = S.ScrollTrack;

            var bar = scrollbar.GetComponent<Scrollbar>();
            bar.direction = Scrollbar.Direction.BottomToTop;
            bar.transition = Selectable.Transition.ColorTint;
            var colors = bar.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
            colors.pressedColor = S.Accent;
            colors.selectedColor = Color.white;
            bar.colors = colors;

            var area = (RectTransform)scrollbar.Find("Sliding Area");
            area.offsetMin = Vector2.zero;
            area.offsetMax = Vector2.zero;
            var handleRect = (RectTransform)area.Find("Handle");
            handleRect.offsetMin = Vector2.zero;
            handleRect.offsetMax = Vector2.zero;
            var handle = handleRect.GetComponent<Image>();
            handle.sprite = _rounded;
            handle.type = Image.Type.Sliced;
            handle.pixelsPerUnitMultiplier = RoundedBorder / (S.ScrollbarWidth * 0.5f);
            handle.color = S.ScrollThumb;
            bar.targetGraphic = handle;

            if (scrollRect != null) scrollRect.verticalScrollbarSpacing = S.ScrollbarInset;
        }

        private static void StyleField(Image image, float radius)
        {
            if (image == null) return;
            image.sprite = _rounded;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = RoundedBorder / radius;
            image.color = Field;
        }

        private static void StyleLabel(TMP_Text text, float size, Color color)
        {
            if (text == null) return;
            if (_label != null) text.font = _label;
            text.fontSize = size;
            text.color = color;
            text.characterSpacing = 2f;
            text.fontStyle = FontStyles.UpperCase;
        }

        private static void AddOutline(GameObject go, Color color, float radius)
        {
            var outline = NewChild(go.transform, "Outline");
            AddImage(outline, _roundedLine, color, Image.Type.Sliced, RoundedBorder / radius);
            Stretch(outline, 0f);
            outline.AddComponent<LayoutElement>().ignoreLayout = true;
        }

        private static GameObject Save(GameObject go, StringBuilder log)
        {
            string path = $"{KitFolder}/{go.name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            log.AppendLine($"  {path}");
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
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

        private static Sprite LoadSprite(string path)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite) return sprite;
            }
            Debug.LogWarning($"[MiniHeaderUIKit] Sprite not found: {path}");
            return null;
        }

        #endregion
    }
}
