using CoreSystem.Skills;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("Bleeding")]
    public class BleedingUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private float tickTimer = 0f;
        #endregion

        #region [Public Methods]
        public override void Tick(float dt)
        {
            tickTimer += dt;
            if (tickTimer >= 1f)
            {
                tickTimer -= 1f;
                var maxHpAttr = Owner.attributeSystem.GetAttribute(AttributeType.MaxHP);
                if (maxHpAttr != null)
                {
                    float dmg = maxHpAttr.CurrentValue * 0.01f;
                    if (dmg > 0f)
                    {
                        DealProcDamage(Owner, dmg, DamageType.Status, ProcSkillIds.BleedTick);
                    }
                }
            }
        }
        #endregion
    }
}
