using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;
using CoreSystem.Settings;
using CoreSystem.Settings.Keybinds;
using CoreSystem.UI;

namespace CoreSystem.UI.SystemMenu
{
    public static class KeybindPageView
    {
        #region [Variables/Fields]
        public const string CapturePrompt = "PRESS A KEY";

        private static string _search = string.Empty;
        private static string _captureId;
        private static int _captureSlot = -1;
        private static SettingsWindow _captureWindow;
        private static IDisposable _captureListener;
        private static bool _restoreEscape;
        #endregion

        #region [Properties]
        public static bool IsCapturing => _captureId != null;
        public static string Search => _search;
        #endregion

        #region [Public Methods]
        public static void Build(UIBuilderContext ui, SettingsWindow window)
        {
            ui.Row(top =>
            {
                top.InputField(UIMiniHeaderStyleRegistry.Input, _search, value =>
                {
                    _search = value ?? string.Empty;
                    window.RefreshBody();
                }).Width(UIMiniHeaderStyleRegistry.KeybindSearchWidth);
                top.Spacer().Fill();
                Header(top, "PRIMARY");
                Header(top, "SECONDARY");
            }).Gap(8f).Height(32f).Fill();

            var definitions = KeybindSetting.Definitions.Where(Matches).ToList();
            if (definitions.Count == 0)
            {
                ui.Label("No key matches the search.").TextColor(UIMiniHeaderStyleRegistry.Sub).Fill();
                return;
            }

            string pageId = window.CurrentPage != null ? window.CurrentPage.Id : SettingKeys.PageControls;
            foreach (var group in definitions.GroupBy(d => d.Group ?? string.Empty))
            {
                window.GroupHeader(ui, pageId, string.IsNullOrEmpty(group.Key) ? "Other" : group.Key);
                if (!window.IsExpanded(pageId, string.IsNullOrEmpty(group.Key) ? "Other" : group.Key)) continue;
                ui.Column(body =>
                {
                    foreach (var definition in group) Row(body, window, definition);
                }).Padding(14, 0, 0, 4).Gap(2f).Fill();
            }
        }

        public static void StartCapture(SettingsWindow window, string id, int slot)
        {
            CancelCapture(window);
            _captureId = id;
            _captureSlot = slot;
            _captureWindow = window;
            if (window.Modal != null)
            {
                _restoreEscape = window.Modal.CloseOnEscape;
                window.Modal.CloseOnEscape = false;
            }
            window.SetNotice("Press a key. Esc cancels, Backspace clears.");
            window.RefreshBody();
            _captureListener = InputSystem.onAnyButtonPress.CallOnce(HandlePress);
        }

        public static void CancelCapture(SettingsWindow window)
        {
            if (_captureWindow != null && _captureWindow != window && window != null) return;
            _captureListener?.Dispose();
            _captureListener = null;
            if (_captureWindow != null && _captureWindow.Modal != null) _captureWindow.Modal.CloseOnEscape = _restoreEscape;
            _captureId = null;
            _captureSlot = -1;
            _captureWindow = null;
        }

        public static bool Assign(SettingsWindow window, string id, int slot, Key key)
        {
            if (window == null || window.Draft == null) return false;
            bool conflict = KeybindSetting.Assign(window.Draft.Keybinds, id, slot, key, out string otherId, out int otherSlot);
            if (conflict)
            {
                var other = KeybindSetting.Find(otherId);
                string otherLabel = other != null ? other.Label : otherId;
                window.SetNotice($"{KeybindSetting.DisplayName(key)} was used by {otherLabel}; the keys were swapped.");
            }
            else window.SetNotice(null);
            window.MarkChanged();
            return conflict;
        }

        public static void ClearSearch()
        {
            _search = string.Empty;
        }
        #endregion

        #region [Private Methods]
        private static bool Matches(KeybindDefinition definition)
        {
            if (string.IsNullOrEmpty(_search)) return true;
            return definition.Label.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0
                || (definition.Group ?? string.Empty).IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void Header(UIBuilderContext row, string text)
        {
            row.Label(text).FontSize(11f).TextColor(UIMiniHeaderStyleRegistry.Sub).TextAlign(TextAlignmentOptions.Center).Width(UIMiniHeaderStyleRegistry.KeyButtonSize.x);
        }

        private static void Row(UIBuilderContext ui, SettingsWindow window, KeybindDefinition definition)
        {
            ui.Row(row =>
            {
                row.Label(definition.Label.ToUpperInvariant()).TextColor(UIMiniHeaderStyleRegistry.Text).Fill();
                for (int slot = 0; slot < KeybindPair.SlotCount; slot++) KeyButton(row, window, definition, slot);
            }).Gap(8f).Height(UIMiniHeaderStyleRegistry.SettingsRowHeight).Align(TextAnchor.MiddleLeft).Fill();
        }

        private static void KeyButton(UIBuilderContext row, SettingsWindow window, KeybindDefinition definition, int slot)
        {
            bool capturing = _captureId == definition.Id && _captureSlot == slot;
            Key key = window.Draft.Keybinds.TryGetValue(definition.Id, out KeybindPair pair) ? pair.Get(slot) : Key.None;
            string label = capturing ? CapturePrompt : KeybindSetting.DisplayName(key);
            int captured = slot;
            row.Button(UIMiniHeaderStyleRegistry.KeyButton, label, () => StartCapture(window, definition.Id, captured))
               .TextColor(capturing ? UIMiniHeaderStyleRegistry.KeyCapturing : key == Key.None ? UIMiniHeaderStyleRegistry.Sub : Color.white)
               .Width(UIMiniHeaderStyleRegistry.KeyButtonSize.x);
        }

        private static void HandlePress(InputControl control)
        {
            var window = _captureWindow;
            string id = _captureId;
            int slot = _captureSlot;
            _captureListener = null;
            if (window == null || id == null)
            {
                CancelCapture(null);
                return;
            }

            Key key = control is KeyControl keyControl ? keyControl.keyCode : Key.None;
            CancelCapture(window);
            if (control is KeyControl)
            {
                if (key == Key.Escape) window.SetNotice(null);
                else if (key == Key.Backspace || key == Key.Delete) Assign(window, id, slot, Key.None);
                else Assign(window, id, slot, key);
            }
            else window.SetNotice(null);
            window.RefreshBody();
        }
        #endregion
    }
}
