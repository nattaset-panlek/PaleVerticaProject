using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1001)]
    public class MultiShotSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public int extraProjectiles = 1;
        #endregion

        #region [Public Methods]
        public MultiShotSupportModifier()
        {
            Chroma = ChromaType.Physical;
            Mineral = MineralType.Hematite;
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
