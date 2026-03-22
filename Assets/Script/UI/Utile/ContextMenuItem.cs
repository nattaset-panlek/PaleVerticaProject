using UnityEngine.Events;

namespace CoreSystem.UI.Utile
{
    /// <summary>
    /// ข้อมูลของแต่ละปุ่มใน ContextMenu
    /// </summary>
    public class ContextMenuItem
    {
        /// <summary>ชื่อที่แสดงบนปุ่ม</summary>
        public string Label;

        /// <summary>Action ที่จะ Invoke เมื่อกดปุ่มนี้</summary>
        public UnityAction OnClick;

        public ContextMenuItem(string label, UnityAction onClick)
        {
            Label = label;
            OnClick = onClick;
        }
    }
}
