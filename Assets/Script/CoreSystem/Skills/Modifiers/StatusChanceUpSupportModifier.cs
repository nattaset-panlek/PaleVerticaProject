using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1010)]
    public class StatusChanceUpSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float statusChancePercent = 15f;
        #endregion

        #region [Public Methods]
        public StatusChanceUpSupportModifier()
        {
            Chroma = ChromaType.Soul;
            Mineral = MineralType.Celestite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, statusChancePercent);
        }
        #endregion
    }
}
