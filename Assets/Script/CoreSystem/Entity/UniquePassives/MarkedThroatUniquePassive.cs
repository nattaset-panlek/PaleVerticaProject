using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("MarkedThroat")]
    public class MarkedThroatUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (!IsOwnHit(payload) || target == null || payload.IsCriticalHit) return;
            Vector3 toTarget = (target.transform.position - Owner.transform.position).normalized;
            if (Vector3.Dot(target.FacingDirection, toTarget) > 0.3f) damage *= DamageFormula.BaseCriticalMultiplier + Stat(AttributeType.CriticalDamage);
        }
        #endregion
    }
}
