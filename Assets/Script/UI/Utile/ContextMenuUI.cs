using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreSystem.UI.Utile
{
    /// <summary>
    /// Component สำหรับ AddComponent ลงบน GameObject ที่ต้องการให้ Left-click แล้วแสดง ContextMenu
    /// ใช้งานคล้าย HoverableUI แต่จะ Setup ด้วย List ของ ContextMenuItem
    ///
    /// ตัวอย่าง:
    ///   var ctx = GetComponent<ContextMenuUI>() ?? gameObject.AddComponent<ContextMenuUI>();
    ///   ctx.SetupMenu(new List<ContextMenuItem> {
    ///       new ContextMenuItem("Info",    () => ShowInfo()),
    ///       new ContextMenuItem("Use",     () => UseItem()),
    ///       new ContextMenuItem("Discard", () => DiscardItem()),
    ///   });
    /// </summary>
    public class ContextMenuUI : MonoBehaviour, IPointerClickHandler
    {
        #region [Variables/Fields]
        [Header("Menu Settings")]
        [SerializeField] private bool _requireDoubleClick = true;
        [SerializeField] private List<ContextMenuItemData> _inspectorItems = new List<ContextMenuItemData>();

        private List<ContextMenuItem> _menuItems = new List<ContextMenuItem>();
        #endregion

        #region [Properties]
        public bool RequireDoubleClick
        {
            get => _requireDoubleClick;
            set => _requireDoubleClick = value;
        }

        public List<ContextMenuItem> MenuItems => _menuItems;
        #endregion

        #region [Unity Methods]
        private void Start()
        {
            // ถ้าไม่ได้ SetupMenu จาก Code ให้ใช้ Items จาก Inspector แทน
            // (Inspector items ไม่มี OnClick — ต้อง Override ผ่าน SetupMenu)
            if (_menuItems.Count == 0 && _inspectorItems.Count > 0)
            {
                foreach (var item in _inspectorItems)
                    _menuItems.Add(new ContextMenuItem(item.Label, null));
            }
        }
        #endregion

        #region [Public Methods]
        /// <summary>
        /// กำหนด ContextMenu items จาก Code
        /// เรียกเมื่อ GameObject ได้รับข้อมูลแล้ว เช่น ใน UpdateUI()
        /// ถ้าเรียกซ้ำจะ Replace items ชุดเดิมทั้งหมด
        /// </summary>
        public void SetupMenu(List<ContextMenuItem> items)
        {
            _menuItems = items ?? new List<ContextMenuItem>();
        }

        /// <summary>
        /// เพิ่ม item เดียวเข้าไปใน Menu
        /// </summary>
        public void AddMenuItem(ContextMenuItem item)
        {
            _menuItems.Add(item);
        }

        /// <summary>
        /// Clear items ทั้งหมด
        /// </summary>
        public void ClearMenu()
        {
            _menuItems.Clear();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (_requireDoubleClick && eventData.clickCount < 2)
                return;

            if (_menuItems == null || _menuItems.Count == 0)
                return;

            if (ContextMenuManager.Instance == null)
            {
                Debug.LogWarning("[ContextMenuUI] ContextMenuManager.Instance is null — ลืม Add ContextMenuManager ลงใน Scene หรือเปล่า?");
                return;
            }

            ContextMenuManager.Instance.Show(_menuItems, eventData.position);
        }
        #endregion

        #region [Private Methods]
        #endregion
    }

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
