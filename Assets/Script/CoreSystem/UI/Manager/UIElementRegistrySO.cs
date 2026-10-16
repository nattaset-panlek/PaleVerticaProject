using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.UI
{
    [CreateAssetMenu(fileName = "UIElementRegistry", menuName = "Realm of Hero/UI/Element Registry (UIBuilder)")]
    public class UIElementRegistrySO : ScriptableObject
    {
        #region [Variables/Fields]
        [SerializeField] private TMPro.TMP_FontAsset _defaultFont;
        [SerializeField] private List<UIElementEntry> _entries = new List<UIElementEntry>();
        [SerializeField] private UITheme _theme = new UITheme();
        [SerializeField] private UITheme _darkTheme = UITheme.CreateDark();
        #endregion

        #region [Properties]
        public TMPro.TMP_FontAsset DefaultFont => _defaultFont;
        public List<UIElementEntry> Entries => _entries;
        public UITheme Theme => _theme ?? UITheme.Default;
        public UITheme DarkTheme => _darkTheme ?? UITheme.DefaultDark;
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public UITheme GetTheme(UIThemeMode mode) => mode == UIThemeMode.Dark ? DarkTheme : Theme;

        public UIElementEntry GetEntry(UIElementType type, string styleName)
        {
            if (string.IsNullOrEmpty(styleName)) return null;

            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                if (entry.Type == type && string.Equals(entry.StyleName, styleName, StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }
            return null;
        }

        public GameObject GetPrefab(UIElementType type, string styleName)
        {
            var entry = GetEntry(type, styleName);
            return entry?.Prefab;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
