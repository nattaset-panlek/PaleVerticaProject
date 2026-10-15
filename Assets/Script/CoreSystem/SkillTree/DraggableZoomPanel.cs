using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreSystem.SkillTree
{
    public class DraggableZoomPanel : MonoBehaviour, IDragHandler, IEndDragHandler, IScrollHandler
    {
        public enum MovementType
        {
            Unrestricted,
            Elastic,
            Clamped
        }

        #region [Variables/Fields]

        [Header("References")]
        [SerializeField] private RectTransform _content;

        [Header("Movement")]
        [SerializeField] private MovementType _movementType = MovementType.Elastic;
        [SerializeField] private float _elasticitySpeed = 15f;

        [Header("Zoom")]
        [SerializeField] private float _minZoom = 0.3f;
        [SerializeField] private float _maxZoom = 2f;
        [SerializeField] private float _zoomSpeed = 0.1f;

        private RectTransform _container;
        private Canvas        _canvas;
        private bool          _isDragging;

        #endregion

        #region [Unity Methods]

        private void Awake()
        {
            _container = GetComponent<RectTransform>();
            _canvas    = GetComponentInParent<Canvas>();
        }

        private void LateUpdate()
        {
            if (_movementType == MovementType.Elastic && !_isDragging)
            {
                Vector2 offset = CalculateOffset(_content.anchoredPosition);
                if (offset != Vector2.zero)
                {
                    // Snap if very close to prevent infinite floating point Lerp dirtying the Canvas
                    if (offset.sqrMagnitude < 0.1f) 
                    {
                        _content.anchoredPosition += offset;
                    }
                    else
                    {
                        _content.anchoredPosition = Vector2.Lerp(_content.anchoredPosition, _content.anchoredPosition + offset, Time.deltaTime * _elasticitySpeed);
                    }
                }
            }
        }

        #endregion

        #region [Public Methods]

        public void OnDrag(PointerEventData eventData)
        {
            _isDragging = true;
            Vector2 delta = eventData.delta / _canvas.scaleFactor;

            if (_movementType == MovementType.Elastic)
            {
                Vector2 offset = CalculateOffset(_content.anchoredPosition);
                // Apply resistance if we are out of bounds
                if (Mathf.Abs(offset.x) > 0.1f) delta.x *= 0.5f;
                if (Mathf.Abs(offset.y) > 0.1f) delta.y *= 0.5f;
            }

            _content.anchoredPosition += delta;

            if (_movementType == MovementType.Clamped)
            {
                ClampPosition();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
        }

        public void OnScroll(PointerEventData eventData)
        {
            float scroll   = eventData.scrollDelta.y;
            float newScale = Mathf.Clamp(_content.localScale.x + scroll * _zoomSpeed, _minZoom, _maxZoom);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _container, eventData.position, eventData.pressEventCamera, out Vector2 localMouse);

            Vector2 posBefore = _content.anchoredPosition;
            Vector2 scaledDiff = localMouse - posBefore;
            float   ratio = newScale / _content.localScale.x;

            _content.localScale      = new Vector3(newScale, newScale, 1f);
            _content.anchoredPosition = localMouse - scaledDiff * ratio;

            if (_movementType == MovementType.Clamped)
            {
                ClampPosition();
            }
        }

        #endregion

        #region [Private Methods]

        private void ClampPosition()
        {
            Vector2 offset = CalculateOffset(_content.anchoredPosition);
            if (offset != Vector2.zero)
            {
                _content.anchoredPosition += offset;
            }
        }

        private Vector2 CalculateOffset(Vector2 position)
        {
            Vector2 offset = Vector2.zero;

            Vector2 containerHalf = _container.rect.size * 0.5f;
            Vector2 contentHalf   = _content.rect.size   * _content.localScale.x * 0.5f;

            Vector2 min = containerHalf - contentHalf;
            Vector2 max = contentHalf   - containerHalf;

            // Correct inverted bounds when content is smaller than container
            // By forcing the offset to center the content
            if (contentHalf.x < containerHalf.x)
            {
                offset.x = -position.x; 
            }
            else
            {
                if (position.x > max.x) offset.x = max.x - position.x;
                else if (position.x < min.x) offset.x = min.x - position.x;
            }

            if (contentHalf.y < containerHalf.y)
            {
                offset.y = -position.y;
            }
            else
            {
                if (position.y > max.y) offset.y = max.y - position.y;
                else if (position.y < min.y) offset.y = min.y - position.y;
            }

            return offset;
        }

        #endregion
    }
}
