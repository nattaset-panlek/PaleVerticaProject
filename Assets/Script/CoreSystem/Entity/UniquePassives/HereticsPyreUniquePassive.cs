using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("HereticsPyre")]
    public class HereticsPyreUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private readonly HashSet<IEntity> _branded = new HashSet<IEntity>();
        #endregion

        #region [Public Methods]
        public override void OnEquip()
        {
            CombatEvents.OnKilled += HandleKilled;
        }

        public override void OnUnequip()
        {
            CombatEvents.OnKilled -= HandleKilled;
        }

        public override void TriggerOnPostHit(IEntity target, float damage, SkillPayload payload)
        {
            if (!IsOwnHit(payload) || target == null) return;
            if (_branded.Count > 64) _branded.Clear();
            _branded.Add(target);
        }

        private void HandleKilled(IEntity killer, IEntity victim)
        {
            if (!ReferenceEquals(killer, Owner) || !_branded.Remove(victim)) return;
            foreach (IEntity enemy in EnemiesAround(victim.transform.position, 3f)) ApplyBurn(enemy, 3f, Stat(AttributeType.MATK) * 0.1f);
        }
        #endregion
    }
}
