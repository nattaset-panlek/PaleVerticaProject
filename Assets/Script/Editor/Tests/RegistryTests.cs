using System.Collections.Generic;
using CoreSystem.Entity.UniquePassives;
using CoreSystem.Skills;
using CoreSystem.Skills.Modifiers;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class RegistryTests
    {
        #region [Variables/Fields]
        private static readonly Dictionary<int, System.Type> ExpectedSupportTypes = new Dictionary<int, System.Type>
        {
            { 1001, typeof(MultiShotSupportModifier) },
            { 1002, typeof(EchoSupportModifier) },
            { 1003, typeof(PierceSupportModifier) },
            { 1004, typeof(ChainSupportModifier) },
            { 1005, typeof(AreaIncreaseSupportModifier) },
            { 1006, typeof(DamageUpSupportModifier) },
            { 1007, typeof(CooldownReductionSupportModifier) },
            { 1008, typeof(CostReductionSupportModifier) },
            { 1009, typeof(DurationUpSupportModifier) },
            { 1010, typeof(StatusChanceUpSupportModifier) },
            { 1011, typeof(BurnChanceSupportModifier) },
            { 1012, typeof(FreezeChanceSupportModifier) },
            { 1013, typeof(KnockbackSupportModifier) },
            { 1014, typeof(CriticalPowerSupportModifier) },
            { 1015, typeof(ManaLeechSupportModifier) },
            { 1016, typeof(LifeLeechSupportModifier) },
            { 1017, typeof(ProjectileSpeedSupportModifier) },
            { 1018, typeof(SplitProjectileSupportModifier) },
            { 1019, typeof(HomingSupportModifier) },
            { 1020, typeof(EmpoweredCastSupportModifier) },
            { 1021, typeof(EchoSpellSupportModifier) },
            { 1022, typeof(SpellCascadeSupportModifier) }
        };

        private static readonly Dictionary<string, System.Type> ExpectedPassiveTypes = new Dictionary<string, System.Type>
        {
            { "Vampire", typeof(VampireUniquePassive) },
            { "TimeCollector", typeof(TimeCollectorUniquePassive) },
            { "DepressionChaos", typeof(DepressionChaosUniquePassive) },
            { "Thorn", typeof(ThornUniquePassive) },
            { "ManaLeech", typeof(ManaLeechUniquePassive) },
            { "Execution", typeof(ExecutionUniquePassive) },
            { "BloodBleeding", typeof(BloodBleedingUniquePassive) },
            { "Bleeding", typeof(BleedingUniquePassive) },
            { "CurseOfKanix", typeof(CurseOfKanixUniquePassive) },
            { "BlessingOfMagearna", typeof(BlessingOfMagearnaUniquePassive) },
            { "BloodMage", typeof(BloodMageUniquePassive) },
            { "EldritchFaith", typeof(EldritchFaithUniquePassive) },
            { "Rock", typeof(RockUniquePassive) }
        };
        #endregion

        #region [Public Methods]
        [Test]
        public void SupportModifier_RegistryMatchesKnownIds()
        {
            CollectionAssert.AreEquivalent(ExpectedSupportTypes, SupportModifier.RegisteredTypes);
        }

        [Test]
        public void SupportModifier_CreateReturnsCorrectTypeAndId()
        {
            foreach (var pair in ExpectedSupportTypes)
            {
                var modifier = SupportModifier.Create(pair.Key);
                Assert.IsNotNull(modifier, $"id {pair.Key}");
                Assert.AreEqual(pair.Value, modifier.GetType(), $"id {pair.Key}");
                Assert.AreEqual(pair.Key, modifier.ID, $"id {pair.Key}");
            }
        }

        [Test]
        public void SupportModifier_UnknownIdReturnsNull()
        {
            Assert.IsNull(SupportModifier.Create(0));
            Assert.IsNull(SupportModifier.Create(9999));
        }

        [Test]
        public void UniquePassive_RegistryMatchesKnownIds()
        {
            CollectionAssert.AreEquivalent(ExpectedPassiveTypes, UniquePassiveSystem.RegisteredTypes);
        }

        [Test]
        public void UniquePassive_CreateReturnsCorrectType()
        {
            foreach (var pair in ExpectedPassiveTypes)
            {
                var passive = UniquePassiveSystem.CreatePassive(pair.Key);
                Assert.IsNotNull(passive, pair.Key);
                Assert.AreEqual(pair.Value, passive.GetType(), pair.Key);
            }
        }

        [Test]
        public void UniquePassive_InternalAndUnknownIdsReturnNull()
        {
            Assert.IsNull(UniquePassiveSystem.CreatePassive(BloodBleedingDOTUniquePassive.PassiveId));
            Assert.IsNull(UniquePassiveSystem.CreatePassive("DoesNotExist"));
            Assert.IsNull(UniquePassiveSystem.CreatePassive(null));
        }
        #endregion
    }
}
