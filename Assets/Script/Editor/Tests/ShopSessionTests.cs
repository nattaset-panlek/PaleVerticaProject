using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Shop;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class ShopSessionTests
    {
        #region [Variables/Fields]
        private const int CreditId = 1000;
        #endregion

        #region [Public Methods]
        [Test]
        public void Parse_ReadsShopsAndDropsBadEntries()
        {
            const string json = "{\"Shops\":[{\"Id\":\"a\",\"Name\":\"A\",\"Items\":[{\"ItemId\":1,\"Price\":10,\"Stock\":2},{\"ItemId\":2,\"Price\":-5}]},{\"Name\":\"no id\"}]}";
            List<ShopDefinition> shops = ShopLoader.Parse(json);
            Assert.AreEqual(1, shops.Count);
            Assert.AreEqual("a", shops[0].Id);
            Assert.AreEqual(1, shops[0].Items.Count);
            Assert.AreEqual(2, shops[0].Items[0].Stock);
        }

        [Test]
        public void Parse_MissingStockMeansUnlimited()
        {
            List<ShopDefinition> shops = ShopLoader.Parse("{\"Shops\":[{\"Id\":\"a\",\"Items\":[{\"ItemId\":1,\"Price\":10}]}]}");
            Assert.IsTrue(shops[0].Items[0].IsUnlimited);
        }

        [Test]
        public void AddToCart_ClampsToStockAndMergesLines()
        {
            ShopSession session = NewSession(Entry(1, 10, 3), Entry(2, 5, -1));
            Assert.AreEqual(2, session.AddToCart(0, 2));
            Assert.AreEqual(1, session.AddToCart(0, 5));
            Assert.AreEqual(0, session.AddToCart(0, 1));
            Assert.AreEqual(1, session.Cart.Count);
            Assert.AreEqual(3, session.InCart(0));
            Assert.AreEqual(99, session.AddToCart(1, 99));
            Assert.AreEqual(30 + 99 * 5, session.Total);
        }

        [Test]
        public void AddToCart_RespectsCartCapacity()
        {
            var entries = new ShopEntry[ShopSession.CartCapacity + 1];
            for (int i = 0; i < entries.Length; i++) entries[i] = Entry(i + 1, 1, -1);
            ShopSession session = NewSession(entries);
            for (int i = 0; i < ShopSession.CartCapacity; i++) Assert.AreEqual(1, session.AddToCart(i, 1));
            Assert.IsTrue(session.IsCartFull);
            Assert.AreEqual(0, session.AddToCart(ShopSession.CartCapacity, 1));
            Assert.AreEqual(1, session.AddToCart(0, 1));
        }

        [Test]
        public void SetCartQuantity_ZeroRemovesLine()
        {
            ShopSession session = NewSession(Entry(1, 10, -1));
            session.AddToCart(0, 4);
            session.SetCartQuantity(0, 0);
            Assert.AreEqual(0, session.Cart.Count);
        }

        [Test]
        public void Checkout_SpendsCreditAddsItemsAndReducesStock()
        {
            Item potion = Database.GetItem(2002);
            if (potion == null || Database.GetItem(CreditId) == null) Assert.Ignore("Item.json has no credit or potion.");
            var inventory = new PlayerInventory();
            inventory.AddCredit(1000);
            ShopSession session = NewSession(Entry(potion.Id, 120, 5));
            session.AddToCart(0, 5);

            Assert.AreEqual(ShopPurchaseResult.Success, session.Checkout(inventory));
            Assert.AreEqual(400, inventory.GetCreditAmount());
            Assert.AreEqual(5, CountOf(inventory, potion.Id));
            Assert.IsTrue(session.IsSoldOut(0));
            Assert.AreEqual(0, session.Cart.Count);
        }

        [Test]
        public void Checkout_NotEnoughCreditChangesNothing()
        {
            Item potion = Database.GetItem(2002);
            if (potion == null || Database.GetItem(CreditId) == null) Assert.Ignore("Item.json has no credit or potion.");
            var inventory = new PlayerInventory();
            inventory.AddCredit(100);
            ShopSession session = NewSession(Entry(potion.Id, 120, -1));
            session.AddToCart(0, 1);

            Assert.AreEqual(ShopPurchaseResult.NotEnoughCredit, session.Checkout(inventory));
            Assert.AreEqual(100, inventory.GetCreditAmount());
            Assert.AreEqual(0, CountOf(inventory, potion.Id));
            Assert.AreEqual(1, session.Cart.Count);
        }

        [Test]
        public void Checkout_EmptyCart()
        {
            ShopSession session = NewSession(Entry(1, 10, -1));
            Assert.AreEqual(ShopPurchaseResult.EmptyCart, session.Checkout(new PlayerInventory()));
        }

        [Test]
        public void AddToCart_EquipmentNeverStacks()
        {
            Item equipment = Database.GetItems.Find(item => item is CoreSystem.Data.Items.Model.Equipment);
            if (equipment == null) Assert.Ignore("Item.json has no equipment.");
            ShopSession session = NewSession(Entry(equipment.Id, 10, 3));

            Assert.AreEqual(2, session.AddToCart(0, 2));
            Assert.AreEqual(1, session.AddToCart(0, 5));
            Assert.AreEqual(3, session.Cart.Count);
            Assert.AreEqual(1, session.Cart[0].Quantity);
            Assert.AreEqual(3, session.InCart(0));
            Assert.AreEqual(30, session.Total);
            session.RemoveLine(0);
            Assert.AreEqual(2, session.InCart(0));
        }

        [Test]
        public void CreateRandomShop_SellsRealItemsWithPricesAndNoCurrency()
        {
            ShopDefinition first = MerchantFactory.CreateRandomShop(new System.Random(7));
            ShopDefinition second = MerchantFactory.CreateRandomShop(new System.Random(7));
            Assert.AreNotEqual(first.Id, second.Id);
            Assert.GreaterOrEqual(first.Items.Count, MerchantFactory.MinWares);
            foreach (ShopEntry entry in first.Items)
            {
                Assert.IsNotNull(entry.Item);
                Assert.Greater(entry.Price, 0);
                Assert.AreNotEqual(ItemType.Currency, entry.Item.Type);
                Assert.AreNotEqual(ItemType.Progression, entry.Item.Type);
            }
        }

        [Test]
        public void ShopJson_LoadsAndPointsAtRealItems()
        {
            ShopDatabase.Reload();
            Assert.Greater(ShopDatabase.All.Count, 0);
            foreach (ShopDefinition shop in ShopDatabase.All)
            {
                foreach (ShopEntry entry in shop.Items) Assert.IsNotNull(entry.Item, $"Shop '{shop.Id}' sells unknown item {entry.ItemId}.");
            }
        }
        #endregion

        #region [Private Methods]
        private static ShopEntry Entry(int itemId, int price, int stock) => new ShopEntry { ItemId = itemId, Price = price, Stock = stock };

        private static ShopSession NewSession(params ShopEntry[] entries)
        {
            return new ShopSession(new ShopDefinition { Id = "test", Name = "Test", Items = new List<ShopEntry>(entries) });
        }

        private static int CountOf(PlayerInventory inventory, int itemId)
        {
            int count = 0;
            foreach (ItemInstance instance in inventory.GetAllItems())
            {
                if (instance.ItemId == itemId) count += instance.Quantity;
            }
            return count;
        }
        #endregion
    }
}
