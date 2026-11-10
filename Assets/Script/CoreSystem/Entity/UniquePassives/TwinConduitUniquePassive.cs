using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("TwinConduit")]
    public class TwinConduitUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override bool CanEquip(Equipment equipment)
        {
            return equipment.Slot != EquipmentSlot.WeaponTwoHand;
        }

        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (!IsOwnHit(payload)) return;
            float atk = Stat(AttributeType.ATK);
            float matk = Stat(AttributeType.MATK);
            float own = payload.DamageType == DamageType.Magic ? matk : atk;
            float other = payload.DamageType == DamageType.Magic ? atk : matk;
            if (own > 0f) damage *= 1f + other * 0.5f / own;
        }
        #endregion
    }
}
