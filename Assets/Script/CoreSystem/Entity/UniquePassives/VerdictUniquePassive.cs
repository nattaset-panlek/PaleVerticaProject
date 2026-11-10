using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("Verdict")]
    public class VerdictUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private readonly HashSet<IEntity> _branded = new HashSet<IEntity>();
        #endregion

        #region [Public Methods]
        public override void TriggerOnPostHit(IEntity target, float damage, SkillPayload payload)
        {
            if (!IsOwnHit(payload) || target == null) return;
            if (_branded.Remove(target))
            {
                DealProcDamage(target, (Stat(AttributeType.ATK) + Stat(AttributeType.MATK)) * 0.5f, DamageType.Magic, ProcSkillIds.UniqueProc, ElementType.Holy);
                return;
            }
            if (_branded.Count > 64) _branded.Clear();
            _branded.Add(target);
        }
        #endregion
    }
}
