using UnityEngine;
using CoreSystem.Skills;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("ManaLeech")]
    public class ManaLeechUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnPostHit(IEntity target, float damage, SkillPayload payload)
        {
            if (payload.Caster == Owner && target != null && Random.Range(0f, 100f) < 30f)
            {
                float mpRecPercent = Owner.attributeSystem.GetAttribute(AttributeType.MPRecoveryPercent).CurrentValue;
                float leechAmount = 5f * (1f + (mpRecPercent / 100f));
                RestoreResource(AttributeType.MP, AttributeType.MaxMP, leechAmount);
            }
        }
        #endregion
    }
}
