using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1006)]
    public class DamageUpSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float damageMultiplier = 1.2f;
        #endregion

        #region [Public Methods]
        public DamageUpSupportModifier()
        {
            Chroma = ChromaType.Origin;
            Mineral = MineralType.Zircon;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.BaseDamage *= damageMultiplier;
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, (damageMultiplier - 1f) * 100f);
        }
        #endregion
    }
}
