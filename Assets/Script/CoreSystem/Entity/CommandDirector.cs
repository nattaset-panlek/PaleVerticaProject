using UnityEngine;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Entity
{
    [System.Serializable]
    public class CommandDirector
    {
        private Character owner;
        private object currentOverrider;

        public bool IsOverridden => currentOverrider != null;
        public object CurrentOverrider => currentOverrider;

        public void Initialize(Character owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Request control over this character. Normal controllers will be ignored.
        /// </summary>
        public bool RequestOverride(object requester)
        {
            if (requester == null) return false;
            
            if (currentOverrider != null && currentOverrider != requester)
            {
                // Already overridden by someone else
                return false;
            }
            
            currentOverrider = requester;
            return true;
        }

        /// <summary>
        /// Release control back to the normal controllers.
        /// </summary>
        public void ReleaseOverride(object requester)
        {
            if (currentOverrider == requester)
            {
                currentOverrider = null;
            }
        }

        /// <summary>
        /// Forceably clear any override (useful for death/reset).
        /// </summary>
        public void ForceClearOverride()
        {
            currentOverrider = null;
        }

        // ═══════════════════════════════════════════════════════════
        //   Movement Commands
        // ═══════════════════════════════════════════════════════════

        public void CommandMove(Vector3 direction, object sender = null)
        {
            if (owner == null || owner.movementSystem == null) return;
            
            // If overridden, ONLY the overrider can send commands
            if (IsOverridden && sender != currentOverrider) return;

            owner.movementSystem.Move(direction);
        }

        // ═══════════════════════════════════════════════════════════
        //   Skill Commands
        // ═══════════════════════════════════════════════════════════

        public void CommandCastSkill(SkillInstance skill, Vector3 targetPos, bool isPlayer = false, object sender = null)
        {
            if (owner == null || owner.skillCaster == null) return;
            if (IsOverridden && sender != currentOverrider) return;

            SkillInstance skillToCast = skill;
            if (skill != null && skill.SkillData != null)
            {
                var overrideData = ActiveSkill.GetActiveChainOverride(owner, skill.SkillData);
                if (overrideData != null && overrideData.ReplacementSkill != null)
                {
                    skillToCast = owner.skillCaster.GrantSkill(overrideData.ReplacementSkill);
                    if (overrideData.ConsumeBuffOnCast)
                    {
                        var buffMgr = owner.GetComponent<CoreSystem.Skills.Logic.TemporaryBuffManager>();
                        if (buffMgr != null) buffMgr.RemoveBuff(overrideData.RequiredBuffCode);
                    }
                }
            }

            owner.skillCaster.CastSkill(skillToCast, targetPos, isPlayer);
        }

        public void CommandCastSkill(SkillInstance skill, SkillPayload payload, object sender = null)
        {
            if (owner == null || owner.skillCaster == null) return;
            if (IsOverridden && sender != currentOverrider) return;

            SkillInstance skillToCast = skill;
            if (skill != null && skill.SkillData != null)
            {
                var overrideData = ActiveSkill.GetActiveChainOverride(owner, skill.SkillData);
                if (overrideData != null && overrideData.ReplacementSkill != null)
                {
                    skillToCast = owner.skillCaster.GrantSkill(overrideData.ReplacementSkill);
                    if (overrideData.ConsumeBuffOnCast)
                    {
                        var buffMgr = owner.GetComponent<CoreSystem.Skills.Logic.TemporaryBuffManager>();
                        if (buffMgr != null) buffMgr.RemoveBuff(overrideData.RequiredBuffCode);
                    }
                    if (payload != null) payload.SkillID = skillToCast.SkillData.Id;
                }
            }

            owner.skillCaster.CastSkill(skillToCast, payload);
        }

        public void CommandInterruptCast(object sender = null)
        {
            if (owner == null || owner.skillCaster == null) return;
            if (IsOverridden && sender != currentOverrider) return;

            owner.skillCaster.InterruptCast();
        }
    }
}
