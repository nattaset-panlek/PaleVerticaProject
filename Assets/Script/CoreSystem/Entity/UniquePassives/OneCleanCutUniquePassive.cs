using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("OneCleanCut")]
    public class OneCleanCutUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private IEntity _target;
        #endregion

        #region [Public Methods]
        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (!IsOwnHit(payload)) return;
            damage *= ReferenceEquals(target, _target) ? 0.7f : 2f;
            _target = target;
        }
        #endregion
    }
}
