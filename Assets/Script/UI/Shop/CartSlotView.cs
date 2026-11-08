using System;
using CoreSystem.Data.Items;
using CoreSystem.Shop;
using CoreSystem.UI.Inventory.Utility;
using CoreSystem.UI.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoreSystem.UI.Shop
{
    [DisallowMultipleComponent]
    public class CartSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IDropHandler, IHoverInteractable
    {
        #region [Variables/Fields]
        [SerializeField] private ItemSlotArtView _art;
        [SerializeField] private GameObject _emptyBody;
        [SerializeField] private GameObject _dropOutline;
        [SerializeField] private GameObject _hoverRing;
        [SerializeField] private Button _removeButton;

        private ShopCartLine _line;
        private int _cartIndex = -1;
        private bool _hovered;

        public static event Action<int> OnRemoveRequested;
        public static event Action<int> OnInfoRequested;
        public static event Action<ShopWarePayload> OnWareDropped;
        #endregion

        #region [Properties]
        public ShopCartLine Line => _line;
        public int CartIndex => _cartIndex;
        public bool IsFilled => _line != null;
        public bool ShowsInteractCursor => IsFilled;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_removeButton != null) _removeButton.onClick.AddListener(HandleRemove);
            Apply(false);
        }

        private void OnDestroy()
        {
            if (_removeButton != null) _removeButton.onClick.RemoveListener(HandleRemove);
        }

        private void OnDisable()
        {
            _hovered = false;
            Apply(false);
        }
        #endregion

        #region [Public Methods]
        public void Bind(ShopCartLine line, int cartIndex)
        {
            _line = line;
            _cartIndex = cartIndex;
            if (_art != null)
            {
                Item item = line != null ? line.Entry.Item : null;
                ItemSlotDisplay display = item != null ? ItemSlotDisplay.FromItem(item, line.Quantity) : ItemSlotDisplay.Empty;
                _art.Show(display);
            }
            Apply(false);
        }

        public void ForceHover(bool hovered)
        {
            _hovered = hovered;
            Apply(false);
        }

        public void ForceDropTarget()
        {
            Apply(true);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            Apply(GlobalDragManager.Payload is ShopWarePayload);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            Apply(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_line == null || eventData.button != PointerEventData.InputButton.Right) return;
            OnInfoRequested?.Invoke(_cartIndex);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!(GlobalDragManager.Payload is ShopWarePayload payload)) return;
            Apply(false);
            OnWareDropped?.Invoke(payload);
        }
        #endregion

        #region [Private Methods]
        private void Apply(bool dropTarget)
        {
            bool filled = _line != null;
            if (_art != null) _art.gameObject.SetActive(filled);
            if (_emptyBody != null) _emptyBody.SetActive(!filled);
            if (_dropOutline != null) _dropOutline.SetActive(dropTarget);
            bool hover = filled && _hovered && !dropTarget;
            if (_hoverRing != null) _hoverRing.SetActive(hover);
            if (_removeButton != null) _removeButton.gameObject.SetActive(hover);
        }

        private void HandleRemove()
        {
            if (_line != null) OnRemoveRequested?.Invoke(_cartIndex);
        }
        #endregion
    }
}
