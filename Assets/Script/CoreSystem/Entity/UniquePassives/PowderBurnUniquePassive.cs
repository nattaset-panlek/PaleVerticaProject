using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("PowderBurn")]
    public class PowderBurnUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnPostHit(IEntity target, float damage, SkillPayload payload)
        {
            if (IsOwnHit(payload) && payload.IsCriticalHit) ApplyBurn(target, 3f, Stat(AttributeType.ATK) * 0.1f);
        }
        #endregion
    }
}
