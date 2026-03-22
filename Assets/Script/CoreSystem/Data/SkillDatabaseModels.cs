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
        public string Category;
        public string MovementType;
        public float MovementDistance;
        public bool ApplyOnce;

        public List<int> ScalingSourceIDs;
        public float ScalingThreshold;
    }

    [System.Serializable]
    public class BaseTargetingData
    {
        public string Type;
        public float Radius;
        
        public float Angle = 60f;
        public UnityEngine.Vector2 BoxSize = new UnityEngine.Vector2(2f, 5f);
        public UnityEngine.Vector2 Offset = UnityEngine.Vector2.zero;
    }

    [System.Serializable]
    public class SupportSkillJsonData
    {
        public int SkillID;
        public string ModifierName;
        public List<ModifierConditionData> Conditions;
        public List<ModifierEffectData> Effects;
    }

    [System.Serializable]
    public class ModifierConditionData
    {
        public string Type;
        public string RequiredElement;
        public int RequiredSkillID;
        public int MinSkillLevel;
    }

    [System.Serializable]
    public class ModifierEffectData
    {
        public string TargetField;
        public string Operation;
        public float FlatValue;
        
        public bool UsePayloadReference;
        public string ReferencePayloadField;
        public float PayloadMultiplier = 1f;
 
        public bool UseAttributeReference;
        public string ReferenceAttribute;
        public float AttributeMultiplier = 1f;
    }
}
