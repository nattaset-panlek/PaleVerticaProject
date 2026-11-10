using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("MarkedThroat")]
    public class MarkedThroatUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        public const float BehindThreshold = 0.3f;
        #endregion

        #region [Properties]
        public override bool UsesFacing => true;
        #endregion

        #region [Public Methods]
        public override bool ForcesCritical(IEntity target, SkillPayload payload)
        {
            if (!IsOwnHit(payload) || target == null || Owner == null) return false;
            return IsBehind(target.FacingDirection, target.transform.position - Owner.transform.position);
        }

        public static bool IsBehind(Vector2 targetFacing, Vector2 attackerToTarget)
        {
            if (targetFacing.sqrMagnitude < 1e-6f || attackerToTarget.sqrMagnitude < 1e-6f) return false;
            return Vector2.Dot(targetFacing.normalized, attackerToTarget.normalized) > BehindThreshold;
        }
        #endregion
    }
}
