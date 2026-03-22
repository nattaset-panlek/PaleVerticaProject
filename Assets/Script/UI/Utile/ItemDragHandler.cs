using CoreSystem.Data.Items;
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

    public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public static ItemInstance DraggedItem { get; private set; }
        public static IItemProvider ActiveSource { get; private set; }

        private Image _dragIcon;
        private Canvas _canvas;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var provider = GetComponent<IItemProvider>();
            if (provider == null) provider = GetComponentInParent<IItemProvider>();
            if (provider == null || provider.GetItem() == null) return;

            DraggedItem = provider.GetItem();
            ActiveSource = provider;

            // Create Visual
            GameObject go = new GameObject("DragIcon");
            go.transform.SetParent(_canvas.transform);
            _dragIcon = go.AddComponent<Image>();
            _dragIcon.sprite = provider.GetIcon();
            _dragIcon.color = new Color(1, 1, 1, 0.6f);
            _dragIcon.raycastTarget = false;
            
            // Set size
            RectTransform iconRT = go.GetComponent<RectTransform>();
            iconRT.sizeDelta = _rectTransform != null ? _rectTransform.sizeDelta : new Vector2(50, 50);

            MoveIcon(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_dragIcon != null)
            {
                MoveIcon(eventData.position);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_dragIcon != null)
            {
                Destroy(_dragIcon.gameObject);
            }
            
            DraggedItem = null;
            ActiveSource = null;
        }

        private void MoveIcon(Vector2 screenPos)
        {
            if (_dragIcon == null || _canvas == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform, 
                screenPos, 
                _canvas.worldCamera, 
                out Vector2 localPos))
            {
                _dragIcon.rectTransform.localPosition = localPos;
            }
        }
    }
}

