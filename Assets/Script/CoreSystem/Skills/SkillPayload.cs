using System.Collections.Generic;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills
{
    public class SkillPayload
    {
        public Character Caster;
        public int SkillID;
        public int SkillLevel;
        public ElementType SkillElement;
        public DamageType DamageType;
        public Logic.TargetingLogic SkillTargeting;
        public List<Logic.EffectLogic> SkillEffects = new List<Logic.EffectLogic>();
        
        public Vector3 OriginPosition;
        public Vector3 TargetPosition; // For ground-targeted skills
        public bool IsTargetPositionSet; // To distinguish between 0,0,0 and unset
        public bool IsPositionLocked; // If true, range-clamping should be skipped or anchored to the lock-in point
        public List<IEntity> Targets = new List<IEntity>();
        
        public Vector3 GetAimDirection()
        {
            if (IsTargetPositionSet)
            {
                Vector3 dir = TargetPosition - OriginPosition;
                dir.z = 0f;
                if (dir.sqrMagnitude > 0.001f)
                {
                    return dir.normalized;
                }
            }
            if (Caster != null)
            {
                return Caster.FacingDirection;
            }
            return Vector3.right;
        }
        
        public float BaseDamage;
        public float Heal;
        public float AOERadius;
        public float CastRange;
        public int ProjectileCount;
        public GameObject SkillIndicatorPrefab;
        
        public float VCT;
        public float FCT;
        public float CastDelay;
        public float Cooldown;
        public float ManaCost;
        public float TotalCastTime;

        public SkillPayload Clone()
        {
            return new SkillPayload
            {
                Caster = this.Caster,
                SkillID = this.SkillID,
                SkillLevel = this.SkillLevel,
                SkillElement = this.SkillElement,
                DamageType = this.DamageType,
                SkillTargeting = this.SkillTargeting,
                SkillEffects = new List<Logic.EffectLogic>(this.SkillEffects),
                OriginPosition = this.OriginPosition,
                TargetPosition = this.TargetPosition,
                IsTargetPositionSet = this.IsTargetPositionSet,
                IsPositionLocked = this.IsPositionLocked,
                Targets = new List<IEntity>(this.Targets),
                BaseDamage = this.BaseDamage,
                Heal = this.Heal,
                AOERadius = this.AOERadius,
                CastRange = this.CastRange,
                ProjectileCount = this.ProjectileCount,
                SkillIndicatorPrefab = this.SkillIndicatorPrefab,
                VCT = this.VCT,
                FCT = this.FCT,
                CastDelay = this.CastDelay,
                Cooldown = this.Cooldown,
                ManaCost = this.ManaCost,
                TotalCastTime = this.TotalCastTime
            };
        }
    }
}
