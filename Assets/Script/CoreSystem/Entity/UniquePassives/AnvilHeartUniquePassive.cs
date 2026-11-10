using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("AnvilHeart")]
    public class AnvilHeartUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (!IsOwnHit(payload)) return;
            float multiplier = DamageFormula.BaseCriticalMultiplier + Stat(AttributeType.CriticalDamage);
            if (payload.IsCriticalHit) damage /= multiplier;
            float chance = Mathf.Clamp01(Stat(AttributeType.CriticalRate) * 0.01f);
            damage *= 1f + chance * (multiplier - 1f);
        }
        #endregion
    }
}
