using CoreSystem.Data;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Visuals;
using NUnit.Framework;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class FireBoltSkillTests
    {
        #region [Variables/Fields]
        private const int FireBoltId = 1020;
        private const string AssetPath = "Skill/ActiveSkill/Fire_Bolt_1020";
        private const float Tolerance = 0.0001f;
        #endregion

        #region [Public Methods]
        [Test]
        public void Asset_SpawnsMagicProjectileOncePerCast()
        {
            var skill = Resources.Load<ActiveSkill>(AssetPath);

            Assert.IsNotNull(skill);
            Assert.AreEqual(FireBoltId, skill.Id);
            Assert.IsInstanceOf<SelfTargeting>(skill.BaseTargeting);
            Assert.AreEqual(1, skill.Effects.Count);

            var spawn = skill.Effects[0] as SpawnProjectileEffect;
            Assert.IsNotNull(spawn);
            Assert.IsTrue(spawn.ApplyOncePerCast);
            Assert.IsNotNull(spawn.ProjectilePrefab);
            Assert.AreEqual("MagicProjectile", spawn.ProjectilePrefab.name);
            Assert.IsNotNull(spawn.ProjectilePrefab.GetComponent<ProjectileBehaviour>());
            Assert.IsInstanceOf<DamageEffect>(spawn.DamageEffectToApply);
        }

        [Test]
        public void Asset_HasNoVisualOnlyProjectile()
        {
            var skill = Resources.Load<ActiveSkill>(AssetPath);

            Assert.AreEqual(VisualCategory.Projectile, skill.VisualType);
            Assert.IsNull(skill.ProjectilePrefab);
            Assert.AreEqual(0f, skill.BaseAOERadius, Tolerance);
        }

        [Test]
        public void Database_MergesJsonNumbers()
        {
            ActiveSkill skill = Database.GetSynchronizedSkill(FireBoltId);

            Assert.IsNotNull(skill);
            Assert.AreEqual("Fire Bolt", skill.Name);
            Assert.AreEqual(ElementType.Fire, skill.SkillElement);
            Assert.AreEqual(DamageType.Magic, skill.SkillDamageType);
            Assert.AreEqual(40f, skill.BaseDamage, Tolerance);
            Assert.AreEqual(1, skill.BaseProjectileCount);
            Assert.IsInstanceOf<SpawnProjectileEffect>(skill.Effects[0]);
            Object.DestroyImmediate(skill);
        }
        #endregion
    }
}
