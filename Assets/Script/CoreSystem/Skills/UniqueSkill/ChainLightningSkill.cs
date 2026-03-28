using UnityEngine;
using CoreSystem.Entity;
using System.Collections.Generic;

namespace CoreSystem.Skills.UniqueSkill
{
    [CreateAssetMenu(fileName = "ChainLightningSkill", menuName = "Skill System/Unique/Chain Lightning")]
    public class ChainLightningSkill : ActiveSkill
    {
        public int MaxJumps = 5;
        public float JumpRadius = 5f;
        public float DamageDecayPerJump = 0.8f;

        public override void Execute(SkillPayload payload, SkillInstance instance, SkillCaster caster)
        {
            // Custom Jump Logic that doesn't fit standard TargetingLogic easily
            IEntity currentTarget = null;
            
            // Initial Target finding (reuse BaseTargeting if available)
            var initialTargets = BaseTargeting?.GetTargets(payload);
            if (initialTargets == null || initialTargets.Count == 0) return;
            
            currentTarget = initialTargets[0];
            float currentDamage = payload.BaseDamage;
            HashSet<IEntity> hitHistory = new HashSet<IEntity>();

            for (int i = 0; i < MaxJumps; i++)
            {
                if (currentTarget == null) break;
                
                hitHistory.Add(currentTarget);
                
                // Apply Damage directly (bypassing Effects list for demonstration)
                // In real use, you'd trigger a DamageEffect logic instance
                Debug.Log($"[ChainLightning] Hit {currentTarget} for {currentDamage} damage.");
                
                // Find next jump target
                currentTarget = FindNextJump(currentTarget, JumpRadius, hitHistory);
                currentDamage *= DamageDecayPerJump;
            }
        }

        private IEntity FindNextJump(IEntity current, float radius, HashSet<IEntity> history)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(((MonoBehaviour)current).transform.position, radius);
            foreach (var hit in hits)
            {
                var ent = hit.GetComponent<IEntity>() ?? hit.GetComponentInParent<IEntity>();
                if (ent != null && !history.Contains(ent)) return ent;
            }
            return null;
        }
    }
}
