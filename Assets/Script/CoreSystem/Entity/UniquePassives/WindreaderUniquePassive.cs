using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("Windreader")]
    public class WindreaderUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (!IsOwnHit(payload) || target == null) return;
            float distance = Vector3.Distance(Owner.transform.position, target.transform.position);
            damage *= 1f + Mathf.Min(distance * 0.06f, 0.6f);
        }
        #endregion
    }
}
