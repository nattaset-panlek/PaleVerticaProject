using System;
using System.Collections.Generic;
using CoreSystem.Data;

namespace CoreSystem.Localization
{
    public static class LocalizationFormula
    {
        #region [Variables/Fields]
        public const string DefaultLanguage = "EN";
        public static readonly string[] Languages = { "EN", "TH" };
        #endregion

        #region [Public Methods]
        public static string Normalize(string language)
        {
            if (string.IsNullOrWhiteSpace(language)) return DefaultLanguage;
            string upper = language.Trim().ToUpperInvariant();
            return Array.IndexOf(Languages, upper) >= 0 ? upper : DefaultLanguage;
        }

        public static string Raw(LocalizedString text, string language)
        {
            if (text == null) return null;
            switch (Normalize(language))
            {
                case "TH": return text.TH;
                default: return text.EN;
            }
        }

        public static string Resolve(LocalizedString text, string language)
        {
            string value = Raw(text, language);
            if (!string.IsNullOrEmpty(value)) return value;
            value = Raw(text, DefaultLanguage);
            return string.IsNullOrEmpty(value) ? null : value;
        }

        public static List<string> MissingLanguages(LocalizedString text)
        {
            var missing = new List<string>();
            foreach (string language in Languages)
            {
                if (string.IsNullOrWhiteSpace(Raw(text, language))) missing.Add(language);
            }
            return missing;
        }

        public static bool IsComplete(LocalizedString text)
        {
            return MissingLanguages(text).Count == 0;
        }

        public static string Format(string template, object[] args)
        {
            if (string.IsNullOrEmpty(template) || args == null || args.Length == 0) return template;
            try
            {
                return string.Format(template, args);
            }
            catch (FormatException)
            {
                return template;
            }
        }
        #endregion
    }
}
