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
        public GameObject ActiveVisual { get; set; }

        public float CurrentCooldown { get; private set; }

        public SkillInstance() { } // Default constructor for Unity serialization

        public SkillInstance(ActiveSkill skillTemplate, int level = 1)
        {
            CurrentLevel = level;
            if (skillTemplate != null)
            {
                if (Application.isPlaying)
                {
                    // Try to get a synchronized clone from the Database
                    ActiveSkill syncedSkill = Database.GetSynchronizedSkill(skillTemplate.Id);
                    
                    // If database isn't ready or doesn't have it, fallback to the template itself
                    SkillData = syncedSkill != null ? syncedSkill : skillTemplate;
                }
                else
                {
                    SkillData = skillTemplate;
                }
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
            float dex = stats.GetAttribute(AttributeType.DEX)?.CurrentValue ?? 0;
            float intel = stats.GetAttribute(AttributeType.INT)?.CurrentValue ?? 0;

            // Flat Reductions (Seconds)
            float vctFlat = stats.GetAttribute(AttributeType.VariableCastTimeReduc)?.CurrentValue ?? 0;
            float gearVCTFlat = stats.GetAttribute(AttributeType.GearVariableCastTime)?.CurrentValue ?? 0;
            float skillVCTFlat = stats.GetAttribute(AttributeType.SkillVariableCastTime)?.CurrentValue ?? 0;

            // Percentage Reductions
            float vctPercent = stats.GetAttribute(AttributeType.VariableCastTimeReducPercent)?.CurrentValue ?? 0;
            float gearVCTPercent = stats.GetAttribute(AttributeType.GearVariableCastTimePercent)?.CurrentValue ?? 0;
            float skillVCTPercent = stats.GetAttribute(AttributeType.SkillVariableCastTimePercent)?.CurrentValue ?? 0;

            float statTerm = Mathf.Clamp01((dex * 2f + intel) / 530f);
            float statReduc = Mathf.Sqrt(statTerm);
            
            // Formula: (Base - Flats) * (1 - StatReduc) * (1 - Gear%) * (1 - Skill%) * (1 - Buff%)
            float vctRemaining = vctOverride - vctFlat - gearVCTFlat - skillVCTFlat;
            float vct = vctRemaining 
                        * (1f - statReduc) 
                        * (1f - (gearVCTPercent / 100f)) 
                        * (1f - (skillVCTPercent / 100f))
                        * (1f - (vctPercent / 100f));
            
            vct = Mathf.Max(0f, vct);

            // --- FCT Calculation ---
            float fctFlat = stats.GetAttribute(AttributeType.FixedCastTimeReduc)?.CurrentValue ?? 0;
            float fctPercent = stats.GetAttribute(AttributeType.FixedCastTimeReducPercent)?.CurrentValue ?? 0;

            // FCT logic: Base - Flat, then apply highest % reduction (usually) or multiplicative
            // Here we use multiplicative for consistency with VCT
            float fct = (fctOverride - fctFlat) * (1f - (fctPercent / 100f));
            fct = Mathf.Max(0f, fct);

            return vct + fct;
        }

        public float CalculateCooldownModified(AttributeSystem stats, float baseCooldown)
        {
            if (stats == null) return baseCooldown;

            float flatReduc = stats.GetAttribute(AttributeType.CooldownReduc)?.CurrentValue ?? 0;
            float percentReduc = stats.GetAttribute(AttributeType.CooldownReducPercent)?.CurrentValue ?? 0;

            float finalCooldown = (baseCooldown - flatReduc) * (1f - (percentReduc / 100f));
            return Mathf.Max(0f, finalCooldown);
        }

        public float CalculateAOERadiusModified(AttributeSystem stats, float baseAOE)
        {
            if (stats == null) return baseAOE;

            // Id 150 = AOE (Area Of Effect)
            float aoeBonusPercent = stats.GetAttribute(AttributeType.AOE)?.CurrentValue ?? 0;

            // Formula: Base * (1 + Bonus%)
            float finalAOE = baseAOE * (1f + (aoeBonusPercent / 100f));
            return Mathf.Max(0f, finalAOE);
        }
    }
}
