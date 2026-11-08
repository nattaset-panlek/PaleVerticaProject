namespace CoreSystem.Entity.UniquePassives
{
    public interface IBasicAttackRepeater
    {
        float RepeatDelay { get; }
        float RepeatDamageMultiplier { get; }
    }
}
