using UnityEngine;

namespace CoreSystem.Skills.Modifiers
{
    [CreateAssetMenu(fileName = "DamageAmplifierModifier", menuName = "Skill System/Modifiers/Damage Amplifier")]
    public class DamageAmplifierModifier : SupportModifier
    {
        [Tooltip("Percentage multiplier (e.g., 1.5 for 50% more base damage)")]
        public float DamageMultiplier = 1.5f;

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.BaseDamage *= DamageMultiplier;
        }
    }
}
