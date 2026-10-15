using TMPro;
using UnityEngine;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Utility;

namespace CoreSystem.UI.Inventory
{
    public class InventoryWindow : UIWindow
    {
        #region [Variables/Fields]
        [SerializeField] private Transform _equipmentSlotParent;
        [SerializeField] private Transform _inventorySlotParent;
        [SerializeField] private TMP_Text _inventoryCountText;
        #endregion

        #region [Properties]
        public Transform EquipmentSlotParent => _equipmentSlotParent;
        public Transform InventorySlotParent => _inventorySlotParent;
        public TMP_Text InventoryCountText => _inventoryCountText;
        #endregion

        #region [Unity Methods]
        protected override void Awake()
        {
            base.Awake();

            if (GameData.Player != null && GameData.Player.Inventory != null)
            {
                GameData.Player.Inventory.OnInventoryChanged += LoadItemSlotWideEquipmentInventory;
                GameData.Player.Inventory.OnInventoryChanged += LoadItemSlotAllInventory;
                GameData.Player.Inventory.OnInventoryChanged += UpdateInventoryCount;
            }
        }

        private void OnDestroy()
        {
            if (GameData.Player != null && GameData.Player.Inventory != null)
            {
                GameData.Player.Inventory.OnInventoryChanged -= LoadItemSlotWideEquipmentInventory;
                GameData.Player.Inventory.OnInventoryChanged -= LoadItemSlotAllInventory;
                GameData.Player.Inventory.OnInventoryChanged -= UpdateInventoryCount;
            }
        }
        #endregion

        #region [Public Methods]
        public override void OnOpen(object data)
        {
            base.OnOpen(data);
            LoadItemSlotWideEquipmentInventory();
            LoadItemSlotAllInventory();
            UpdateInventoryCount();
        }

        public override void OnClose()
        {
            base.OnClose();
        }

        public void LoadItemSlotAllInventory()
        {
            if (_inventorySlotParent == null || GameData.Player == null || GameData.Player.Inventory == null) return;

            foreach (Transform child in _inventorySlotParent)
            {
                Destroy(child.gameObject);
            }

            foreach (ItemInstance item in GameData.Player.Inventory.GetAllItems())
            {
                InventoryManager.Instance.CreateItemSlotSquareOnParent(item, _inventorySlotParent);
            }
        }

        public void LoadItemSlotWideEquipmentInventory()
        {
            if (_equipmentSlotParent == null || GameData.Player == null || GameData.Player.Inventory == null) return;

            foreach (Transform child in _equipmentSlotParent)
            {
                Destroy(child.gameObject);
            }

            foreach (ItemInstance item in GameData.Player.Inventory.GetAllItems())
            {
                if (item.Item is Equipment)
                {
                    InventoryManager.Instance.CreateItemSlotWideOnParent(item, _equipmentSlotParent);
                }
            }
        }

        public void UpdateInventoryCount()
        {
            if (_inventoryCountText == null || GameData.Player == null || GameData.Player.Inventory == null) return;

            int count = GameData.Player.Inventory.GetAllItems().Count;
            _inventoryCountText.text = NumberFormatter.Format(count);
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
