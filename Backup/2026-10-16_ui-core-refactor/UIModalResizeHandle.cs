using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoreSystem.UI
{
    public class UIModalResizeHandle : MonoBehaviour, 
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region [Variables/Fields]
        [SerializeField] private UIGlobalModal _modal;
        [SerializeField] private Image _handleImage;

        private Color _normalColor = new Color(1f, 1f, 1f, 0.35f);
        private Color _hoverColor = new Color(1f, 1f, 1f, 0.85f);
        private Color _activeColor = new Color(0.25f, 0.75f, 1f, 1f);

        private bool _isHovered;
        private bool _isDragging;
        #endregion

        #region [Properties]
        public UIGlobalModal Modal => _modal;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_handleImage == null)
            {
                _handleImage = GetComponent<Image>();
            }

            if (_modal == null)
            {
                _modal = GetComponentInParent<UIGlobalModal>();
            }

            UpdateVisual();
        }

        private void OnDisable()
        {
            _isHovered = false;
            _isDragging = false;
            CursorManager.Instance?.ResetCursor();
        }
        #endregion

        #region [Public Methods]
        public void Initialize(UIGlobalModal modal)
        {
            _modal = modal;
            if (_handleImage == null)
            {
                _handleImage = GetComponent<Image>();
            }
            UpdateVisual();
        }

        public void SetColors(Color normal, Color hover, Color active)
        {
            _normalColor = normal;
            _hoverColor = hover;
            _activeColor = active;
            UpdateVisual();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            UpdateVisual();
            CursorManager.Instance?.SetCursor(CursorManager.CursorType.ResizeHandle);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            UpdateVisual();
            if (!_isDragging)
            {
                CursorManager.Instance?.ResetCursor();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UpdateVisual();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            UpdateVisual();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            UpdateVisual();
            CursorManager.Instance?.SetCursor(CursorManager.CursorType.ResizeHandle);
            _modal?.OnResizeBegin(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _modal?.OnResize(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            UpdateVisual();
            if (!_isHovered)
            {
                CursorManager.Instance?.ResetCursor();
            }
            _modal?.OnResizeEnd(eventData);
        }
        #endregion

        #region [Private Methods]
        private void UpdateVisual()
        {
            if (_handleImage == null) return;

            if (_isDragging)
            {
                _handleImage.color = _activeColor;
            }
            else if (_isHovered)
            {
                _handleImage.color = _hoverColor;
            }
            else
            {
                _handleImage.color = _normalColor;
            }
        }
        #endregion
    }
}
