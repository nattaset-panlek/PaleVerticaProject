namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("DoubleAttack")]
    public class DoubleAttackUniquePassive : UniquePassive, IBasicAttackRepeater
    {
        #region [Variables/Fields]
        public const float SecondStrikeDelay = 0.12f;
        public const float SecondStrikeDamageMultiplier = 1f;
        #endregion

        #region [Properties]
        public float RepeatDelay => SecondStrikeDelay;
        public float RepeatDamageMultiplier => SecondStrikeDamageMultiplier;
        #endregion
    }
}
