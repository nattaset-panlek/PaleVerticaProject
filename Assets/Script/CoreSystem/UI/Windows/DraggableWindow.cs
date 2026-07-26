using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreSystem.UI.Windows
{
    public class DraggableWindow : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        #region [Variables/Fields]
        [Header("Drag Settings")]
        [SerializeField] private RectTransform _headerRect;
        [SerializeField] private bool _clampToCanvas = true;

        private RectTransform _windowRect;
        private Canvas _canvas;
        private RectTransform _canvasRect;
        private Vector2 _pointerOffset;
        private bool _isDragging;
        private Vector3 _initialPosition;
        #endregion

        #region [Properties]
        public RectTransform HeaderRect
        {
            get => _headerRect;
            set => _headerRect = value;
        }

        public RectTransform WindowRect => _windowRect;

        public bool ClampToCanvas
        {
            get => _clampToCanvas;
            set => _clampToCanvas = value;
        }
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_windowRect == null)
            {
                _windowRect = GetComponent<RectTransform>();
            }

            if (_windowRect != null)
            {
                _initialPosition = _windowRect.localPosition;
            }

            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null)
            {
                _canvasRect = _canvas.transform as RectTransform;
            }
        }

        private void OnEnable()
        {
            if (_windowRect != null)
            {
                _windowRect.localPosition = _initialPosition;
            }
        }
        #endregion

        #region [Public Methods]
        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = false;

            if (_headerRect != null)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(_headerRect, eventData.pressPosition, eventData.pressEventCamera))
                {
                    return;
                }
            }

            if (_canvasRect == null || _windowRect == null) return;

            _isDragging = true;
            _windowRect.SetAsLastSibling();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, eventData.pressPosition, eventData.pressEventCamera, out Vector2 localPointerPosition);
            _pointerOffset = (Vector2)_windowRect.localPosition - localPointerPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            if (_canvasRect == null || _windowRect == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 localPointerPosition))
            {
                Vector2 newPosition = localPointerPosition + _pointerOffset;
                _windowRect.localPosition = _clampToCanvas ? ClampToCanvasBounds(newPosition) : newPosition;
            }
        }

        public void ResetPosition()
        {
            if (_windowRect != null)
            {
                _windowRect.localPosition = _initialPosition;
            }
        }
        #endregion

        #region [Private Methods]
        private Vector2 ClampToCanvasBounds(Vector2 position)
        {
            Vector2 canvasSize = _canvasRect.rect.size;
            Vector2 windowSize = _windowRect.rect.size;

            Vector2 windowPivot = _windowRect.pivot;
            Vector2 canvasPivot = _canvasRect.pivot;

            float minX = -canvasSize.x * canvasPivot.x + windowSize.x * windowPivot.x;
            float maxX = canvasSize.x * (1 - canvasPivot.x) - windowSize.x * (1 - windowPivot.x);

            float minY = -canvasSize.y * canvasPivot.y + windowSize.y * windowPivot.y;
            float maxY = canvasSize.y * (1 - canvasPivot.y) - windowSize.y * (1 - windowPivot.y);

            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.y = Mathf.Clamp(position.y, minY, maxY);

            return position;
        }
        #endregion
    }
}
