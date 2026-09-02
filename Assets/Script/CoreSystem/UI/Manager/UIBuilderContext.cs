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

        public UIElementRef SetFlexibleSize(float flexWidth = -1, float flexHeight = -1)
        {
            if (GameObject == null) return this;
            var le = GameObject.GetComponent<LayoutElement>();
            if (le == null) le = GameObject.AddComponent<LayoutElement>();
            if (flexWidth >= 0) le.flexibleWidth = flexWidth;
            if (flexHeight >= 0) le.flexibleHeight = flexHeight;
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
            var group = GameObject.GetComponent<LayoutGroup>();
            if (group != null) group.padding = new RectOffset(left, right, top, bottom);
            return this;
        }

        public UIElementRef SetSpacing(float spacing)
        {
            if (GameObject == null) return this;
            var group = GameObject.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (group != null) group.spacing = spacing;
            return this;
        }

        public UIElementRef SetChildAlignment(TextAnchor anchor)
        {
            if (GameObject == null) return this;
            var group = GameObject.GetComponent<LayoutGroup>();
            if (group != null) group.childAlignment = anchor;
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
                    return UnityEngine.Object.Instantiate(entry.Prefab, _parent);
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
                    cs.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                    cs.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
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
    }
}
