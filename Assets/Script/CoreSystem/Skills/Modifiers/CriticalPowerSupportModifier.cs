using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1014)]
    public class CriticalPowerSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float critDamageMult = 1.5f;
        public float lukScalingMult = 0.05f;
        #endregion

        #region [Public Methods]
        public CriticalPowerSupportModifier()
        {
            Chroma = ChromaType.Soul;
            Mineral = MineralType.Moldavite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            float bonus = 0f;
            if (payload.Caster != null && payload.Caster.attributeSystem != null)
            {
                bonus = payload.Caster.attributeSystem.GetAttribute(AttributeType.LUK).CurrentValue * lukScalingMult;
            }
            payload.BaseDamage *= (critDamageMult + bonus);
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, (critDamageMult - 1f) * 100f, lukScalingMult * 100f);
        }
        #endregion
    }
}
