using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Utility;
using CoreSystem.UI.Inventory.Utility;
using CoreSystem.UI.Visualizer;
using GlobalVisualizer;

namespace CoreSystem.UI.Inventory
{
    public class InventoryWindow : UIWindow
    {
        #region [Variables/Fields]
        [SerializeField] private Transform _equipmentSlotParent;
        [SerializeField] private Transform _inventorySlotParent;
        [SerializeField] private TMP_Text _inventoryCountText;

        private readonly List<ItemInstance> _inventoryItems = new List<ItemInstance>();
        private readonly List<ItemInstance> _equipmentItems = new List<ItemInstance>();
        private GlobalVisualizerView _inventoryView;
        private GlobalVisualizerView _equipmentView;
        private PlayerInventory _inventory;
        private Coroutine _refreshRoutine;
        private bool _dirty = true;
        #endregion

        #region [Properties]
        public Transform EquipmentSlotParent => _equipmentSlotParent;
        public Transform InventorySlotParent => _inventorySlotParent;
        public TMP_Text InventoryCountText => _inventoryCountText;
        public GlobalVisualizerView InventoryView => _inventoryView;
        public GlobalVisualizerView EquipmentView => _equipmentView;
        public bool IsDirty => _dirty;
        #endregion

        #region [Unity Methods]
        protected override void Awake()
        {
            base.Awake();
            Subscribe();
        }

        private void OnDisable()
        {
            _refreshRoutine = null;
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
        #endregion

        #region [Public Methods]
        public override void OnOpen(object data)
        {
            base.OnOpen(data);
            Subscribe();
            RefreshNow();
        }

        public override void OnClose()
        {
            base.OnClose();
        }

        public void RefreshNow()
        {
            _dirty = false;
            CollectItems();
            if (_inventorySlotParent != null) _inventoryView = Present(_inventoryView, _inventorySlotParent, SlotPrefab(false), _inventoryItems);
            if (_equipmentSlotParent != null) _equipmentView = Present(_equipmentView, _equipmentSlotParent, SlotPrefab(true), _equipmentItems);
            UpdateInventoryCount();
        }

        public void LoadItemSlotAllInventory()
        {
            RefreshNow();
        }

        public void LoadItemSlotWideEquipmentInventory()
        {
            RefreshNow();
        }

        public void UpdateInventoryCount()
        {
            if (_inventoryCountText == null || _inventory == null) return;
            _inventoryCountText.text = NumberFormatter.Format(_inventory.Count);
        }
        #endregion

        #region [Private Methods]
        private void Subscribe()
        {
            PlayerInventory inventory = GameData.Player != null ? GameData.Player.Inventory : null;
            if (inventory == _inventory) return;
            Unsubscribe();
            _inventory = inventory;
            if (_inventory != null) _inventory.OnInventoryChanged += HandleInventoryChanged;
            _dirty = true;
        }

        private void Unsubscribe()
        {
            if (_inventory != null) _inventory.OnInventoryChanged -= HandleInventoryChanged;
            _inventory = null;
        }

        private void HandleInventoryChanged()
        {
            _dirty = true;
            if (!isActiveAndEnabled || _refreshRoutine != null) return;
            _refreshRoutine = StartCoroutine(RefreshAtFrameEnd());
        }

        private IEnumerator RefreshAtFrameEnd()
        {
            yield return null;
            _refreshRoutine = null;
            if (_dirty) RefreshNow();
        }

        private void CollectItems()
        {
            _inventoryItems.Clear();
            _equipmentItems.Clear();
            if (_inventory == null) return;
            foreach (ItemInstance item in _inventory.Items)
            {
                if (item?.Item == null) continue;
                if (item.Item.Type == ItemType.Progression) continue;
                _inventoryItems.Add(item);
                if (item.Item is Equipment) _equipmentItems.Add(item);
            }
        }

        private static GlobalVisualizerView Present(GlobalVisualizerView view, Transform parent, GameObject slotPrefab, List<ItemInstance> items)
        {
            if (view != null)
            {
                view.NotifyReset();
                return view;
            }
            if (slotPrefab == null) return null;
            view = VisualizerViewFactory.AttachTo(parent, (RectTransform)slotPrefab.transform);
            if (view != null) view.SetItems(items, BindSlot);
            return view;
        }

        private static GameObject SlotPrefab(bool wide)
        {
            InventoryManager manager = InventoryManager.Instance;
            if (manager == null) return null;
            return wide ? manager.ItemSlotWidePrefab : manager.ItemSlotSquarePrefab;
        }

        private static void BindSlot(GameObject view, ItemInstance item, int index)
        {
            ItemSlotGlobal slot = view.GetComponent<ItemSlotGlobal>();
            if (slot != null) slot.UpdateUI(item);
        }
        #endregion
    }
}
