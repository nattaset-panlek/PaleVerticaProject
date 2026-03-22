using UnityEngine;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Attributes;
using CoreSystem.Data.Items.Model;

namespace CoreSystem.Data.Tests
{
    public class DataSystemTest : MonoBehaviour
    {
        public AttributeSystem playerAttributeSystem;


        [ContextMenu("TestSystem")]
        private void test()
        {
            Database.Instance.Initialize();

            // 2. Bind Attribute Data 
            if (playerAttributeSystem != null)
            {
                GameData.Player.CharacterManager.Bind(playerAttributeSystem);
                Debug.Log($"[Attribute] Player Base STR: {GameData.Player.CharacterManager.Str}");
            }

            // Test new Database routing
            CharacterModel testChar = Database.GetCharacter(100);
            if (testChar != null) Debug.Log($"Loaded Character via Database: {testChar.Name} (HP: {testChar.BaseHP})");

            // 3. Add items to Inventory
            Item healthPotion = Database.GetItem(100);
            Item ironSword = Database.GetItem(200);

            if (healthPotion != null)
            {
                GameData.Player.Inventory.AddItem(healthPotion, 5);
                Debug.Log($"Added 5 {healthPotion.Name}. Inventory size: {GameData.Player.Inventory.GetAllItems().Count}");
            }

            if (ironSword != null)
            {
                GameData.Player.Inventory.AddItem(ironSword, 1);
                GameData.Player.Inventory.AddItem(ironSword, 1); // Should add as a separate instance (unstackable)
                Debug.Log($"Added 2 {ironSword.Name}. Inventory size: {GameData.Player.Inventory.GetAllItems().Count}");
            }

            // 4. Test Instance fetching
            foreach(var instance in GameData.Player.Inventory.GetAllItems())
            {
                Debug.Log($"Item: {instance.Name} | InstanceID: {instance.Id} | Quantity: {instance.Quantity}");
            }

            // 5. Test specific Equipment subclass
            ItemInstance checkInstance = GameData.Player.Inventory.GetFirstByModelId(200);
            if (checkInstance != null && checkInstance.Item is Equipment equipment)
            {
                Debug.Log($"Got Equipment: {equipment.Name}, Slot: {equipment.Slot}");
            }
        }
    }
}
