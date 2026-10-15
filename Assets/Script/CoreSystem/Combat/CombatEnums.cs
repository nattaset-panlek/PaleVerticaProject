namespace CoreSystem.Combat
{
    public enum EffectKind
    {
        Damage,
        Heal,
        Resource,
        Status,
        Displace,
        Control,
        Summon,
        Transform,
        Custom
    }

    public enum TargetRule
    {
        Any,
        Hostile,
        Ally
    }

    public enum BlockReason
    {
        None,
        NoTarget,
        NoStats,
        NotHostile,
        NotAlly,
        NoHandler,
        InvalidAction
    }

    public enum RiderTrigger
    {
        OnHit,
        OnCrit,
        OnKill
    }
}
