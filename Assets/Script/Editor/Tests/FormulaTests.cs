using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Entity;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class FormulaTests
    {
        #region [Variables/Fields]
        private const float Tolerance = 0.0001f;
        #endregion

        #region [Public Methods]
        [Test]
        public void StatFormula_AppliesBaseFlatPercentThenChain()
        {
            var modifiers = new List<AttributeModifier>
            {
                new AttributeModifier(5f, ModifierType.Flat),
                new AttributeModifier(0.5f, ModifierType.PercentAdd),
                new AttributeModifier(2f, ModifierType.PercentMult)
            };
            var chain = new AdditionalChain();
            chain.Add(1, AdditionalOp.Add, 1f);

            StatResult result = StatFormula.Compute(10f, 2f, 3f, 0.5f, modifiers, chain);

            Assert.AreEqual(5f, result.FlatSum, Tolerance);
            Assert.AreEqual(0.5f, result.PercentAddSum, Tolerance);
            Assert.AreEqual(((10f * 1.5f + 3f + 2f + 5f) * 1.5f) * 2f + 1f, result.Value, Tolerance);
        }

        [Test]
        public void DerivedStatFormula_MatchesDesignTable()
        {
            DerivedStats d = DerivedStatFormula.Compute(new CoreStats { Str = 30f, Agi = 20f, Vit = 50f, Int = 150f, Dex = 25f, Luk = 9f });

            Assert.AreEqual(30f + 5f + 3f, d.AtkFlat, Tolerance);
            Assert.AreEqual(225f + 5f + 3f, d.MatkFlat, Tolerance);
            Assert.AreEqual(900f, d.WeightLimitFlat, Tolerance);
            Assert.AreEqual(20f + 1.8f, d.FleeFlat, Tolerance);
            Assert.AreEqual(25f + 3f, d.HitFlat, Tolerance);
            Assert.AreEqual(25f + 4f + 1.8f + 10f + 5f + 150f, d.StatusDefFlat, Tolerance);
            Assert.AreEqual(10f + 3.75f, d.AspdFlat, Tolerance);
            Assert.AreEqual(0.5f, d.MaxHpPercent, Tolerance);
            Assert.AreEqual(1.5f, d.MaxMpPercent, Tolerance);
            Assert.AreEqual(1f, d.HealingItemsPercent, Tolerance);
            Assert.AreEqual(10f, d.HpRecoveryFlat, Tolerance);
            Assert.AreEqual(25f + 15f, d.MpRecoveryFlat, Tolerance);
            Assert.AreEqual(4.5f + 37.5f, d.StatusDmgFlat, Tolerance);
            Assert.AreEqual(2.7f, d.CriticalRateFlat, Tolerance);
            Assert.AreEqual(1.8f, d.CritResistanceFlat, Tolerance);
            Assert.AreEqual(0.9f, d.PerfectDodgeFlat, Tolerance);
        }

        [Test]
        public void CastTimeFormula_SplitsVariableAndFixed()
        {
            var input = new CastTimeInput { Dex = 66.25f, VariablePercent = 50f, FixedFlat = 0.5f, FixedPercent = 20f };

            CastTime castTime = CastTimeFormula.Compute(input, 4f, 2f);

            Assert.AreEqual(4f * 0.5f * 0.5f, castTime.Variable, Tolerance);
            Assert.AreEqual(1.5f * 0.8f, castTime.Fixed, Tolerance);
            Assert.AreEqual(castTime.Variable + castTime.Fixed, castTime.Total, Tolerance);
        }

        [Test]
        public void CastTimeFormula_ClampsBothPartsAtZero()
        {
            var input = new CastTimeInput { VariableFlat = 10f, FixedFlat = 10f };

            CastTime castTime = CastTimeFormula.Compute(input, 1f, 1f);

            Assert.AreEqual(0f, castTime.Variable, Tolerance);
            Assert.AreEqual(0f, castTime.Fixed, Tolerance);
        }

        [Test]
        public void SkillValueFormula_CooldownAreaScaling()
        {
            Assert.AreEqual(4.5f, SkillValueFormula.Cooldown(10f, 1f, 50f), Tolerance);
            Assert.AreEqual(0f, SkillValueFormula.Cooldown(1f, 2f, 0f), Tolerance);
            Assert.AreEqual(6f, SkillValueFormula.Area(4f, 50f), Tolerance);
            Assert.AreEqual((20f * 1.5f + 10f + 2f * 3) * 2f, SkillValueFormula.Scaling(20f, 1.5f, 10f, 2f, 3, 2f), Tolerance);
        }

        [Test]
        public void ResourceFormula_AffordAndPayFloors()
        {
            Assert.IsTrue(ResourceFormula.CanAfford(new ResourceCost { Mana = 10f }, 0f, 10f));
            Assert.IsFalse(ResourceFormula.CanAfford(new ResourceCost { Mana = 10f }, 999f, 9.9f));
            Assert.IsTrue(ResourceFormula.CanAfford(new ResourceCost { Hp = 30f, IsHpCost = true }, 30f, 0f));
            Assert.IsFalse(ResourceFormula.CanAfford(new ResourceCost { Hp = 30f, IsHpCost = true }, 29f, 999f));
            Assert.AreEqual(1f, ResourceFormula.HpAfterCost(10f, 50f), Tolerance);
            Assert.AreEqual(0f, ResourceFormula.MpAfterCost(10f, 50f), Tolerance);
        }

        [Test]
        public void ResourceFormula_RegenRules()
        {
            Assert.IsTrue(ResourceFormula.TryRegen(50f, 100f, 10f, true, out float regen));
            Assert.AreEqual(60f, regen, Tolerance);
            Assert.IsTrue(ResourceFormula.TryRegen(95f, 100f, 10f, true, out float capped));
            Assert.AreEqual(100f, capped, Tolerance);
            Assert.IsFalse(ResourceFormula.TryRegen(100f, 100f, 10f, true, out _));
            Assert.IsFalse(ResourceFormula.TryRegen(50f, 100f, 0f, true, out _));
            Assert.IsFalse(ResourceFormula.TryRegen(50f, 100f, 10f, false, out _));
        }

        [Test]
        public void BasicAttackFormula_BonusAndDelay()
        {
            Assert.AreEqual(7f, BasicAttackFormula.BonusDamage(DamageType.Magic, AutoAttackType.Melee, 5f, 6f, 7f), Tolerance);
            Assert.AreEqual(5f, BasicAttackFormula.BonusDamage(DamageType.Physical, AutoAttackType.Melee, 5f, 6f, 7f), Tolerance);
            Assert.AreEqual(6f, BasicAttackFormula.BonusDamage(DamageType.Physical, AutoAttackType.Range, 5f, 6f, 7f), Tolerance);
            Assert.AreEqual(1f, BasicAttackFormula.AttackDelay(150f), Tolerance);
            Assert.AreEqual(0.5f, BasicAttackFormula.AttackDelay(175f), Tolerance);
            Assert.AreEqual(0.01f, BasicAttackFormula.AttackDelay(250f), Tolerance);
            Assert.AreEqual(1f, BasicAttackFormula.AttackDelay(0f), Tolerance);
        }

        [Test]
        public void ProgressionFormula_RefineReforgeAndExp()
        {
            Assert.AreEqual(0f, ProgressionFormula.RefineMultiplier(0), Tolerance);
            Assert.AreEqual(0.5f, ProgressionFormula.RefineMultiplier(7), Tolerance);
            Assert.AreEqual(5f, ProgressionFormula.RefineMultiplier(99), Tolerance);
            Assert.AreEqual(20f * 1.5f + 10f, ProgressionFormula.EquipmentBaseline(20f, 7, 2), Tolerance);
            Assert.AreEqual(1f, ProgressionFormula.EquipmentBaseline(0f, 0, 0), Tolerance);
            Assert.AreEqual(20f * 0.5f + 10f, ProgressionFormula.EquipmentUpgradeBonus(20f, 7, 2), Tolerance);
            Assert.AreEqual(100L, ProgressionFormula.FallbackBaseExp(1));
            Assert.AreEqual((long)(100f * UnityEngine.Mathf.Pow(10, 1.5f)), ProgressionFormula.FallbackBaseExp(10));
            Assert.AreEqual((long)(50f * UnityEngine.Mathf.Pow(10, 1.6f)), ProgressionFormula.FallbackJobExp(10));
        }

        [Test]
        public void EquipmentDatabase_ForwardsToProgressionFormula()
        {
            Assert.AreSame(ProgressionFormula.RefineTable, Data.Items.EquipmentDatabase.RefineTable);
            Assert.AreEqual(ProgressionFormula.RefineMultiplier(12), Data.Items.EquipmentDatabase.GetRefineMultiplier(12));
        }
        #endregion
    }
}
