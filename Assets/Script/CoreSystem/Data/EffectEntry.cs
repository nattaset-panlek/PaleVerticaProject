using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Skills;

namespace CoreSystem.Data.Skills
{
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
        public string BuffCode;
        public string PrefabPath;
        public string Category;
        public string MovementType;
        public float MovementDistance;
        public bool ApplyOnce;
        public bool ApplyToCaster;
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
}
