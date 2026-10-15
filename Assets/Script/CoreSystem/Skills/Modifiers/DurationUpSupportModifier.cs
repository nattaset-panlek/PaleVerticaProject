using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1009)]
    public class DurationUpSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float durationPercent = 20f;
        #endregion

        #region [Public Methods]
        public DurationUpSupportModifier()
        {
            Chroma = ChromaType.Cosmic;
            Mineral = MineralType.Tektite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, durationPercent);
        }
        #endregion
    }
}
