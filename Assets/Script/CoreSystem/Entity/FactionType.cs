namespace CoreSystem.Entity
{
    public enum FactionType
    {
        Player,
        Enemy,
        Neutral
    }

    public static class FactionExtensions
    {
        public static bool IsHostileTo(this FactionType self, FactionType other)
        {
            if (self == FactionType.Player && other == FactionType.Enemy) return true;
            if (self == FactionType.Enemy && other == FactionType.Player) return true;
            return false;
        }
    }
}
