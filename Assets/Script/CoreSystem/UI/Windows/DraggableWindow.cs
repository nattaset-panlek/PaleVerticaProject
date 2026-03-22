using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreSystem.UI.Windows
{
    public class DraggableWindow : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        [Header("Drag Settings")]
        [SerializeField] private RectTransform headerRect;
        [SerializeField] private RectTransform windowRect;
        [SerializeField] private bool clampToCanvas = true;

        private Canvas canvas;
        private RectTransform canvasRect;
        private Vector2 pointerOffset;
        private bool isDragging;

        private Vector3 initialPosition;

        private void Awake()
        {
            if (windowRect == null)
            {
                windowRect = GetComponent<RectTransform>();
            }

            initialPosition = windowRect.localPosition;

            canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvasRect = canvas.transform as RectTransform;
            }
        }

        private void OnEnable()
        {
            if (windowRect != null)
            {
                windowRect.localPosition = initialPosition;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = false;
            
            if (headerRect != null)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(headerRect, eventData.pressPosition, eventData.pressEventCamera))
                {
                    return;
                }
            }

            if (canvasRect == null || windowRect == null) return;

            isDragging = true;
            windowRect.SetAsLastSibling();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.pressPosition, eventData.pressEventCamera, out Vector2 localPointerPosition);
            pointerOffset = (Vector2)windowRect.localPosition - localPointerPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            if (canvasRect == null || windowRect == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 localPointerPosition))
            {
                Vector2 newPosition = localPointerPosition + pointerOffset;
                windowRect.localPosition = clampToCanvas ? ClampToCanvasBounds(newPosition) : newPosition;
            }
        }

        private Vector2 ClampToCanvasBounds(Vector2 position)
        {
            Vector2 canvasSize = canvasRect.rect.size;
            Vector2 windowSize = windowRect.rect.size;
            
            Vector2 windowPivot = windowRect.pivot;
            Vector2 canvasPivot = canvasRect.pivot;

            float minX = -canvasSize.x * canvasPivot.x + windowSize.x * windowPivot.x;
            float maxX = canvasSize.x * (1 - canvasPivot.x) - windowSize.x * (1 - windowPivot.x);
            
            float minY = -canvasSize.y * canvasPivot.y + windowSize.y * windowPivot.y;
            float maxY = canvasSize.y * (1 - canvasPivot.y) - windowSize.y * (1 - windowPivot.y);

            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.y = Mathf.Clamp(position.y, minY, maxY);

            return position;
        }
    }
}
