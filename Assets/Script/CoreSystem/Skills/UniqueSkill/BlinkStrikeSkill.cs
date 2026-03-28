using UnityEngine;
using CoreSystem.Entity;

namespace CoreSystem.Skills.UniqueSkill
{
    [CreateAssetMenu(fileName = "BlinkStrikeSkill", menuName = "Skill System/Unique/Blink Strike")]
    public class BlinkStrikeSkill : ActiveSkill
    {
        public float StrikeDamageMult = 1.5f;

        public override void Execute(SkillPayload payload, SkillInstance instance, SkillCaster caster)
        {
            if (!payload.IsTargetPositionSet) return;

            // 1. Teleport behind target position (facing direction)
            Vector3 targetPos = payload.TargetPosition;
            Vector3 blinkPos = targetPos - (caster.CharacterRef.transform.up * 1f); 
            
            caster.CharacterRef.transform.position = blinkPos;
            
            // 2. Immediate Strike
            Debug.Log("[BlinkStrike] Teleported and Striking from behind!");
            
            // Apply standard effects
            base.Execute(payload, instance, caster);
        }
    }
}
