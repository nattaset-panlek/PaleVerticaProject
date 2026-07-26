using CoreSystem.Data.Items;
using CoreSystem.Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoreSystem.UI.Utile
{
    public interface IItemProvider
    {
        ItemInstance GetItem();
        Sprite GetIcon();
    }

    public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region [Variables/Fields]
        public static ItemInstance DraggedItem { get; private set; }
        public static IItemProvider ActiveSource { get; private set; }

        [Header("Drag Detection Settings")]
        [SerializeField] private float _dragThresholdDistance = 10f;
        [SerializeField] private float _horizontalAngleThreshold = 45f;

        private bool _isDragging = false;
        private bool _isScrollingParent = false;
        private bool _dragDirectionDetermined = false;

        private Canvas _canvas;
        private RectTransform _rectTransform;
        private ScrollRect _parentScrollRect;
        private Vector2 _pressPosition;
        #endregion

        #region [Properties]
        public float DragThresholdDistance
        {
            get => _dragThresholdDistance;
            set => _dragThresholdDistance = value;
        }

        public float HorizontalAngleThreshold
        {
            get => _horizontalAngleThreshold;
            set => _horizontalAngleThreshold = value;
        }
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _rectTransform = GetComponent<RectTransform>();
            _parentScrollRect = GetComponentInParent<ScrollRect>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressPosition = eventData.position;
            _dragDirectionDetermined = false;
            _isDragging = false;
            _isScrollingParent = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_dragDirectionDetermined) return;

            Vector2 delta = eventData.position - _pressPosition;
            if (delta.magnitude < _dragThresholdDistance)
            {
                return;
            }

            EvaluateDragDirection(eventData, delta);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragDirectionDetermined)
            {
                Vector2 delta = eventData.position - _pressPosition;
                if (delta.magnitude >= _dragThresholdDistance)
                {
                    EvaluateDragDirection(eventData, delta);
                }
                else
                {
                    return;
                }
            }

            if (_isDragging)
            {
                GlobalDragManager.Instance.UpdatePosition(eventData.position);
            }
            else if (_isScrollingParent && _parentScrollRect != null)
            {
                ExecuteEvents.Execute(_parentScrollRect.gameObject, eventData, ExecuteEvents.dragHandler);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                GlobalDragManager.Instance.FinishDrag();
            }

            if (_isScrollingParent && _parentScrollRect != null)
            {
                ExecuteEvents.Execute(_parentScrollRect.gameObject, eventData, ExecuteEvents.endDragHandler);
            }

            _isDragging = false;
            _isScrollingParent = false;
            _dragDirectionDetermined = false;
            DraggedItem = null;
            ActiveSource = null;
        }
        #endregion

        #region [Public Methods]
        #endregion

        #region [Private Methods]
        private void EvaluateDragDirection(PointerEventData eventData, Vector2 delta)
        {
            _dragDirectionDetermined = true;

            float angle = Vector2.Angle(Vector2.right, new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y)));

            if (angle <= _horizontalAngleThreshold)
            {
                StartItemDrag(eventData);
            }
            else
            {
                StartScrollParentDrag(eventData);
            }
        }

        private void StartItemDrag(PointerEventData eventData)
        {
            var provider = GetComponent<IItemProvider>();
            if (provider == null) provider = GetComponentInParent<IItemProvider>();
            if (provider == null || provider.GetItem() == null)
            {
                _isDragging = false;
                return;
            }

            if (_canvas == null)
            {
                _canvas = GetComponentInParent<Canvas>();
            }
            if (_canvas == null)
            {
                _canvas = Object.FindObjectOfType<Canvas>();
            }
            if (_canvas == null)
            {
                _isDragging = false;
                return;
            }

            _isDragging = true;
            _isScrollingParent = false;

            DraggedItem = provider.GetItem();
            ActiveSource = provider;

            Sprite icon = provider.GetIcon();
            Color? rarityColor = null;
            if (DraggedItem != null && DraggedItem.Item != null)
            {
                rarityColor = ItemRarityExtensions.GetColor(DraggedItem.Item.Rarity);
            }

            GlobalDragManager.Instance.StartDrag(
                source: this,
                payload: DraggedItem,
                icon: icon,
                screenPos: eventData.position,
                customSize: _rectTransform != null ? _rectTransform.sizeDelta : new Vector2(50, 50),
                raritySprite: null,
                rarityColor: rarityColor
            );
        }

        private void StartScrollParentDrag(PointerEventData eventData)
        {
            _isDragging = false;

            if (_parentScrollRect == null)
            {
                _parentScrollRect = GetComponentInParent<ScrollRect>();
            }

            if (_parentScrollRect != null)
            {
                _isScrollingParent = true;
                ExecuteEvents.Execute(_parentScrollRect.gameObject, eventData, ExecuteEvents.beginDragHandler);
            }
            else
            {
                _isScrollingParent = false;
            }
        }
        #endregion
    }
}


