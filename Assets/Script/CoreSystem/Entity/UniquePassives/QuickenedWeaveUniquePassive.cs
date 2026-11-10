using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("QuickenedWeave")]
    public class QuickenedWeaveUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private int _stacks;
        #endregion

        #region [Public Methods]
        public override void TriggerOnBasicAttack(IEntity target)
        {
            _stacks = Mathf.Min(_stacks + 1, 5);
        }

        public override void ModifyCast(SkillPayload payload)
        {
            payload.TotalCastTime *= 1f - _stacks * 0.1f;
        }

        public override void TriggerOnSkillExecuted(ActiveSkill skill, SkillPayload payload)
        {
            _stacks = 0;
        }
        #endregion
    }
}
