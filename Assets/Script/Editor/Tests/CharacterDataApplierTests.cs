using CoreSystem.Agent;
using CoreSystem.Attributes;
using CoreSystem.Data;
using CoreSystem.Entity;
using CoreSystem.Enums;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class CharacterDataApplierTests
    {
        #region [Variables/Fields]
        private const float Tolerance = 0.0001f;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [Test]
        public void ApplyStats_SetsBaseValuesFromData()
        {
            AttributeSystem attributes = NewAttributes();
            CharacterDataApplier.ApplyStats(attributes, new CharacterModel { Level = 7, STR = 12, BaseHP = 340, Defense = 9 });

            Assert.AreEqual(7f, attributes.GetAttribute(AttributeType.BaseLevel).BaseValue, Tolerance);
            Assert.AreEqual(12f, attributes.GetAttribute(AttributeType.STR).BaseValue, Tolerance);
            Assert.AreEqual(340f, attributes.GetAttribute(AttributeType.MaxHP).BaseValue, Tolerance);
            Assert.AreEqual(9f, attributes.GetAttribute(AttributeType.Defense).BaseValue, Tolerance);
        }

        [Test]
        public void ApplyStats_ZeroMoveSpeedKeepsPrefabValue()
        {
            AttributeSystem attributes = NewAttributes();
            float before = attributes.GetAttribute(AttributeType.MoveSpeed).BaseValue;
            CharacterDataApplier.ApplyStats(attributes, new CharacterModel { MoveSpeed = 0f });
            Assert.AreEqual(before, attributes.GetAttribute(AttributeType.MoveSpeed).BaseValue, Tolerance);
        }

        [Test]
        public void FillVitals_StartsAtFullHp()
        {
            AttributeSystem attributes = NewAttributes();
            CharacterDataApplier.ApplyStats(attributes, new CharacterModel { BaseHP = 340, BaseMP = 20 });
            CharacterDataApplier.FillVitals(attributes);
            Assert.AreEqual(attributes.GetAttribute(AttributeType.MaxHP).CurrentValue, attributes.GetAttribute(AttributeType.HP).BaseValue, Tolerance);
        }

        [Test]
        public void ApplyBehavior_CopiesAiSettings()
        {
            var profile = new AgentProfile();
            CharacterDataApplier.ApplyBehavior(profile, new CharacterModel { Combat = CombatStyle.Ranged, DetectionRadius = 14f, CanRoam = false });

            Assert.AreEqual(CombatStyle.Ranged, profile.Combat);
            Assert.AreEqual(14f, profile.DetectionRadius, Tolerance);
            Assert.IsFalse(profile.CanRoam);
        }
        #endregion

        #region [Private Methods]
        private static AttributeSystem NewAttributes()
        {
            var attributes = new AttributeSystem();
            attributes.Initialize(null);
            return attributes;
        }
        #endregion
    }
}
