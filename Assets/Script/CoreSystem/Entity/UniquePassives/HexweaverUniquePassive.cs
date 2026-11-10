using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("Hexweaver")]
    public class HexweaverUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private readonly Dictionary<IEntity, int> _hexes = new Dictionary<IEntity, int>();
        #endregion

        #region [Public Methods]
        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (IsOwnHit(payload) && target != null && _hexes.TryGetValue(target, out int stacks)) damage *= 1f + stacks * 0.08f;
        }

        public override void TriggerOnPostHit(IEntity target, float damage, SkillPayload payload)
        {
            if (!IsOwnHit(payload) || IsBasicAttack(payload) || target == null) return;
            if (_hexes.Count > 64) _hexes.Clear();
            _hexes.TryGetValue(target, out int stacks);
            _hexes[target] = Mathf.Min(stacks + 1, 5);
        }
        #endregion
    }
}
