using CoreSystem.Skills;
using CoreSystem.UI;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("Execution")]
    public class ExecutionUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnPostHit(IEntity target, float damage, SkillPayload payload)
        {
            if (payload.Caster == Owner && target != null && target != Owner)
            {
                var targetHp = target.attributeSystem.GetAttribute(AttributeType.HP);
                var targetMaxHp = target.attributeSystem.GetAttribute(AttributeType.MaxHP);
                if (targetHp != null && targetMaxHp != null && targetHp.CurrentValue > 0f)
                {
                    if (targetHp.CurrentValue <= targetMaxHp.CurrentValue * 0.05f)
                    {
                        targetHp.BaseValue = 0f;
                        targetHp.Recalculate();
                        if (DamageTextManager.Instance != null)
                        {
                            DamageTextManager.Instance.ShowDamage(target.transform.position, 999999f, DamageType.Status, ElementType.Ghost, true);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
