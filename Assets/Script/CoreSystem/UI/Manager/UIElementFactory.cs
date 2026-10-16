using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI
{
    public sealed class UIElementFactory
    {
        #region [Variables/Fields]
        private readonly UIElementRegistrySO _registry;
        private readonly UITheme _theme;
        #endregion

        #region [Properties]
        public UIElementRegistrySO Registry => _registry;
        public UITheme Theme => _theme;
        #endregion

        #region [Public Methods]
        public UIElementFactory(UIElementRegistrySO registry)
        {
            _registry = registry;
            _theme = registry != null ? registry.Theme : UITheme.Default;
        }

        public bool HasStyle(UIElementType type, string styleName)
        {
            var entry = _registry != null ? _registry.GetEntry(type, styleName) : null;
            return entry != null && entry.Prefab != null;
        }

        public GameObject Create(UIElementType type, string styleName, Transform parent, UILayoutKind layout = UILayoutKind.Column)
        {
            GameObject go = InstantiateFromRegistry(type, styleName, parent) ?? Generate(type, styleName, parent, layout);
            if (type == UIElementType.Container) EnsureContainerLayout(go, layout);
            RemoveFitterUnderLayout(go, parent);
            return go;
        }

        public GameObject CreateScrollView(string styleName, Transform parent, bool horizontal, bool vertical, ScrollRect.MovementType movementType, out Transform content)
        {
            GameObject instance = InstantiateFromRegistry(UIElementType.ScrollRect, styleName, parent);
            if (instance != null)
            {
                ScrollRect sr = GetTarget<ScrollRect>(instance, UIElementType.ScrollRect, styleName);
                if (sr != null)
                {
                    sr.horizontal = horizontal;
                    sr.vertical = vertical;
                    sr.movementType = movementType;
                    content = sr.content != null ? sr.content : sr.transform;
                }
                else
                {
                    content = instance.transform;
                }
                return instance;
            }

            return GenerateScrollView(styleName, parent, horizontal, vertical, movementType, out content);
        }

        public T GetTarget<T>(GameObject instance, UIElementType type, string styleName) where T : Component
        {
            if (instance == null) return null;

            var entry = _registry != null ? _registry.GetEntry(type, styleName) : null;
            if (entry != null && !string.IsNullOrEmpty(entry.TargetComponentPath))
            {
                Transform target = instance.transform.Find(entry.TargetComponentPath);
                if (target != null)
                {
                    T component = target.GetComponent<T>();
                    if (component != null) return component;
                }
            }

            return instance.GetComponentInChildren<T>(true);
        }

        public void FitControl(GameObject go, TMP_Text label)
        {
            if (go == null) return;

            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();

            var group = go.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (group != null)
            {
                group.enabled = true;
                group.padding = new RectOffset(_theme.ControlPaddingX, _theme.ControlPaddingX, _theme.ControlPaddingY, _theme.ControlPaddingY);
                group.childControlWidth = true;
                group.childControlHeight = true;
                group.childForceExpandWidth = false;
                group.childForceExpandHeight = false;
                group.childAlignment = TextAnchor.MiddleCenter;
            }

            float height = _theme.ControlHeight;
            if (label != null)
            {
                label.fontSize = _theme.ControlFontSize;
                label.textWrappingMode = TextWrappingModes.NoWrap;
                label.overflowMode = TextOverflowModes.Overflow;

                Vector2 size = label.GetPreferredValues(label.text, float.PositiveInfinity, float.PositiveInfinity);
                float width = Mathf.Ceil(size.x) + _theme.ControlPaddingX * 2;
                height = Mathf.Max(height, Mathf.Ceil(size.y) + _theme.ControlPaddingY * 2);
                le.minWidth = Mathf.Max(le.minWidth, width);
                if (le.preferredWidth < width) le.preferredWidth = width;
            }

            le.minHeight = height;
            le.preferredHeight = height;
        }

        public void FitInput(GameObject go)
        {
            if (go == null) return;

            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            le.minHeight = _theme.ControlHeight;
            le.preferredHeight = _theme.ControlHeight;
            if (le.preferredWidth < 0f) le.preferredWidth = _theme.InputWidth;
            if (le.minWidth < 0f) le.minWidth = 60f;
        }

        public static UILayoutKind GetLayoutKind(Transform parent)
        {
            if (parent == null) return UILayoutKind.Column;
            var group = parent.GetComponent<LayoutGroup>();
            if (group is HorizontalLayoutGroup) return UILayoutKind.Row;
            if (group is GridLayoutGroup) return UILayoutKind.Grid;
            return UILayoutKind.Column;
        }

        public static bool HasLayoutGroup(Transform parent)
        {
            return parent != null && parent.GetComponent<LayoutGroup>() != null;
        }
        #endregion

        #region [Private Methods]
        private GameObject InstantiateFromRegistry(UIElementType type, string styleName, Transform parent)
        {
            var entry = _registry != null ? _registry.GetEntry(type, styleName) : null;
            if (entry == null || entry.Prefab == null) return null;

            GameObject instance = Object.Instantiate(entry.Prefab, parent);
            instance.name = entry.Prefab.name;

            var prefabRect = entry.Prefab.GetComponent<RectTransform>();
            if (prefabRect != null && prefabRect.sizeDelta.y > 0 && UsesPrefabHeight(type))
            {
                var le = instance.GetComponent<LayoutElement>();
                if (le == null) le = instance.AddComponent<LayoutElement>();
                if (le.preferredHeight < 0) le.preferredHeight = prefabRect.sizeDelta.y;
            }
            return instance;
        }

        private static bool UsesPrefabHeight(UIElementType type)
        {
            switch (type)
            {
                case UIElementType.Container:
                case UIElementType.Text:
                case UIElementType.Button:
                case UIElementType.InputField:
                case UIElementType.Toggle:
                case UIElementType.Divider:
                    return false;
                default:
                    return true;
            }
        }

        private static void RemoveFitterUnderLayout(GameObject go, Transform parent)
        {
            if (go == null || !HasLayoutGroup(parent)) return;

            var fitter = go.GetComponent<ContentSizeFitter>();
            if (fitter != null) DestroyComponent(fitter);
        }

        private void EnsureContainerLayout(GameObject go, UILayoutKind layout)
        {
            var existing = go.GetComponent<LayoutGroup>();
            if (MatchesLayout(existing, layout)) return;

            RectOffset padding = existing != null ? existing.padding : new RectOffset(0, 0, 0, 0);
            TextAnchor alignment = existing != null ? existing.childAlignment : DefaultAlignment(layout);
            float spacing = existing is HorizontalOrVerticalLayoutGroup hv ? hv.spacing : _theme.Gap;
            if (existing != null) DestroyComponent(existing);

            AddLayoutGroup(go, layout, padding, alignment, spacing);
        }

        private static bool MatchesLayout(LayoutGroup group, UILayoutKind layout)
        {
            switch (layout)
            {
                case UILayoutKind.Row: return group is HorizontalLayoutGroup;
                case UILayoutKind.Grid: return group is GridLayoutGroup;
                default: return group is VerticalLayoutGroup;
            }
        }

        private static TextAnchor DefaultAlignment(UILayoutKind layout)
        {
            return layout == UILayoutKind.Row ? TextAnchor.MiddleLeft : TextAnchor.UpperLeft;
        }

        private void AddLayoutGroup(GameObject go, UILayoutKind layout, RectOffset padding, TextAnchor alignment, float spacing)
        {
            if (layout == UILayoutKind.Grid)
            {
                var grid = go.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(100f, 100f);
                grid.spacing = new Vector2(_theme.Gap, _theme.Gap);
                grid.padding = padding;
                grid.childAlignment = alignment;
                grid.constraint = GridLayoutGroup.Constraint.Flexible;
                return;
            }

            HorizontalOrVerticalLayoutGroup group = layout == UILayoutKind.Row
                ? go.AddComponent<HorizontalLayoutGroup>()
                : (HorizontalOrVerticalLayoutGroup)go.AddComponent<VerticalLayoutGroup>();
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            group.spacing = spacing;
            group.padding = padding;
            group.childAlignment = alignment;
        }

        private GameObject Generate(UIElementType type, string styleName, Transform parent, UILayoutKind layout)
        {
            GameObject go = new GameObject($"[{type}] {styleName}", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            switch (type)
            {
                case UIElementType.Container:
                    AddLayoutGroup(go, layout, new RectOffset(0, 0, 0, 0), DefaultAlignment(layout), _theme.Gap);
                    if (!HasLayoutGroup(parent))
                    {
                        var fitter = go.AddComponent<ContentSizeFitter>();
                        fitter.horizontalFit = layout == UILayoutKind.Row ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
                        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    }
                    break;

                case UIElementType.Text:
                    var text = go.AddComponent<TextMeshProUGUI>();
                    ApplyFont(text);
                    text.color = _theme.Text;
                    text.fontSize = _theme.BodySize;
                    text.alignment = TextAlignmentOptions.MidlineLeft;
                    text.raycastTarget = false;
                    break;

                case UIElementType.Button:
                    var buttonImage = go.AddComponent<Image>();
                    buttonImage.color = _theme.Secondary;
                    var button = go.AddComponent<Button>();
                    button.targetGraphic = buttonImage;
                    var buttonLayout = go.AddComponent<HorizontalLayoutGroup>();
                    buttonLayout.childControlWidth = true;
                    buttonLayout.childControlHeight = true;
                    buttonLayout.childForceExpandWidth = true;
                    buttonLayout.childForceExpandHeight = true;
                    buttonLayout.childAlignment = TextAnchor.MiddleCenter;
                    buttonLayout.padding = new RectOffset(14, 14, 6, 6);
                    var buttonLe = go.AddComponent<LayoutElement>();
                    buttonLe.minHeight = _theme.ControlHeight;
                    var label = CreateChildText(go.transform, "Text", _theme.TextOnAccent, _theme.BodySize, TextAlignmentOptions.Center);
                    label.raycastTarget = false;
                    break;

                case UIElementType.Image:
                    go.AddComponent<Image>().preserveAspect = true;
                    break;

                case UIElementType.Toggle:
                    GenerateToggle(go);
                    break;

                case UIElementType.InputField:
                    GenerateInputField(go);
                    break;

                case UIElementType.Custom:
                    go.AddComponent<UICustomElement>();
                    break;

                case UIElementType.RawImage:
                    go.AddComponent<RawImage>();
                    break;

                case UIElementType.Scrollbar:
                    GenerateScrollbar(go);
                    break;

                case UIElementType.ProgressBar:
                    GenerateProgressBar(go);
                    break;

                case UIElementType.Divider:
                    var divider = go.AddComponent<Image>();
                    divider.color = _theme.Divider;
                    divider.raycastTarget = false;
                    var dividerLe = go.AddComponent<LayoutElement>();
                    dividerLe.preferredHeight = 1f;
                    dividerLe.flexibleWidth = 1f;
                    break;

                case UIElementType.Mask:
                    go.AddComponent<RectMask2D>();
                    break;
            }

            return go;
        }

        private void GenerateToggle(GameObject go)
        {
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = _theme.Gap;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleLeft;
            var toggle = go.AddComponent<Toggle>();

            GameObject box = new GameObject("Background", typeof(RectTransform));
            box.transform.SetParent(go.transform, false);
            var boxImage = box.AddComponent<Image>();
            boxImage.color = _theme.PanelAlt;
            var boxLe = box.AddComponent<LayoutElement>();
            boxLe.preferredWidth = 22f;
            boxLe.preferredHeight = 22f;

            GameObject check = new GameObject("Checkmark", typeof(RectTransform));
            check.transform.SetParent(box.transform, false);
            var checkRect = (RectTransform)check.transform;
            checkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;
            var checkImage = check.AddComponent<Image>();
            checkImage.color = _theme.Primary;

            toggle.targetGraphic = boxImage;
            toggle.graphic = checkImage;

            CreateChildText(go.transform, "Label", _theme.Text, _theme.BodySize, TextAlignmentOptions.MidlineLeft).raycastTarget = false;
        }

        private void GenerateInputField(GameObject go)
        {
            var background = go.AddComponent<Image>();
            background.color = _theme.PanelAlt;
            var input = go.AddComponent<TMP_InputField>();
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = _theme.ControlHeight;
            le.preferredHeight = _theme.ControlHeight;
            le.preferredWidth = 160f;

            GameObject area = new GameObject("Text Area", typeof(RectTransform));
            area.transform.SetParent(go.transform, false);
            var areaRect = (RectTransform)area.transform;
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(10f, 4f);
            areaRect.offsetMax = new Vector2(-10f, -4f);
            area.AddComponent<RectMask2D>();

            var text = CreateChildText(area.transform, "Text", _theme.Text, _theme.BodySize, TextAlignmentOptions.MidlineLeft);
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            input.textViewport = areaRect;
            input.textComponent = text;
            input.customCaretColor = true;
            input.caretColor = _theme.Text;
        }

        private void GenerateScrollbar(GameObject go)
        {
            var background = go.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.3f);
            var scrollbar = go.AddComponent<Scrollbar>();

            GameObject area = new GameObject("Sliding Area", typeof(RectTransform));
            area.transform.SetParent(go.transform, false);
            var areaRect = (RectTransform)area.transform;
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.sizeDelta = Vector2.zero;

            GameObject handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(area.transform, false);
            var handleRect = (RectTransform)handle.transform;
            handleRect.sizeDelta = new Vector2(8f, 8f);
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = _theme.TextMuted;

            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleImage;
        }

        private void GenerateProgressBar(GameObject go)
        {
            var background = go.AddComponent<Image>();
            background.color = _theme.PanelAlt;
            var slider = go.AddComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 10f;
            le.preferredHeight = 10f;

            GameObject area = new GameObject("Fill Area", typeof(RectTransform));
            area.transform.SetParent(go.transform, false);
            var areaRect = (RectTransform)area.transform;
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(area.transform, false);
            var fillRect = (RectTransform)fill.transform;
            fillRect.sizeDelta = Vector2.zero;
            fill.AddComponent<Image>().color = _theme.Primary;

            slider.fillRect = fillRect;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
        }

        private GameObject GenerateScrollView(string styleName, Transform parent, bool horizontal, bool vertical, ScrollRect.MovementType movementType, out Transform content)
        {
            GameObject root = new GameObject($"[ScrollView] {styleName}", typeof(RectTransform));
            root.transform.SetParent(parent, false);

            var rootLe = root.AddComponent<LayoutElement>();
            rootLe.preferredHeight = vertical ? 220f : 50f;
            rootLe.flexibleWidth = 1f;

            var rootImage = root.AddComponent<Image>();
            rootImage.color = Color.clear;

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(root.transform, false);
            var viewportRect = (RectTransform)viewport.transform;
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.pivot = new Vector2(0f, 1f);
            viewport.AddComponent<RectMask2D>();

            GameObject contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewport.transform, false);
            var contentRect = (RectTransform)contentGo.transform;
            var fitter = contentGo.AddComponent<ContentSizeFitter>();

            if (horizontal && !vertical)
            {
                contentRect.anchorMin = new Vector2(0f, 0f);
                contentRect.anchorMax = new Vector2(0f, 1f);
                contentRect.pivot = new Vector2(0f, 0.5f);
                AddLayoutGroup(contentGo, UILayoutKind.Row, new RectOffset(0, 0, 0, 0), TextAnchor.MiddleLeft, _theme.Gap);
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else
            {
                contentRect.anchorMin = new Vector2(0f, 1f);
                contentRect.anchorMax = new Vector2(1f, 1f);
                contentRect.pivot = new Vector2(0.5f, 1f);
                AddLayoutGroup(contentGo, UILayoutKind.Column, new RectOffset(0, 0, 0, 0), TextAnchor.UpperLeft, _theme.Gap);
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            var scrollRect = root.AddComponent<ScrollRect>();
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = horizontal;
            scrollRect.vertical = vertical;
            scrollRect.movementType = movementType;
            scrollRect.elasticity = 0.1f;
            scrollRect.inertia = true;
            scrollRect.scrollSensitivity = 25f;

            content = contentGo.transform;
            return root;
        }

        private TextMeshProUGUI CreateChildText(Transform parent, string name, Color color, float size, TextAlignmentOptions alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<TextMeshProUGUI>();
            ApplyFont(text);
            text.color = color;
            text.fontSize = size;
            text.alignment = alignment;
            return text;
        }

        private void ApplyFont(TMP_Text text)
        {
            if (_registry != null && _registry.DefaultFont != null) text.font = _registry.DefaultFont;
        }

        private static void DestroyComponent(Component component)
        {
            Object.DestroyImmediate(component);
        }
        #endregion
    }
}
