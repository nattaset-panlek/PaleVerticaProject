using CoreSystem;
using CoreSystem.Attributes;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class AttributeMathTests
    {
        #region [Variables/Fields]
        private const float Tolerance = 0.0001f;
        #endregion

        #region [Public Methods]
        [Test]
        public void Recalculate_AppliesBaseThenFlatThenPercentThenAdditional()
        {
            var attribute = new Attribute { BaseValue = 10f };
            attribute.AddModifier(new AttributeModifier(5f, ModifierType.Flat));
            attribute.AddModifier(new AttributeModifier(0.1f, ModifierType.PercentAdd));
            attribute.AddModifier(new AttributeModifier(2f, ModifierType.PercentMult));

            Assert.AreEqual(33f, attribute.CurrentValue, Tolerance);

            attribute.Additional.Add(2, AdditionalOp.Multiply, 2f);
            attribute.Additional.Add(1, AdditionalOp.Add, 1f);
            attribute.Recalculate();

            Assert.AreEqual(68f, attribute.CurrentValue, Tolerance);
        }

        [Test]
        public void Recalculate_StatBonusesApplyBeforeModifiers()
        {
            var attribute = new Attribute { BaseValue = 100f, InitialValue = 10f };
            attribute.StatPercentBonus = 0.5f;
            attribute.StatFlatBonus = 20f;
            attribute.AddModifier(new AttributeModifier(0.5f, ModifierType.PercentAdd));

            Assert.AreEqual((100f * 1.5f + 20f + 10f) * 1.5f, attribute.CurrentValue, Tolerance);
        }

        [Test]
        public void AdditionalChain_SortsByOrder()
        {
            var chain = new AdditionalChain();
            chain.Add(5, AdditionalOp.Multiply, 2f);
            chain.Add(1, AdditionalOp.Add, 3f);

            Assert.AreEqual((10f + 3f) * 2f, chain.Apply(10f), Tolerance);
        }

        [Test]
        public void AdditionalChain_KeepsInsertionOrderForEqualOrder()
        {
            var addFirst = new AdditionalChain();
            addFirst.Add(2, AdditionalOp.Add, 5f);
            addFirst.Add(2, AdditionalOp.Multiply, 2f);

            var multiplyFirst = new AdditionalChain();
            multiplyFirst.Add(2, AdditionalOp.Multiply, 2f);
            multiplyFirst.Add(2, AdditionalOp.Add, 5f);

            Assert.AreEqual(30f, addFirst.Apply(10f), Tolerance);
            Assert.AreEqual(25f, multiplyFirst.Apply(10f), Tolerance);
        }

        [Test]
        public void AdditionalChain_KeepsInsertionOrderBeyondSmallListSize()
        {
            var chain = new AdditionalChain();
            for (int i = 0; i < 40; i++)
            {
                chain.Add(i % 3, AdditionalOp.Add, i, "t" + i);
            }

            int lastOrder = int.MinValue;
            int lastValueInOrder = -1;
            foreach (var entry in chain.Entries)
            {
                if (entry.Order != lastOrder)
                {
                    Assert.Greater(entry.Order, lastOrder);
                    lastOrder = entry.Order;
                    lastValueInOrder = -1;
                }
                Assert.Greater(entry.Value, lastValueInOrder);
                lastValueInOrder = (int)entry.Value;
            }
        }

        [Test]
        public void AdditionalChain_RemoveByTagOnlyRemovesMatchingEntries()
        {
            var chain = new AdditionalChain();
            chain.Add(1, AdditionalOp.Add, 5f, "Passive");
            chain.Add(1, AdditionalOp.Add, 7f, "Buff");
            chain.RemoveByTag("Passive");

            Assert.AreEqual(1, chain.Entries.Count);
            Assert.AreEqual(17f, chain.Apply(10f), Tolerance);
        }

        [Test]
        public void DerivedStats_FollowCoreAttributeFormulas()
        {
            var stats = TestStats.Create(str: 30f, agi: 20f, vit: 50f, @int: 60f, dex: 25f, luk: 9f);

            Assert.AreEqual(30f + 25f / 5f + 9f / 3f, stats.GetAttribute(AttributeType.ATK).StatFlatBonus, Tolerance);
            Assert.AreEqual(60f * 1.5f + 25f / 5f + 9f / 3f, stats.GetAttribute(AttributeType.MATK).StatFlatBonus, Tolerance);
            Assert.AreEqual(20f + 9f / 5f, stats.GetAttribute(AttributeType.Flee).StatFlatBonus, Tolerance);
            Assert.AreEqual(25f + 9f / 3f, stats.GetAttribute(AttributeType.Hit).StatFlatBonus, Tolerance);
            Assert.AreEqual(20f * 0.5f + 25f * 0.15f, stats.GetAttribute(AttributeType.ASPD).StatFlatBonus, Tolerance);
            Assert.AreEqual(50f * 0.01f, stats.GetAttribute(AttributeType.MaxHP).StatPercentBonus, Tolerance);
            Assert.AreEqual(9f * 0.3f, stats.GetAttribute(AttributeType.CriticalRate).StatFlatBonus, Tolerance);
            Assert.AreEqual(50f / 2f + 20f / 5f + 9f / 5f + 50f / 5f + 25f / 5f + 60f, stats.GetAttribute(AttributeType.StatusDEF).StatFlatBonus, Tolerance);
            Assert.AreEqual(60f / 6f, stats.GetAttribute(AttributeType.MPRecovery).StatFlatBonus, Tolerance);
        }

        [Test]
        public void DerivedStats_MpRecoveryGainsBonusFrom120Int()
        {
            var stats = TestStats.Create(@int: 140f);

            Assert.AreEqual(140f / 6f + (140f - 120f) / 2f, stats.GetAttribute(AttributeType.MPRecovery).StatFlatBonus, Tolerance);
        }
        #endregion
    }
}
