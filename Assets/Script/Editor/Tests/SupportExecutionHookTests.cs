using System.Collections.Generic;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Modifiers;
using CoreSystem.Skills.Visuals;
using CoreSystem.Utility;
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
            Assert.AreEqual(0.5f, parts[0].DamageMultiplier, Tolerance);
            Assert.AreEqual(0.75f, parts[1].DamageMultiplier, Tolerance);
            Assert.AreEqual(0.5f, parts[2].DamageMultiplier, Tolerance);
            Assert.AreEqual(100f, parts[1].BaseDamage, Tolerance);
            Assert.AreEqual(1f, payload.DamageMultiplier, Tolerance);
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
            Assert.AreEqual(0.6f, repeat.DamageMultiplier, Tolerance);
            Assert.AreEqual(100f, repeat.BaseDamage, Tolerance);
            Assert.AreEqual(2.6f, repeat.AOERadius, Tolerance);
            Assert.AreEqual(1f, payload.DamageMultiplier, Tolerance);
            Assert.AreEqual(100f, payload.BaseDamage, Tolerance);
            Assert.AreEqual(2f, payload.AOERadius, Tolerance);
        }
        [Test]
        public void SpellCascade_EverySplitPlaysItsOwnEruptionVisual()
        {
            var template = new GameObject("CascadeEruptionTest");
            var skill = ScriptableObject.CreateInstance<ActiveSkill>();
            skill.VisualType = VisualCategory.GroundEruption;
            skill.EruptionVfxPrefab = template;

            try
            {
                var cascade = new SpellCascadeSupportModifier();
                var payload = new SkillPayload { TargetPosition = new Vector3(10f, 0f, 0f), IsTargetPositionSet = true };
                foreach (var part in cascade.Split(payload, Vector3.zero))
                {
                    VisualManager.Ensure().PlayVisual(skill, part);
                }

                List<Vector3> spawned = SpawnedPositions(template.name + "(Clone)");
                Assert.AreEqual(3, spawned.Count);
                Assert.IsTrue(spawned.Exists(p => Vector3.Distance(p, new Vector3(10f, 5f, 0f)) < Tolerance));
                Assert.IsTrue(spawned.Exists(p => Vector3.Distance(p, new Vector3(10f, 0f, 0f)) < Tolerance));
                Assert.IsTrue(spawned.Exists(p => Vector3.Distance(p, new Vector3(10f, -5f, 0f)) < Tolerance));
            }
            finally
            {
                foreach (var clone in FindByName(template.name + "(Clone)")) Object.DestroyImmediate(clone);
                Object.DestroyImmediate(template);
                Object.DestroyImmediate(skill);
                Object.DestroyImmediate(RuntimeRoot.Get().gameObject);
            }
        }

        [Test]
        public void RepeatCast_SkipsTheIndicatorVisual_OriginalStillShowsIt()
        {
            var indicator = new GameObject("RepeatIndicatorTest");
            var skill = ScriptableObject.CreateInstance<ActiveSkill>();
            skill.VisualType = VisualCategory.GroundEruption;
            skill.EruptionVfxPrefab = indicator;
            skill.SkillIndicatorPrefab = indicator;

            try
            {
                VisualManager.Ensure().PlayVisual(skill, new SkillPayload { TargetPosition = Vector3.zero, IsTargetPositionSet = true });
                VisualManager.Ensure().PlayVisual(skill, new SkillPayload { TargetPosition = Vector3.one, IsTargetPositionSet = true, IsRepeatCast = true });

                List<Vector3> spawned = SpawnedPositions(indicator.name + "(Clone)");
                Assert.AreEqual(1, spawned.Count);
                AssertPosition(Vector3.zero, spawned[0]);
            }
            finally
            {
                foreach (var clone in FindByName(indicator.name + "(Clone)")) Object.DestroyImmediate(clone);
                Object.DestroyImmediate(indicator);
                Object.DestroyImmediate(skill);
                Object.DestroyImmediate(RuntimeRoot.Get().gameObject);
            }
        }

        [Test]
        public void RepeatCast_StillPlaysARealEffect()
        {
            var eruption = new GameObject("RepeatEruptionTest");
            var indicator = new GameObject("RepeatOtherIndicatorTest");
            var skill = ScriptableObject.CreateInstance<ActiveSkill>();
            skill.VisualType = VisualCategory.GroundEruption;
            skill.EruptionVfxPrefab = eruption;
            skill.SkillIndicatorPrefab = indicator;

            try
            {
                VisualManager.Ensure().PlayVisual(skill, new SkillPayload { TargetPosition = Vector3.one, IsTargetPositionSet = true, IsRepeatCast = true });

                Assert.AreEqual(1, FindByName(eruption.name + "(Clone)").Count);
            }
            finally
            {
                foreach (var clone in FindByName(eruption.name + "(Clone)")) Object.DestroyImmediate(clone);
                Object.DestroyImmediate(eruption);
                Object.DestroyImmediate(indicator);
                Object.DestroyImmediate(skill);
                Object.DestroyImmediate(RuntimeRoot.Get().gameObject);
            }
        }

        [Test]
        public void RepeatCast_SurvivesEchoAndCascade()
        {
            var repeat = new SkillPayload { TargetPosition = new Vector3(10f, 0f, 0f), IsRepeatCast = true };

            Assert.IsTrue(new EchoSpellSupportModifier().CreateRepeatPayload(repeat).IsRepeatCast);
            foreach (var part in new SpellCascadeSupportModifier().Split(repeat, Vector3.zero))
            {
                Assert.IsTrue(part.IsRepeatCast);
            }
            Assert.IsFalse(new SkillPayload().IsRepeatCast);
        }

        [Test]
        public void CastIndicatorShown_SkipsTheIndicatorVisualAtExecution()
        {
            var indicator = new GameObject("ShownIndicatorTest");
            var skill = ScriptableObject.CreateInstance<ActiveSkill>();
            skill.VisualType = VisualCategory.GroundEruption;
            skill.EruptionVfxPrefab = indicator;
            skill.SkillIndicatorPrefab = indicator;

            try
            {
                VisualManager.Ensure().PlayVisual(skill, new SkillPayload { IsTargetPositionSet = true, CastIndicatorShown = true });
                Assert.AreEqual(0, FindByName(indicator.name + "(Clone)").Count);

                VisualManager.Ensure().PlayVisual(skill, new SkillPayload { IsTargetPositionSet = true, CastIndicatorShown = false });
                Assert.AreEqual(1, FindByName(indicator.name + "(Clone)").Count);
            }
            finally
            {
                foreach (var clone in FindByName(indicator.name + "(Clone)")) Object.DestroyImmediate(clone);
                Object.DestroyImmediate(indicator);
                Object.DestroyImmediate(skill);
                Object.DestroyImmediate(RuntimeRoot.Get().gameObject);
            }
        }

        [Test]
        public void CastIndicators_OnePerCascadePoint_NoneForInstantCasts()
        {
            var prefab = new GameObject("CastIndicatorTest");
            var host = new GameObject("CastIndicatorHost");
            var manager = host.AddComponent<SkillIndicatorManager>();

            try
            {
                var payload = new SkillPayload { TargetPosition = new Vector3(10f, 0f, 0f), IsTargetPositionSet = true, SkillIndicatorPrefab = prefab, TotalCastTime = 1f };
                Assert.IsTrue(manager.StartCastIndicators(new SpellCascadeSupportModifier().Split(payload, Vector3.zero)));

                List<Vector3> spawned = SpawnedPositions(prefab.name + "(Clone)");
                Assert.AreEqual(3, spawned.Count);
                Assert.IsTrue(spawned.Exists(p => Vector3.Distance(p, new Vector3(10f, 5f, 0f)) < Tolerance));
                Assert.IsTrue(spawned.Exists(p => Vector3.Distance(p, new Vector3(10f, -5f, 0f)) < Tolerance));

                var instant = new SkillPayload { SkillIndicatorPrefab = prefab, TotalCastTime = 0f };
                var otherHost = new GameObject("CastIndicatorHostInstant");
                try
                {
                    Assert.IsFalse(otherHost.AddComponent<SkillIndicatorManager>().StartCastIndicators(new[] { instant }));
                }
                finally
                {
                    Object.DestroyImmediate(otherHost);
                }
            }
            finally
            {
                foreach (var clone in FindByName(prefab.name + "(Clone)")) Object.DestroyImmediate(clone);
                Object.DestroyImmediate(prefab);
                Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void VisualManager_EnsureCreatesOneRuntimeInstance()
        {
            try
            {
                VisualManager first = VisualManager.Ensure();

                Assert.IsNotNull(first);
                Assert.AreSame(first, VisualManager.Ensure());
                Assert.AreSame(first, VisualManager.Instance);
                Assert.AreEqual("[Runtime] VisualManager", first.gameObject.name);
                Assert.AreSame(RuntimeRoot.Get(), first.transform.parent);
            }
            finally
            {
                Object.DestroyImmediate(RuntimeRoot.Get().gameObject);
            }
        }
        #endregion

        #region [Private Methods]
        private static List<GameObject> FindByName(string name)
        {
            var found = new List<GameObject>();
            foreach (var transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (transform.name == name) found.Add(transform.gameObject);
            }
            return found;
        }

        private static List<Vector3> SpawnedPositions(string name)
        {
            return FindByName(name).ConvertAll(go => go.transform.position);
        }

        private static void AssertPosition(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance);
            Assert.AreEqual(expected.y, actual.y, Tolerance);
            Assert.AreEqual(expected.z, actual.z, Tolerance);
        }
        #endregion
    }
}
