using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Entity;
using CoreSystem.Skills.Logic;
using UnityEngine;

namespace CoreSystem.Skills
{
    public static class SupportHitApplier
    {
        #region [Variables/Fields]
        public const float BurnDuration = 3f;
        public const float BurnTickInterval = 0.5f;
        public const float BurnPercentPerTick = 10f;
        public const float FreezeDuration = 1.5f;
        public const float KnockbackLerp = 0.15f;
        #endregion

        #region [Public Methods]
        public static void Apply(SkillPayload payload, IEntity target, ActionResult result, ICombatRandom random = null)
        {
            if (payload == null || target == null || result == null || result.IsBlocked) return;
            if (payload.Caster == null || !result.HasHit()) return;

            float damageDealt = result.Total(EffectKind.Damage);
            random = random ?? UnityCombatRandom.Instance;

            ApplyLeech(payload, damageDealt);
            ApplyKnockback(payload, target);

            if (target is Character targetCharacter && targetCharacter.IsDead) return;

            if (SupportHitFormula.Rolls(SupportHitFormula.StatusChance(payload.BurnChance, payload.StatusChanceBonus), random.Range(0f, 100f)))
            {
                ApplyBurn(payload, target, damageDealt);
            }

            if (SupportHitFormula.Rolls(SupportHitFormula.StatusChance(payload.FreezeChance, payload.StatusChanceBonus), random.Range(0f, 100f)))
            {
                ApplyFreeze(payload, target);
            }
        }
        #endregion

        #region [Private Methods]
        private static void ApplyLeech(SkillPayload payload, float damageDealt)
        {
            Restore(payload.Caster, AttributeType.HP, AttributeType.MaxHP, SupportHitFormula.Leech(damageDealt, payload.LifeLeechPercent));
            Restore(payload.Caster, AttributeType.MP, AttributeType.MaxMP, SupportHitFormula.Leech(damageDealt, payload.ManaLeechPercent));
        }

        private static void ApplyKnockback(SkillPayload payload, IEntity target)
        {
            if (payload.KnockbackDistance <= 0f) return;
            Vector3 direction = target.transform.position - payload.Caster.transform.position;
            direction.z = 0f;
            if (direction.sqrMagnitude < 0.0001f) direction = payload.GetAimDirection();
            target.ApplyImpulse(direction.normalized, payload.KnockbackDistance, KnockbackLerp);
        }

        private static void ApplyBurn(SkillPayload payload, IEntity target, float damageDealt)
        {
            float tickDamage = SupportHitFormula.TickDamage(damageDealt, BurnPercentPerTick);
            if (tickDamage <= 0f) return;

            StatusHandler.Apply(payload.Caster, target, new StatusSpec
            {
                Status = StatusKind.Burn,
                Duration = SupportHitFormula.Duration(BurnDuration, payload.DurationMultiplier),
                TickDamage = tickDamage,
                TickInterval = BurnTickInterval,
                CreditTo = payload.Caster
            }, ProcSkillIds.StatusApply);
        }

        private static void ApplyFreeze(SkillPayload payload, IEntity target)
        {
            StatusHandler.Apply(payload.Caster, target, new StatusSpec
            {
                Status = StatusKind.Freeze,
                Duration = SupportHitFormula.Duration(FreezeDuration, payload.DurationMultiplier)
            }, ProcSkillIds.StatusApply);
        }

        private static void Restore(IEntity entity, AttributeType resource, AttributeType maxResource, float amount)
        {
            if (amount <= 0f || entity == null || entity.attributeSystem == null) return;
            var current = entity.attributeSystem.GetAttribute(resource);
            var max = entity.attributeSystem.GetAttribute(maxResource);
            if (current == null || max == null) return;
            current.BaseValue = Mathf.Min(current.CurrentValue + amount, max.CurrentValue);
            current.Recalculate();
        }
        #endregion
    }
}
