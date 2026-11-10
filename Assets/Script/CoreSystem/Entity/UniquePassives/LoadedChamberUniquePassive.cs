using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("LoadedChamber")]
    public class LoadedChamberUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private Vector3 _lastPosition;
        private float _stored;
        #endregion

        #region [Public Methods]
        public override void Tick(float dt)
        {
            Vector3 position = Owner.transform.position;
            _stored = (position - _lastPosition).sqrMagnitude < 0.0001f ? Mathf.Min(_stored + dt, 3f) : 0f;
            _lastPosition = position;
        }

        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (!IsOwnHit(payload)) return;
            damage *= 1f + _stored * 0.5f;
            _stored = 0f;
        }
        #endregion
    }
}
