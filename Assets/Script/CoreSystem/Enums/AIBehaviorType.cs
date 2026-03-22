namespace CoreSystem.Enums
{
    public enum AIBehaviorType
    {
        /// <summary>
        /// Never attacks or retaliates. Just wanders or stands still.
        /// </summary>
        Passive,

        /// <summary>
        /// Attacks any hostile targets that enter detection range.
        /// </summary>
        Aggressive,

        /// <summary>
        /// Ignores targets until attacked. Retaliates against the attacker.
        /// </summary>
        Defensive,

        /// <summary>
        /// Runs away from the attacker when taking damage.
        /// </summary>
        Coward
    }
}
