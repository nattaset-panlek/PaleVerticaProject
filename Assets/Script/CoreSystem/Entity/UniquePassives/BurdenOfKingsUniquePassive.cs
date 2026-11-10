using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("BurdenOfKings")]
    public class BurdenOfKingsUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private float _timer;
        private int _pieces = -1;
        #endregion

        #region [Public Methods]
        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (IsOwnHit(payload) && payload.DamageType == DamageType.Physical) damage *= 1f + EquippedPieces() * 0.03f;
        }

        public override void Tick(float dt)
        {
            _timer -= dt;
            if (_timer > 0f) return;
            _timer = 1f;
            int pieces = EquippedPieces();
            if (pieces == _pieces) return;
            _pieces = pieces;
            SetBonus(AttributeType.MoveSpeedPercent, -2f * pieces);
        }

        public override void OnUnequip()
        {
            ClearBonus(AttributeType.MoveSpeedPercent);
        }

        private int EquippedPieces()
        {
            int count = 0;
            foreach (var item in Owner.EquipmentSlots) if (item != null) count++;
            return count;
        }
        #endregion
    }
}
