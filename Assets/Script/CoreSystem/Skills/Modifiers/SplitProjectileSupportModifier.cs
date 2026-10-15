using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1018)]
    public class SplitProjectileSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public int extraProjectiles = 2;
        #endregion

        #region [Public Methods]
        public SplitProjectileSupportModifier()
        {
            Chroma = ChromaType.Cosmic;
            Mineral = MineralType.Pallasite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.ProjectileCount += extraProjectiles;
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.Tags.Contains(SkillTag.Projectile);
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, extraProjectiles);
        }
        #endregion
    }
}
