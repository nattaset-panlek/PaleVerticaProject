using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("TransmutedBlood")]
    public class TransmutedBloodUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnConsumableUsed(Consumable consumable)
        {
            if (consumable.HealHP > 0f) RestoreResource(AttributeType.MP, AttributeType.MaxMP, consumable.HealHP * 0.5f);
        }
        #endregion
    }
}
