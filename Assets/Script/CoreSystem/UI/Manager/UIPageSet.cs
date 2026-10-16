using System;
using System.Collections.Generic;

namespace CoreSystem.UI
{
    public sealed class UIPageSet
    {
        #region [Variables/Fields]
        private readonly List<string> _titles = new List<string>();
        private readonly List<Action<UIBuilderContext>> _builders = new List<Action<UIBuilderContext>>();
        private Action<UIBuilderContext> _header;
        #endregion

        #region [Properties]
        public int Count => _titles.Count;
        public Action<UIBuilderContext> Header => _header;
        public float NavWidth { get; set; } = 170f;
        #endregion

        #region [Public Methods]
        public UIPageSet Add(string title, Action<UIBuilderContext> build)
        {
            _titles.Add(title ?? string.Empty);
            _builders.Add(build);
            return this;
        }

        public UIPageSet WithHeader(Action<UIBuilderContext> header)
        {
            _header = header;
            return this;
        }

        public string GetTitle(int index) => index >= 0 && index < _titles.Count ? _titles[index] : string.Empty;

        public int IndexOf(string title) => _titles.IndexOf(title);

        public void BuildPage(int index, UIBuilderContext context)
        {
            if (index < 0 || index >= _builders.Count) return;
            _builders[index]?.Invoke(context);
        }
        #endregion
    }
}
