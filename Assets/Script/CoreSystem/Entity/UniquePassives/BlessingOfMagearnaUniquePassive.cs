using UnityEngine;
using CoreSystem.Skills;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("BlessingOfMagearna")]
    public class BlessingOfMagearnaUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnTakeDamage(IEntity attacker, ref float damage, SkillPayload payload)
        {
            if (damage > 0f)
            {
                float absorbable = damage * 0.4f;
                var mpAttr = Owner.attributeSystem.GetAttribute(AttributeType.MP);
                if (mpAttr != null && mpAttr.CurrentValue > 0f)
                {
                    float absorbed = Mathf.Min(absorbable, mpAttr.CurrentValue);
                    mpAttr.BaseValue = mpAttr.CurrentValue - absorbed;
                    mpAttr.Recalculate();
                    damage -= absorbed;
                }
            }
        }
        #endregion
    }
}
