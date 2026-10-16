using System;
using System.Collections.Generic;

namespace CoreSystem.UI
{
    public sealed class UIPageSet
    {
        #region [Variables/Fields]
        private readonly List<string> _titles = new List<string>();
        private readonly List<Action<UIBuilderContext>> _builders = new List<Action<UIBuilderContext>>();
        private readonly List<bool?> _scrolls = new List<bool?>();
        private Action<UIBuilderContext> _header;
        private Action<UIElementRef, bool> _navStyle;
        #endregion

        #region [Properties]
        public int Count => _titles.Count;
        public Action<UIBuilderContext> Header => _header;
        public float NavWidth { get; set; } = 170f;
        public bool ScrollPages { get; set; }
        public string NavButtonStyle { get; set; }
        #endregion

        #region [Public Methods]
        public UIPageSet Add(string title, Action<UIBuilderContext> build)
        {
            return AddPage(title, build, null);
        }

        public UIPageSet Add(string title, Action<UIBuilderContext> build, bool scroll)
        {
            return AddPage(title, build, scroll);
        }

        public bool ScrollsPage(int index)
        {
            return index >= 0 && index < _scrolls.Count && _scrolls[index].HasValue ? _scrolls[index].Value : ScrollPages;
        }

        public UIPageSet WithHeader(Action<UIBuilderContext> header)
        {
            _header = header;
            return this;
        }

        public UIPageSet WithNavStyle(Action<UIElementRef> style)
        {
            _navStyle = style != null ? (button, selected) => style(button) : (Action<UIElementRef, bool>)null;
            return this;
        }

        public UIPageSet WithNavStyle(Action<UIElementRef, bool> style)
        {
            _navStyle = style;
            return this;
        }

        public void StyleNavButton(UIElementRef button, bool selected)
        {
            if (button.IsValid) _navStyle?.Invoke(button, selected);
        }

        public string GetTitle(int index) => index >= 0 && index < _titles.Count ? _titles[index] : string.Empty;

        public int IndexOf(string title) => _titles.IndexOf(title);

        public void BuildPage(int index, UIBuilderContext context)
        {
            if (index < 0 || index >= _builders.Count) return;
            _builders[index]?.Invoke(context);
        }
        #endregion

        #region [Private Methods]
        private UIPageSet AddPage(string title, Action<UIBuilderContext> build, bool? scroll)
        {
            _titles.Add(title ?? string.Empty);
            _builders.Add(build);
            _scrolls.Add(scroll);
            return this;
        }
        #endregion
    }
}
