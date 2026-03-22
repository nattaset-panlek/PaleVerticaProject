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
        
        public Vector3 OriginPosition;
        public List<IEntity> Targets = new List<IEntity>();
        
        public float BaseDamage;
        public float Heal;
        public float AOERadius;
        public int ProjectileCount;
        
        public float VCT;
        public float FCT;
        public float CastDelay;
        public float Cooldown;
        public float ManaCost;

        public SkillPayload Clone()
        {
            return new SkillPayload
            {
                Caster = this.Caster,
                SkillID = this.SkillID,
                SkillLevel = this.SkillLevel,
                SkillElement = this.SkillElement,
                OriginPosition = this.OriginPosition,
                Targets = new List<IEntity>(this.Targets),
                BaseDamage = this.BaseDamage,
                DamageType = this.DamageType,
                Heal = this.Heal,
                AOERadius = this.AOERadius,
                ProjectileCount = this.ProjectileCount,
                VCT = this.VCT,
                FCT = this.FCT,
                CastDelay = this.CastDelay,
                Cooldown = this.Cooldown,
                ManaCost = this.ManaCost
            };
        }
    }
}
