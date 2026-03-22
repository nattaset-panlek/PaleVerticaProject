using UnityEngine;

namespace CoreSystem.Skills.Modifiers
{
    [CreateAssetMenu(fileName = "AOEAmplifierModifier", menuName = "Skill System/Modifiers/AOE Amplifier")]
    public class AOEAmplifierModifier : SupportModifier
    {
        [Tooltip("Flat addition to AOE Radius (Useful if base AOE is 0)")]
        public float FlatBonusRadius = 0f;

        [Tooltip("Percentage multiplier (e.g., 1.25 for 25% larger area bounding)")]
        public float RadiusMultiplier = 1.25f;

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.AOERadius = (payload.AOERadius + FlatBonusRadius) * RadiusMultiplier;
        }
    }
}
