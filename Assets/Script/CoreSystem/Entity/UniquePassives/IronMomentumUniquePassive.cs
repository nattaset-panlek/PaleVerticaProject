using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("IronMomentum")]
    public class IronMomentumUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private IEntity _target;
        private int _stacks;
        #endregion

        #region [Public Methods]
        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (!IsOwnHit(payload)) return;
            _stacks = ReferenceEquals(target, _target) ? Mathf.Min(_stacks + 1, 10) : 0;
            _target = target;
            damage *= 1f + _stacks * 0.04f;
        }
        #endregion
    }
}
