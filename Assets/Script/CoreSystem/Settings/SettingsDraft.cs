using System.Collections.Generic;
using System.Linq;
using CoreSystem.Settings.Keybinds;
using CoreSystem.Settings.Options;

namespace CoreSystem.Settings
{
    public sealed class SettingsDraft
    {
        #region [Variables/Fields]
        public const string KeybindsLabel = "Key Bindings";

        private Dictionary<string, object> _values = new Dictionary<string, object>();
        private Dictionary<string, KeybindPair> _keybinds = new Dictionary<string, KeybindPair>();
        #endregion

        #region [Properties]
        public IDictionary<string, KeybindPair> Keybinds => _keybinds;
        public int Count => _values.Count;
        #endregion

        #region [Public Methods]
        public static SettingsDraft Capture()
        {
            var draft = new SettingsDraft();
            foreach (var option in SettingsRegistry.Options) draft._values[option.Key] = option.Read();
            draft._keybinds = KeybindSetting.CaptureCurrent();
            return draft;
        }

        public SettingsDraft Clone()
        {
            return new SettingsDraft
            {
                _values = new Dictionary<string, object>(_values),
                _keybinds = new Dictionary<string, KeybindPair>(_keybinds)
            };
        }

        public object Get(string key)
        {
            if (_values.TryGetValue(key, out object value)) return value;
            var option = SettingsRegistry.Find(key);
            return option != null ? option.Read() : null;
        }

        public T Get<T>(string key, T fallback = default)
        {
            return Get(key) is T typed ? typed : fallback;
        }

        public void Set(string key, object value)
        {
            var option = SettingsRegistry.Find(key);
            if (option == null) return;
            _values[key] = option.Normalize(value);
        }

        public void ResetPage(string pageId)
        {
            var page = SettingsRegistry.FindPage(pageId);
            if (page != null && page.ViewId == SettingKeys.KeybindView) _keybinds = KeybindSetting.CaptureDefaults();
            foreach (var option in SettingsRegistry.GetOptions(pageId)) _values[option.Key] = option.Normalize(option.DefaultValue);
        }

        public List<string> Differences(SettingsDraft other)
        {
            var changed = new List<string>();
            if (other == null) return changed;
            foreach (var option in SettingsRegistry.Options)
            {
                if (option.ValuesEqual(Get(option.Key), other.Get(option.Key))) continue;
                changed.Add(DisplayLabel(option));
            }
            if (!SameKeybinds(_keybinds, other._keybinds)) changed.Add(KeybindsLabel);
            return changed;
        }

        public void Apply()
        {
            foreach (var option in SettingsRegistry.Options)
            {
                if (!_values.TryGetValue(option.Key, out object value)) continue;
                if (option.ValuesEqual(option.Read(), value)) continue;
                option.Write(value);
                SettingsEvents.Raise(option.Key);
            }
            if (!SameKeybinds(_keybinds, KeybindSetting.CaptureCurrent())) KeybindSetting.SetAll(_keybinds);
            SettingsEvents.RaiseApplied();
        }

        public static string DisplayLabel(SettingOption option)
        {
            if (!option.IsInline) return option.Label;
            var parent = SettingsRegistry.Find(option.InlineWithKey);
            return parent != null ? $"{parent.Label} {option.Label}" : option.Label;
        }
        #endregion

        #region [Private Methods]
        private static bool SameKeybinds(Dictionary<string, KeybindPair> a, Dictionary<string, KeybindPair> b)
        {
            if (a.Count != b.Count) return false;
            return a.All(pair => b.TryGetValue(pair.Key, out KeybindPair other) && other.Equals(pair.Value));
        }
        #endregion
    }
}
