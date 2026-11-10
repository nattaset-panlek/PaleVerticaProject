using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("SpellboundEdge")]
    public class SpellboundEdgeUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private bool _charged;
        private ElementType _element;
        #endregion

        #region [Public Methods]
        public override void TriggerOnSkillExecuted(ActiveSkill skill, SkillPayload payload)
        {
            _charged = true;
            _element = payload.SkillElement;
        }

        public override void ModifyBasicAttack(SkillPayload payload)
        {
            if (!_charged) return;
            payload.SkillElement = _element;
            _charged = false;
        }
        #endregion
    }
}
