using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Skills;

namespace CoreSystem.Data.Skills
{
    [System.Serializable]
    public class SkillDataList
    {
        public List<ActiveSkillJsonData> ActiveSkills;
        public List<SupportSkillJsonData> SupportSkills;
    }

    [System.Serializable]
    public class SupportModifierDataList
    {
        public List<SupportSkillJsonData> SupportModifiers;
    }

    [System.Serializable]
    public class ActiveSkillJsonData
    {
        public int SkillID;
        public string CustomClassName;
        public string SkillName;
        public string SkillElement;
        public string DamageType;
        
        public float BaseDamage;
        public float BaseAOERadius; 
        public int BaseProjectileCount;
        public float BaseManaCost;
        
        public bool IsToggle;
        
        public float BaseVCT;
        public float BaseFCT;
        public float BaseCastDelay;
        public float BaseCooldown;

        public float Heal;
        
        public BaseTargetingData BaseTargeting;
        
        public List<OutputScaling> Scalings;
        public List<EffectEntry> Effects;
        
        public string VisualType;
        public string CastVfxPrefabName;
        public string CastVfxSpawnPoint;
        
        public string SkillIndicatorPrefabName;
        public string EruptionVfxPrefabName;
        public string ProjectilePrefabName;
        public string ImpactVfxPrefabName;
        public string AuraVfxPrefabName;
        public string BeamVfxPrefabName;

        public List<string> Tags;
        public List<UniqueSkillFieldData> UniqueFields;
        public List<int> SupportSkillIDs;
    }

    [System.Serializable]
    public class UniqueSkillFieldData
    {
        public string FieldName;
        public string FieldType;
        public string StringValue;
    }

    public enum VisualSpawnPoint
    {
        Target,
        Caster
    }

    [System.Serializable]
    public class EffectEntry
    {
        public string EffectType;
        public int AttributeID;
        public float Value;
        public string TriggerType;
        public float TriggerChance;
        public string SkillName;
        public int SkillID;
        public string ModType;
        public float Duration;
        public int MaxStacks;
        public string BuffName;
        public string PrefabPath;
        public string Category;
        public string MovementType;
        public float MovementDistance;
        public bool ApplyOnce;
        public bool UseLerp;
        public float LerpDuration;
        public bool ParentToCaster;

        public List<int> ScalingSourceIDs;
        public float ScalingThreshold;

        public int ConditionAttributeID;
        public string ConditionComparison;
        public float ConditionThreshold;
        public string BuffIconPath;
    }

    [System.Serializable]
    public class BaseTargetingData
    {
        public string Type;
        public float Radius;
        public float CastRange = 5f;
        
        public float Angle = 60f;
        public UnityEngine.Vector2 BoxSize = new UnityEngine.Vector2(2f, 5f);
        public UnityEngine.Vector2 Offset = UnityEngine.Vector2.zero;

        public string Alignment;
        public bool IncludeSelf;
    }

    [System.Serializable]
    public class SupportSkillJsonData
    {
        public int SkillID;
        public string ModifierName;
        public string Description;
    }
}
