using System.Collections.Generic;
using CoreSystem.Entity.UniquePassives;
using CoreSystem.Skills;
using CoreSystem.Skills.Modifiers;
using NUnit.Framework;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class DoubleUniquePassiveTests
    {
        #region [Variables/Fields]
        private const float Tolerance = 0.0001f;
        private ActiveSkill _skill;
        #endregion

        #region [Public Methods]
        [SetUp]
        public void SetUp()
        {
            _skill = ScriptableObject.CreateInstance<ActiveSkill>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_skill);
        }

        [Test]
        public void DoubleSpell_OnlyRepeatsLevelOneSkills()
        {
            var system = CreateSystem("DoubleSpell");
            var repeaters = new List<ICastRepeater>();

            system.CollectCastRepeaters(new SkillInstance { SkillData = _skill, CurrentLevel = 1 }, repeaters);
            Assert.AreEqual(1, repeaters.Count);
            Assert.IsInstanceOf<DoubleSpellUniquePassive>(repeaters[0]);

            repeaters.Clear();
            system.CollectCastRepeaters(new SkillInstance { SkillData = _skill, CurrentLevel = 2 }, repeaters);
            Assert.AreEqual(0, repeaters.Count);
        }

        [Test]
        public void DoubleSpell_SecondCastDealsSixtyPercentLess_AndKeepsTheOriginal()
        {
            var passive = new DoubleSpellUniquePassive();
            var original = new SkillPayload { BaseDamage = 100f, TargetPosition = new Vector3(4f, 2f, 0f), IsTargetPositionSet = true };

            SkillPayload repeat = passive.CreateRepeatPayload(original);

            Assert.AreNotSame(original, repeat);
            Assert.AreEqual(0.4f, repeat.DamageMultiplier, Tolerance);
            Assert.AreEqual(100f, repeat.BaseDamage, Tolerance);
            Assert.AreEqual(original.TargetPosition, repeat.TargetPosition);
            Assert.AreEqual(1f, original.DamageMultiplier, Tolerance);
        }

        [Test]
        public void DoubleSpell_StacksWithEchoSpell()
        {
            var passive = new DoubleSpellUniquePassive();
            var echo = new EchoSpellSupportModifier();
            var original = new SkillPayload { BaseDamage = 100f };

            SkillPayload echoOfDouble = echo.CreateRepeatPayload(passive.CreateRepeatPayload(original));

            Assert.AreEqual(0.4f * echo.damageMultiplier, echoOfDouble.DamageMultiplier, Tolerance);
            Assert.AreEqual(100f, echoOfDouble.BaseDamage, Tolerance);
        }

        [Test]
        public void DoubleAttack_RepeatsBasicAttacksAtFullDamage()
        {
            var system = CreateSystem("DoubleAttack");
            var repeaters = new List<IBasicAttackRepeater>();

            system.CollectBasicAttackRepeaters(repeaters);

            Assert.AreEqual(1, repeaters.Count);
            Assert.AreEqual(1f, repeaters[0].RepeatDamageMultiplier, Tolerance);
            Assert.Greater(repeaters[0].RepeatDelay, 0f);
        }

        [Test]
        public void DoubleAttack_IsNotASkillRepeater()
        {
            var system = CreateSystem("DoubleAttack");
            var repeaters = new List<ICastRepeater>();

            system.CollectCastRepeaters(new SkillInstance { SkillData = _skill, CurrentLevel = 1 }, repeaters);

            Assert.AreEqual(0, repeaters.Count);
        }
        #endregion

        #region [Private Methods]
        private static UniquePassiveSystem CreateSystem(string passiveId)
        {
            var system = new UniquePassiveSystem();
            system.Initialize(null);
            system.AddPassive(passiveId);
            return system;
        }
        #endregion
    }
}
