using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CoreSystem.UI.Utile
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

            /// <summary>
            /// ตรวจสอบว่ามีการกดปุ่มใดๆ ใน List หรือไม่
            /// </summary>
            public bool IsKeyPressed()
            {
                if (Keys == null || Keys.Count == 0) return false;
                
                // ทุกปุ่มใน List ต้องถูกกดค้างอยู่ (Held)
                foreach (var key in Keys)
                {
                    if (!Input.GetKey(key)) return false;
                }

                // และต้องมีอย่างน้อยหนึ่งปุ่มที่เพิ่งถูกกดใน Frame นี้ (Pressed)
                // เพื่อป้องกันการ Toggle รัวๆ เมื่อกดค้าง
                foreach (var key in Keys)
                {
                    if (Input.GetKeyDown(key)) return true;
                }

                return false;
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
