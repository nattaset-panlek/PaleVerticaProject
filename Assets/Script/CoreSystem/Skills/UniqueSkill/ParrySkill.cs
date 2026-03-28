using UnityEngine;
using CoreSystem.Entity;
using System.Collections;

namespace CoreSystem.Skills.UniqueSkill
{
    [CreateAssetMenu(fileName = "ParrySkill", menuName = "Skill System/Unique/Parry Skill")]
    public class ParrySkill : ActiveSkill
    {
        public float ParryWindow = 0.5f;
        public float CounterDamageMult = 2.0f;
        public GameObject CounterVfx;

        public override void Execute(SkillPayload payload, SkillInstance instance, SkillCaster caster)
        {
            var character = caster.CharacterRef;
            if (character == null) return;

            // Start a Parry sequence (e.g., via a Coroutine or a State on the character)
            // For simplicity, we just log and apply a temporary buff or state
            Debug.Log("[Parry] Caster is now in Parry Window!");
            
            // In a real game, you would listen to 'OnTakeDamage' during this window
            // and if damage is taken, cancel damage and trigger counter-attack.
        }
    }
}
