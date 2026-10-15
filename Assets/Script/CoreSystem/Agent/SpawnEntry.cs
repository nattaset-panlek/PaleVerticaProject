using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Enums;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using CoreSystem.Data;

namespace CoreSystem.Agent
{
    // ─── Spawn Entry ──────────────────────────────────────────────────────────────
    [System.Serializable]
    public class SpawnEntry
    {
        [Tooltip("Character ID from Database (Character.json).")]
        public int CharacterId;

        [Tooltip("Display name in Inspector.")]
        public string Label = "Entity";

        [Range(0f, 100f)]
        [Tooltip("Spawn weight (probability relative to other entries). 100 = always eligible, 0 = never.")]
        public float Weight = 100f;

        [Header("Quantity")]
        [Min(1)] public int MinCount = 1;
        [Min(1)] public int MaxCount = 1;

        // ── Override Toggles/Values ──
        [Header("Overrides (enable to apply)")]

        [Tooltip("Override AgentCore.Behavior.")]
        public bool OverrideBehavior;
        public AIBehaviorType Behavior = AIBehaviorType.Aggressive;

        [Tooltip("Override AgentCore.Combat style.")]
        public bool OverrideCombatStyle;
        public CombatStyle Combat = CombatStyle.Melee;

        [Tooltip("Override AgentCore.GroupID.")]
        public bool OverrideGroupID;
        public int GroupID = 0;

        [Tooltip("Override base level of the spawned entity.")]
        public bool OverrideLevel;
        [Min(1)] public int Level = 1;

        [Tooltip("Override AgentCore.DetectionRadius.")]
        public bool OverrideDetectionRadius;
        public float DetectionRadius = 10f;

        [Tooltip("Override AgentCore.AttackRange.")]
        public bool OverrideAttackRange;
        public float AttackRange = 3f;
    }
}
