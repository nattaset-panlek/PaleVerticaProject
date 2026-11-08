using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.UI.Inventory.Utility;
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
        [SerializeField] private ItemSlotArtView _art;

        private EquipmentInstance _currentEquipment;
        #endregion

        #region [Properties]
        public EquipmentInstance CurrentEquipment => _currentEquipment;
        public RefinementWindow Window => _window;
        public Image SlotBorderImage => _slotBorderImage;
        public Image ItemIcon => _itemIcon;
        public Sprite DefaultSlotSprite => _defaultSlotSprite;
        public ItemSlotArtView Art => _art;
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
            ItemInstance item = ItemInsertSlot.ResolveDraggedItem();
            RefinementWindow window = ResolveWindow();
            if (item == null || window == null) return;
            window.TryInsert(item, ItemInsertRegistry.AnySlot);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
            RefinementWindow window = ResolveWindow();
            if (window != null) window.ClearInsert(ItemInsertRegistry.AnySlot);
        }

        public void UpdateDisplay(EquipmentInstance equipment)
        {
            _currentEquipment = equipment;
            if (_art != null) _art.Show(ItemSlotDisplay.From(equipment));

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
        private RefinementWindow ResolveWindow()
        {
            if (_window == null) _window = GetComponentInParent<RefinementWindow>();
            return _window;
        }
        #endregion
    }
}
