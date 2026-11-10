using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("HollowCrown")]
    public class HollowCrownUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void OnEquip()
        {
            SetBonus(AttributeType.MaxHPPercent, -50f);
        }

        public override void OnUnequip()
        {
            ClearBonus(AttributeType.MaxHPPercent);
        }

        public override void TriggerOnPostHit(IEntity target, float damage, SkillPayload payload)
        {
            if (!IsOwnHit(payload) || damage <= 0f) return;
            var hp = Owner.attributeSystem.GetAttribute(AttributeType.HP);
            hp.BaseValue = Mathf.Min(hp.CurrentValue + damage * 0.15f, Stat(AttributeType.MaxHP) * 1.5f);
            hp.Recalculate();
        }
        #endregion
    }
}
