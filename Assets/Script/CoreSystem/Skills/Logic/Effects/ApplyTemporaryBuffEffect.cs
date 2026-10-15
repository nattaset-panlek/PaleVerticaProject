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
        public string BuffCode = "";
        public BuffCategory Category = BuffCategory.Attribute;
        public Sprite Icon;
        public string BuffIconPath = "";

        public override void Apply(SkillPayload payload, IEntity target)
        {
            if (target.attributeSystem != null)
            {
                MonoBehaviour targetMono = target as MonoBehaviour;
                if (targetMono != null)
                {
                    BuffSystem buffManager = targetMono.GetComponent<BuffSystem>();
                    if (buffManager == null)
                    {
                        buffManager = targetMono.gameObject.AddComponent<BuffSystem>();
                    }

                    AttributeModifier buffModifier = new AttributeModifier(ModifierValue, ModType);
                    
                    string finalBuffName = string.IsNullOrEmpty(BuffName) ? payload.SkillElement.ToString() + " Buff" : BuffName;
                    string finalBuffCode = string.IsNullOrEmpty(BuffCode) ? finalBuffName : BuffCode;
                    
                    Sprite finalIcon = Icon;
                    if (finalIcon == null && !string.IsNullOrEmpty(BuffIconPath))
                    {
                        finalIcon = BuffIconLoader.Load(BuffIconPath);
                    }

                    buffManager.ApplyTemporaryBuff(finalBuffName, finalBuffCode, TargetAttribute, buffModifier, Duration, finalIcon, Category, MaxStacks);
                }
            }
        }
    }
}
