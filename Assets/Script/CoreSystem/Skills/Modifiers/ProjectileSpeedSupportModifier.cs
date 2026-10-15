using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1017)]
    public class ProjectileSpeedSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float speedPercent = 30f;
        #endregion

        #region [Public Methods]
        public ProjectileSpeedSupportModifier()
        {
            Chroma = ChromaType.Cosmic;
            Mineral = MineralType.Tektite;
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
            return string.Format(template, speedPercent);
        }
        #endregion
    }
}
