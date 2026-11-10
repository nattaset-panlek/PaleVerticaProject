using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("SkirmishersPace")]
    public class SkirmishersPaceUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private System.Func<float, float> _previous;
        #endregion

        #region [Public Methods]
        public override void OnEquip()
        {
            _previous = Owner.OnResolveAutoAttackRange;
            Owner.OnResolveAutoAttackRange = range => (_previous != null ? _previous(range) : range) * 0.75f;
            SetBonus(AttributeType.MoveSpeedPercent, 15f);
        }

        public override void OnUnequip()
        {
            Owner.OnResolveAutoAttackRange = _previous;
            ClearBonus(AttributeType.MoveSpeedPercent);
        }
        #endregion
    }
}
