using System;
using CoreSystem.Localization;
using CoreSystem.Settings;

namespace CoreSystem.Data
{
    [Serializable]
    public class LocalizedString
    {
        #region [Variables/Fields]
        public string EN;
        public string TH;
        #endregion

        #region [Properties]
        public string this[string lang] => LocalizationFormula.Resolve(this, lang) ?? string.Empty;
        public bool IsComplete => LocalizationFormula.IsComplete(this);
        #endregion

        #region [Public Methods]
        public override string ToString()
        {
            return this[GameSetting.Language];
        }

        public static implicit operator string(LocalizedString localized)
        {
            if (localized == null) return string.Empty;
            return localized.ToString();
        }
        #endregion
    }
}
