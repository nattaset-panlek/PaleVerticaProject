namespace CoreSystem.Enums
{
    /// <summary>
    /// Determines how an Agent positions itself during combat.
    /// </summary>
    public enum CombatStyle
    {
        /// <summary>Moves directly into AttackRange and stays tight.</summary>
        Melee,
        
        /// <summary>Maintains a preferred distance band from target; backs away if too close.</summary>
        Ranged,
        
        /// <summary>Orbits around the target at AttackRange distance; changes orbit direction periodically.</summary>
        Circler,
        
        /// <summary>Stays near allies; prioritizes buff/heal skills over attacking.</summary>
        Supporter
    }
}
