using CoreSystem.Attributes;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public class ApplyBuffEffect : EffectLogic
    {
        public AttributeType TargetAttribute;
        public float ModifierValue;
        public ModifierType ModType;

        public override void Apply(SkillPayload payload, IEntity target)
        {
            if (target.attributeSystem != null)
            {
                AttributeModifier buffModifier = new AttributeModifier(ModifierValue, ModType);
                target.attributeSystem.AddModifier(TargetAttribute, buffModifier);
            }
        }
    }
}
