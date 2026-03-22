using System.Collections.Generic;
using UnityEngine;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Data;

namespace CoreSystem.Skills
{
    [System.Serializable]
    public class SkillInstance
    {
        public ActiveSkill SkillData;
        public List<SupportModifier> LinkedModifiers = new List<SupportModifier>();
        public int CurrentLevel = 1;

        public bool IsToggledOn { get; set; }

        public float CurrentCooldown { get; private set; }

        public SkillInstance() { } // Default constructor for Unity serialization

        public SkillInstance(ActiveSkill skillTemplate, int level = 1)
        {
            CurrentLevel = level;
            if (skillTemplate != null)
            {
                // Try to get a synchronized clone from the Database
                ActiveSkill syncedSkill = Database.GetSynchronizedSkill(skillTemplate.Id);
                
                // If database isn't ready or doesn't have it, fallback to the template itself
                SkillData = syncedSkill != null ? syncedSkill : skillTemplate;
            }
        }

        public bool IsReady()
        {
            return CurrentCooldown <= 0f;
        }

        public void StartCooldown()
        {
            if (SkillData != null)
            {
                StartCooldown(SkillData.BaseCooldown);
            }
        }

        public void StartCooldown(float customCooldown)
        {
            CurrentCooldown = customCooldown;
        }

        public void UpdateCooldown(float deltaTime)
        {
            if (CurrentCooldown > 0f)
            {
                CurrentCooldown -= deltaTime;
            }
        }

        public float CalculateCastTime(AttributeSystem stats)
        {
            if (SkillData == null) return 0f;
            return CalculateCastTimeModified(stats, SkillData.BaseVCT, SkillData.BaseFCT);
        }

        public float CalculateCastTimeModified(AttributeSystem stats, float vctOverride, float fctOverride)
        {
            if (stats == null)
            {
                // Fallback to overrides if no stats available
                return vctOverride + fctOverride;
            }

            // --- VCT Calculation ---
            float dex = stats.GetAttribute(AttributeType.DEX).CurrentValue;
            float intel = stats.GetAttribute(AttributeType.INT).CurrentValue;

            float sumVCT = stats.GetAttribute(AttributeType.VCTReductionFlat).CurrentValue;
            float sumGearVCTReduc = stats.GetAttribute(AttributeType.GearVCTReduction).CurrentValue;
            float sumSkillVCTReduc = stats.GetAttribute(AttributeType.SkillVCTReduction).CurrentValue;

            // Cap the stat reduction term so it doesn't go below 0 (no negative square root) or become negative cast time
            float statTerm = (dex * 2f + intel) / 530f;
            statTerm = Mathf.Clamp01(statTerm);
            
            float vct = (vctOverride - sumVCT) 
                        * (1f - Mathf.Sqrt(statTerm)) 
                        * (1f - (sumGearVCTReduc / 100f)) 
                        * (1f - (sumSkillVCTReduc / 100f));
            
            vct = Mathf.Max(0f, vct);

            // --- FCT Calculation ---
            float sumFCT = stats.GetAttribute(AttributeType.FCTReductionFlat).CurrentValue;
            float maxFCTReduc = stats.GetAttribute(AttributeType.MaxFCTReduction).CurrentValue; 

            float fct = (fctOverride - sumFCT) * (1f - (maxFCTReduc / 100f));
            fct = Mathf.Max(0f, fct);

            return vct + fct;
        }
    }
}
