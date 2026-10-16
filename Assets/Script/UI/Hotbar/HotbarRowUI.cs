using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoreSystem.UI.Hotbar
{
    public class HotbarRowUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerClickHandler
    {
        #region [Variables/Fields]
        [Header("UI References")]
        [SerializeField] private RectTransform _dragHeaderArea;
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private GameObject _slotPrefab;

        [Header("Row Configuration")]
        [SerializeField] private int _rowIndex;
        [SerializeField] private bool _isLocked = false;
        [SerializeField] private bool _clampToCanvas = true;
        private List<HotbarSlotView> _slots = new List<HotbarSlotView>();

        private RectTransform _rectTransform;
        private Vector2 _dragOffset;
        private Canvas _parentCanvas;
        private RectTransform _canvasRect;
        private readonly Vector3[] _corners = new Vector3[4];
        #endregion

        #region [Properties]
        public int RowIndex => _rowIndex;
        public bool IsLocked => _isLocked;
        public List<HotbarSlotView> Slots => _slots;
        public RectTransform RectTransform => _rectTransform;

        public bool ClampToCanvas
        {
            get => _clampToCanvas;
            set
            {
                _clampToCanvas = value;
                if (_clampToCanvas) ClampToCanvasBounds();
            }
        }
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentCanvas = GetComponentInParent<Canvas>();
            if (_parentCanvas != null) _canvasRect = _parentCanvas.rootCanvas.transform as RectTransform;
        }
        #endregion

        #region [Event System Interfaces]
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isLocked || eventData.button != PointerEventData.InputButton.Left) return;

            if (_parentCanvas != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                _dragOffset = localPoint;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isLocked || eventData.button != PointerEventData.InputButton.Left) return;

            if (_parentCanvas != null)
            {
                RectTransform parentRect = _rectTransform.parent as RectTransform;
                if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
                {
                    _rectTransform.anchoredPosition = localPoint - _dragOffset;
                    if (_clampToCanvas) ClampToCanvasBounds();
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (IsPointerInHeader(eventData.position))
                {
                    if (HotbarManager.Instance != null)
                    {
                        HotbarManager.Instance.OpenContextMenu(this, eventData.position);
                    }
                }
            }
        }
        #endregion

        #region [Public Methods]
        public void SetupRow(int rowIndex, int slotCount, Vector2 position, bool isLocked)
        {
            _rowIndex = rowIndex;
            _isLocked = isLocked;

            if (_rectTransform != null && position != Vector2.zero)
            {
                _rectTransform.anchoredPosition = position;
            }

            if (_slots.Count != slotCount)
            {
                BuildSlots(slotCount);
            }
            else
            {
                ReindexRow(rowIndex);
            }

            if (_clampToCanvas && _rectTransform != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
                ClampToCanvasBounds();
            }
        }

        public void ReindexRow(int newRowIndex)
        {
            _rowIndex = newRowIndex;
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i] != null)
                {
                    string hotkeyText = GetDefaultHotkeyText(_rowIndex, i);
                    _slots[i].SetupSlot(_rowIndex, i, hotkeyText, _slots[i].SlotData);
                }
            }
        }

        public void ToggleLockPosition()
        {
            _isLocked = !_isLocked;
            Debug.Log($"[HotbarRowUI] Row {_rowIndex} position locked state: {_isLocked}");
        }

        public void SetLockPosition(bool locked)
        {
            _isLocked = locked;
        }

        public HotbarSlotView GetSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < _slots.Count)
            {
                return _slots[slotIndex];
            }
            return null;
        }
        #endregion

        #region [Private Methods]
        private void BuildSlots(int count)
        {
            if (_slotContainer == null || _slotPrefab == null) return;

            // Clear existing slots
            foreach (var slot in _slots)
            {
                if (slot != null) Destroy(slot.gameObject);
            }
            _slots.Clear();

            for (int i = 0; i < count; i++)
            {
                GameObject slotObj = Instantiate(_slotPrefab, _slotContainer);
                slotObj.SetActive(true);

                HotbarSlotView slotUI = slotObj.GetComponent<HotbarSlotView>();
                if (slotUI == null)
                {
                    slotUI = slotObj.AddComponent<HotbarSlotView>();
                }

                string hotkeyText = GetDefaultHotkeyText(_rowIndex, i);
                slotUI.SetupSlot(_rowIndex, i, hotkeyText);
                _slots.Add(slotUI);
            }
        }

        private void ClampToCanvasBounds()
        {
            if (_canvasRect == null || _rectTransform == null) return;

            _rectTransform.GetWorldCorners(_corners);
            Vector2 min = _canvasRect.InverseTransformPoint(_corners[0]);
            Vector2 max = _canvasRect.InverseTransformPoint(_corners[2]);
            Rect bounds = _canvasRect.rect;

            Vector2 shift = Vector2.zero;
            if (min.x < bounds.xMin) shift.x = bounds.xMin - min.x;
            else if (max.x > bounds.xMax) shift.x = bounds.xMax - max.x;
            if (min.y < bounds.yMin) shift.y = bounds.yMin - min.y;
            else if (max.y > bounds.yMax) shift.y = bounds.yMax - max.y;

            if (shift == Vector2.zero) return;
            _rectTransform.position += _canvasRect.TransformVector(shift);
        }

        private bool IsPointerInHeader(Vector2 screenPos)
        {
            if (_dragHeaderArea == null) return true;
            return RectTransformUtility.RectangleContainsScreenPoint(_dragHeaderArea, screenPos);
        }

        private string GetDefaultHotkeyText(int row, int slot)
        {
            string slotLabel = (slot + 1 % 10).ToString();
            if (slot == 9) slotLabel = "0";

            switch (row)
            {
                case 0: return slotLabel;
                case 1: return $"Alt+{slotLabel}";
                case 2: return $"Ctrl+{slotLabel}";
                case 3: return $"Shift+{slotLabel}";
                default: return $"R{row+1}-{slotLabel}";
            }
        }
        #endregion
    }
}
