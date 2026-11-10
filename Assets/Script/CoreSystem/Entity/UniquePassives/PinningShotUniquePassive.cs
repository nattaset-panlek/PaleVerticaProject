using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("PinningShot")]
    public class PinningShotUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private IEntity _target;
        private int _hits;
        #endregion

        #region [Public Methods]
        public override void TriggerOnPostHit(IEntity target, float damage, SkillPayload payload)
        {
            if (!IsOwnHit(payload) || target == null) return;
            _hits = ReferenceEquals(target, _target) ? _hits + 1 : 1;
            _target = target;
            if (_hits < 3) return;
            _hits = 0;
            target.ApplyStun(0.6f);
        }
        #endregion
    }
}
