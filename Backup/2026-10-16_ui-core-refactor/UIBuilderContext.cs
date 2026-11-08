using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CoreSystem.UI
{
    public struct UIElementRef
    {
        public GameObject GameObject;
        public RectTransform RectTransform => GameObject != null ? GameObject.GetComponent<RectTransform>() : null;
        public TMP_Text TextComponent => GameObject != null ? GameObject.GetComponentInChildren<TMP_Text>(true) : null;

        public UIElementRef(GameObject go)
        {
            GameObject = go;
        }

        public UIElementRef SetSize(float width, float height)
        {
            if (RectTransform != null) RectTransform.sizeDelta = new Vector2(width, height);
            return this;
        }

        public UIElementRef SetPreferredSize(float width = -1, float height = -1)
        {
            if (GameObject == null) return this;
            var le = GameObject.GetComponent<LayoutElement>();
            if (le == null) le = GameObject.AddComponent<LayoutElement>();
            if (width >= 0) le.preferredWidth = width;
            if (height >= 0) le.preferredHeight = height;

            var cs = GameObject.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (cs != null)
            {
                if (width >= 0) cs.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
                if (height >= 0) cs.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
            }
            return this;
        }
        
        public UIElementRef SetMinSize(float width = -1, float height = -1)
        {
            if (GameObject == null) return this;
            var le = GameObject.GetComponent<LayoutElement>();
            if (le == null) le = GameObject.AddComponent<LayoutElement>();
            if (width >= 0) le.minWidth = width;
            if (height >= 0) le.minHeight = height;
            return this;
        }

        public UIElementRef SetGridCellSize(Vector2 cellSize)
        {
            if (GameObject == null) return this;
            var group = GameObject.GetComponent<GridLayoutGroup>();
            if (group != null) group.cellSize = cellSize;
            return this;
        }

        public UIElementRef SetGridConstraint(GridLayoutGroup.Constraint constraint, int count)
        {
            if (GameObject == null) return this;
            var group = GameObject.GetComponent<GridLayoutGroup>();
            if (group != null)
            {
                group.constraint = constraint;
                group.constraintCount = count;
            }
            return this;
        }

        public UIElementRef SetGridSpacing(Vector2 spacing)
        {
            if (GameObject == null) return this;
            var group = GameObject.GetComponent<GridLayoutGroup>();
            if (group != null) group.spacing = spacing;
            return this;
        }

        public UIElementRef SetFlexibleSize(float flexWidth = -1, float flexHeight = -1)
        {
            if (GameObject == null) return this;
            var le = GameObject.GetComponent<LayoutElement>();
            if (le == null) le = GameObject.AddComponent<LayoutElement>();
            if (flexWidth >= 0) le.flexibleWidth = flexWidth;
            if (flexHeight >= 0) le.flexibleHeight = flexHeight;

            var cs = GameObject.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (cs != null)
            {
                if (flexWidth >= 0) cs.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
                if (flexHeight >= 0) cs.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
            }
            return this;
        }

        public UIElementRef SetFitParent(bool stretchWidth = true, bool stretchHeight = true)
        {
            if (GameObject == null) return this;
            
            // Turn off ContentSizeFitter if it exists
            var cs = GameObject.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (cs != null)
            {
                if (stretchWidth) cs.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
                if (stretchHeight) cs.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;

                if (cs.horizontalFit == UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained && 
                    cs.verticalFit == UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
                {
                    UnityEngine.Object.Destroy(cs); // Fully remove if unconstrained on both axes
                }
            }

            // Force layout element to flex and fill parent
            var le = GameObject.GetComponent<LayoutElement>();
            if (le == null) le = GameObject.AddComponent<LayoutElement>();
            if (stretchWidth) le.flexibleWidth = 1;
            if (stretchHeight) le.flexibleHeight = 1;

            return this;
        }

        // --- CSS-like Style Extensions ---

        public UIElementRef SetBackgroundColor(Color color)
        {
            if (GameObject == null) return this;
            var img = GameObject.GetComponent<Image>();
            if (img == null) img = GameObject.AddComponent<Image>();
            img.color = color;
            return this;
        }

        public UIElementRef SetSprite(Sprite sprite)
        {
            if (GameObject == null) return this;
            var img = GameObject.GetComponent<Image>();
            if (img == null) img = GameObject.AddComponent<Image>();
            img.sprite = sprite;
            if (sprite != null) img.color = Color.white;
            return this;
        }

        public UIElementRef SetOnClick(Action onClick)
        {
            if (GameObject == null) return this;
            var btn = GameObject.GetComponent<Button>();
            if (btn == null) btn = GameObject.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return this;
        }

        public UIElementRef SetColor(Color color)
        {
            if (GameObject == null) return this;
            var graphic = GameObject.GetComponent<Graphic>();
            if (graphic != null) graphic.color = color;
            return this;
        }

        public UIElementRef SetTextColor(Color color)
        {
            if (GameObject == null) return this;
            var txt = GameObject.GetComponentInChildren<TMP_Text>(true);
            if (txt != null) txt.color = color;
            return this;
        }

        public UIElementRef SetFontSize(float size)
        {
            if (GameObject == null) return this;
            var txt = GameObject.GetComponentInChildren<TMP_Text>(true);
            if (txt != null) txt.fontSize = size;
            return this;
        }

        public UIElementRef SetFontStyle(FontStyles style)
        {
            if (GameObject == null) return this;
            var txt = GameObject.GetComponentInChildren<TMP_Text>(true);
            if (txt != null) txt.fontStyle = style;
            return this;
        }

        public UIElementRef SetTextAlignment(TextAlignmentOptions alignment)
        {
            if (GameObject == null) return this;
            var txt = GameObject.GetComponentInChildren<TMP_Text>(true);
            if (txt != null) txt.alignment = alignment;
            return this;
        }

        public UIElementRef SetPadding(int left, int right, int top, int bottom)
        {
            if (GameObject == null) return this;
            var group = GameObject.GetComponentInChildren<LayoutGroup>(true);
            if (group != null) group.padding = new RectOffset(left, right, top, bottom);
            return this;
        }

        public UIElementRef SetSpacing(float spacing)
        {
            if (GameObject == null) return this;
            var group = GameObject.GetComponentInChildren<HorizontalOrVerticalLayoutGroup>(true);
            if (group != null) group.spacing = spacing;
            return this;
        }

        public UIElementRef SetChildAlignment(TextAnchor anchor)
        {
            if (GameObject == null) return this;
            var group = GameObject.GetComponentInChildren<LayoutGroup>(true);
            if (group != null) group.childAlignment = anchor;
            return this;
        }

        public UIElementRef SetScrollMovementType(UnityEngine.UI.ScrollRect.MovementType movementType)
        {
            if (GameObject == null) return this;
            var sr = GameObject.GetComponentInChildren<UnityEngine.UI.ScrollRect>(true);
            if (sr != null) sr.movementType = movementType;
            return this;
        }

        public UIElementRef SetScrollElasticity(float elasticity)
        {
            if (GameObject == null) return this;
            var sr = GameObject.GetComponentInChildren<UnityEngine.UI.ScrollRect>(true);
            if (sr != null) sr.elasticity = elasticity;
            return this;
        }
    }

    public class UIBuilderContext
    {
        private readonly Transform _parent;
        private readonly UIElementRegistrySO _registry;

        public UIBuilderContext(Transform parent, UIElementRegistrySO registry)
        {
            _parent = parent;
            _registry = registry;
        }

        private GameObject InstantiateElement(UIElementType type, string styleName, string layoutHint = "")
        {
            if (_registry != null)
            {
                var entry = _registry.GetEntry(type, styleName);
                if (entry != null && entry.Prefab != null)
                {
                    var instance = UnityEngine.Object.Instantiate(entry.Prefab, _parent);
                    var prefabRect = entry.Prefab.GetComponent<RectTransform>();
                    if (prefabRect != null && prefabRect.sizeDelta.y > 0)
                    {
                        var le = instance.GetComponent<LayoutElement>();
                        if (le == null) le = instance.AddComponent<LayoutElement>();
                        if (le.preferredHeight < 0)
                        {
                            le.preferredHeight = prefabRect.sizeDelta.y;
                        }
                    }
                    return instance;
                }
            }

            // Fallback: Generate basic element via code
            return GenerateBasicElement(type, styleName, layoutHint);
        }

        private GameObject GenerateBasicElement(UIElementType type, string styleName, string layoutHint)
        {
            GameObject go = new GameObject($"[{type}] {styleName}");
            go.transform.SetParent(_parent, false);
            
            RectTransform rect = go.AddComponent<RectTransform>();
            
            switch (type)
            {
                case UIElementType.Container:
                    if (layoutHint == "Horizontal")
                    {
                        var group = go.AddComponent<HorizontalLayoutGroup>();
                        group.childControlWidth = true;
                        group.childControlHeight = true;
                        group.childForceExpandWidth = false;
                        group.childForceExpandHeight = false;
                        group.spacing = 8f;
                        group.padding = new RectOffset(5, 5, 5, 5);
                    }
                    else if (layoutHint == "Grid")
                    {
                        var group = go.AddComponent<GridLayoutGroup>();
                        group.cellSize = new Vector2(100, 100);
                        group.spacing = new Vector2(8, 8);
                        group.padding = new RectOffset(5, 5, 5, 5);
                        group.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                        group.constraintCount = 4;
                    }
                    else
                    {
                        var group = go.AddComponent<VerticalLayoutGroup>();
                        group.childControlWidth = true;
                        group.childControlHeight = true;
                        group.childForceExpandWidth = false;
                        group.childForceExpandHeight = false;
                        group.spacing = 8f;
                        group.padding = new RectOffset(5, 5, 5, 5);
                    }
                    var cs = go.AddComponent<UnityEngine.UI.ContentSizeFitter>();
                    if (layoutHint == "Grid")
                    {
                        cs.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
                        cs.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                    }
                    else
                    {
                        cs.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                        cs.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                    }
                    break;
                    
                case UIElementType.Text:
                    var txt = go.AddComponent<TextMeshProUGUI>();
                    if (_registry != null && _registry.DefaultFont != null) txt.font = _registry.DefaultFont;
                    txt.color = Color.black;
                    txt.fontSize = 18;
                    txt.enableAutoSizing = false;
                    txt.alignment = TextAlignmentOptions.MidlineLeft;
                    break;

                case UIElementType.Button:
                    var img = go.AddComponent<Image>();
                    img.color = new Color(0.2f, 0.2f, 0.25f, 1f); // Dark blue-ish gray
                    var btn = go.AddComponent<Button>();
                    
                    GameObject txtObj = new GameObject("Text");
                    txtObj.transform.SetParent(go.transform, false);
                    var bTxt = txtObj.AddComponent<TextMeshProUGUI>();
                    if (_registry != null && _registry.DefaultFont != null) bTxt.font = _registry.DefaultFont;
                    bTxt.color = Color.white; // Keep button text white for contrast against dark button background
                    bTxt.fontSize = 16;
                    bTxt.alignment = TextAlignmentOptions.Center;
                    
                    var bLayout = go.AddComponent<HorizontalLayoutGroup>();
                    bLayout.childControlWidth = true;
                    bLayout.childControlHeight = true;
                    bLayout.childForceExpandWidth = true;
                    bLayout.childForceExpandHeight = true;
                    bLayout.padding = new RectOffset(16, 16, 8, 8); // nice padding for button
                    break;

                case UIElementType.Image:
                    go.AddComponent<Image>();
                    break;

                case UIElementType.Toggle:
                    var tLayout = go.AddComponent<HorizontalLayoutGroup>();
                    tLayout.spacing = 10;
                    tLayout.childControlWidth = true;
                    tLayout.childControlHeight = true;
                    var toggle = go.AddComponent<Toggle>();
                    
                    GameObject bgObj = new GameObject("Background");
                    bgObj.transform.SetParent(go.transform, false);
                    var bgRect = bgObj.AddComponent<RectTransform>();
                    bgRect.sizeDelta = new Vector2(24, 24);
                    var bgImg = bgObj.AddComponent<Image>();
                    bgImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
                    
                    var bgLayout = bgObj.AddComponent<HorizontalLayoutGroup>();
                    bgLayout.childAlignment = TextAnchor.MiddleCenter;
                    
                    GameObject chkObj = new GameObject("Checkmark");
                    chkObj.transform.SetParent(bgObj.transform, false);
                    var chkRect = chkObj.AddComponent<RectTransform>();
                    chkRect.sizeDelta = new Vector2(16, 16);
                    var chkImg = chkObj.AddComponent<Image>();
                    chkImg.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Nice green
                    
                    toggle.targetGraphic = bgImg;
                    toggle.graphic = chkImg;
                    
                    GameObject lObj = new GameObject("Label");
                    lObj.transform.SetParent(go.transform, false);
                    var lTxt = lObj.AddComponent<TextMeshProUGUI>();
                    if (_registry != null && _registry.DefaultFont != null) lTxt.font = _registry.DefaultFont;
                    lTxt.color = Color.black;
                    lTxt.fontSize = 16;
                    lTxt.alignment = TextAlignmentOptions.MidlineLeft;
                    break;

                case UIElementType.InputField:
                    var inputImg = go.AddComponent<Image>();
                    inputImg.color = new Color(0.9f, 0.9f, 0.9f, 1f); // Light gray background for input field to contrast black text
                    var inputField = go.AddComponent<TMP_InputField>();
                    
                    var le = go.AddComponent<LayoutElement>();
                    le.preferredHeight = 40; // Force height in layout groups
                    
                    GameObject textArea = new GameObject("Text Area");
                    textArea.transform.SetParent(go.transform, false);
                    var tRect = textArea.AddComponent<RectTransform>();
                    tRect.anchorMin = Vector2.zero;
                    tRect.anchorMax = Vector2.one;
                    tRect.offsetMin = new Vector2(10, 5);
                    tRect.offsetMax = new Vector2(-10, -5);
                    textArea.AddComponent<RectMask2D>();
                    
                    GameObject tObj2 = new GameObject("Text");
                    tObj2.transform.SetParent(textArea.transform, false);
                    var txtRect = tObj2.AddComponent<RectTransform>();
                    txtRect.anchorMin = Vector2.zero;
                    txtRect.anchorMax = Vector2.one;
                    txtRect.offsetMin = Vector2.zero;
                    txtRect.offsetMax = Vector2.zero;
                    
                    var iTxt = tObj2.AddComponent<TextMeshProUGUI>();
                    if (_registry != null && _registry.DefaultFont != null) iTxt.font = _registry.DefaultFont;
                    iTxt.color = Color.black;
                    iTxt.fontSize = 16;
                    iTxt.alignment = TextAlignmentOptions.MidlineLeft;
                    
                    inputField.textViewport = tRect;
                    inputField.textComponent = iTxt;
                    break;
                    
                case UIElementType.Custom:
                    go.AddComponent<UICustomElement>();
                    break;

                case UIElementType.RawImage:
                    go.AddComponent<RawImage>();
                    break;

                case UIElementType.Scrollbar:
                    var sbImg = go.AddComponent<Image>();
                    sbImg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
                    var sb = go.AddComponent<Scrollbar>();
                    
                    GameObject slidingArea = new GameObject("Sliding Area");
                    slidingArea.transform.SetParent(go.transform, false);
                    var saRect = slidingArea.AddComponent<RectTransform>();
                    saRect.anchorMin = Vector2.zero;
                    saRect.anchorMax = Vector2.one;
                    saRect.sizeDelta = Vector2.zero;

                    GameObject handle = new GameObject("Handle");
                    handle.transform.SetParent(slidingArea.transform, false);
                    var hRect = handle.AddComponent<RectTransform>();
                    hRect.sizeDelta = new Vector2(20, 20);
                    var hImg = handle.AddComponent<Image>();
                    hImg.color = new Color(0.7f, 0.7f, 0.7f, 1f);

                    sb.handleRect = hRect;
                    sb.targetGraphic = hImg;
                    break;

                case UIElementType.ProgressBar:
                    var pbBg = go.AddComponent<Image>();
                    pbBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
                    var pbSlider = go.AddComponent<Slider>();
                    pbSlider.interactable = false;
                    pbSlider.transition = Selectable.Transition.None;

                    GameObject fillArea = new GameObject("Fill Area");
                    fillArea.transform.SetParent(go.transform, false);
                    var faRect = fillArea.AddComponent<RectTransform>();
                    faRect.anchorMin = Vector2.zero;
                    faRect.anchorMax = Vector2.one;
                    faRect.sizeDelta = Vector2.zero;

                    GameObject fill = new GameObject("Fill");
                    fill.transform.SetParent(fillArea.transform, false);
                    var fillRect = fill.AddComponent<RectTransform>();
                    fillRect.sizeDelta = Vector2.zero;
                    var fillImg = fill.AddComponent<Image>();
                    fillImg.color = new Color(0.2f, 0.7f, 0.9f, 1f);

                    pbSlider.fillRect = fillRect;
                    pbSlider.minValue = 0f;
                    pbSlider.maxValue = 1f;
                    pbSlider.value = 1f;
                    break;

                case UIElementType.Divider:
                    var divImg = go.AddComponent<Image>();
                    divImg.color = new Color(0.3f, 0.3f, 0.35f, 0.7f);
                    var divLe = go.AddComponent<LayoutElement>();
                    divLe.preferredHeight = 2f;
                    divLe.flexibleWidth = 1f;
                    break;

                case UIElementType.Mask:
                    go.AddComponent<RectMask2D>();
                    break;
            }
            
            return go;
        }

        private T GetTargetComponent<T>(GameObject instance, UIElementType type, string styleName) where T : Component
        {
            if (_registry != null)
            {
                var entry = _registry.GetEntry(type, styleName);
                if (entry != null && !string.IsNullOrEmpty(entry.TargetComponentPath))
                {
                    var targetObj = instance.transform.Find(entry.TargetComponentPath);
                    if (targetObj != null)
                    {
                        return targetObj.GetComponent<T>();
                    }
                }
            }
            
            // Fallback if path is empty, not found, or no registry entry (dynamically generated)
            return instance.GetComponentInChildren<T>(true);
        }

        // --- Container Elements (Div, Horizontal, Vertical) ---

        public UIElementRef Grid(string styleName, Action<UIBuilderContext> contentBuilder)
        {
            var go = InstantiateElement(UIElementType.Container, styleName, "Grid");
            if (contentBuilder != null) contentBuilder(new UIBuilderContext(go.transform, _registry));
            return new UIElementRef(go);
        }

        public UIElementRef Div(string styleName, Action<UIBuilderContext> builderAction)
        {
            var go = InstantiateElement(UIElementType.Container, styleName, "Vertical");
            if (go != null && builderAction != null)
            {
                var childContext = new UIBuilderContext(go.transform, _registry);
                builderAction(childContext);
            }
            return new UIElementRef(go);
        }

        public UIElementRef Horizontal(string styleName, Action<UIBuilderContext> builderAction)
        {
            var go = InstantiateElement(UIElementType.Container, styleName, "Horizontal");
            if (go != null && builderAction != null)
            {
                var childContext = new UIBuilderContext(go.transform, _registry);
                builderAction(childContext);
            }
            return new UIElementRef(go);
        }

        public UIElementRef Vertical(string styleName, Action<UIBuilderContext> builderAction)
        {
            var go = InstantiateElement(UIElementType.Container, styleName, "Vertical");
            if (go != null && builderAction != null)
            {
                var childContext = new UIBuilderContext(go.transform, _registry);
                builderAction(childContext);
            }
            return new UIElementRef(go);
        }

        public UIElementRef ScrollView(string styleName, Action<UIBuilderContext> contentBuilder, bool horizontal = true, bool vertical = false, UnityEngine.UI.ScrollRect.MovementType movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic)
        {
            if (_registry != null)
            {
                var entry = _registry.GetEntry(UIElementType.ScrollRect, styleName);
                if (entry != null && entry.Prefab != null)
                {
                    var instance = UnityEngine.Object.Instantiate(entry.Prefab, _parent);
                    UnityEngine.UI.ScrollRect sr = null;
                    if (!string.IsNullOrEmpty(entry.TargetComponentPath))
                    {
                        var targetObj = instance.transform.Find(entry.TargetComponentPath);
                        if (targetObj != null) sr = targetObj.GetComponent<UnityEngine.UI.ScrollRect>();
                    }
                    if (sr == null)
                    {
                        sr = instance.GetComponentInChildren<UnityEngine.UI.ScrollRect>(true);
                    }
                    if (sr != null)
                    {
                        sr.horizontal = horizontal;
                        sr.vertical = vertical;
                        sr.movementType = movementType;

                        Transform targetContainer = sr.content != null ? sr.content : sr.transform;
                        if (contentBuilder != null)
                        {
                            contentBuilder(new UIBuilderContext(targetContainer, _registry));
                        }
                    }
                    else if (contentBuilder != null)
                    {
                        contentBuilder(new UIBuilderContext(instance.transform, _registry));
                    }
                    return new UIElementRef(instance);
                }
            }

            GameObject root = new GameObject($"[ScrollView] {styleName}");
            root.transform.SetParent(_parent, false);

            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0, 0);
            rootRect.anchorMax = new Vector2(1, 1);
            rootRect.sizeDelta = Vector2.zero;

            var rootLe = root.AddComponent<LayoutElement>();
            rootLe.preferredHeight = 50f;
            rootLe.flexibleWidth = 1f;

            var rootImg = root.AddComponent<Image>();
            rootImg.color = Color.clear;
            rootImg.raycastTarget = true;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(root.transform, false);

            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.pivot = new Vector2(0, 1);

            viewport.AddComponent<RectMask2D>();

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRect = content.AddComponent<RectTransform>();

            if (horizontal && !vertical)
            {
                contentRect.anchorMin = new Vector2(0, 0);
                contentRect.anchorMax = new Vector2(0, 1);
                contentRect.pivot = new Vector2(0, 0.5f);

                var hLayout = content.AddComponent<HorizontalLayoutGroup>();
                hLayout.childControlWidth = true;
                hLayout.childControlHeight = true;
                hLayout.childForceExpandWidth = false;
                hLayout.childForceExpandHeight = false;
                hLayout.spacing = 8f;

                var cs = content.AddComponent<ContentSizeFitter>();
                cs.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                cs.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (!horizontal && vertical)
            {
                contentRect.anchorMin = new Vector2(0, 1);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.pivot = new Vector2(0.5f, 1);

                var vLayout = content.AddComponent<VerticalLayoutGroup>();
                vLayout.childControlWidth = true;
                vLayout.childControlHeight = true;
                vLayout.childForceExpandWidth = false;
                vLayout.childForceExpandHeight = false;
                vLayout.spacing = 8f;

                var cs = content.AddComponent<ContentSizeFitter>();
                cs.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                cs.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                contentRect.anchorMin = new Vector2(0, 1);
                contentRect.anchorMax = new Vector2(0, 1);
                contentRect.pivot = new Vector2(0, 1);

                var cs = content.AddComponent<ContentSizeFitter>();
                cs.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                cs.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            var scrollRect = root.AddComponent<UnityEngine.UI.ScrollRect>();
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = horizontal;
            scrollRect.vertical = vertical;
            scrollRect.movementType = movementType;
            scrollRect.elasticity = 0.1f;
            scrollRect.inertia = true;
            scrollRect.scrollSensitivity = 25f;

            if (contentBuilder != null)
            {
                var childContext = new UIBuilderContext(content.transform, _registry);
                contentBuilder(childContext);
            }

            return new UIElementRef(root);
        }

        public UIElementRef HorizontalScrollView(string styleName, Action<UIBuilderContext> contentBuilder, UnityEngine.UI.ScrollRect.MovementType movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic)
        {
            return ScrollView(styleName, contentBuilder, horizontal: true, vertical: false, movementType: movementType);
        }

        public UIElementRef ScrollRect(string styleName, Action<UIBuilderContext> contentBuilder, bool horizontal = true, bool vertical = false, UnityEngine.UI.ScrollRect.MovementType movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic)
        {
            return ScrollView(styleName, contentBuilder, horizontal, vertical, movementType);
        }

        // --- UI Elements ---

        public UIElementRef Text(string styleName, string content)
        {
            var go = InstantiateElement(UIElementType.Text, styleName);
            if (go != null)
            {
                var tmp = GetTargetComponent<TMP_Text>(go, UIElementType.Text, styleName);
                if (tmp != null)
                {
                    tmp.text = content;
                }
            }
            return new UIElementRef(go);
        }

        public UIElementRef Button(string styleName, string label, Action onClick)
        {
            var go = InstantiateElement(UIElementType.Button, styleName);
            if (go != null)
            {
                var btn = GetTargetComponent<Button>(go, UIElementType.Button, styleName);
                if (btn != null && onClick != null)
                {
                    btn.onClick.AddListener(() => onClick());
                }

                // If button prefab also has a text component for the label, try to set it
                var tmp = go.GetComponentInChildren<TMP_Text>(true);
                if (tmp != null)
                {
                    tmp.text = label;
                }
            }
            return new UIElementRef(go);
        }

        public UIElementRef Image(string styleName, Sprite sprite)
        {
            var go = InstantiateElement(UIElementType.Image, styleName);
            if (go != null)
            {
                var img = GetTargetComponent<Image>(go, UIElementType.Image, styleName);
                if (img != null && sprite != null)
                {
                    img.sprite = sprite;
                }
            }
            return new UIElementRef(go);
        }

        public UIElementRef Toggle(string styleName, bool isOn, string label, Action<bool> onValueChanged)
        {
            var go = InstantiateElement(UIElementType.Toggle, styleName);
            if (go != null)
            {
                var toggle = GetTargetComponent<Toggle>(go, UIElementType.Toggle, styleName);
                if (toggle != null)
                {
                    toggle.isOn = isOn;
                    if (onValueChanged != null)
                    {
                        toggle.onValueChanged.AddListener((val) => onValueChanged(val));
                    }
                }

                var tmp = go.GetComponentInChildren<TMP_Text>(true);
                if (tmp != null && !string.IsNullOrEmpty(label))
                {
                    tmp.text = label;
                }
            }
            return new UIElementRef(go);
        }

        public UIElementRef InputField(string styleName, string initialText, Action<string> onValueChanged)
        {
            var go = InstantiateElement(UIElementType.InputField, styleName);
            if (go != null)
            {
                var inputField = GetTargetComponent<TMP_InputField>(go, UIElementType.InputField, styleName);
                if (inputField != null)
                {
                    if (initialText != null)
                    {
                        inputField.text = initialText;
                    }
                    if (onValueChanged != null)
                    {
                        inputField.onValueChanged.AddListener((val) => onValueChanged(val));
                    }
                }
            }
            return new UIElementRef(go);
        }

        public UICustomElement Custom(string styleName)
        {
            var go = InstantiateElement(UIElementType.Custom, styleName);
            if (go != null)
            {
                var custom = go.GetComponent<UICustomElement>();
                if (custom == null)
                {
                    custom = go.AddComponent<UICustomElement>();
                }
                return custom;
            }
            return null;
        }

        public UIElementRef RawImage(string styleName, Texture texture = null)
        {
            var go = InstantiateElement(UIElementType.RawImage, styleName);
            if (go != null && texture != null)
            {
                var rawImg = GetTargetComponent<RawImage>(go, UIElementType.RawImage, styleName);
                if (rawImg != null) rawImg.texture = texture;
            }
            return new UIElementRef(go);
        }

        public UIElementRef ProgressBar(string styleName, float progress = 1f, Color? fillColor = null)
        {
            var go = InstantiateElement(UIElementType.ProgressBar, styleName);
            if (go != null)
            {
                var slider = GetTargetComponent<Slider>(go, UIElementType.ProgressBar, styleName);
                if (slider != null)
                {
                    slider.value = Mathf.Clamp01(progress);
                    if (fillColor.HasValue && slider.fillRect != null)
                    {
                        var img = slider.fillRect.GetComponent<Image>();
                        if (img != null) img.color = fillColor.Value;
                    }
                }
            }
            return new UIElementRef(go);
        }

        public UIElementRef Divider(string styleName, float thickness = 2f, Color? color = null)
        {
            var go = InstantiateElement(UIElementType.Divider, styleName);
            if (go != null)
            {
                var le = go.GetComponent<LayoutElement>();
                if (le == null) le = go.AddComponent<LayoutElement>();
                le.preferredHeight = thickness;
                le.flexibleWidth = 1f;

                if (color.HasValue)
                {
                    var img = go.GetComponent<Image>();
                    if (img != null) img.color = color.Value;
                }
            }
            return new UIElementRef(go);
        }
    }
}
