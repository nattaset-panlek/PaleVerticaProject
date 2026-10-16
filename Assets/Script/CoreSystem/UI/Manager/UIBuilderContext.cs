using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI
{
    public class UIBuilderContext
    {
        #region [Variables/Fields]
        public const string DefaultContainerStyle = "Div";
        public const string DefaultGridStyle = "Grid";
        public const string DefaultTextStyle = "Text";
        public const string DefaultButtonStyle = "Button";
        public const string PrimaryButtonStyle = "Primary Button";
        public const string DefaultInputStyle = "InputField";
        public const string DefaultImageStyle = "Image";
        public const string DefaultToggleStyle = "Toggle";
        public const string DefaultScrollStyle = "ScrollRect";
        public const string TabsScrollStyle = "TabsScrollView";
        public const string DefaultDividerStyle = "Divider";
        public const string DefaultProgressStyle = "ProgressBar";

        private readonly Transform _parent;
        private readonly UIDocument _document;
        #endregion

        #region [Properties]
        public Transform Parent => _parent;
        public UIDocument Document => _document;
        public UITheme Theme => _document.Theme;
        private UIElementFactory Factory => _document.Factory;
        #endregion

        #region [Public Methods]
        public UIBuilderContext(Transform parent, UIDocument document)
        {
            _parent = parent;
            _document = document;
        }

        public UIBuilderContext(Transform parent, UIElementRegistrySO registry)
            : this(parent, new UIDocument(parent as RectTransform, registry))
        {
        }

        public UIElementRef Column(Action<UIBuilderContext> content) => Container(DefaultContainerStyle, UILayoutKind.Column, content);

        public UIElementRef Row(Action<UIBuilderContext> content) => Container(DefaultContainerStyle, UILayoutKind.Row, content);

        public UIElementRef Grid(Action<UIBuilderContext> content) => Container(DefaultGridStyle, UILayoutKind.Grid, content);

        public UIElementRef Panel(Action<UIBuilderContext> content)
        {
            return Column(content).Background(Theme.Panel).Padding(Theme.Padding).Gap(Theme.Gap);
        }

        public UIElementRef Div(string styleName, Action<UIBuilderContext> content) => Container(styleName, UILayoutKind.Column, content);

        public UIElementRef Vertical(string styleName, Action<UIBuilderContext> content) => Container(styleName, UILayoutKind.Column, content);

        public UIElementRef Horizontal(string styleName, Action<UIBuilderContext> content) => Container(styleName, UILayoutKind.Row, content);

        public UIElementRef Grid(string styleName, Action<UIBuilderContext> content) => Container(styleName, UILayoutKind.Grid, content);

        public UIElementRef Region(string id, Action<UIBuilderContext> content)
        {
            var region = Container(DefaultContainerStyle, UILayoutKind.Column, null);
            region.GameObject.name = $"[Region] {id}";
            _document.RegisterRegion(id, region.GameObject.transform, content);
            content?.Invoke(new UIBuilderContext(region.GameObject.transform, _document));
            return region;
        }

        public UIElementRef Scroll(Action<UIBuilderContext> content, float height = 240f)
        {
            return ScrollView(DefaultScrollStyle, content, horizontal: false, vertical: true, UnityEngine.UI.ScrollRect.MovementType.Clamped).Height(height);
        }

        public UIElementRef ScrollView(string styleName, Action<UIBuilderContext> content, bool horizontal = true, bool vertical = false, UnityEngine.UI.ScrollRect.MovementType movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic)
        {
            GameObject go = Factory.CreateScrollView(styleName, _parent, horizontal, vertical, movementType, out Transform contentRoot);
            ApplyFlow(go, true);
            content?.Invoke(new UIBuilderContext(contentRoot, _document));
            return new UIElementRef(go, _document);
        }

        public UIElementRef HorizontalScrollView(string styleName, Action<UIBuilderContext> content, UnityEngine.UI.ScrollRect.MovementType movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic)
        {
            return ScrollView(styleName, content, horizontal: true, vertical: false, movementType);
        }

        public UIElementRef ScrollRect(string styleName, Action<UIBuilderContext> content, bool horizontal = true, bool vertical = false, UnityEngine.UI.ScrollRect.MovementType movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic)
        {
            return ScrollView(styleName, content, horizontal, vertical, movementType);
        }

        public UIElementRef Text(string styleName, string content)
        {
            GameObject go = Factory.Create(UIElementType.Text, styleName, _parent);
            var text = Factory.GetTarget<TMP_Text>(go, UIElementType.Text, styleName);
            if (text != null) text.text = content;
            ApplyFlow(go, true);
            return new UIElementRef(go, _document);
        }

        public UIElementRef Label(string content) => Text(DefaultTextStyle, content);

        public UIElementRef Title(string content) => Label(content).FontSize(Theme.TitleSize).Bold();

        public UIElementRef Heading(string content) => Label(content).FontSize(Theme.HeadingSize).Bold();

        public UIElementRef Caption(string content) => Label(content).FontSize(Theme.CaptionSize).TextColor(Theme.TextMuted);

        public UIElementRef KeyValue(string key, string value, float keyWidth = 110f)
        {
            return Row(row =>
            {
                row.Caption(key).Width(keyWidth);
                row.Label(value).Fill();
            }).Align(TextAnchor.UpperLeft);
        }

        public UIElementRef Badge(string content, Color? background = null)
        {
            Color fill = background ?? Theme.PanelAlt;
            Color ink = background.HasValue ? Theme.TextOnAccent : Theme.Text;
            return Row(row => row.Label(content).FontSize(Theme.CaptionSize).Bold().TextColor(ink).Fill(0f))
                .Background(fill)
                .Padding(6, 6, 2, 2)
                .Align(TextAnchor.MiddleCenter)
                .Fill(0f);
        }

        public UIElementRef Button(string styleName, string label, Action onClick)
        {
            GameObject go = Factory.Create(UIElementType.Button, styleName, _parent);
            var button = Factory.GetTarget<Button>(go, UIElementType.Button, styleName);
            if (button != null && onClick != null) button.onClick.AddListener(() => onClick());

            var text = go.GetComponentInChildren<TMP_Text>(true);
            if (text != null) text.text = label;
            Factory.FitControl(go, text);
            if (text != null && UIElementFactory.GetLayoutKind(_parent) == UILayoutKind.Grid) text.overflowMode = TextOverflowModes.Ellipsis;
            ApplyFlow(go, false);
            return new UIElementRef(go, _document);
        }

        public UIElementRef Button(string label, Action onClick, UIVariant variant = UIVariant.Default)
        {
            string style = variant == UIVariant.Primary ? PrimaryButtonStyle : DefaultButtonStyle;
            return Button(style, label, onClick).Variant(variant);
        }

        public UIElementRef Image(string styleName, Sprite sprite)
        {
            GameObject go = Factory.Create(UIElementType.Image, styleName, _parent);
            var image = Factory.GetTarget<Image>(go, UIElementType.Image, styleName);
            if (image != null && sprite != null) image.sprite = sprite;
            ApplyFlow(go, false);
            return new UIElementRef(go, _document);
        }

        public UIElementRef Image(Sprite sprite) => Image(DefaultImageStyle, sprite);

        public UIElementRef Toggle(string styleName, bool isOn, string label, Action<bool> onValueChanged)
        {
            GameObject go = Factory.Create(UIElementType.Toggle, styleName, _parent);
            var toggle = Factory.GetTarget<Toggle>(go, UIElementType.Toggle, styleName);
            if (toggle != null)
            {
                toggle.isOn = isOn;
                if (onValueChanged != null) toggle.onValueChanged.AddListener(value => onValueChanged(value));
            }

            var text = go.GetComponentInChildren<TMP_Text>(true);
            if (text != null && !string.IsNullOrEmpty(label)) text.text = label;
            ApplyFlow(go, false);
            return new UIElementRef(go, _document);
        }

        public UIElementRef Toggle(string label, bool isOn, Action<bool> onValueChanged) => Toggle(DefaultToggleStyle, isOn, label, onValueChanged);

        public UIElementRef InputField(string styleName, string initialText, Action<string> onValueChanged)
        {
            GameObject go = Factory.Create(UIElementType.InputField, styleName, _parent);
            var input = Factory.GetTarget<TMP_InputField>(go, UIElementType.InputField, styleName);
            if (input != null)
            {
                if (initialText != null) input.SetTextWithoutNotify(initialText);
                if (onValueChanged != null) input.onValueChanged.AddListener(value => onValueChanged(value));
            }
            Factory.FitInput(go);
            ApplyFlow(go, false);
            return new UIElementRef(go, _document);
        }

        public UIElementRef Input(string initialText, Action<string> onValueChanged) => InputField(DefaultInputStyle, initialText, onValueChanged);

        public UICustomElement Custom(string styleName)
        {
            GameObject go = Factory.Create(UIElementType.Custom, styleName, _parent);
            var custom = go.GetComponent<UICustomElement>();
            return custom != null ? custom : go.AddComponent<UICustomElement>();
        }

        public UIElementRef RawImage(string styleName, Texture texture = null)
        {
            GameObject go = Factory.Create(UIElementType.RawImage, styleName, _parent);
            var raw = Factory.GetTarget<RawImage>(go, UIElementType.RawImage, styleName);
            if (raw != null && texture != null) raw.texture = texture;
            ApplyFlow(go, false);
            return new UIElementRef(go, _document);
        }

        public UIElementRef ProgressBar(string styleName, float progress = 1f, Color? fillColor = null)
        {
            GameObject go = Factory.Create(UIElementType.ProgressBar, styleName, _parent);
            var slider = Factory.GetTarget<Slider>(go, UIElementType.ProgressBar, styleName);
            if (slider != null)
            {
                slider.value = Mathf.Clamp01(progress);
                if (fillColor.HasValue && slider.fillRect != null)
                {
                    var fill = slider.fillRect.GetComponent<Image>();
                    if (fill != null) fill.color = fillColor.Value;
                }
            }
            ApplyFlow(go, true);
            return new UIElementRef(go, _document);
        }

        public UIElementRef Progress(float value, UIVariant variant = UIVariant.Primary)
        {
            return ProgressBar(DefaultProgressStyle, value, Theme.GetVariantColor(variant));
        }

        public UIElementRef Divider(string styleName, float thickness = 2f, Color? color = null)
        {
            GameObject go = Factory.Create(UIElementType.Divider, styleName, _parent);
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            le.preferredHeight = thickness;
            le.minHeight = thickness;
            le.flexibleWidth = 1f;

            if (color.HasValue)
            {
                var image = go.GetComponent<Image>();
                if (image != null) image.color = color.Value;
            }
            return new UIElementRef(go, _document);
        }

        public UIElementRef Separator() => Divider(DefaultDividerStyle, 1f, Theme.Divider);

        public UIElementRef Spacer(float size = -1f)
        {
            GameObject go = new GameObject("[Spacer]", typeof(RectTransform));
            go.transform.SetParent(_parent, false);
            var le = go.AddComponent<LayoutElement>();
            bool row = UIElementFactory.GetLayoutKind(_parent) == UILayoutKind.Row;

            if (size < 0f)
            {
                if (row) le.flexibleWidth = 1f;
                else le.flexibleHeight = 1f;
            }
            else if (row)
            {
                le.preferredWidth = size;
                le.minWidth = size;
            }
            else
            {
                le.preferredHeight = size;
                le.minHeight = size;
            }
            return new UIElementRef(go, _document);
        }

        public UIElementRef Foldout(string id, string title, Action<UIBuilderContext> content, bool expanded = false)
        {
            string stateKey = id + ".open";
            return Region(id, section =>
            {
                bool isOpen = section.Document.GetState(stateKey, expanded);
                section.Button($"{(isOpen ? "[-]" : "[+]")}  {title}", () =>
                {
                    section.Document.SetState(stateKey, !isOpen);
                    section.Document.Refresh(id);
                }, UIVariant.Secondary).TextAlign(TextAlignmentOptions.MidlineLeft).Fill();

                if (isOpen) content?.Invoke(section);
            });
        }

        public UIElementRef Pages(string id, Action<UIPageSet> define, UIPageNav nav = UIPageNav.Tabs)
        {
            var pages = new UIPageSet();
            define?.Invoke(pages);
            _document.RegisterPages(id, pages);

            string navId = UIDocument.NavId(id);
            string contentId = UIDocument.ContentId(id);

            if (nav == UIPageNav.SideMenu)
            {
                return Row(row =>
                {
                    row.Region(navId, menu => BuildNavigation(menu, id, pages, vertical: true))
                       .Width(pages.NavWidth)
                       .Gap(4f);

                    row.Column(main =>
                    {
                        pages.Header?.Invoke(main);
                        main.Region(contentId, page => pages.BuildPage(CurrentPage(page.Document, id, pages), page));
                    }).Fill().Gap(Theme.Gap);
                }).Align(TextAnchor.UpperLeft).Gap(Theme.Padding);
            }

            return Column(column =>
            {
                column.Region(navId, bar => bar.ScrollView(TabsScrollStyle, tabs => BuildNavigation(tabs, id, pages, vertical: false), horizontal: true, vertical: false)
                    .Id(id + ".tabs")
                    .Height(Theme.ControlHeight + 10f));
                pages.Header?.Invoke(column);
                column.Region(contentId, page => pages.BuildPage(CurrentPage(page.Document, id, pages), page));
            }).Gap(Theme.Gap);
        }
        #endregion

        #region [Private Methods]
        private UIElementRef Container(string styleName, UILayoutKind layout, Action<UIBuilderContext> content)
        {
            GameObject go = Factory.Create(UIElementType.Container, styleName, _parent, layout);
            ApplyFlow(go, true);
            content?.Invoke(new UIBuilderContext(go.transform, _document));
            return new UIElementRef(go, _document);
        }

        private void ApplyFlow(GameObject go, bool isBlock)
        {
            if (go == null || !isBlock) return;
            if (!UIElementFactory.HasLayoutGroup(_parent) || UIElementFactory.GetLayoutKind(_parent) != UILayoutKind.Column) return;

            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            if (le.flexibleWidth < 0f) le.flexibleWidth = 1f;
        }

        private static int CurrentPage(UIDocument document, string id, UIPageSet pages)
        {
            return Mathf.Clamp(document.GetPage(id), 0, Mathf.Max(0, pages.Count - 1));
        }

        private static void BuildNavigation(UIBuilderContext nav, string id, UIPageSet pages, bool vertical)
        {
            int selected = CurrentPage(nav.Document, id, pages);
            for (int i = 0; i < pages.Count; i++)
            {
                int index = i;
                UIVariant variant = index == selected ? UIVariant.Selected : (vertical ? UIVariant.Ghost : UIVariant.Secondary);
                var button = nav.Button(pages.GetTitle(i), () => nav.Document.ShowPage(id, index), variant);
                if (vertical) button.Fill().TextAlign(TextAlignmentOptions.MidlineLeft);
                else button.MinWidth(96f);
            }
        }
        #endregion
    }
}
