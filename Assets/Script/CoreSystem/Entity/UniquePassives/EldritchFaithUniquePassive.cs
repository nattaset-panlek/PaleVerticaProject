using CoreSystem.Skills;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("EldritchFaith")]
    public class EldritchFaithUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void ModifyCoreAttributes(ref float str, ref float agi, ref float vit, ref float @int, ref float dex, ref float luk)
        {
            @int *= 0.5f;
        }

        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (payload.Caster == Owner)
            {
                float currentInt = Owner.attributeSystem.GetAttribute(AttributeType.INT).CurrentValue;
                damage += currentInt;
            }
        }
        #endregion
    }
}
