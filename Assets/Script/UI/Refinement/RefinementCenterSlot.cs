using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.UI.Utility;

namespace CoreSystem.UI.Refinement
{
    public class RefinementCenterSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        #region [Variables/Fields]
        [SerializeField] private RefinementWindow _window;
        [SerializeField] private Image _slotBorderImage;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Sprite _defaultSlotSprite;

        private EquipmentInstance _currentEquipment;
        #endregion

        #region [Properties]
        public EquipmentInstance CurrentEquipment => _currentEquipment;
        public RefinementWindow Window => _window;
        public Image SlotBorderImage => _slotBorderImage;
        public Image ItemIcon => _itemIcon;
        public Sprite DefaultSlotSprite => _defaultSlotSprite;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_window == null)
            {
                _window = GetComponentInParent<RefinementWindow>();
            }

            if (_slotBorderImage != null && _defaultSlotSprite != null && _slotBorderImage.sprite == null)
            {
                _slotBorderImage.sprite = _defaultSlotSprite;
            }
        }
        #endregion

        #region [Public Methods]
        public void OnDrop(PointerEventData eventData)
        {
            EquipmentInstance equipment = GlobalDragManager.Payload as EquipmentInstance
                ?? ItemDragHandler.DraggedItem as EquipmentInstance;

            if (equipment == null)
            {
                equipment = ResolveEquipment(GlobalDragManager.Payload)
                    ?? ResolveEquipment(ItemDragHandler.DraggedItem)
                    ?? ResolveEquipment(ItemDragHandler.ActiveSource);
            }

            if (equipment != null)
            {
                if (_window == null)
                {
                    _window = GetComponentInParent<RefinementWindow>();
                }

                if (_window != null)
                {
                    _window.SetTarget(equipment);
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (_window == null)
                {
                    _window = GetComponentInParent<RefinementWindow>();
                }

                if (_window != null)
                {
                    _window.SetTarget(null);
                }
            }
        }

        public void UpdateDisplay(EquipmentInstance equipment)
        {
            _currentEquipment = equipment;

            if (equipment == null)
            {
                if (_itemIcon != null)
                {
                    _itemIcon.sprite = null;
                    _itemIcon.enabled = false;
                }

                if (_slotBorderImage != null && _defaultSlotSprite != null)
                {
                    _slotBorderImage.sprite = _defaultSlotSprite;
                }

                return;
            }

            if (_itemIcon != null)
            {
                if (equipment.Item != null)
                {
                    _itemIcon.sprite = equipment.Item.Icon;
                    _itemIcon.enabled = true;
                }
                else
                {
                    _itemIcon.sprite = null;
                    _itemIcon.enabled = false;
                }
            }

            if (_slotBorderImage != null && _defaultSlotSprite != null)
            {
                _slotBorderImage.sprite = _defaultSlotSprite;
            }
        }
        #endregion

        #region [Private Methods]
        private EquipmentInstance ResolveEquipment(object payload)
        {
            if (payload is EquipmentInstance eq)
            {
                return eq;
            }

            if (payload is ItemInstance item && item is EquipmentInstance itemEq)
            {
                return itemEq;
            }

            if (payload is IItemProvider provider && provider.GetItem() is EquipmentInstance providerEq)
            {
                return providerEq;
            }

            return null;
        }
        #endregion
    }
}
