using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("RicochetDoctrine")]
    public class RicochetDoctrineUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private float _lastDamage;
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
            if (IsOwnHit(payload)) _lastDamage = damage;
        }

        private void HandleKilled(IEntity killer, IEntity victim)
        {
            if (!ReferenceEquals(killer, Owner)) return;
            IEntity next = NearestEnemy(victim.transform.position, 5f, victim);
            if (next != null) DealProcDamage(next, _lastDamage * 0.5f, DamageType.Physical, ProcSkillIds.UniqueProc);
        }
        #endregion
    }
}
