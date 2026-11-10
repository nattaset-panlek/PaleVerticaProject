using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("LivingPhilosopher")]
    public class LivingPhilosopherUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override bool CanUseConsumable()
        {
            return false;
        }

        public override void Tick(float dt)
        {
            RestoreResource(AttributeType.HP, AttributeType.MaxHP, Stat(AttributeType.VIT) * 0.5f * dt);
        }
        #endregion
    }
}
