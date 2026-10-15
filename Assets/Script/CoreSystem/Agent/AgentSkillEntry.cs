using CoreSystem.Skills;
using UnityEngine;

namespace CoreSystem.Agent
{
    [System.Serializable]
    public class AgentSkillEntry
    {
        #region [Variables/Fields]
        public ActiveSkill Skill;
        public bool IsEquipped = true;
        [Tooltip("AI-specific delay (seconds) before this skill can be used again by this agent.")]
        public float AIDelay = 2.0f;

        [Header("Skill Modifiers (Add to base)")]
        public float ExtraCastTime = 0f;
        public float ExtraCastDelay = 0f;
        public float ExtraCooldown = 0f;
        #endregion
    }
}
