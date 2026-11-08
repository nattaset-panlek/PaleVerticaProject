using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CoreSystem.Settings.Keybinds
{
    public static class KeybindCatalog
    {
        #region [Variables/Fields]
        public const string PlayerMap = "Player";
        public const int HotbarSlots = 10;

        public static readonly Dictionary<string, string> ActionGroups = new Dictionary<string, string>
        {
            { "Move", "Movement" },
            { "Sprint", "Movement" },
            { "Jump", "Movement" },
            { "Crouch", "Movement" },
            { "Attack", "Combat" },
            { "Interact", "Combat" },
            { "Previous", "Actions" },
            { "Next", "Actions" }
        };

        public static readonly HashSet<string> SkippedActions = new HashSet<string> { "Look" };
        #endregion

        #region [Public Methods]
        public static void RegisterDefaults()
        {
            RegisterInputActions();
            RegisterHotbar();
        }

        public static void RegisterInputActions()
        {
            InputSystem_Actions template = null;
            try
            {
                template = new InputSystem_Actions();
                RegisterFrom(template.asset, PlayerMap);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[KeybindCatalog] Could not read InputSystem_Actions: {e.Message}");
            }
            finally
            {
                if (template != null)
                {
                    if (Application.isPlaying) template.Dispose();
                    else UnityEngine.Object.DestroyImmediate(template.asset);
                }
            }
        }

        public static void RegisterFrom(InputActionAsset asset, string mapName)
        {
            var map = asset != null ? asset.FindActionMap(mapName) : null;
            if (map == null) return;
            int order = 0;
            foreach (var action in map.actions)
            {
                if (SkippedActions.Contains(action.name)) continue;
                var parts = new List<string>();
                bool hasPlain = false;
                foreach (var binding in action.bindings)
                {
                    if (binding.isComposite || string.IsNullOrEmpty(binding.path) || !binding.path.StartsWith(KeybindSetting.KeyboardPrefix, StringComparison.OrdinalIgnoreCase)) continue;
                    if (binding.isPartOfComposite)
                    {
                        if (!parts.Contains(binding.name)) parts.Add(binding.name);
                    }
                    else hasPlain = true;
                }

                string group = ActionGroups.TryGetValue(action.name, out string g) ? g : "Actions";
                if (hasPlain) Register(action, null, group, order++);
                foreach (string part in parts) Register(action, part, group, order++);
            }
        }

        public static void RegisterHotbar()
        {
            for (int i = 0; i < HotbarSlots; i++)
            {
                Key key = i < 9 ? (Key)((int)Key.Digit1 + i) : Key.Digit0;
                KeybindSetting.AddDefinition(new KeybindDefinition(KeybindSetting.HotbarId(i), $"Hotbar Slot {i + 1}", "Hotbar", i, new KeybindPair(key, Key.None)));
            }
        }

        public static string Pretty(string action, string part)
        {
            string label = SplitWords(action);
            return string.IsNullOrEmpty(part) ? label : $"{label} {SplitWords(part)}";
        }
        #endregion

        #region [Private Methods]
        private static void Register(InputAction action, string part, string group, int order)
        {
            List<int> indices = KeybindSetting.KeyboardBindingIndices(action, part);
            var bindings = action.bindings;
            Key primary = indices.Count > 0 ? KeybindSetting.FromControlPath(bindings[indices[0]].path) : Key.None;
            Key secondary = indices.Count > 1 ? KeybindSetting.FromControlPath(bindings[indices[1]].path) : Key.None;
            KeybindSetting.AddDefinition(KeybindDefinition.ForInputAction(action.actionMap.name, action.name, part, Pretty(action.name, part), group, order, new KeybindPair(primary, secondary)));
        }

        private static string SplitWords(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var sb = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (i == 0) sb.Append(char.ToUpperInvariant(c));
                else if (char.IsUpper(c) && !char.IsUpper(value[i - 1])) sb.Append(' ').Append(c);
                else sb.Append(c);
            }
            return sb.ToString();
        }
        #endregion
    }
}
