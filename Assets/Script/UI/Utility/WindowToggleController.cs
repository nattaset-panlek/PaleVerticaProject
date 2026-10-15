using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CoreSystem.UI.Utility
{
    /// <summary>
    /// Controller สำหรับจัดการการเปิด-ปิด Window ผ่าน Keyboard Shortcuts
    /// รองรับการตั้งค่าหลายปุ่มต่อ 1 Window และออกแบบมาให้ขยายผลต่อได้ในอนาคต (เช่น ระบบ Rebinding)
    /// </summary>
    public class WindowToggleController : MonoBehaviour
    {
        [System.Serializable]
        public class WindowToggleMapping
        {
            [Tooltip("ชื่อเรียกสำหรับระบุ Mapping นี้ (เช่น Inventory, Equipment)")]
            public string Name;

            [Tooltip("List ของปุ่มที่สามารถกดเพื่อ Toggle Window นี้ได้")]
            public List<KeyCode> Keys = new List<KeyCode>();

            [Tooltip("GameObject ของ Window หรือ UI ที่ต้องการเปิด-ปิด")]
            public GameObject WindowObject;

            [Tooltip("Event ที่จะเรียกเมื่อ Window ถูกเปิด")]
            public UnityEvent OnOpen;

            public bool IsKeyPressed()
            {
                if (Keys == null || Keys.Count == 0) return false;
                var keyboard = UnityEngine.InputSystem.Keyboard.current;
                if (keyboard == null) return false;
                
                bool requiresAlt = false;
                bool requiresCtrl = false;
                bool requiresShift = false;

                foreach (var k in Keys)
                {
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

                foreach (var legacyKey in Keys)
                {
                    var key = ToInputSystemKey(legacyKey);
                    if (key == UnityEngine.InputSystem.Key.None || !keyboard[key].isPressed) return false;
                }

                foreach (var legacyKey in Keys)
                {
                    var key = ToInputSystemKey(legacyKey);
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

            [Tooltip("Event ที่จะเรียกเมื่อ Window ถูกปิด (เรียกเฉพาะตอนกดปุ่มและ Window กำลังแสดงอยู่)")]
            public UnityEvent OnClose;

            public void Toggle()
            {
                if (WindowObject != null)
                {
                    bool isActive = WindowObject.activeSelf;
                    bool newState = !isActive;

                    if (newState)
                    {
                        WindowObject.transform.SetAsLastSibling();
                        OnOpen?.Invoke();
                    }
                    else
                    {
                        OnClose?.Invoke();
                    }

                    WindowObject.SetActive(newState);
                }
            }
        }

        [Header("Mappings Settings")]
        [SerializeField]
        private List<WindowToggleMapping> _mappings = new List<WindowToggleMapping>();

        /// <summary>
        /// เข้าถึง Mappings ทั้งหมด (เผื่อใช้จัดการจาก Script อื่นๆ เช่น UI Settings)
        /// </summary>
        public List<WindowToggleMapping> Mappings => _mappings;

        private void Update()
        {
            foreach (var mapping in _mappings)
            {
                if (mapping.IsKeyPressed())
                {
                    mapping.Toggle();
                }
            }
        }

        /// <summary>
        /// เพิ่ม Mapping ใหม่ผ่าน Code
        /// </summary>
        public void AddMapping(WindowToggleMapping newMapping)
        {
            if (newMapping != null)
                _mappings.Add(newMapping);
        }

        /// <summary>
        /// ค้นหา Mapping ตามชื่อ
        /// </summary>
        public WindowToggleMapping GetMapping(string name)
        {
            return _mappings.Find(m => m.Name == name);
        }
    }
}
