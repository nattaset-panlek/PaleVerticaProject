using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1020)]
    public class EmpoweredCastSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float baseEmpowerDamage = 50f;
        public float intScalingMult = 1f;
        #endregion

        #region [Public Methods]
        public EmpoweredCastSupportModifier()
        {
            Chroma = ChromaType.Magic;
            Mineral = MineralType.Labradorite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            float bonus = 0f;
            if (payload.Caster != null && payload.Caster.attributeSystem != null)
            {
                bonus = payload.Caster.attributeSystem.GetAttribute(AttributeType.INT).CurrentValue * intScalingMult;
            }
            payload.BaseDamage += (baseEmpowerDamage + bonus);
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.SkillDamageType == DamageType.Magic;
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, baseEmpowerDamage, intScalingMult * 100f);
        }
        #endregion
    }
}
