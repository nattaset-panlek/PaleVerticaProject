using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.UI
{
    [CreateAssetMenu(fileName = "UIWindowRegistry", menuName = "Realm of Hero/UI/Window Registry")]
    public class UIWindowRegistrySO : ScriptableObject
    {
        #region [Variables/Fields]
        [SerializeField] private List<UIWindowEntry> _entries = new List<UIWindowEntry>();
        #endregion

        #region [Properties]
        public List<UIWindowEntry> Entries => _entries;
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public UIWindowEntry GetEntry(string windowName)
        {
            if (string.IsNullOrEmpty(windowName)) return null;
            return _entries.Find(e => string.Equals(e.WindowName, windowName, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasEntry(string windowName)
        {
            return GetEntry(windowName) != null;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
