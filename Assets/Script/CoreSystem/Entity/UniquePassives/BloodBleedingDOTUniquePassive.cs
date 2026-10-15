using CoreSystem.Skills;

namespace CoreSystem.Entity.UniquePassives
{
    public class BloodBleedingDOTUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        public const string PassiveId = "BloodBleedingDOT";
        public float CasterInt { get; set; }
        public float CasterDex { get; set; }
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
                    float dmg = (maxHpAttr.CurrentValue * 0.01f) + ((CasterInt + CasterDex) * 0.01f);
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
