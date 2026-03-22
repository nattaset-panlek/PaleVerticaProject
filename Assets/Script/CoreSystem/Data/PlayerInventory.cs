using System;
using System.Collections.Generic;
using System.Linq;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;

namespace CoreSystem.Data
{
    public class PlayerInventory
    {
        private readonly Dictionary<long, ItemInstance> _itemsByInstanceId = new Dictionary<long, ItemInstance>();
        private long _nextInstanceId = 1;

        public Action OnInventoryChanged;

        public List<ItemInstance> ItemInstances => _itemsByInstanceId.Values.ToList();

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

            if (template.IsStackable)
            {
                // Find an existing stack
                var existingInstance = _itemsByInstanceId.Values
                    .FirstOrDefault(i => i.ItemId == template.Id);

                if (existingInstance != null)
                {
                    existingInstance.Quantity += amount;
                    OnInventoryChanged?.Invoke();
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
            OnInventoryChanged?.Invoke();
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
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
            return false;
        }

        public void RemoveItem(long instanceId)
        {
            if (_itemsByInstanceId.Remove(instanceId))
            {
                OnInventoryChanged?.Invoke();
            }
        }

        private long GenerateInstanceId()
        {
            // Simple unique ID logic
            return (long)(DateTime.UtcNow - new DateTime(2024, 1, 1)).TotalMilliseconds + (_nextInstanceId++);
        }
    }
}
