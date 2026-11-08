using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace CoreSystem.Settings
{
    public static class SettingsStorage
    {
        #region [Variables/Fields]
        public const int Version = 1;
        public const string PrefsKey = "Settings.Store";

        private static Dictionary<string, string> _values;

        [Serializable]
        private class Entry
        {
            public string Key;
            public string Value;
        }

        [Serializable]
        private class Payload
        {
            public int Version;
            public List<Entry> Entries = new List<Entry>();
        }
        #endregion

        #region [Properties]
        public static int Count => Values.Count;
        public static int LoadedVersion { get; private set; }

        private static Dictionary<string, string> Values
        {
            get
            {
                if (_values == null) Load();
                return _values;
            }
        }
        #endregion

        #region [Public Methods]
        public static bool Has(string key) => !string.IsNullOrEmpty(key) && Values.ContainsKey(key);

        public static bool GetBool(string key, bool fallback) => Values.TryGetValue(key, out string raw) && bool.TryParse(raw, out bool value) ? value : fallback;

        public static float GetFloat(string key, float fallback) => Values.TryGetValue(key, out string raw) && float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ? value : fallback;

        public static int GetInt(string key, int fallback) => Values.TryGetValue(key, out string raw) && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : fallback;

        public static string GetString(string key, string fallback) => Values.TryGetValue(key, out string raw) ? raw : fallback;

        public static void SetBool(string key, bool value) => Set(key, value ? "True" : "False");

        public static void SetFloat(string key, float value) => Set(key, value.ToString("R", CultureInfo.InvariantCulture));

        public static void SetInt(string key, int value) => Set(key, value.ToString(CultureInfo.InvariantCulture));

        public static void SetString(string key, string value) => Set(key, value ?? string.Empty);

        public static void Remove(string key)
        {
            if (string.IsNullOrEmpty(key) || !Values.Remove(key)) return;
            Save();
        }

        public static void Save()
        {
            var payload = new Payload { Version = Version };
            foreach (var pair in Values) payload.Entries.Add(new Entry { Key = pair.Key, Value = pair.Value });
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(payload));
            PlayerPrefs.Save();
        }

        public static void Load()
        {
            _values = new Dictionary<string, string>();
            LoadedVersion = Version;
            string json = PlayerPrefs.GetString(PrefsKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return;
            Payload payload;
            try
            {
                payload = JsonUtility.FromJson<Payload>(json);
            }
            catch (ArgumentException)
            {
                Debug.LogWarning("[SettingsStorage] Stored settings could not be read; defaults are used.");
                return;
            }
            if (payload == null || payload.Entries == null) return;
            LoadedVersion = payload.Version;
            foreach (var entry in payload.Entries)
            {
                if (!string.IsNullOrEmpty(entry.Key)) _values[entry.Key] = entry.Value ?? string.Empty;
            }
            Migrate(payload.Version);
        }

        public static void ClearAll()
        {
            _values = new Dictionary<string, string>();
            PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.Save();
        }

        public static string Export()
        {
            Save();
            return PlayerPrefs.GetString(PrefsKey, string.Empty);
        }

        public static void Import(string json)
        {
            PlayerPrefs.SetString(PrefsKey, json ?? string.Empty);
            Load();
        }
        #endregion

        #region [Private Methods]
        private static void Set(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (Values.TryGetValue(key, out string current) && current == value) return;
            Values[key] = value;
            Save();
        }

        private static void Migrate(int fromVersion)
        {
            if (fromVersion == Version) return;
            LoadedVersion = Version;
            Save();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _values = null;
        }
        #endregion
    }
}
