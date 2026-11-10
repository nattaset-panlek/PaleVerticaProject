using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("VeilStep")]
    public class VeilStepUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private float _time;
        #endregion

        #region [Public Methods]
        public override void TriggerOnTakeDamage(IEntity attacker, ref float damage, SkillPayload payload)
        {
            if (_time > 0f)
            {
                damage = 0f;
                return;
            }
            if (Random.Range(0f, 100f) >= Stat(AttributeType.PerfectDodge)) return;
            damage = 0f;
            _time = 1.5f;
        }

        public override void Tick(float dt)
        {
            _time -= dt;
        }
        #endregion
    }
}
