using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1003)]
    public class PierceSupportModifier : SupportModifier
    {
        #region [Public Methods]
        public PierceSupportModifier()
        {
            Chroma = ChromaType.Physical;
            Mineral = MineralType.Obsidian;
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
