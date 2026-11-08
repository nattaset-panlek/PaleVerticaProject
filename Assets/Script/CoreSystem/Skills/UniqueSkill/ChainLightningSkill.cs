using System.Collections.Generic;
using CoreSystem.Combat;
using CoreSystem.Entity;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Visuals;
using UnityEngine;

namespace CoreSystem.Skills.UniqueSkill
{
    [CreateAssetMenu(fileName = "ChainLightningSkill", menuName = "Skill System/Unique/Chain Lightning")]
    public class ChainLightningSkill : ActiveSkill
    {
        #region [Variables/Fields]
        public int MaxJumps = 5;
        public float JumpRadius = 5f;
        public float DamageDecayPerJump = 0.8f;

        private readonly DamageEffect _damage = new DamageEffect();
        #endregion

        #region [Public Methods]
        public override void Execute(SkillPayload payload, SkillInstance instance, SkillCaster caster)
        {
            var initialTargets = BaseTargeting?.GetTargets(payload);
            if (initialTargets == null || initialTargets.Count == 0) return;

            IEntity current = initialTargets[0];
            var hit = new HashSet<IEntity>();
            for (int jump = 0; jump < MaxJumps && current != null; jump++)
            {
                hit.Add(current);
                SkillPayload jumpPayload = payload.Clone();
                jumpPayload.DamageMultiplier *= ActiveSkillFormula.ChainJumpMultiplier(DamageDecayPerJump, jump);
                _damage.Apply(jumpPayload, current);
                current = FindNextJump(payload.Caster, current, hit);
            }

            VisualManager.Ensure().PlayVisual(this, payload);
            if (payload.Caster != null && payload.Caster.UniquePassives != null) payload.Caster.UniquePassives.TriggerOnSkillExecuted(this, payload);
        }
        #endregion

        #region [Private Methods]
        private IEntity FindNextJump(IEntity caster, IEntity current, HashSet<IEntity> hit)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(current.transform.position, JumpRadius);
            IEntity best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < colliders.Length; i++)
            {
                IEntity entity = colliders[i].GetComponentInParent<IEntity>();
                if (entity == null || hit.Contains(entity) || !TeamRegistry.IsHostile(caster, entity)) continue;
                if (entity is Character character && character.IsDead) continue;
                float distance = (entity.transform.position - current.transform.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = entity;
                }
            }
            return best;
        }
        #endregion
    }
}
