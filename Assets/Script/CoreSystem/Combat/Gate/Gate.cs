using CoreSystem.Data;
using CoreSystem.Entity;

namespace CoreSystem.Combat
{
    public static class Gate
    {
        #region [Public Methods]
        public static BlockReason CheckAction(IEntity target)
        {
            if (target == null) return BlockReason.NoTarget;
            if (target.attributeSystem == null) return BlockReason.NoStats;
            return BlockReason.None;
        }

        public static BlockReason CheckEffect(TargetRule rule, CombatAction action, IEntity user, IEntity target)
        {
            if (user == null || rule == TargetRule.Any) return BlockReason.None;
            if (action.IsProc && ReferenceEquals(user, target)) return BlockReason.None;

            switch (rule)
            {
                case TargetRule.Hostile:
                    return FactionDatabase.IsHostile(user.Faction, target.Faction) ? BlockReason.None : BlockReason.NotHostile;
                case TargetRule.Ally:
                    return ReferenceEquals(user, target) || FactionDatabase.IsAlly(user.Faction, target.Faction) ? BlockReason.None : BlockReason.NotAlly;
                default:
                    return BlockReason.None;
            }
        }
        #endregion
    }
}
