using System.Collections.Generic;
using CoreSystem.Entity;
using CoreSystem.Enums;
using UnityEngine;

namespace CoreSystem.Agent
{
    [System.Serializable]
    public class AgentProfile
    {
        #region [Variables/Fields]
        [Header("Identity")]
        public string AgentName = "Agent";

        [Header("AI Configuration")]
        public AIBehaviorType Behavior = AIBehaviorType.Aggressive;
        public CombatStyle Combat = CombatStyle.Melee;
        public float DetectionRadius = 10f;
        [SerializeField] private float _attackRange = 3f;
        public float LoseSightRadius = 15f;
        [Tooltip("Rotation smoothness speed. Higher = faster. (Try 5-20)")]
        public float RotationSpeed = 10f;

        [Header("Combat Style Settings")]
        [Tooltip("Ranged: min preferred distance from target.")]
        public float PreferredRangeMin = 5f;
        [Tooltip("Ranged: max preferred distance from target.")]
        public float PreferredRangeMax = 7f;
        [Tooltip("Circler: orbit speed in degrees per second.")]
        public float OrbitSpeed = 90f;

        [Header("Home Zone & Roam")]
        public bool UseHomeZone = true;
        public float HomeZoneRadius = 8f;
        public float RoamRadius = 5f;
        public float RoamWaitDuration = 3f;
        public bool CanRoam = true;

        [Header("Group AI")]
        [Tooltip("0 = Solo. Same GroupID agents coordinate and alert each other.")]
        public int GroupID = 0;
        public float AlertRadius = 12f;

        [Header("Skill Pool")]
        public List<AgentSkillEntry> SkillPool = new List<AgentSkillEntry>();
        [Tooltip("Global delay (seconds) between AI skill casts to prevent spamming.")]
        public float AISkillCooldown = 1.5f;

        [Header("Pathfinding")]
        [Tooltip("Enable A* pathfinding. Requires a PathfinderGrid in the scene.")]
        public bool UsePathfinding = true;
        [Tooltip("How often (seconds) to re-request a new path.")]
        public float PathRefreshRate = 0.5f;
        #endregion

        #region [Properties]
        public float FallbackAttackRange => _attackRange;
        #endregion

        #region [Public Methods]
        public void OverrideAttackRange(Character owner, float value)
        {
            _attackRange = value;
            if (owner != null) owner.OnResolveAutoAttackRange = _ => value;
        }
        #endregion
    }
}
