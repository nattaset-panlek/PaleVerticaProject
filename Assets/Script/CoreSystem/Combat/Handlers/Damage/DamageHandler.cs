using CoreSystem.Entity;
using CoreSystem.UI;
using CoreSystem.Visuals;
using UnityEngine;

namespace CoreSystem.Combat
{
    [EffectHandler(EffectKind.Damage)]
    public sealed class DamageHandler : EffectHandler<DamageSpec, DamageHandler.Outgoing>
    {
        public struct Outgoing
        {
            #region [Variables/Fields]
            public DamageInput Input;
            public DamageOffense Offense;
            #endregion
        }

        #region [Properties]
        public override TargetRule TargetRule => TargetRule.Hostile;
        #endregion

        #region [Private Methods]
        protected override Outgoing Offense(DamageSpec spec, ResolveContext context)
        {
            DamageInput input = DamageFormula.BuildInput(context.Action.Payload, context.User, context.Target);
            float critRoll = input.HasCasterStats ? context.Random.Range(0f, 100f) : 0f;
            return new Outgoing { Input = input, Offense = DamageFormula.Offense(input, critRoll) };
        }

        protected override Outcome Defense(DamageSpec spec, Outgoing outgoing, ResolveContext context)
        {
            Outcome outcome = context.CreateOutcome(spec);
            outcome.Attribute = spec.TargetAttribute;
            outcome.Amount = DamageFormula.Mitigate(outgoing.Offense.RawDamage, outgoing.Input.TargetDefense);
            outcome.IsCritical = outgoing.Offense.IsCritical;
            return outcome;
        }

        protected override void Commit(DamageSpec spec, Outcome outcome, ActionResult result)
        {
            var payload = result.Action.Payload;
            IEntity user = outcome.Source;
            IEntity target = outcome.Target;
            float finalDamage = outcome.Amount;

            var userCharacter = user as Character;
            var targetCharacter = target as Character;

            payload.IsCriticalHit = outcome.IsCritical;

            if (userCharacter != null && userCharacter.UniquePassives != null)
            {
                userCharacter.UniquePassives.TriggerOnHit(target, ref finalDamage, payload);
            }

            if (targetCharacter != null && targetCharacter.UniquePassives != null)
            {
                targetCharacter.UniquePassives.TriggerOnTakeDamage(user, ref finalDamage, payload);
            }

            outcome.Amount = finalDamage;

            bool killingBlow = false;
            float hpRemoved = 0f;
            var attribute = target.attributeSystem.GetAttribute(spec.TargetAttribute);
            if (attribute != null)
            {
                float hpBefore = attribute.CurrentValue;
                attribute.BaseValue = Mathf.Max(0f, attribute.CurrentValue - finalDamage);
                attribute.Recalculate();
                if (spec.TargetAttribute == AttributeType.HP)
                {
                    hpRemoved = Mathf.Max(0f, hpBefore - attribute.CurrentValue);
                    killingBlow = CombatEvents.IsKillingBlow(hpBefore, attribute.CurrentValue);
                }
            }

            if (userCharacter != null && userCharacter.UniquePassives != null)
            {
                userCharacter.UniquePassives.TriggerOnPostHit(target, finalDamage, payload);
            }

            IEntity credited = user ?? result.Action.CreditTo;
            if (credited != null)
            {
                credited.TriggerOnHit(target, finalDamage);
            }

            target.TriggerOnTakeDamage(credited, finalDamage);
            KillCreditRegistry.Record(credited, target, hpRemoved);
            if (killingBlow)
            {
                CombatEvents.RaiseKilled(credited, target);
                KillCreditRegistry.Forget(target);
            }

            if (DamageTextManager.Instance != null)
            {
                DamageTextManager.Instance.ShowDamage(target.transform.position, finalDamage, payload.DamageType, payload.SkillElement, outcome.IsCritical);
            }

            HitVFXManager.PlayHitVFX(target.transform.position, payload.DamageType, payload.SkillElement);
        }
        #endregion
    }
}
