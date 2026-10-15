using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Modifiers;
using NUnit.Framework;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class SupportExecutionHookTests
    {
        #region [Variables/Fields]
        private const float Tolerance = 0.0001f;
        #endregion

        #region [Public Methods]
        [Test]
        public void SpellCascade_SplitsIntoLeftMainRight()
        {
            var cascade = new SpellCascadeSupportModifier();
            var payload = new SkillPayload { TargetPosition = new Vector3(10f, 0f, 0f), BaseDamage = 100f };

            var parts = cascade.Split(payload, Vector3.zero);

            Assert.AreEqual(3, parts.Count);
            AssertPosition(new Vector3(10f, 5f, 0f), parts[0].TargetPosition);
            AssertPosition(new Vector3(10f, 0f, 0f), parts[1].TargetPosition);
            AssertPosition(new Vector3(10f, -5f, 0f), parts[2].TargetPosition);
            Assert.AreEqual(50f, parts[0].BaseDamage, Tolerance);
            Assert.AreEqual(75f, parts[1].BaseDamage, Tolerance);
            Assert.AreEqual(50f, parts[2].BaseDamage, Tolerance);
            Assert.AreEqual(100f, payload.BaseDamage, Tolerance);
        }

        [Test]
        public void SpellCascade_UsesUpwardDirectionWhenTargetIsOnCaster()
        {
            var cascade = new SpellCascadeSupportModifier();
            var payload = new SkillPayload { TargetPosition = Vector3.zero, BaseDamage = 10f };

            var parts = cascade.Split(payload, Vector3.zero);

            AssertPosition(new Vector3(-5f, 0f, 0f), parts[0].TargetPosition);
            AssertPosition(new Vector3(5f, 0f, 0f), parts[2].TargetPosition);
        }

        [Test]
        public void SpellCascade_OnlySplitsTargetLocationSkills()
        {
            var cascade = new SpellCascadeSupportModifier();
            var groundSkill = ScriptableObject.CreateInstance<ActiveSkill>();
            groundSkill.BaseTargeting = new TargetLocationTargeting();
            var selfSkill = ScriptableObject.CreateInstance<ActiveSkill>();
            selfSkill.BaseTargeting = new SelfTargeting();

            try
            {
                Assert.IsTrue(cascade.CanSplit(groundSkill));
                Assert.IsFalse(cascade.CanSplit(selfSkill));
                Assert.IsFalse(cascade.CanSplit(null));
            }
            finally
            {
                Object.DestroyImmediate(groundSkill);
                Object.DestroyImmediate(selfSkill);
            }
        }

        [Test]
        public void EchoSpell_RepeatPayloadIsWeakerAndWider()
        {
            var echo = new EchoSpellSupportModifier();
            var payload = new SkillPayload { BaseDamage = 100f, AOERadius = 2f };

            var repeat = echo.CreateRepeatPayload(payload);

            Assert.AreEqual(0.15f, echo.RepeatDelay, Tolerance);
            Assert.AreEqual(60f, repeat.BaseDamage, Tolerance);
            Assert.AreEqual(2.6f, repeat.AOERadius, Tolerance);
            Assert.AreEqual(100f, payload.BaseDamage, Tolerance);
            Assert.AreEqual(2f, payload.AOERadius, Tolerance);
        }
        #endregion

        #region [Private Methods]
        private static void AssertPosition(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance);
            Assert.AreEqual(expected.y, actual.y, Tolerance);
            Assert.AreEqual(expected.z, actual.z, Tolerance);
        }
        #endregion
    }
}
