using System;
using System.Collections.Generic;
using CoreSystem.Loot;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class LootFormulaTests
    {
        #region [Variables/Fields]
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [Test]
        public void Roll_DropsEntryWhenRollIsBelowChance()
        {
            var rolls = LootFormula.Roll(Table(0.5f, 1, 1), Sequence(0.49f, 0f));
            Assert.AreEqual(1, rolls.Count);
            Assert.AreEqual(7, rolls[0].ItemId);
        }

        [Test]
        public void Roll_SkipsEntryWhenRollEqualsChance()
        {
            Assert.AreEqual(0, LootFormula.Roll(Table(0.5f, 1, 1), Sequence(0.5f)).Count);
        }

        [Test]
        public void Roll_ZeroChanceNeverDrops()
        {
            Assert.AreEqual(0, LootFormula.Roll(Table(0f, 1, 1), Sequence(0f)).Count);
        }

        [TestCase(0f, 5)]
        [TestCase(0.999f, 20)]
        public void Roll_QuantityStaysInRange(float quantityRoll, int expected)
        {
            var rolls = LootFormula.Roll(Table(1f, 5, 20), Sequence(0f, quantityRoll));
            Assert.AreEqual(expected, rolls[0].Quantity);
        }

        [Test]
        public void Roll_NullTableGivesNothing()
        {
            Assert.AreEqual(0, LootFormula.Roll(null, Sequence(0f)).Count);
        }

        [Test]
        public void CanPickUp_OnlyOwnerDuringLock()
        {
            object owner = new object();
            object other = new object();
            Assert.IsTrue(LootFormula.CanPickUp(owner, owner, 1f, 10f));
            Assert.IsFalse(LootFormula.CanPickUp(owner, other, 9.9f, 10f));
        }

        [Test]
        public void CanPickUp_AnyoneAfterLockOrWithoutOwner()
        {
            object other = new object();
            Assert.IsTrue(LootFormula.CanPickUp(new object(), other, 10f, 10f));
            Assert.IsTrue(LootFormula.CanPickUp(null, other, 0f, 10f));
        }
        #endregion

        #region [Private Methods]
        private static DropTableModel Table(float chance, int min, int max)
        {
            return new DropTableModel
            {
                Entries = new List<DropEntry> { new DropEntry { ItemId = 7, Chance = chance, MinQty = min, MaxQty = max } }
            };
        }

        private static Func<float> Sequence(params float[] values)
        {
            int index = 0;
            return () => values[index++];
        }
        #endregion
    }
}
