using UnityEngine;
using CoreSystem;
using CoreSystem.Attributes;

namespace CoreSystem.Skills.Modifiers
{

    [System.Serializable]
    public class SupportCondition
    {
        public SupportConditionType Type;
        public ElementType RequiredElement;
        public int RequiredSkillID;
        public int MinSkillLevel;

        public bool IsMet(SkillPayload payload)
        {
            switch (Type)
            {
                case SupportConditionType.RequireElement:
                    return payload.SkillElement == RequiredElement;
                case SupportConditionType.RequireSkillID:
                    return payload.SkillID == RequiredSkillID;
                case SupportConditionType.MinSkillLevel:
                    return payload.SkillLevel >= MinSkillLevel;
                default:
                    return true;
            }
        }
    }

    [System.Serializable]
    public class SupportEffect
    {
        [Tooltip("Which property of the skill payload to modify")]
        public PayloadTarget TargetField;
        
        [Tooltip("How to apply the value (e.g. Add, Multiply, Override)")]
        public ModifierOperation Operation;
        
        [Header("Formula: (FlatValue + (PayloadRef * PayloadMult) + (AttrRef * AttrMult))")]
        public float FlatValue;
        public float PercentValue; // New percentage bonus

        [Header("Payload Reference")]
        public bool UsePayloadReference;
        public PayloadTarget ReferencePayloadField;
        public float PayloadMultiplier = 1f;

        [Header("Attribute Reference")]
        public bool UseAttributeReference;
        public AttributeType ReferenceAttribute;
        public float AttributeMultiplier = 1f;

        public float CalculateModifierValue(SkillPayload payload)
        {
            float total = FlatValue;

            if (UsePayloadReference)
            {
                float payloadRefValue = GetPayloadValue(payload, ReferencePayloadField);
                total += (payloadRefValue * PayloadMultiplier);
            }

            if (UseAttributeReference && payload.Caster != null)
            {
                var stats = payload.Caster.GetComponent<AttributeSystem>();
                if (stats != null)
                {
                    total += (stats.GetAttribute(ReferenceAttribute) * AttributeMultiplier);
                }
            }

            return total;
        }

        public float GetPayloadValue(SkillPayload payload, PayloadTarget target)
        {
            switch (target)
            {
                case PayloadTarget.BaseDamage: return payload.BaseDamage;
                case PayloadTarget.AOERadius: return payload.AOERadius;
                case PayloadTarget.ProjectileCount: return payload.ProjectileCount;
                case PayloadTarget.Heal: return payload.Heal;
                case PayloadTarget.VCT: return payload.VCT;
                case PayloadTarget.FCT: return payload.FCT;
                case PayloadTarget.CastDelay: return payload.CastDelay;
                case PayloadTarget.Cooldown: return payload.Cooldown;
                case PayloadTarget.ManaCost: return payload.ManaCost;
                default: return 0f;
            }
        }
    }

    [CreateAssetMenu(fileName = "GenericModifier", menuName = "Skill System/Modifiers/Generic Modifier")]
    public class GenericSupportModifier : SupportModifier
    {
        public string ModifierName = "Basic Modifier";
        
        public System.Collections.Generic.List<SupportCondition> Conditions = new System.Collections.Generic.List<SupportCondition>();
        public System.Collections.Generic.List<SupportEffect> Effects = new System.Collections.Generic.List<SupportEffect>();

        public override void ModifyPayload(ref SkillPayload payload)
        {
            foreach (var condition in Conditions)
            {
                if (!condition.IsMet(payload)) return;
            }

            foreach (var effect in Effects)
            {
                float modValue = effect.CalculateModifierValue(payload);
                float currentValue = effect.GetPayloadValue(payload, effect.TargetField);
                
                // 1. Apply Flat/Ref bonuses via the chosen operation first
                float result = ApplyOperation(currentValue, modValue, effect.Operation);
                
                // 2. Apply Percent last on the resulting value
                if (effect.PercentValue != 0)
                {
                    result += (result * effect.PercentValue / 100f);
                }

                switch (effect.TargetField)
                {
                    case PayloadTarget.BaseDamage:
                        payload.BaseDamage = result;
                        break;
                    case PayloadTarget.AOERadius:
                        payload.AOERadius = result;
                        break;
                    case PayloadTarget.ProjectileCount:
                        payload.ProjectileCount = (int)result;
                        break;
                    case PayloadTarget.Heal:
                        payload.Heal = result;
                        break;
                    case PayloadTarget.VCT:
                        payload.VCT = result;
                        break;
                    case PayloadTarget.FCT:
                        payload.FCT = result;
                        break;
                    case PayloadTarget.CastDelay:
                        payload.CastDelay = result;
                        break;
                    case PayloadTarget.Cooldown:
                        payload.Cooldown = result;
                        break;
                    case PayloadTarget.ManaCost:
                        payload.ManaCost = result;
                        break;
                }
            }
        }

        private float ApplyOperation(float currentValue, float modValue, ModifierOperation op)
        {
            switch (op)
            {
                case ModifierOperation.Add:
                    return currentValue + modValue;
                case ModifierOperation.Multiply:
                    return currentValue * modValue;
                case ModifierOperation.Override:
                    return modValue;
                default:
                    return currentValue;
            }
        }
    }
}
