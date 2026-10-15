using CoreSystem.Skills;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("Vampire")]
    public class VampireUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnPostHit(IEntity target, float damage, SkillPayload payload)
        {
            if (payload.Caster == Owner && payload.DamageType == DamageType.Physical && damage > 0f)
            {
                RestoreResource(AttributeType.HP, AttributeType.MaxHP, damage * 0.3f);
            }
        }
        #endregion
    }
}
