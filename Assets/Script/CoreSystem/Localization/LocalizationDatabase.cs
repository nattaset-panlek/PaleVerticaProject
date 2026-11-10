using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Settings;
using UnityEngine;

namespace CoreSystem.Localization
{
    public static class LocalizationDatabase
    {
        #region [Variables/Fields]
        public const string ResourcePath = "Localization/Strings";
        public const string AssetPath = "Assets/Resources/Localization/Strings.json";

        private static Dictionary<string, LocalizedString> _entries;
        private static readonly HashSet<string> ReportedMissing = new HashSet<string>();
        #endregion

        #region [Properties]
        public static int Count => Entries.Count;
        public static string CurrentLanguage => LocalizationFormula.Normalize(GameSetting.Language);

        private static Dictionary<string, LocalizedString> Entries
        {
            get
            {
                if (_entries == null) Load();
                return _entries;
            }
        }
        #endregion

        #region [Public Methods]
        public static string Get(string key, params object[] args)
        {
            return LocalizationFormula.Format(GetOrDefault(key, key), args);
        }

        public static string GetOrDefault(string key, string fallback)
        {
            if (string.IsNullOrEmpty(key)) return fallback ?? string.Empty;
            if (Entries.TryGetValue(key, out LocalizedString text))
            {
                string value = LocalizationFormula.Resolve(text, CurrentLanguage);
                if (value != null) return value;
            }
            ReportMissing(key);
            return fallback ?? string.Empty;
        }

        public static bool Has(string key)
        {
            return !string.IsNullOrEmpty(key) && Entries.ContainsKey(key);
        }

        public static LocalizationTable Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new LocalizationTable();
            LocalizationTable table = JsonUtility.FromJson<LocalizationTable>(json);
            return table ?? new LocalizationTable();
        }

        public static void Reload()
        {
            _entries = null;
            ReportedMissing.Clear();
        }
        #endregion

        #region [Private Methods]
        private static void Load()
        {
            _entries = new Dictionary<string, LocalizedString>();
            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset == null)
            {
                Debug.LogWarning($"[Localization] No string table at Resources/{ResourcePath}.json");
                return;
            }
            foreach (LocalizationEntry entry in Parse(asset.text).Entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Key)) continue;
                _entries[entry.Key] = entry.Text;
            }
        }

        private static void ReportMissing(string key)
        {
            if (!ReportedMissing.Add(key)) return;
            Debug.LogWarning($"[Localization] Missing text for key '{key}' in {CurrentLanguage}");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Reload();
        }
        #endregion
    }
}
