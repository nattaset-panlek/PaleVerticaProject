using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Editor.UI;
using CoreSystem.UI.Inventory.Utility;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class ItemSlotDisplayTests
    {
        #region [Public Methods]
        [TestCase(0, 0)]
        [TestCase(-3, 0)]
        [TestCase(1, 1)]
        [TestCase(4, 1)]
        [TestCase(5, 2)]
        [TestCase(16, 4)]
        [TestCase(17, 5)]
        [TestCase(20, 5)]
        [TestCase(99, 5)]
        public void StarsForRefine_MapsRefineToFiveStars(int refine, int stars)
        {
            Assert.AreEqual(stars, ItemSlotDisplay.StarsForRefine(refine));
        }

        [Test]
        public void From_Null_IsEmpty()
        {
            ItemSlotDisplay display = ItemSlotDisplay.From(null);
            Assert.AreEqual(ItemSlotKind.Empty, display.Kind);
            Assert.IsFalse(display.ShowQuantity);
            Assert.IsFalse(display.ShowStars);
        }

        [Test]
        public void From_Equipment_ShowsRefineAdvancementAndSockets()
        {
            Equipment model = FirstEquipment();
            var instance = new EquipmentInstance(1, model.Id) { Refine = 6, Reforge = 12, IsEquipped = true };
            ItemSlotDisplay display = ItemSlotDisplay.From(instance);

            Assert.AreEqual(ItemSlotKind.Instance, display.Kind);
            Assert.AreEqual(2, display.Stars);
            Assert.AreEqual(12, display.Advancement);
            Assert.IsTrue(display.Equipped);
            Assert.IsFalse(display.ShowQuantity);
            Assert.AreEqual(Mathf.Clamp(model.MaxSockets, 0, ItemSlotDisplay.MaxSockets), display.SocketSlots);
            Assert.AreEqual(0, display.SocketFilled);
        }

        [Test]
        public void From_Stackable_ShowsQuantityOnly()
        {
            Item stackable = Database.GetItems.Find(i => i.IsStackable);
            if (stackable == null) Assert.Ignore("No stackable item in Item.json.");
            ItemSlotDisplay display = ItemSlotDisplay.From(new ItemInstance(2, stackable.Id, 25));

            Assert.AreEqual(ItemSlotKind.Stack, display.Kind);
            Assert.IsTrue(display.ShowQuantity);
            Assert.AreEqual(25, display.Quantity);
            Assert.IsFalse(display.ShowStars);
            Assert.IsFalse(display.ShowSockets);
            Assert.IsFalse(display.ShowAdvancement);
        }

        [Test]
        public void ArtPrefab_ShowHidesPartsByKind()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ItemSlotPrefabBuilder.ArtPrefabPath);
            if (prefab == null) Assert.Ignore("ItemSlot Art prefab not built yet.");
            var go = Object.Instantiate(prefab);
            try
            {
                var view = go.GetComponent<ItemSlotArtView>();
                view.Show(new ItemSlotDisplay { Kind = ItemSlotKind.Stack, Quantity = 7 });
                Assert.IsTrue(go.transform.Find("Art/Quantity").gameObject.activeSelf);
                Assert.IsFalse(go.transform.Find("Art/Advancement").gameObject.activeSelf);
                Assert.IsFalse(go.transform.Find("Art/Refine").gameObject.activeSelf);
                Assert.IsFalse(go.transform.Find("Art/Sockets").gameObject.activeSelf);

                view.Show(new ItemSlotDisplay { Kind = ItemSlotKind.Instance, Stars = 3, Advancement = 4, SocketSlots = 3, SocketFilled = 1 });
                Assert.IsFalse(go.transform.Find("Art/Quantity").gameObject.activeSelf);
                Assert.IsTrue(go.transform.Find("Art/Advancement").gameObject.activeSelf);
                Assert.IsTrue(go.transform.Find("Art/Refine").gameObject.activeSelf);
                Assert.IsTrue(go.transform.Find("Art/Sockets/Socket 3").gameObject.activeSelf);
                Assert.IsFalse(go.transform.Find("Art/Sockets/Socket 4").gameObject.activeSelf);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
        #endregion

        #region [Private Methods]
        private static Equipment FirstEquipment()
        {
            Item item = Database.GetItems.Find(i => i is Equipment);
            if (item == null) Assert.Ignore("No equipment in Item.json.");
            return (Equipment)item;
        }
        #endregion
    }
}
