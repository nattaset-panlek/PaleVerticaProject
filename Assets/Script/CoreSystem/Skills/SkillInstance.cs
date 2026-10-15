using System.Collections.Generic;
using UnityEngine;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data;

namespace CoreSystem.Skills
{
    [System.Serializable]
    public class SkillInstance
    {
        public ActiveSkill SkillData;
        public List<SupportModifier> LinkedModifiers = new List<SupportModifier>();
        public int CurrentLevel = 1;

        public event System.Action OnSupportSkillsChanged;

        public bool IsToggledOn { get; set; }
        public GameObject ActiveVisual { get; set; }

        public float CurrentCooldown { get; private set; }
        public float MaxCooldown { get; private set; }

        public SkillInstance() { } // Default constructor for Unity serialization

        public SkillInstance(ActiveSkill skillTemplate, int level = 1)
        {
            CurrentLevel = level;
            if (skillTemplate != null)
            {
                if (Application.isPlaying)
                {
                    ActiveSkill syncedSkill = Database.GetSynchronizedSkill(skillTemplate.Id);
                    SkillData = syncedSkill != null ? syncedSkill : skillTemplate;
                }
                else
                {
                    SkillData = skillTemplate;
                }

                if (Application.isPlaying && SkillData != null)
                {
                    List<int> supportIDs = Database.GetLinkedSupportIDs(SkillData.Id);
                    if (supportIDs != null)
                    {
                        for (int i = 0; i < supportIDs.Count; i++)
                        {
                            SupportModifier mod = Database.GetSynchronizedSupport(supportIDs[i]);
                            if (mod != null)
                            {
                                LinkModifier(mod);
                            }
                        }
                    }
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
            MaxCooldown = customCooldown > 0f ? customCooldown : 1f;
            CurrentCooldown = customCooldown;
        }

        public void UpdateCooldown(float deltaTime)
        {
            if (CurrentCooldown > 0f)
            {
                CurrentCooldown -= deltaTime;
            }
        }

        public void ReduceCooldown(float amount)
        {
            CurrentCooldown = Mathf.Max(0f, CurrentCooldown - amount);
        }

        public float CalculateCastTime(AttributeSystem stats)
        {
            if (SkillData == null) return 0f;
            return CalculateCastTimeModified(stats, SkillData.BaseVCT, SkillData.BaseFCT);
        }

        public float CalculateCastTimeModified(AttributeSystem stats, float vctOverride, float fctOverride)
        {
            if (stats == null) return vctOverride + fctOverride;
            return CastTimeFormula.Compute(CastTimeFormula.BuildInput(stats), vctOverride, fctOverride).Total;
        }

        public float CalculateCooldownModified(AttributeSystem stats, float baseCooldown)
        {
            if (stats == null) return baseCooldown;

            float flatReduc = stats.GetAttribute(AttributeType.CooldownReduc)?.CurrentValue ?? 0;
            float percentReduc = stats.GetAttribute(AttributeType.CooldownReducPercent)?.CurrentValue ?? 0;
            return SkillValueFormula.Cooldown(baseCooldown, flatReduc, percentReduc);
        }

        public float CalculateAOERadiusModified(AttributeSystem stats, float baseAOE)
        {
            if (stats == null) return baseAOE;

            float aoeBonusPercent = stats.GetAttribute(AttributeType.AOE)?.CurrentValue ?? 0;
            return SkillValueFormula.Area(baseAOE, aoeBonusPercent);
        }

        public bool LinkModifier(SupportModifier modifier)
        {
            if (modifier == null || SkillData == null) return false;
            if (!modifier.IsCompatible(SkillData)) return false;
            if (LinkedModifiers.Contains(modifier)) return false;

            int maxSlots = SkillData.MaxSupportSlots > 0 ? SkillData.MaxSupportSlots : 5;
            for (int i = 0; i < LinkedModifiers.Count; i++)
            {
                if (LinkedModifiers[i] == null)
                {
                    LinkedModifiers[i] = modifier;
                    OnSupportSkillsChanged?.Invoke();
                    return true;
                }
            }

            if (LinkedModifiers.Count < maxSlots)
            {
                LinkedModifiers.Add(modifier);
                OnSupportSkillsChanged?.Invoke();
                return true;
            }

            return false;
        }

        public void UnlinkModifier(SupportModifier modifier)
        {
            if (modifier == null) return;
            if (LinkedModifiers.Remove(modifier))
            {
                OnSupportSkillsChanged?.Invoke();
            }
        }

        public void UnlinkModifierAt(int index)
        {
            if (index >= 0 && index < LinkedModifiers.Count)
            {
                LinkedModifiers[index] = null;
                OnSupportSkillsChanged?.Invoke();
            }
        }

        public SupportModifier GetModifierAt(int index)
        {
            if (index >= 0 && index < LinkedModifiers.Count)
            {
                return LinkedModifiers[index];
            }
            return null;
        }

        public bool SetModifierAt(int index, SupportModifier modifier)
        {
            if (SkillData == null || index < 0) return false;
            int maxSlots = SkillData.MaxSupportSlots > 0 ? SkillData.MaxSupportSlots : 5;
            if (index >= maxSlots) return false;

            if (modifier != null && !modifier.IsCompatible(SkillData)) return false;

            while (LinkedModifiers.Count <= index)
            {
                LinkedModifiers.Add(null);
            }

            LinkedModifiers[index] = modifier;
            OnSupportSkillsChanged?.Invoke();
            return true;
        }
    }
}
