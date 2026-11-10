using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("SixFates")]
    public class SixFatesUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private static readonly float[] Fates = { 0f, 0.5f, 1f, 1f, 1.5f, 3f };
        #endregion

        #region [Public Methods]
        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (IsOwnHit(payload)) damage *= Fates[Random.Range(0, Fates.Length)];
        }
        #endregion
    }
}
