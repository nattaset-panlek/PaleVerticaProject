using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableZoomPanel : MonoBehaviour, IDragHandler, IScrollHandler
{
    #region Variables/Fields

    [Header("References")]
    [SerializeField] private RectTransform _content;

    [Header("Zoom")]
    [SerializeField] private float _minZoom = 0.3f;
    [SerializeField] private float _maxZoom = 2f;
    [SerializeField] private float _zoomSpeed = 0.1f;

    private RectTransform _container;
    private Canvas        _canvas;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        _container = GetComponent<RectTransform>();
        _canvas    = GetComponentInParent<Canvas>();
    }

    #endregion

    #region Public Methods

    public void OnDrag(PointerEventData eventData)
    {
        _content.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        ClampPosition();
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

        ClampPosition();
    }

    #endregion

    #region Private Methods

    private void ClampPosition()
    {
        Vector2 containerHalf = _container.rect.size * 0.5f;
        Vector2 contentHalf   = _content.rect.size   * _content.localScale.x * 0.5f;

        Vector2 min = containerHalf - contentHalf;
        Vector2 max = contentHalf   - containerHalf;

        _content.anchoredPosition = new Vector2(
            Mathf.Clamp(_content.anchoredPosition.x, min.x, max.x),
            Mathf.Clamp(_content.anchoredPosition.y, min.y, max.y));
    }

    #endregion
}
