using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1008)]
    public class CostReductionSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float costMultiplier = 0.5f;
        #endregion

        #region [Public Methods]
        public CostReductionSupportModifier()
        {
            Chroma = ChromaType.Dimensional;
            Mineral = MineralType.Ulexite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.ManaCost *= costMultiplier;
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, (1f - costMultiplier) * 100f);
        }
        #endregion
    }
}
