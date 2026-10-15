using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1007)]
    public class CooldownReductionSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float cdrPercent = 10f;
        #endregion

        #region [Public Methods]
        public CooldownReductionSupportModifier()
        {
            Chroma = ChromaType.Dimensional;
            Mineral = MineralType.Alexandrite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.Cooldown *= (1f - (cdrPercent / 100f));
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, cdrPercent);
        }
        #endregion
    }
}
