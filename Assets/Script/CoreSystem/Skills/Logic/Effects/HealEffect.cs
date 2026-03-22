using CoreSystem.Attributes;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public class HealEffect : EffectLogic
    {
        public AttributeType TargetAttribute = AttributeType.HP;
        public float BaseHealAmount = 10f;

        public override void Apply(SkillPayload payload, IEntity target)
        {
            if (target.attributeSystem != null)
            {
                float calculatedHeal = BaseHealAmount;

                var targetAttr = target.attributeSystem.GetAttribute(TargetAttribute);
                if (targetAttr != null)
                {
                    float newValue = targetAttr.CurrentValue + calculatedHeal;

                    // Clamp to max if applicable
                    if (TargetAttribute == AttributeType.HP)
                    {
                        var maxHp = target.attributeSystem.GetAttribute(AttributeType.MaxHP);
                        if (maxHp != null) newValue = Mathf.Min(newValue, maxHp.CurrentValue);
                    }
                    else if (TargetAttribute == AttributeType.MP)
                    {
                        var maxMp = target.attributeSystem.GetAttribute(AttributeType.MaxMP);
                        if (maxMp != null) newValue = Mathf.Min(newValue, maxMp.CurrentValue);
                    }

                    targetAttr.BaseValue = newValue;
                    targetAttr.Recalculate();
                }
            }
        }
    }
}
