using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Combat;

namespace CoreSystem.Skills
{


    [System.Serializable]
    public class OutputScaling
    {
        public PayloadTarget TargetField;
        public AttributeType ScalingAttribute = AttributeType.STR; 
        public float AttributeMultiplier = 1f;
        public float GlobalMultiplier = 1f;
        public float FlatBonus = 0f;
        public float LevelMultiplier = 0f;

        public void ApplyScaling(ref SkillPayload payload, AttributeSystem casterStats, int skillLevel = 1)
        {
            // Determine base stat value
            float statValue = 0f;
            if (casterStats != null)
            {
                statValue = casterStats.GetAttribute(ScalingAttribute);
            }

            float calculatedBonus = SkillValueFormula.Scaling(statValue, AttributeMultiplier, FlatBonus, LevelMultiplier, skillLevel, GlobalMultiplier);
            
            // Depending on the field, we may only want positive values, but we'll apply it directly
            switch (TargetField)
            {
                case PayloadTarget.BaseDamage:
                    payload.BaseDamage += calculatedBonus;
                    break;
                case PayloadTarget.AOERadius:
                    payload.AOERadius += calculatedBonus;
                    break;
                case PayloadTarget.ProjectileCount:
                    payload.ProjectileCount += (int)calculatedBonus;
                    break;
                case PayloadTarget.Heal:
                    payload.Heal += calculatedBonus;
                    break;
                case PayloadTarget.VCT:
                    payload.VCT += calculatedBonus;
                    break;
                case PayloadTarget.FCT:
                    payload.FCT += calculatedBonus;
                    break;
                case PayloadTarget.CastDelay:
                    payload.CastDelay += calculatedBonus;
                    break;
                case PayloadTarget.Cooldown:
                    payload.Cooldown += calculatedBonus;
                    break;
                case PayloadTarget.ManaCost:
                    payload.ManaCost += calculatedBonus;
                    break;
            }
        }
    }
}
