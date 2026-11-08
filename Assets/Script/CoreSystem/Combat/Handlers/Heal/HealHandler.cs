using CoreSystem.Entity;

namespace CoreSystem.Combat
{
    [EffectHandler(EffectKind.Heal)]
    public sealed class HealHandler : EffectHandler<HealSpec, float>
    {
        #region [Properties]
        public override TargetRule TargetRule => TargetRule.Any;
        #endregion

        #region [Private Methods]
        protected override float Offense(HealSpec spec, ResolveContext context)
        {
            float matk = 0f;
            if (spec.MatkScale != 0f && context.User != null && context.User.attributeSystem != null)
            {
                var attribute = context.User.attributeSystem.GetAttribute(AttributeType.MATK);
                if (attribute != null) matk = attribute.CurrentValue;
            }
            return HealFormula.Amount(spec.BaseAmount, matk, spec.MatkScale);
        }

        protected override Outcome Defense(HealSpec spec, float outgoing, ResolveContext context)
        {
            Outcome outcome = context.CreateOutcome(spec);
            outcome.Attribute = spec.TargetAttribute;
            outcome.Amount = outgoing;
            return outcome;
        }

        protected override void Commit(HealSpec spec, Outcome outcome, ActionResult result)
        {
            IEntity target = outcome.Target;
            if (target == null || target.attributeSystem == null) return;

            var current = target.attributeSystem.GetAttribute(spec.TargetAttribute);
            if (current == null) return;

            float value = current.CurrentValue + outcome.Amount;
            AttributeType maxType = MaxOf(spec.TargetAttribute);
            if (maxType != spec.TargetAttribute)
            {
                var max = target.attributeSystem.GetAttribute(maxType);
                if (max != null) value = HealFormula.Clamp(current.CurrentValue, outcome.Amount, max.CurrentValue);
            }

            current.BaseValue = value;
            current.Recalculate();
        }

        private static AttributeType MaxOf(AttributeType type)
        {
            if (type == AttributeType.HP) return AttributeType.MaxHP;
            if (type == AttributeType.MP) return AttributeType.MaxMP;
            return type;
        }
        #endregion
    }
}
