using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("VowOfStillness")]
    public class VowOfStillnessUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private Vector3 _lastPosition;
        private float _still;
        private bool _active;
        #endregion

        #region [Public Methods]
        public override void Tick(float dt)
        {
            Vector3 position = Owner.transform.position;
            _still = (position - _lastPosition).sqrMagnitude < 0.0001f ? _still + dt : 0f;
            _lastPosition = position;
            bool active = _still >= 1f;
            if (active == _active) return;
            _active = active;
            if (active)
            {
                SetBonus(AttributeType.MDEF, 20f);
                SetBonus(AttributeType.MPRecovery, 5f);
            }
            else Clear();
        }

        public override void OnUnequip()
        {
            Clear();
        }

        private void Clear()
        {
            ClearBonus(AttributeType.MDEF);
            ClearBonus(AttributeType.MPRecovery);
        }
        #endregion
    }
}
