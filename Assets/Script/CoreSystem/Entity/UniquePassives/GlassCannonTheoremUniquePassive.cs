using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("GlassCannonTheorem")]
    public class GlassCannonTheoremUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (IsOwnHit(payload) && payload.DamageType == DamageType.Magic) damage *= 1f + MpRatio() * 0.5f;
        }
        #endregion
    }
}
