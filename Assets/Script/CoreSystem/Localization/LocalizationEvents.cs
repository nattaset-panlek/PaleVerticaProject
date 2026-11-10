using System;
using UnityEngine;

namespace CoreSystem.Localization
{
    public static class LocalizationEvents
    {
        #region [Variables/Fields]
        public static event Action<string> OnLanguageChanged;
        #endregion

        #region [Public Methods]
        public static void RaiseLanguageChanged(string language)
        {
            OnLanguageChanged?.Invoke(language);
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OnLanguageChanged = null;
        }
        #endregion
    }
}
