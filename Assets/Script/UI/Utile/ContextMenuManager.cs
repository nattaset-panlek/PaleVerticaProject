using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Utile
{
    /// <summary>
    /// Singleton Manager สำหรับระบบ ContextMenu
    /// ทำหน้าที่ Spawn ปุ่ม, วางตำแหน่ง, และ Clamp ไม่ให้ล้นจอ
    /// คล้ายกับ HoverTextManager แต่สำหรับ Right-click Menu
    /// </summary>
    public class ContextMenuManager : MonoBehaviour
    {
        public static ContextMenuManager Instance { get; private set; }

        [Header("Prefab & Container")]
        [Tooltip("Prefab ปุ่ม 1 ปุ่ม ต้องมี Button และ TMP_Text เป็น child")]
        public GameObject ButtonPrefab;

        [Tooltip("Transform ที่ปุ่มจะถูก Instantiate เข้าไปเป็น child")]
        public Transform MenuContainer;

        [Header("Panel")]
        [Tooltip("RectTransform ของ Panel ทั้งหมด ใช้กำหนดตำแหน่งและ Clamp")]
        public RectTransform MenuPivot;
        public GameObject MenuContainerPanel;

        private List<GameObject> _spawnedButtons = new List<GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Hide();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// แสดง ContextMenu ที่ตำแหน่ง screenPos พร้อม items ที่กำหนด
        /// จะ Clear ปุ่มเก่าทิ้งก่อนเสมอ
        /// </summary>
        public void Show(List<ContextMenuItem> items, Vector2 screenPos)
        {
            if (ButtonPrefab == null || MenuContainer == null || MenuPivot == null)
            {
                Debug.LogError("[ContextMenuManager] ButtonPrefab / MenuContainer / MenuPanel ยังไม่ได้ Assign!");
                return;
            }

            ClearButtons();

            foreach (var item in items)
            {
                var btnObj = Instantiate(ButtonPrefab, MenuContainer);
                btnObj.SetActive(true);

                // หา Button component
                var btn = btnObj.GetComponent<Button>();
                if (btn == null)
                    btn = btnObj.GetComponentInChildren<Button>();

                // หา TMP_Text component
                var label = btnObj.GetComponent<TMP_Text>();
                if (label == null)
                    label = btnObj.GetComponentInChildren<TMP_Text>();

                if (label != null)
                    label.text = item.Label;

                if (btn != null && item.OnClick != null)
                {
                    var capturedAction = item.OnClick; // capture ให้ถูก
                    btn.onClick.AddListener(() =>
                    {
                        capturedAction.Invoke();
                        Hide();
                    });
                }

                _spawnedButtons.Add(btnObj);
            }

            // วางตำแหน่ง Panel
            MenuPivot.position = screenPos;
            MenuContainerPanel.gameObject.SetActive(true);

            // Clamp ไม่ให้ล้นจอ (ต้อง rebuild layout ก่อนจึงจะรู้ขนาดจริง)
            Canvas.ForceUpdateCanvases();
            ClampToScreenEdges();
        }

        /// <summary>
        /// ซ่อน ContextMenu และ Clear ปุ่มทั้งหมด
        /// </summary>
        public void Hide()
        {
            if (MenuContainerPanel != null)
                MenuContainerPanel.gameObject.SetActive(false);

            ClearButtons();
        }

        private void ClearButtons()
        {
            foreach (var btn in _spawnedButtons)
            {
                if (btn != null)
                    Destroy(btn);
            }
            _spawnedButtons.Clear();
        }

        private void ClampToScreenEdges()
        {
            Vector3[] corners = new Vector3[4];
            MenuPivot.GetWorldCorners(corners);

            float width  = corners[2].x - corners[0].x;
            float height = corners[1].y - corners[0].y;

            Vector3 pos = MenuPivot.position;

            if (pos.x + width  > Screen.width)  pos.x = Screen.width  - width;
            if (pos.x          < 0)              pos.x = 0;
            if (pos.y          < height)         pos.y = height;
            if (pos.y          > Screen.height)  pos.y = Screen.height;

            MenuPivot.position = pos;
        }
    }
}
