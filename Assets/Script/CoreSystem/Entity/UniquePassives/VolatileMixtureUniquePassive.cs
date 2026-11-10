using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("VolatileMixture")]
    public class VolatileMixtureUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnConsumableUsed(Consumable consumable)
        {
            float amount = (consumable.HealHP + consumable.HealMP) * 0.5f + Stat(AttributeType.MATK) * 0.5f;
            foreach (IEntity enemy in EnemiesAround(Owner.transform.position, 4f)) DealProcDamage(enemy, amount, DamageType.Magic, ProcSkillIds.UniqueProc, ElementType.Fire);
        }
        #endregion
    }
}
