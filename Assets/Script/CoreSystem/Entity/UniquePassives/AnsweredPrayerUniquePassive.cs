using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("AnsweredPrayer")]
    public class AnsweredPrayerUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void ModifyOutgoingHeal(IEntity target, ref float amount, SkillPayload payload)
        {
            if (ReferenceEquals(target, Owner)) amount *= 2f - HpRatio();
        }
        #endregion
    }
}
