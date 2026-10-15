using CoreSystem.Entity;
using CoreSystem.UI;
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

            if (userCharacter != null && userCharacter.UniquePassives != null)
            {
                userCharacter.UniquePassives.TriggerOnHit(target, ref finalDamage, payload);
            }

            if (targetCharacter != null && targetCharacter.UniquePassives != null)
            {
                targetCharacter.UniquePassives.TriggerOnTakeDamage(user, ref finalDamage, payload);
            }

            outcome.Amount = finalDamage;

            var attribute = target.attributeSystem.GetAttribute(spec.TargetAttribute);
            if (attribute != null)
            {
                attribute.BaseValue = Mathf.Max(0f, attribute.CurrentValue - finalDamage);
                attribute.Recalculate();
            }

            if (userCharacter != null && userCharacter.UniquePassives != null)
            {
                userCharacter.UniquePassives.TriggerOnPostHit(target, finalDamage, payload);
            }

            if (user != null)
            {
                user.TriggerOnHit(target, finalDamage);
            }

            target.TriggerOnTakeDamage(user, finalDamage);

            if (DamageTextManager.Instance != null)
            {
                DamageTextManager.Instance.ShowDamage(target.transform.position, finalDamage, payload.DamageType, payload.SkillElement, outcome.IsCritical);
            }
        }
        #endregion
    }
}
