using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.UI
{
    public enum UIElementType
    {
        Container, // For Layouts (Horizontal, Vertical, Grid, Div)
        Text,
        Button,
        Image,
        InputField,
        Dropdown,
        Toggle,
        Slider,
        Custom
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
        #endregion

        #region [Properties]
        public TMPro.TMP_FontAsset DefaultFont => _defaultFont;
        public List<UIElementEntry> Entries => _entries;
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
