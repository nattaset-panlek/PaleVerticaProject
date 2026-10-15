using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1019)]
    public class HomingSupportModifier : SupportModifier
    {
        #region [Public Methods]
        public HomingSupportModifier()
        {
            Chroma = ChromaType.Dimensional;
            Mineral = MineralType.Alexandrite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.Tags.Contains(SkillTag.Projectile);
        }

        public override string GetDescription(string template)
        {
            return template;
        }
        #endregion
    }
}
