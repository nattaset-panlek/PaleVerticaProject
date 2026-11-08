using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CoreSystem.Settings.Keybinds
{
    public static class KeybindSetting
    {
        #region [Variables/Fields]
        public const string StorageKeyPrefix = "keybind.";
        public const string ChangedEventKey = "keybind";
        public const string KeyboardPrefix = "<Keyboard>/";

        public static readonly List<string> GroupOrder = new List<string> { "Movement", "Combat", "Actions", "Hotbar", "Interface" };

        private static readonly List<KeybindDefinition> _definitions = new List<KeybindDefinition>();
        private static readonly List<InputActionAsset> _attached = new List<InputActionAsset>();
        private static bool _defaultsRegistered;

        public static event Action OnChanged;
        #endregion

        #region [Properties]
        public static IReadOnlyList<KeybindDefinition> Definitions
        {
            get
            {
                EnsureDefaults();
                return _definitions
                    .OrderBy(d => GroupIndex(d.Group))
                    .ThenBy(d => d.Order)
                    .ToList();
            }
        }
        #endregion

        #region [Public Methods]
        public static void AddDefinition(KeybindDefinition definition)
        {
            if (definition == null || string.IsNullOrEmpty(definition.Id)) return;
            _definitions.RemoveAll(d => d.Id == definition.Id);
            _definitions.Add(definition);
            if (!string.IsNullOrEmpty(definition.Group) && !GroupOrder.Contains(definition.Group)) GroupOrder.Add(definition.Group);
        }

        public static KeybindDefinition Find(string id)
        {
            EnsureDefaults();
            return _definitions.FirstOrDefault(d => d.Id == id);
        }

        public static KeybindPair GetPair(string id)
        {
            var definition = Find(id);
            KeybindPair fallback = definition != null ? definition.Default : default;
            return TryReadOverride(id, out KeybindPair stored) ? stored : fallback;
        }

        public static Dictionary<string, KeybindPair> CaptureCurrent()
        {
            var map = new Dictionary<string, KeybindPair>();
            foreach (var definition in Definitions) map[definition.Id] = GetPair(definition.Id);
            return map;
        }

        public static Dictionary<string, KeybindPair> CaptureDefaults()
        {
            var map = new Dictionary<string, KeybindPair>();
            foreach (var definition in Definitions) map[definition.Id] = definition.Default;
            return map;
        }

        public static void SetAll(IDictionary<string, KeybindPair> map)
        {
            if (map == null) return;
            foreach (var definition in Definitions)
            {
                if (!map.TryGetValue(definition.Id, out KeybindPair pair)) continue;
                string key = StorageKeyPrefix + definition.Id;
                if (pair.Equals(definition.Default)) SettingsStorage.Remove(key);
                else SettingsStorage.SetString(key, $"{(int)pair.Primary},{(int)pair.Secondary}");
            }
            ApplyToAttached();
            OnChanged?.Invoke();
            SettingsEvents.Raise(ChangedEventKey);
        }

        public static void ResetAll()
        {
            SetAll(CaptureDefaults());
        }

        public static bool Assign(IDictionary<string, KeybindPair> map, string id, int slot, Key key, out string conflictId, out int conflictSlot)
        {
            conflictId = null;
            conflictSlot = -1;
            if (map == null || !map.TryGetValue(id, out KeybindPair current)) return false;
            Key previous = current.Get(slot);
            if (key != Key.None)
            {
                foreach (var pair in map.ToList())
                {
                    for (int s = 0; s < KeybindPair.SlotCount; s++)
                    {
                        if (pair.Key == id && s == slot) continue;
                        if (pair.Value.Get(s) != key) continue;
                        conflictId = pair.Key;
                        conflictSlot = s;
                        map[pair.Key] = pair.Value.With(s, previous);
                    }
                }
            }
            map[id] = map[id].With(slot, key);
            return conflictId != null;
        }

        public static void Attach(InputActionAsset asset)
        {
            if (asset == null) return;
            _attached.RemoveAll(a => a == null);
            if (!_attached.Contains(asset)) _attached.Add(asset);
            ApplyTo(asset);
        }

        public static void Detach(InputActionAsset asset)
        {
            _attached.Remove(asset);
            _attached.RemoveAll(a => a == null);
        }

        public static void ApplyTo(InputActionAsset asset)
        {
            if (asset == null) return;
            foreach (var definition in Definitions)
            {
                if (!definition.IsInputAction) continue;
                var action = asset.FindAction($"{definition.ActionMap}/{definition.ActionName}");
                if (action == null) continue;
                List<int> indices = KeyboardBindingIndices(action, definition.PartName);
                KeybindPair pair = GetPair(definition.Id);
                for (int slot = 0; slot < indices.Count && slot < KeybindPair.SlotCount; slot++)
                {
                    int index = indices[slot];
                    Key key = pair.Get(slot);
                    if (key == definition.Default.Get(slot)) action.RemoveBindingOverride(index);
                    else action.ApplyBindingOverride(index, key == Key.None ? string.Empty : ToControlPath(key));
                }
            }
        }

        public static List<int> KeyboardBindingIndices(InputAction action, string part)
        {
            var indices = new List<int>();
            if (action == null) return indices;
            var bindings = action.bindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (binding.isComposite) continue;
                if (string.IsNullOrEmpty(binding.path) || !binding.path.StartsWith(KeyboardPrefix, StringComparison.OrdinalIgnoreCase)) continue;
                bool isPart = binding.isPartOfComposite;
                if (string.IsNullOrEmpty(part) ? isPart : !isPart || !string.Equals(binding.name, part, StringComparison.OrdinalIgnoreCase)) continue;
                indices.Add(i);
            }
            return indices;
        }

        public static string HotbarId(int slotIndex) => $"hotbar.slot.{slotIndex + 1}";

        public static string WindowId(string windowName) => $"window.{windowName}";

        public static Key GetHotbarKey(int slotIndex, int slot = 0) => GetPair(HotbarId(slotIndex)).Get(slot);

        public static bool TryGetWindowKeys(string windowName, out KeybindPair pair)
        {
            pair = default;
            if (Find(WindowId(windowName)) == null) return false;
            pair = GetPair(WindowId(windowName));
            return true;
        }

        public static string ToControlPath(Key key)
        {
            return KeyboardPrefix + ControlName(key);
        }

        public static string ControlName(Key key)
        {
            string name = key.ToString();
            if (name.StartsWith("Digit", StringComparison.Ordinal) && name.Length == 6) return name.Substring(5);
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }

        public static Key FromControlPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return Key.None;
            string name = path.StartsWith(KeyboardPrefix, StringComparison.OrdinalIgnoreCase) ? path.Substring(KeyboardPrefix.Length) : path;
            if (name.Length == 1 && char.IsDigit(name[0])) name = "Digit" + name;
            return Enum.TryParse(name, true, out Key key) ? key : Key.None;
        }

        public static string DisplayName(Key key)
        {
            switch (key)
            {
                case Key.None: return "-";
                case Key.LeftShift:
                case Key.RightShift: return "SHIFT";
                case Key.LeftCtrl:
                case Key.RightCtrl: return "CTRL";
                case Key.LeftAlt:
                case Key.RightAlt: return "ALT";
                case Key.UpArrow: return "UP";
                case Key.DownArrow: return "DOWN";
                case Key.LeftArrow: return "LEFT";
                case Key.RightArrow: return "RIGHT";
                case Key.Space: return "SPACE";
                case Key.Enter: return "ENTER";
                case Key.Escape: return "ESC";
                case Key.Backquote: return "`";
            }
            string name = key.ToString();
            if (name.StartsWith("Digit", StringComparison.Ordinal) && name.Length == 6) return name.Substring(5);
            if (name.StartsWith("Numpad", StringComparison.Ordinal)) return "NUM " + name.Substring(6).ToUpperInvariant();
            return name.ToUpperInvariant();
        }

        public static void EnsureDefaults()
        {
            if (_defaultsRegistered) return;
            _defaultsRegistered = true;
            KeybindCatalog.RegisterDefaults();
        }
        #endregion

        #region [Private Methods]
        private static bool TryReadOverride(string id, out KeybindPair pair)
        {
            pair = default;
            string raw = SettingsStorage.GetString(StorageKeyPrefix + id, null);
            if (string.IsNullOrEmpty(raw)) return false;
            string[] parts = raw.Split(',');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int primary) || !int.TryParse(parts[1], out int secondary)) return false;
            pair = new KeybindPair((Key)primary, (Key)secondary);
            return true;
        }

        private static void ApplyToAttached()
        {
            _attached.RemoveAll(a => a == null);
            foreach (var asset in _attached) ApplyTo(asset);
        }

        private static int GroupIndex(string group)
        {
            int index = GroupOrder.IndexOf(group ?? string.Empty);
            return index >= 0 ? index : GroupOrder.Count;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _definitions.Clear();
            _attached.Clear();
            _defaultsRegistered = false;
            OnChanged = null;
        }
        #endregion
    }
}
