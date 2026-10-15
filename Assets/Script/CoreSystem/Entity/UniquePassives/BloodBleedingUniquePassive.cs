using UnityEngine;
using CoreSystem.Skills;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("BloodBleeding")]
    public class BloodBleedingUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnPostHit(IEntity target, float damage, SkillPayload payload)
        {
            if (payload.Caster == Owner && target != null && target != Owner && Random.Range(0f, 100f) < 20f)
            {
                var targetCharacter = target as Character;
                if (targetCharacter != null && targetCharacter.UniquePassives != null)
                {
                    float casterInt = Owner.attributeSystem.GetAttribute(AttributeType.INT).CurrentValue;
                    float casterDex = Owner.attributeSystem.GetAttribute(AttributeType.DEX).CurrentValue;
                    
                    var dotBuff = new BloodBleedingDOTUniquePassive
                    {
                        CasterInt = casterInt,
                        CasterDex = casterDex,
                        Duration = 5f,
                        RemainingDuration = 5f
                    };
                    dotBuff.Initialize(BloodBleedingDOTUniquePassive.PassiveId, targetCharacter);
                    targetCharacter.UniquePassives.AddPassive(dotBuff);
                }
            }
        }
        #endregion
    }
}
