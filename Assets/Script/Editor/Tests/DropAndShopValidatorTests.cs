using System.Collections.Generic;
using CoreSystem.Loot;
using CoreSystem.Shop;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class DropAndShopValidatorTests
    {
        #region [Variables/Fields]
        private const int RealItem = 1003;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [Test]
        public void ValidDropTableHasNoProblems()
        {
            Assert.AreEqual(0, Check(Table(1f, 1, 2)).Count);
        }

        [Test]
        public void ZeroChanceIsReported()
        {
            Assert.AreEqual(1, Check(Table(0f, 1, 1)).Count);
        }

        [Test]
        public void MaxBelowMinIsReported()
        {
            Assert.AreEqual(1, Check(Table(1f, 3, 2)).Count);
        }

        [Test]
        public void TableIdZeroIsReported()
        {
            DropTableModel table = Table(1f, 1, 1);
            table.Id = 0;
            Assert.AreEqual(1, Check(table).Count);
        }

        [Test]
        public void EmptyTableIsReported()
        {
            DropTableModel table = Table(1f, 1, 1);
            table.Entries.Clear();
            Assert.AreEqual(1, Check(table).Count);
        }

        [Test]
        public void ValidShopHasNoProblems()
        {
            Assert.AreEqual(0, Check(Shop(10, ShopEntry.Unlimited)).Count);
        }

        [Test]
        public void EmptyShopIdIsReported()
        {
            ShopDefinition shop = Shop(10, 1);
            shop.Id = "";
            Assert.AreEqual(1, Check(shop).Count);
        }

        [Test]
        public void NegativePriceIsReported()
        {
            Assert.AreEqual(1, Check(Shop(-1, 1)).Count);
        }

        [Test]
        public void ZeroStockIsReported()
        {
            Assert.AreEqual(1, Check(Shop(10, 0)).Count);
        }
        #endregion

        #region [Private Methods]
        private static List<string> Check(DropTableModel table)
        {
            return DropTableDataValidator.Validate(table, new List<DropTableModel> { table }, id => id == RealItem);
        }

        private static List<string> Check(ShopDefinition shop)
        {
            return ShopDataValidator.Validate(shop, new List<ShopDefinition> { shop }, id => id == RealItem);
        }

        private static DropTableModel Table(float chance, int min, int max)
        {
            return new DropTableModel
            {
                Id = 1,
                Entries = new List<DropEntry> { new DropEntry { ItemId = RealItem, Chance = chance, MinQty = min, MaxQty = max } }
            };
        }

        private static ShopDefinition Shop(int price, int stock)
        {
            return new ShopDefinition
            {
                Id = "a",
                Items = new List<ShopEntry> { new ShopEntry { ItemId = RealItem, Price = price, Stock = stock } }
            };
        }
        #endregion
    }
}
