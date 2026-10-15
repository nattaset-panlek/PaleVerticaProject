using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreSystem.UI.Utility
{
    /// <summary>
    /// ข้อมูล Inspector-friendly สำหรับแสดงใน Editor (ไม่มี UnityAction)
    /// ใช้เฉพาะแสดงชื่อ Label ใน Inspector เท่านั้น Action ต้องกำหนดผ่าน SetupMenu()
    /// </summary>
    [System.Serializable]
    public class ContextMenuItemData
    {
        public string Label;
    }
}
