namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("TimeCollector")]
    public class TimeCollectorUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnBasicAttack(IEntity target)
        {
            if (Owner.skillCaster != null)
            {
                foreach (var instance in Owner.skillCaster.EquippedSkills)
                {
                    if (instance != null && instance.CurrentCooldown > 0f)
                    {
                        instance.ReduceCooldown(0.1f);
                    }
                }
                foreach (var instance in Owner.skillCaster.GrantedSkills)
                {
                    if (instance != null && instance.CurrentCooldown > 0f)
                    {
                        instance.ReduceCooldown(0.1f);
                    }
                }
            }
        }
        #endregion
    }
}
