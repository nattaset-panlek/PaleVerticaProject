using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1015)]
    public class ManaLeechSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float leechPercent = 10f;
        #endregion

        #region [Public Methods]
        public ManaLeechSupportModifier()
        {
            Chroma = ChromaType.Soul;
            Mineral = MineralType.Celestite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, leechPercent);
        }
        #endregion
    }
}
