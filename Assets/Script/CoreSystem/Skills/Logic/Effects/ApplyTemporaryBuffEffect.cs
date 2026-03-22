using CoreSystem.Attributes;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public class ApplyTemporaryBuffEffect : EffectLogic
    {
        public AttributeType TargetAttribute;
        public float ModifierValue;
        public ModifierType ModType;
        public float Duration = 5f;
        public int MaxStacks = 1;
        public string BuffName = "";
        public BuffCategory Category = BuffCategory.Attribute;
        public Sprite Icon;

        public override void Apply(SkillPayload payload, IEntity target)
        {
            if (target.attributeSystem != null)
            {
                MonoBehaviour targetMono = target as MonoBehaviour;
                if (targetMono != null)
                {
                    TemporaryBuffManager buffManager = targetMono.GetComponent<TemporaryBuffManager>();
                    if (buffManager == null)
                    {
                        buffManager = targetMono.gameObject.AddComponent<TemporaryBuffManager>();
                    }

                    AttributeModifier buffModifier = new AttributeModifier(ModifierValue, ModType);
                    
                    string finalBuffName = string.IsNullOrEmpty(BuffName) ? payload.SkillElement.ToString() + " Buff" : BuffName;
                    
                    buffManager.ApplyTemporaryBuff(finalBuffName, TargetAttribute, buffModifier, Duration, Icon, Category, MaxStacks);
                }
            }
        }
    }
}
