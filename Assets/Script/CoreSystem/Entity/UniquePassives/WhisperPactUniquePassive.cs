using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("WhisperPact")]
    public class WhisperPactUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private float _corruption;
        private int _applied = -1;
        #endregion

        #region [Public Methods]
        public override void TriggerOnSkillExecuted(ActiveSkill skill, SkillPayload payload)
        {
            _corruption = Mathf.Min(_corruption + 4f, 100f);
        }

        public override void Tick(float dt)
        {
            _corruption = Mathf.Max(0f, _corruption - dt);
            int step = Mathf.FloorToInt(_corruption / 10f);
            if (step == _applied) return;
            _applied = step;
            SetBonus(AttributeType.MaxHPPercent, step * -4f);
        }

        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (IsOwnHit(payload)) damage *= 1f + _corruption * 0.01f;
        }

        public override void OnUnequip()
        {
            ClearBonus(AttributeType.MaxHPPercent);
        }
        #endregion
    }
}
