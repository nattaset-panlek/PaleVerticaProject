using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1004)]
    public class ChainSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public int chainCount = 2;
        public float chainRange = 5f;
        #endregion

        #region [Public Methods]
        public ChainSupportModifier()
        {
            Chroma = ChromaType.Physical;
            Mineral = MineralType.Obsidian;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.ChainCount += chainCount;
            payload.ChainRange = Mathf.Max(payload.ChainRange, chainRange);
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
