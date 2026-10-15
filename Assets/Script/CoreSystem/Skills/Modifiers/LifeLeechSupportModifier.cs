using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1016)]
    public class LifeLeechSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float leechPercent = 10f;
        #endregion

        #region [Public Methods]
        public LifeLeechSupportModifier()
        {
            Chroma = ChromaType.Soul;
            Mineral = MineralType.Moldavite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.SkillDamageType == DamageType.Physical;
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, leechPercent);
        }
        #endregion
    }
}
