using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1021)]
    public class EchoSpellSupportModifier : SupportModifier, ICastRepeater
    {
        #region [Variables/Fields]
        public float aoeMultiplier = 1.3f;
        public float damageMultiplier = 0.6f;
        public float repeatDelay = 0.15f;
        #endregion

        #region [Properties]
        public float RepeatDelay => repeatDelay;
        #endregion

        #region [Public Methods]
        public EchoSpellSupportModifier()
        {
            Chroma = ChromaType.Magic;
            Mineral = MineralType.Fluorite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public SkillPayload CreateRepeatPayload(SkillPayload original)
        {
            var echoPayload = original.Clone();
            echoPayload.AOERadius *= aoeMultiplier;
            echoPayload.BaseDamage *= damageMultiplier;
            return echoPayload;
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, (aoeMultiplier - 1f) * 100f, (1f - damageMultiplier) * 100f);
        }
        #endregion
    }
}
