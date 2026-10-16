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
        private bool _isResizing;
        private Vector2 _startSize;
        private Vector2 _startPosition;
        private Vector2 _startPointer;
        private RectTransform _parentRect;

        private static Sprite _gripSprite;
        #endregion

        #region [Properties]
        public UIGlobalModal Modal => _modal;
        public static Sprite DefaultGripSprite => _gripSprite != null ? _gripSprite : (_gripSprite = CreateGripSprite());
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_handleImage == null) _handleImage = GetComponent<Image>();
            if (_modal == null) _modal = GetComponentInParent<UIGlobalModal>();
            UpdateVisual();
        }

        private void OnDisable()
        {
            _isHovered = false;
            _isDragging = false;
            _isResizing = false;
            CursorManager.Instance?.ResetCursor();
        }
        #endregion

        #region [Public Methods]
        public void Initialize(UIGlobalModal modal)
        {
            _modal = modal;
            if (_handleImage == null) _handleImage = GetComponent<Image>();
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
            if (!_isDragging) CursorManager.Instance?.ResetCursor();
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
            BeginResize(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isResizing || _modal == null || !_modal.IsResizable || _parentRect == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, eventData.position, eventData.pressEventCamera, out Vector2 pointer)) return;

            RectTransform rect = _modal.WindowRect;
            Vector2 delta = pointer - _startPointer;
            float width = Mathf.Clamp(_startSize.x + delta.x, _modal.MinSize.x, _modal.MaxSize.x);
            float height = Mathf.Clamp(_startSize.y - delta.y, _modal.MinSize.y, _modal.MaxSize.y);
            Vector2 pivot = rect.pivot;

            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(
                _startPosition.x + pivot.x * (width - _startSize.x),
                _startPosition.y - (1f - pivot.y) * (height - _startSize.y));

            LayoutRebuilder.MarkLayoutForRebuild(rect);
            _modal.NotifyResized(rect.sizeDelta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            _isResizing = false;
            UpdateVisual();
            if (!_isHovered) CursorManager.Instance?.ResetCursor();
        }
        #endregion

        #region [Private Methods]
        private void BeginResize(PointerEventData eventData)
        {
            _isResizing = false;
            if (_modal == null || !_modal.IsResizable) return;

            RectTransform rect = _modal.WindowRect;
            if (rect == null) return;

            _parentRect = rect.parent as RectTransform;
            if (_parentRect == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null) _parentRect = canvas.transform as RectTransform;
            }
            if (_parentRect == null) return;

            _startSize = rect.sizeDelta;
            _startPosition = rect.anchoredPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, eventData.pressPosition, eventData.pressEventCamera, out _startPointer);
            _isResizing = true;
        }

        private void UpdateVisual()
        {
            if (_handleImage == null) return;
            _handleImage.color = _isDragging ? _activeColor : _isHovered ? _hoverColor : _normalColor;
        }

        private static Sprite CreateGripSprite()
        {
            const int size = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "ModalResizeGrip"
            };

            var pixels = new Color[size * size];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int cornerDistance = (size - 1 - x) + y;
                    bool isLine = cornerDistance == 3 || cornerDistance == 7 || cornerDistance == 11;
                    pixels[y * size + x] = isLine ? Color.white : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(1f, 0f));
        }
        #endregion
    }
}
