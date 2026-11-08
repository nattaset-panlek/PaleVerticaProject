using CoreSystem.Combat;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Modifiers;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class SupportModifierPayloadTests
    {
        #region [Variables/Fields]
        private const float Tolerance = 0.0001f;
        #endregion

        #region [Public Methods]
        [Test]
        public void Pierce_AddsPierceCount()
        {
            var payload = Modify(new PierceSupportModifier());
            Assert.AreEqual(2, payload.PierceCount);
        }

        [Test]
        public void Chain_AddsChainCountAndRange()
        {
            var payload = Modify(new ChainSupportModifier());
            Assert.AreEqual(2, payload.ChainCount);
            Assert.AreEqual(5f, payload.ChainRange, Tolerance);
        }

        [Test]
        public void Homing_TurnsHomingOn()
        {
            Assert.IsTrue(Modify(new HomingSupportModifier()).IsHoming);
        }

        [Test]
        public void ProjectileSpeed_RaisesSpeedMultiplier()
        {
            Assert.AreEqual(1.3f, Modify(new ProjectileSpeedSupportModifier()).ProjectileSpeedMultiplier, Tolerance);
        }

        [Test]
        public void Knockback_SetsDistance()
        {
            Assert.AreEqual(2f, Modify(new KnockbackSupportModifier()).KnockbackDistance, Tolerance);
        }

        [Test]
        public void LifeAndManaLeech_SetPercents()
        {
            Assert.AreEqual(10f, Modify(new LifeLeechSupportModifier()).LifeLeechPercent, Tolerance);
            Assert.AreEqual(10f, Modify(new ManaLeechSupportModifier()).ManaLeechPercent, Tolerance);
        }

        [Test]
        public void BurnAndFreeze_SetChances()
        {
            Assert.AreEqual(20f, Modify(new BurnChanceSupportModifier()).BurnChance, Tolerance);
            Assert.AreEqual(20f, Modify(new FreezeChanceSupportModifier()).FreezeChance, Tolerance);
        }

        [Test]
        public void StatusChanceUp_AddsBonus()
        {
            Assert.AreEqual(15f, Modify(new StatusChanceUpSupportModifier()).StatusChanceBonus, Tolerance);
        }

        [Test]
        public void DurationUp_RaisesDurationMultiplier()
        {
            Assert.AreEqual(1.2f, Modify(new DurationUpSupportModifier()).DurationMultiplier, Tolerance);
        }

        [Test]
        public void StackedSupports_Accumulate()
        {
            var payload = new SkillPayload();
            new PierceSupportModifier().ModifyPayload(ref payload);
            new PierceSupportModifier().ModifyPayload(ref payload);
            new ProjectileSpeedSupportModifier().ModifyPayload(ref payload);
            new ProjectileSpeedSupportModifier().ModifyPayload(ref payload);

            Assert.AreEqual(4, payload.PierceCount);
            Assert.AreEqual(1.69f, payload.ProjectileSpeedMultiplier, Tolerance);
        }

        [Test]
        public void Supports_DoNotTouchDamageNumbers()
        {
            var payload = new SkillPayload { BaseDamage = 100f };
            new PierceSupportModifier().ModifyPayload(ref payload);
            new BurnChanceSupportModifier().ModifyPayload(ref payload);
            new LifeLeechSupportModifier().ModifyPayload(ref payload);

            Assert.AreEqual(100f, payload.BaseDamage, Tolerance);
            Assert.AreEqual(1f, payload.DamageMultiplier, Tolerance);
        }

        [Test]
        public void Clone_CopiesSupportFields()
        {
            var payload = new SkillPayload
            {
                ProjectileSpeedMultiplier = 1.5f, PierceCount = 1, ChainCount = 2, ChainRange = 7f, IsHoming = true,
                KnockbackDistance = 3f, LifeLeechPercent = 4f, ManaLeechPercent = 5f, BurnChance = 6f,
                FreezeChance = 7f, StatusChanceBonus = 8f, DurationMultiplier = 1.4f
            };

            var copy = payload.Clone();

            Assert.AreEqual(1.5f, copy.ProjectileSpeedMultiplier, Tolerance);
            Assert.AreEqual(1, copy.PierceCount);
            Assert.AreEqual(2, copy.ChainCount);
            Assert.AreEqual(7f, copy.ChainRange, Tolerance);
            Assert.IsTrue(copy.IsHoming);
            Assert.AreEqual(3f, copy.KnockbackDistance, Tolerance);
            Assert.AreEqual(4f, copy.LifeLeechPercent, Tolerance);
            Assert.AreEqual(5f, copy.ManaLeechPercent, Tolerance);
            Assert.AreEqual(6f, copy.BurnChance, Tolerance);
            Assert.AreEqual(7f, copy.FreezeChance, Tolerance);
            Assert.AreEqual(8f, copy.StatusChanceBonus, Tolerance);
            Assert.AreEqual(1.4f, copy.DurationMultiplier, Tolerance);
        }

        [Test]
        public void Formula_Leech()
        {
            Assert.AreEqual(15f, SupportHitFormula.Leech(150f, 10f), Tolerance);
            Assert.AreEqual(0f, SupportHitFormula.Leech(0f, 10f), Tolerance);
            Assert.AreEqual(0f, SupportHitFormula.Leech(150f, 0f), Tolerance);
        }

        [Test]
        public void Formula_StatusChance_AddsBonusOnlyWhenBaseExists()
        {
            Assert.AreEqual(35f, SupportHitFormula.StatusChance(20f, 15f), Tolerance);
            Assert.AreEqual(0f, SupportHitFormula.StatusChance(0f, 15f), Tolerance);
            Assert.AreEqual(100f, SupportHitFormula.StatusChance(90f, 50f), Tolerance);
        }

        [Test]
        public void Formula_Rolls()
        {
            Assert.IsTrue(SupportHitFormula.Rolls(20f, 19.9f));
            Assert.IsFalse(SupportHitFormula.Rolls(20f, 20f));
            Assert.IsFalse(SupportHitFormula.Rolls(0f, 0f));
        }

        [Test]
        public void Formula_DurationAndSpeed()
        {
            Assert.AreEqual(3.6f, SupportHitFormula.Duration(3f, 1.2f), Tolerance);
            Assert.AreEqual(0f, SupportHitFormula.Duration(-1f, 1.2f), Tolerance);
            Assert.AreEqual(13f, SupportHitFormula.ProjectileSpeed(10f, 1.3f), Tolerance);
            Assert.AreEqual(5f, SupportHitFormula.TickDamage(50f, 10f), Tolerance);
        }

        [Test]
        public void Projectile_NextStep_ChainBeforePierceBeforeDestroy()
        {
            Assert.AreEqual(ProjectileHitStep.Chain, ProjectileBehaviour.NextStep(true, 1, 1, true));
            Assert.AreEqual(ProjectileHitStep.Pierce, ProjectileBehaviour.NextStep(true, 1, 1, false));
            Assert.AreEqual(ProjectileHitStep.Pierce, ProjectileBehaviour.NextStep(true, 1, 0, true));
            Assert.AreEqual(ProjectileHitStep.Destroy, ProjectileBehaviour.NextStep(true, 0, 0, false));
            Assert.AreEqual(ProjectileHitStep.Continue, ProjectileBehaviour.NextStep(false, 0, 0, false));
        }

        [Test]
        public void FreezeChance_LinksToAnyElement()
        {
            var skill = UnityEngine.ScriptableObject.CreateInstance<ActiveSkill>();
            try
            {
                skill.SkillElement = ElementType.Fire;
                Assert.IsTrue(new FreezeChanceSupportModifier().IsCompatible(skill));
                skill.SkillElement = ElementType.Neutral;
                Assert.IsTrue(new FreezeChanceSupportModifier().IsCompatible(skill));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(skill);
            }
        }

        [Test]
        public void ChainJump_DecaysPerJumpAndStartsFull()
        {
            Assert.AreEqual(1f, ActiveSkillFormula.ChainJumpMultiplier(0.8f, 0), Tolerance);
            Assert.AreEqual(0.8f, ActiveSkillFormula.ChainJumpMultiplier(0.8f, 1), Tolerance);
            Assert.AreEqual(0.64f, ActiveSkillFormula.ChainJumpMultiplier(0.8f, 2), Tolerance);
        }

        [Test]
        public void ParryCounter_IsAProc()
        {
            Assert.IsTrue(ProcSkillIds.IsProc(ProcSkillIds.ParryCounter));
        }

        [Test]
        public void BurnTick_IsAProc()
        {
            Assert.IsTrue(ProcSkillIds.IsProc(ProcSkillIds.BurnTick));
        }
        #endregion

        #region [Private Methods]
        private static SkillPayload Modify(SupportModifier modifier)
        {
            var payload = new SkillPayload();
            modifier.ModifyPayload(ref payload);
            return payload;
        }
        #endregion
    }
}
