using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("LastBanner")]
    public class LastBannerUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private float _shield;
        private float _shieldTime;
        private float _cooldown;
        #endregion

        #region [Public Methods]
        public override void TriggerOnTakeDamage(IEntity attacker, ref float damage, SkillPayload payload)
        {
            if (_cooldown <= 0f && Stat(AttributeType.HP) - damage < Stat(AttributeType.MaxHP) * 0.5f)
            {
                _shield = Stat(AttributeType.MaxHP) * 0.2f;
                _shieldTime = 5f;
                _cooldown = 30f;
            }
            float absorbed = Mathf.Min(_shield, damage);
            _shield -= absorbed;
            damage -= absorbed;
        }

        public override void Tick(float dt)
        {
            _cooldown -= dt;
            _shieldTime -= dt;
            if (_shieldTime <= 0f) _shield = 0f;
        }
        #endregion
    }
}
