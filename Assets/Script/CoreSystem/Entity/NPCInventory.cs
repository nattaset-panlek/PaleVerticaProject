using UnityEngine;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;

namespace CoreSystem.Entity
{
    [RequireComponent(typeof(Character))]
    public class NPCInventory : MonoBehaviour
    {
        #region [Variables/Fields]
        private PlayerInventory _inventory = new PlayerInventory();
        private Character _character;
        #endregion

        #region [Properties]
        public PlayerInventory Inventory => _inventory;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _character = GetComponent<Character>();
        }
        #endregion

        #region [Public Methods]
        public void AddItem(Item itemTemplate, int amount = 1)
        {
            if (itemTemplate != null)
            {
                _inventory.AddItem(itemTemplate, amount);
            }
        }

        public void RemoveItem(long instanceId, int amount = 1)
        {
            _inventory.RemoveItemByInstance(instanceId, amount);
        }

        public bool HasItem(int itemId, int amount = 1)
        {
            var inst = _inventory.GetFirstByModelId(itemId);
            return inst != null && inst.Quantity >= amount;
        }

        public void ConsumePotion()
        {
            // Note: Commented out because potion auto-consumption requires
            // fetching the exact logic or skill trigger for consumables.
            
            /*
            var allItems = _inventory.GetAllItems();
            var potion = allItems.Find(x => x.Item is Consumable);
            if (potion != null && _character != null)
            {
                // Execute potion logic (e.g. heal)
                // var consumableTemplate = potion.Item as Consumable;
                // consumableTemplate.Use(_character);
                
                // Remove item from inventory
                // RemoveItem(potion.Id, 1);
            }
            */
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
