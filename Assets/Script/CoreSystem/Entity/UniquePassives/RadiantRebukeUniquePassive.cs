using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("RadiantRebuke")]
    public class RadiantRebukeUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnHealed(IEntity target, float amount, SkillPayload payload)
        {
            if (payload == null || payload.SkillID == 0 || amount <= 0f) return;
            foreach (IEntity enemy in EnemiesAround(Owner.transform.position, 4f)) DealProcDamage(enemy, amount * 0.5f, DamageType.Magic, ProcSkillIds.UniqueProc, ElementType.Holy);
        }
        #endregion
    }
}
