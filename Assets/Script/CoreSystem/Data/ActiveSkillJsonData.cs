using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Skills;

namespace CoreSystem.Data.Skills
{
    [System.Serializable]
    public class ActiveSkillJsonData
    {
        public int SkillID;
        public string CustomClassName;
        public string SkillName;
        public string IconPath;
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
        public int MaxSupportSlots = 5;
        
        public List<SkillChainOverrideJsonData> ChainOverrides;
    }
}
