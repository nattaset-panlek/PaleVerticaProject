using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1013)]
    public class KnockbackSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float knockbackDistance = 2f;
        #endregion

        #region [Public Methods]
        public KnockbackSupportModifier()
        {
            Chroma = ChromaType.Physical;
            Mineral = MineralType.Hematite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, knockbackDistance);
        }
        #endregion
    }
}
