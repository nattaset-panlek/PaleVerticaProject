using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("CovenantKeeper")]
    public class CovenantKeeperUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void ModifyOutgoingHeal(IEntity target, ref float amount, SkillPayload payload)
        {
            amount *= 1.75f;
        }

        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (payload != null && payload.Caster == Owner && payload.SkillElement != ElementType.Holy) damage = 0f;
        }
        #endregion
    }
}
