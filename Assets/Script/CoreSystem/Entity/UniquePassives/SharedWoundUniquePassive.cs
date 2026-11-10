using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("SharedWound")]
    public class SharedWoundUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnTakeDamage(IEntity attacker, ref float damage, SkillPayload payload)
        {
            foreach (SummonedUnit unit in OwnedSummons())
            {
                var hp = unit.Body.attributeSystem.GetAttribute(AttributeType.HP);
                if (hp == null || hp.CurrentValue <= 1f) continue;
                float shared = Mathf.Min(damage * 0.3f, hp.CurrentValue - 1f);
                hp.BaseValue = hp.CurrentValue - shared;
                hp.Recalculate();
                damage -= shared;
                return;
            }
        }
        #endregion
    }
}
