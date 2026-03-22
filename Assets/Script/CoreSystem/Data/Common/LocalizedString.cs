using System;
using CoreSystem.Settings;
using UnityEngine;

namespace CoreSystem.Data
{
    [Serializable]
    public class LocalizedString
    {
        public string EN;
        public string TH;

        public string this[string lang]
        {
            get
            {
                if (lang.ToUpper() == "TH") return TH;
                return EN;
            }
        }

        public override string ToString()
        {
            return this[GameSetting.Language];
        }

        // Implicit conversion to string allows using LocalizedString directly where a string is expected
        public static implicit operator string(LocalizedString localized)
        {
            if (localized == null) return string.Empty;
            return localized.ToString();
        }
    }
}
