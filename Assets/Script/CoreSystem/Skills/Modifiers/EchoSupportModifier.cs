using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1002)]
    public class EchoSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float damageMultiplier = 0.8f;
        public float cooldownMultiplier = 1.25f;
        #endregion

        #region [Public Methods]
        public EchoSupportModifier()
        {
            Chroma = ChromaType.Magic;
            Mineral = MineralType.Fluorite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.BaseDamage = (payload.BaseDamage * damageMultiplier) + (payload.Cooldown * cooldownMultiplier);
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, damageMultiplier * 100f, cooldownMultiplier);
        }
        #endregion
    }
}
