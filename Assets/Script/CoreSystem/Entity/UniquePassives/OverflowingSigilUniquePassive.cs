using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("OverflowingSigil")]
    public class OverflowingSigilUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private float _stored;
        #endregion

        #region [Public Methods]
        public override void Tick(float dt)
        {
            if (MpRatio() < 1f) return;
            _stored = Mathf.Min(_stored + Stat(AttributeType.MPRecovery) * dt, Stat(AttributeType.MaxMP) * 0.3f);
        }

        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (!IsOwnHit(payload) || IsBasicAttack(payload)) return;
            damage += _stored;
            _stored = 0f;
        }
        #endregion
    }
}
