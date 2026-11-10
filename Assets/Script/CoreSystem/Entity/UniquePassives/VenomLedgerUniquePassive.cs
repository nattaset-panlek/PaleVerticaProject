using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("VenomLedger")]
    public class VenomLedgerUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (!IsOwnHit(payload) || !(target is Character character) || character.Buffs == null) return;
            damage *= 1f + Mathf.Min(character.Buffs.ActiveBuffs.Count, 5) * 0.1f;
        }
        #endregion
    }
}
