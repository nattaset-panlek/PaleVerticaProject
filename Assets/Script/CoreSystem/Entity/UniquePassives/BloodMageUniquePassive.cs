using CoreSystem.Skills;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("BloodMage")]
    public class BloodMageUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void ModifyManaCost(ref float manaCost, ref float hpCost, ref bool isHpCost)
        {
            hpCost = manaCost;
            manaCost = 0f;
            isHpCost = true;
        }

        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (payload.SkillID > 0)
            {
                damage *= 1.3f;
            }
        }
        #endregion
    }
}
