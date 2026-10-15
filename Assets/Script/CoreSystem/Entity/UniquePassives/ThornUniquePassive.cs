using CoreSystem.Skills;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("Thorn")]
    public class ThornUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnTakeDamage(IEntity attacker, ref float damage, SkillPayload payload)
        {
            if (attacker != null && attacker != Owner && !ProcSkillIds.IsProc(payload.SkillID) && damage > 0f)
            {
                float str = Owner.attributeSystem.GetAttribute(AttributeType.STR).CurrentValue;
                float reflectDmg = damage * 0.1f * (str * 0.01f);
                if (reflectDmg > 0f)
                {
                    DealProcDamage(attacker, reflectDmg, DamageType.Physical, ProcSkillIds.ThornReflect);
                }
            }
        }
        #endregion
    }
}
