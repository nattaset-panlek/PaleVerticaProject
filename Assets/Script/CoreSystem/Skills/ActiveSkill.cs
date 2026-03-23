using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Visuals;
using UnityEngine;

namespace CoreSystem.Skills
{
    [CreateAssetMenu(fileName = "NewActiveSkill", menuName = "Skill System/Active Skill")]
    public class ActiveSkill : ScriptableObject
    {
        public int Id;
        public string Name;
        public ElementType SkillElement = ElementType.Neutral;
        public DamageType SkillDamageType = DamageType.Physical;
        
        [SerializeReference, SubclassSelector]
        public TargetingLogic BaseTargeting;
        
        [SerializeReference, SubclassSelector]
        public List<EffectLogic> Effects = new List<EffectLogic>();
        public VisualPayload VisualData;
        public GameObject VisualPrefab;
        public GameObject CastVisualPrefab;
        public VisualAppearance CastSpawnPoint = VisualAppearance.TargetPosition;
        
        public float BaseDamage;
        public float BaseAOERadius;
        public int BaseProjectileCount;
        public float BaseManaCost;
        
        [Header("Skill Type")]
        public bool IsToggle;
        
        [Header("Continuous Area Setup")]
        [Tooltip("If checked, the targeting shape stays on the ground and repeatedly deals damage.")]
        public bool IsContinuousArea = false;
        public float AreaDuration = 5f;
        public float AreaTickRate = 0.5f;
        
        [Header("Cast & Delays")]
        public float BaseVCT; // Variable Cast Time
        public float BaseFCT; // Fixed Cast Time
        public float BaseCastDelay; // Global delay applied to ALL skills after casting
        public float BaseCooldown; // Skill-specific cooldown
        
        [Header("Dynamic Scaling (Stat x AttrMult + Flat + LevelMult x Lv) x Global")]
        [Tooltip("Formula: (Stat x AttrMult + FlatBonus + LevelMult x Level) x GlobalMult")]
        public List<OutputScaling> Scalings = new List<OutputScaling>();
    }
}
