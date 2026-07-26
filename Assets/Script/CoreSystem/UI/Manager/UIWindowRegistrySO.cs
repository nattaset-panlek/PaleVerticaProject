using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.UI
{
    public enum UIType
    {
        Window,
        Modal
    }

    [Serializable]
    public class UIWindowEntry
    {
        public string WindowName;
        public UIType Type;
        public GameObject Prefab;
        public bool DestroyOnClose = false;
        public List<KeyCode> Keys = new List<KeyCode>();

        public bool IsKeyPressed()
        {
            if (Keys == null || Keys.Count == 0) return false;
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return false;

            bool requiresAlt = false;
            bool requiresCtrl = false;
            bool requiresShift = false;

            for (int i = 0; i < Keys.Count; i++)
            {
                KeyCode k = Keys[i];
                if (k == KeyCode.LeftAlt || k == KeyCode.RightAlt) requiresAlt = true;
                if (k == KeyCode.LeftControl || k == KeyCode.RightControl) requiresCtrl = true;
                if (k == KeyCode.LeftShift || k == KeyCode.RightShift) requiresShift = true;
            }

            bool isAltPressed = keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed;
            bool isCtrlPressed = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
            bool isShiftPressed = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;

            if (requiresAlt != isAltPressed) return false;
            if (requiresCtrl != isCtrlPressed) return false;
            if (requiresShift != isShiftPressed) return false;

            for (int i = 0; i < Keys.Count; i++)
            {
                var key = ToInputSystemKey(Keys[i]);
                if (key == UnityEngine.InputSystem.Key.None || !keyboard[key].isPressed) return false;
            }

            for (int i = 0; i < Keys.Count; i++)
            {
                var key = ToInputSystemKey(Keys[i]);
                if (key != UnityEngine.InputSystem.Key.None && keyboard[key].wasPressedThisFrame) return true;
            }

            return false;
        }

        private static UnityEngine.InputSystem.Key ToInputSystemKey(KeyCode keyCode)
        {
            if (keyCode >= KeyCode.A && keyCode <= KeyCode.Z)
                return (UnityEngine.InputSystem.Key)((int)UnityEngine.InputSystem.Key.A + (keyCode - KeyCode.A));
            if (keyCode >= KeyCode.Alpha0 && keyCode <= KeyCode.Alpha9)
                return (UnityEngine.InputSystem.Key)((int)UnityEngine.InputSystem.Key.Digit0 + (keyCode - KeyCode.Alpha0));
            if (keyCode >= KeyCode.Keypad0 && keyCode <= KeyCode.Keypad9)
                return (UnityEngine.InputSystem.Key)((int)UnityEngine.InputSystem.Key.Numpad0 + (keyCode - KeyCode.Keypad0));
            if (keyCode >= KeyCode.F1 && keyCode <= KeyCode.F12)
                return (UnityEngine.InputSystem.Key)((int)UnityEngine.InputSystem.Key.F1 + (keyCode - KeyCode.F1));

            switch (keyCode)
            {
                case KeyCode.Escape: return UnityEngine.InputSystem.Key.Escape;
                case KeyCode.Space: return UnityEngine.InputSystem.Key.Space;
                case KeyCode.Return: return UnityEngine.InputSystem.Key.Enter;
                case KeyCode.Tab: return UnityEngine.InputSystem.Key.Tab;
                case KeyCode.LeftShift: return UnityEngine.InputSystem.Key.LeftShift;
                case KeyCode.RightShift: return UnityEngine.InputSystem.Key.RightShift;
                case KeyCode.LeftControl: return UnityEngine.InputSystem.Key.LeftCtrl;
                case KeyCode.RightControl: return UnityEngine.InputSystem.Key.RightCtrl;
                case KeyCode.LeftAlt: return UnityEngine.InputSystem.Key.LeftAlt;
                case KeyCode.RightAlt: return UnityEngine.InputSystem.Key.RightAlt;
                case KeyCode.CapsLock: return UnityEngine.InputSystem.Key.CapsLock;
                case KeyCode.Backspace: return UnityEngine.InputSystem.Key.Backspace;
                case KeyCode.Delete: return UnityEngine.InputSystem.Key.Delete;
                case KeyCode.Insert: return UnityEngine.InputSystem.Key.Insert;
                case KeyCode.Home: return UnityEngine.InputSystem.Key.Home;
                case KeyCode.End: return UnityEngine.InputSystem.Key.End;
                case KeyCode.PageUp: return UnityEngine.InputSystem.Key.PageUp;
                case KeyCode.PageDown: return UnityEngine.InputSystem.Key.PageDown;
                case KeyCode.UpArrow: return UnityEngine.InputSystem.Key.UpArrow;
                case KeyCode.DownArrow: return UnityEngine.InputSystem.Key.DownArrow;
                case KeyCode.LeftArrow: return UnityEngine.InputSystem.Key.LeftArrow;
                case KeyCode.RightArrow: return UnityEngine.InputSystem.Key.RightArrow;
                default: return UnityEngine.InputSystem.Key.None;
            }
        }
    }

    [CreateAssetMenu(fileName = "UIWindowRegistry", menuName = "Realm of Hero/UI/Window Registry")]
    public class UIWindowRegistrySO : ScriptableObject
    {
        #region [Variables/Fields]
        [SerializeField] private List<UIWindowEntry> _entries = new List<UIWindowEntry>();
        #endregion

        #region [Properties]
        public List<UIWindowEntry> Entries => _entries;
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public UIWindowEntry GetEntry(string windowName)
        {
            if (string.IsNullOrEmpty(windowName)) return null;
            return _entries.Find(e => string.Equals(e.WindowName, windowName, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasEntry(string windowName)
        {
            return GetEntry(windowName) != null;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
