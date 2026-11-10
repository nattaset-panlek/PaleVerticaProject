using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("SecondDose")]
    public class SecondDoseUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override bool ShouldKeepConsumable()
        {
            return Random.Range(0f, 100f) < 25f;
        }
        #endregion
    }
}
