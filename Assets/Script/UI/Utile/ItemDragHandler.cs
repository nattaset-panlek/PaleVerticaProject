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

    public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public static ItemInstance DraggedItem { get; private set; }
        public static IItemProvider ActiveSource { get; private set; }

        [SerializeField] private float dragHoldThreshold = 0.05f;
        private float _pointerDownTime;
        private bool _isDragging = false;

        private RectTransform _dragVisual;
        private Canvas _canvas;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pointerDownTime = Time.realtimeSinceStartup;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Time.realtimeSinceStartup - _pointerDownTime < dragHoldThreshold)
            {
                eventData.pointerDrag = null;
                return;
            }

            _isDragging = true;
            var provider = GetComponent<IItemProvider>();
            if (provider == null) provider = GetComponentInParent<IItemProvider>();
            if (provider == null || provider.GetItem() == null) return;

            DraggedItem = provider.GetItem();
            ActiveSource = provider;

            // Get the source gameObject to clone (usually the slot)
            GameObject sourceGO = (provider as MonoBehaviour)?.gameObject ?? gameObject;

            // Create Visual by cloning the gameobject
            GameObject go = Instantiate(sourceGO, _canvas.transform);
            go.name = "DragVisual_Clone";
            
            // Setup RectTransform to follow mouse
            _dragVisual = go.GetComponent<RectTransform>();
            _dragVisual.sizeDelta = _rectTransform != null ? _rectTransform.sizeDelta : new Vector2(50, 50);

            // Important: Disable interaction and raycasting on the clone so it doesn't block the drop target
            CanvasGroup group = go.GetComponent<CanvasGroup>();
            if (group == null) group = go.AddComponent<CanvasGroup>();
            group.alpha = 0.6f;
            group.blocksRaycasts = false;
            group.interactable = false;

            // Clean up scripts on the clone to prevent unwanted behavior (multiple drag handlers, etc.)
            MonoBehaviour[] scripts = go.GetComponentsInChildren<MonoBehaviour>();
            foreach (var script in scripts)
            {
                // We keep visual components like Image, TMP_Text, etc. 
                // but remove anything that implements Event interfaces or this script
                if (script is IEventSystemHandler || script is ItemDragHandler || script is Selectable)
                {
                    // For selectables (Button, etc.), we can also just disable them, but removing is cleaner for a static clone
                    Destroy(script);
                }
            }

            MoveIcon(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isDragging && _dragVisual != null)
            {
                MoveIcon(eventData.position);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isDragging && _dragVisual != null)
            {
                Destroy(_dragVisual.gameObject);
            }
            
            _isDragging = false;
            DraggedItem = null;
            ActiveSource = null;
        }

        private void MoveIcon(Vector2 screenPos)
        {
            if (_dragVisual == null || _canvas == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform, 
                screenPos, 
                _canvas.worldCamera, 
                out Vector2 localPos))
            {
                _dragVisual.localPosition = localPos;
            }
        }
    }
}


