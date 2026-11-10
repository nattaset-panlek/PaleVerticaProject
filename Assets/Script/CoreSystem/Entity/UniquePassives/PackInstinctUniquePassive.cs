using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("PackInstinct")]
    public class PackInstinctUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private float _timer;
        private int _count = -1;
        #endregion

        #region [Public Methods]
        public override void Tick(float dt)
        {
            _timer -= dt;
            if (_timer > 0f) return;
            _timer = 0.5f;
            int count = OwnedSummons().Count;
            if (count == _count) return;
            _count = count;
            SetBonus(AttributeType.MoveSpeedPercent, count * 5f);
            SetBonus(AttributeType.ASPDPercent, count * 5f);
        }

        public override void OnUnequip()
        {
            ClearBonus(AttributeType.MoveSpeedPercent);
            ClearBonus(AttributeType.ASPDPercent);
        }
        #endregion
    }
}
