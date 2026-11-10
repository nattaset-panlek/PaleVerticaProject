using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("SingularElement")]
    public class SingularElementUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private bool _locked;
        private ElementType _element;
        #endregion

        #region [Public Methods]
        public override bool CanCast(SkillPayload payload)
        {
            return !_locked || payload.SkillElement == _element || payload.SkillElement == ElementType.Neutral;
        }

        public override void TriggerOnSkillExecuted(ActiveSkill skill, SkillPayload payload)
        {
            if (_locked || payload.SkillElement == ElementType.Neutral) return;
            _locked = true;
            _element = payload.SkillElement;
        }

        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (IsOwnHit(payload) && _locked && payload.SkillElement == _element) damage *= 1.5f;
        }
        #endregion
    }
}
