using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("GraveTithe")]
    public class GraveTitheUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void OnEquip()
        {
            CombatEvents.OnKilled += HandleKilled;
        }

        public override void OnUnequip()
        {
            CombatEvents.OnKilled -= HandleKilled;
        }

        private void HandleKilled(IEntity killer, IEntity victim)
        {
            if (!TeamRegistry.IsHostile(Owner, victim)) return;
            if (Vector3.Distance(Owner.transform.position, victim.transform.position) > 5f) return;
            RestoreResource(AttributeType.HP, AttributeType.MaxHP, Stat(AttributeType.MaxHP) * 0.03f);
        }
        #endregion
    }
}
