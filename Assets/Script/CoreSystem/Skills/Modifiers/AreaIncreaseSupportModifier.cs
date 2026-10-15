using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1005)]
    public class AreaIncreaseSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float aoePercentBonus = 4f;
        public float vctPercentBonus = 2f;
        #endregion

        #region [Public Methods]
        public AreaIncreaseSupportModifier()
        {
            Chroma = ChromaType.Cosmic;
            Mineral = MineralType.Pallasite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.AOERadius += (payload.AOERadius * aoePercentBonus / 100f);
            payload.VCT += (payload.VCT * vctPercentBonus / 100f);
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.Tags.Contains(SkillTag.AreaOfEffect);
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, aoePercentBonus, vctPercentBonus);
        }
        #endregion
    }
}
