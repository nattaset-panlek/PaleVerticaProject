using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public class HealEffect : EffectLogic
    {
        public AttributeType TargetAttribute = AttributeType.HP;
        public float BaseHealAmount = 10f;
        public float MatkScale = HealFormula.DefaultMatkScale;

        public override void Apply(SkillPayload payload, IEntity target)
        {
            if (target == null) return;
            var spec = new HealSpec
            {
                TargetAttribute = TargetAttribute,
                BaseAmount = BaseHealAmount,
                MatkScale = TargetAttribute == AttributeType.HP ? MatkScale : 0f
            };
            CombatCore.Execute(new CombatAction(payload).WithEffect(spec), payload.Caster, target);
        }
    }
}
