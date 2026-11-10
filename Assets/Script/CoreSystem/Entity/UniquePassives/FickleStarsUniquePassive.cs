using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("FickleStars")]
    public class FickleStarsUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private bool _free;
        #endregion

        #region [Public Methods]
        public override void ModifyManaCost(ref float manaCost, ref float hpCost, ref bool isHpCost)
        {
            if (_free) manaCost = 0f;
        }

        public override void TriggerOnSkillExecuted(ActiveSkill skill, SkillPayload payload)
        {
            _free = Random.Range(0f, 100f) < Mathf.Min(Stat(AttributeType.LUK) * 0.3f, 40f);
        }
        #endregion
    }
}
