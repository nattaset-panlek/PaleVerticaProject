using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("HollowQuiver")]
    public class HollowQuiverUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override bool CanBasicAttack()
        {
            return false;
        }

        public override void ModifyCast(SkillPayload payload)
        {
            payload.PierceCount += 2;
        }

        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (IsOwnHit(payload) && !IsBasicAttack(payload)) damage *= 1.4f;
        }
        #endregion
    }
}
