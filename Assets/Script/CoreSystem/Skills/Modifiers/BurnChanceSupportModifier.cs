using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1011)]
    public class BurnChanceSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float burnChancePercent = 20f;
        #endregion

        #region [Public Methods]
        public BurnChanceSupportModifier()
        {
            Chroma = ChromaType.Origin;
            Mineral = MineralType.Amber;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.SkillElement == ElementType.Fire;
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, burnChancePercent);
        }
        #endregion
    }
}
