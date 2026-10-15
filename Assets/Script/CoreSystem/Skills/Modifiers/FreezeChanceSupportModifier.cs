using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1012)]
    public class FreezeChanceSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float freezeChancePercent = 20f;
        #endregion

        #region [Public Methods]
        public FreezeChanceSupportModifier()
        {
            Chroma = ChromaType.Origin;
            Mineral = MineralType.Amber;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.SkillElement == ElementType.Water;
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, freezeChancePercent);
        }
        #endregion
    }
}
