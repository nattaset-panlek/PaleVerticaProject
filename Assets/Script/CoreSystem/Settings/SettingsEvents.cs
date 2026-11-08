using System;
using UnityEngine;

namespace CoreSystem.Settings
{
    public static class SettingsEvents
    {
        #region [Variables/Fields]
        public static event Action<string> OnChanged;
        public static event Action OnApplied;
        #endregion

        #region [Public Methods]
        public static void Raise(string key)
        {
            if (!string.IsNullOrEmpty(key)) OnChanged?.Invoke(key);
        }

        public static void RaiseApplied()
        {
            OnApplied?.Invoke();
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OnChanged = null;
            OnApplied = null;
        }
        #endregion
    }
}
