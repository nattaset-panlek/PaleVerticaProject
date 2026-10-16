using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.UI
{
    public enum UIElementType
    {
        Container = 0,
        Text = 1,
        Button = 2,
        Image = 3,
        InputField = 4,
        Dropdown = 5,
        Toggle = 6,
        Slider = 7,
        Custom = 8,
        ScrollRect = 9,
        Scrollbar = 10,
        RawImage = 11,
        ProgressBar = 12,
        Divider = 13,
        Mask = 14
    }

    [Serializable]
    public class UIElementEntry
    {
        public string StyleName;
        public UIElementType Type;
        public GameObject Prefab;
        
        [Tooltip("The path to the child object holding the actual component, used by UIBuilder to inject data/events.")]
        public string TargetComponentPath;
    }

    [CreateAssetMenu(fileName = "UIElementRegistry", menuName = "Realm of Hero/UI/Element Registry (UIBuilder)")]
    public class UIElementRegistrySO : ScriptableObject
    {
        #region [Variables/Fields]
        [SerializeField] private TMPro.TMP_FontAsset _defaultFont;
        [SerializeField] private List<UIElementEntry> _entries = new List<UIElementEntry>();
        [SerializeField] private UITheme _theme = new UITheme();
        #endregion

        #region [Properties]
        public TMPro.TMP_FontAsset DefaultFont => _defaultFont;
        public List<UIElementEntry> Entries => _entries;
        public UITheme Theme => _theme ?? UITheme.Default;
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
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
