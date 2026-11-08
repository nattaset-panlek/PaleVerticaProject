using System;
using System.Collections.Generic;
using System.Linq;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;

namespace CoreSystem.Data
{
    public class PlayerInventory
    {
        #region [Variables/Fields]
        private readonly Dictionary<long, ItemInstance> _itemsByInstanceId = new Dictionary<long, ItemInstance>();
        private long _nextInstanceId = 1;
        private int _batchDepth;
        private bool _batchChanged;

        public Action OnInventoryChanged;
        public Action<Item, int> OnProgressionReceived;
        #endregion

        #region [Properties]
        public List<ItemInstance> ItemInstances => _itemsByInstanceId.Values.ToList();
        public Dictionary<long, ItemInstance>.ValueCollection Items => _itemsByInstanceId.Values;
        public int Count => _itemsByInstanceId.Count;
        public bool IsBatching => _batchDepth > 0;
        #endregion

        #region [Public Methods]
        public List<ItemInstance> GetAllItems()
        {
            return ItemInstances;
        }

        public ItemInstance GetByInstanceId(long instanceId)
        {
            _itemsByInstanceId.TryGetValue(instanceId, out ItemInstance instance);
            return instance;
        }

        public ItemInstance GetFirstByModelId(int itemId)
        {
            return _itemsByInstanceId.Values.FirstOrDefault(instance => instance.ItemId == itemId);
        }

        public void BeginBatch()
        {
            _batchDepth++;
        }

        public void EndBatch()
        {
            if (_batchDepth == 0) return;
            _batchDepth--;
            if (_batchDepth > 0 || !_batchChanged) return;
            _batchChanged = false;
            OnInventoryChanged?.Invoke();
        }

        public void AddItems(IEnumerable<Item> templates, int amount = 1)
        {
            if (templates == null) return;
            BeginBatch();
            try
            {
                foreach (Item template in templates) AddItem(template, amount);
            }
            finally
            {
                EndBatch();
            }
        }

        public void AddItem(int id, int qty = 1)
        {
            Item template = Database.GetItem(id);
            if (template != null)
            {
                AddItem(template, qty);
            }
        }

        public void AddItem(Item template, int amount = 1)
        {
            if (template == null || amount <= 0) return;

            if (template.Type == ItemType.Progression)
            {
                OnProgressionReceived?.Invoke(template, amount);
                return;
            }

            if (template.IsStackable)
            {
                // Find an existing stack
                var existingInstance = _itemsByInstanceId.Values
                    .FirstOrDefault(i => i.ItemId == template.Id);

                if (existingInstance != null)
                {
                    existingInstance.Quantity += amount;
                    RaiseChanged();
                    return;
                }
            }

            int loopCount = template.IsStackable ? 1 : amount;
            int addQty = template.IsStackable ? amount : 1;

            for (int i = 0; i < loopCount; i++)
            {
                ItemInstance newInstance;
                if (template is Equipment)
                {
                    newInstance = new EquipmentInstance(GenerateInstanceId(), template.Id, addQty);
                }
                else if (template is Costume)
                {
                    newInstance = new CostumeInstance(GenerateInstanceId(), template.Id, addQty);
                }
                else
                {
                    newInstance = new ItemInstance(GenerateInstanceId(), template.Id, addQty);
                }
                _itemsByInstanceId.Add(newInstance.Id, newInstance);
            }

            // [FIX] Invoke AFTER items are added to dictionary
            RaiseChanged();
        }

        public bool RemoveItemByInstance(long instanceId, int amount = 1)
        {
            if (_itemsByInstanceId.TryGetValue(instanceId, out ItemInstance instance))
            {
                bool removed = false;
                if (instance.Quantity > amount)
                {
                    instance.Quantity -= amount;
                    removed = true;
                }
                else if (instance.Quantity == amount)
                {
                    _itemsByInstanceId.Remove(instanceId);
                    removed = true;
                }

                if (removed)
                {
                    RaiseChanged();
                    return true;
                }
            }
            return false;
        }

        public void RemoveItem(long instanceId)
        {
            if (_itemsByInstanceId.Remove(instanceId))
            {
                RaiseChanged();
            }
        }

        public void AddDirectInstance(ItemInstance instance)
        {
            if (instance == null) return;
            _itemsByInstanceId[instance.Id] = instance;
            RaiseChanged();
        }

        public void Clear()
        {
            _itemsByInstanceId.Clear();
            RaiseChanged();
        }

        public int GetCreditAmount()
        {
            foreach (var inst in _itemsByInstanceId.Values)
            {
                if (inst.ItemId == 1000 || string.Equals(inst.Name, "Xeny Credit", StringComparison.OrdinalIgnoreCase))
                {
                    return inst.Quantity;
                }
            }
            return 0;
        }

        public bool HasCredit(int amount)
        {
            return GetCreditAmount() >= amount;
        }

        public bool SpendCredit(int amount)
        {
            if (amount <= 0) return true;
            foreach (var inst in _itemsByInstanceId.Values)
            {
                if (inst.ItemId == 1000 || string.Equals(inst.Name, "Xeny Credit", StringComparison.OrdinalIgnoreCase))
                {
                    if (inst.Quantity >= amount)
                    {
                        return RemoveItemByInstance(inst.Id, amount);
                    }
                    return false;
                }
            }
            return false;
        }

        public void AddCredit(int amount)
        {
            if (amount <= 0) return;
            Item creditItem = Database.GetItem(1000);
            if (creditItem != null)
            {
                AddItem(creditItem, amount);
            }
        }
        #endregion

        #region [Private Methods]
        private void RaiseChanged()
        {
            if (_batchDepth > 0)
            {
                _batchChanged = true;
                return;
            }
            OnInventoryChanged?.Invoke();
        }

        private long GenerateInstanceId()
        {
            // Simple unique ID logic
            return (long)(DateTime.UtcNow - new DateTime(2024, 1, 1)).TotalMilliseconds + (_nextInstanceId++);
        }
        #endregion
    }
}
