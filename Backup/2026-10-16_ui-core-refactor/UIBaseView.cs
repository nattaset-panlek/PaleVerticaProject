using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreSystem.UI
{
    public abstract class UIBaseView : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        #region [Variables/Fields]
        [Header("Base View Settings")]
        [SerializeField] private bool _destroyOnClose = false;

        [Header("Drag Settings")]
        [SerializeField] private bool _isDraggable = true;
        [SerializeField] private RectTransform _headerRect;
        [SerializeField] private bool _clampToCanvas = true;

        private string _windowName;
        private bool _isInitialized = false;

        private Canvas _canvas;
        private RectTransform _canvasRect;
        private Vector2 _pointerOffset;
        private bool _isDragging;
        private Vector3 _initialPosition;
        #endregion

        #region [Properties]
        public string WindowName
        {
            get => _windowName;
            set => _windowName = value;
        }

        public RectTransform WindowRect => transform as RectTransform;
        public bool DestroyOnClose
        {
            get => _destroyOnClose;
            set => _destroyOnClose = value;
        }
        public bool IsOpen => gameObject.activeSelf;
        public bool IsInitialized => _isInitialized;

        public bool IsDraggable
        {
            get => _isDraggable;
            set => _isDraggable = value;
        }

        public RectTransform HeaderRect
        {
            get => _headerRect;
            set => _headerRect = value;
        }

        public bool ClampToCanvas
        {
            get => _clampToCanvas;
            set => _clampToCanvas = value;
        }

        public UISignal SignalAction => UIManager.Instance != null ? UIManager.Instance.SignalAction(_windowName) : null;
        #endregion

        #region [Unity Methods]
        protected virtual void Awake()
        {
            RectTransform rect = WindowRect;
            if (rect != null)
            {
                _initialPosition = rect.localPosition;
            }

            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null)
            {
                _canvasRect = _canvas.transform as RectTransform;
            }
        }

        protected virtual void OnEnable()
        {
            ResetPosition();
        }
        #endregion

        #region [Public Methods]
        public virtual void OnLoad(object data)
        {
            _isInitialized = true;
        }

        public virtual void OnOpen(object data)
        {
            transform.SetAsLastSibling();
        }

        public virtual void OnClose()
        {
        }

        public virtual void CloseSelf()
        {
            if (UIManager.Instance != null && !string.IsNullOrEmpty(_windowName))
            {
                UIManager.Instance.Close(_windowName);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isDraggable) return;

            _isDragging = false;
            
            if (_headerRect != null)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(_headerRect, eventData.pressPosition, eventData.pressEventCamera))
                {
                    return;
                }
            }

            RectTransform rect = WindowRect;
            if (_canvasRect == null || rect == null) return;

            _isDragging = true;
            rect.SetAsLastSibling();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, eventData.pressPosition, eventData.pressEventCamera, out Vector2 localPointerPosition);
            _pointerOffset = (Vector2)rect.localPosition - localPointerPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDraggable || !_isDragging) return;

            RectTransform rect = WindowRect;
            if (_canvasRect == null || rect == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 localPointerPosition))
            {
                Vector2 newPosition = localPointerPosition + _pointerOffset;
                rect.localPosition = _clampToCanvas ? ClampToCanvasBounds(newPosition) : newPosition;
            }
        }

        public void ResetPosition()
        {
            RectTransform rect = WindowRect;
            if (rect != null)
            {
                rect.localPosition = _initialPosition;
            }
        }
        #endregion

        #region [Private Methods]
        private Vector2 ClampToCanvasBounds(Vector2 position)
        {
            RectTransform rect = WindowRect;
            Vector2 canvasSize = _canvasRect.rect.size;
            Vector2 windowSize = rect.rect.size;
            
            Vector2 windowPivot = rect.pivot;
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
