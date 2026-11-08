using UnityEngine;

namespace CoreSystem.Combat
{
    public static class KillRewardFormula
    {
        #region [Variables/Fields]
        public const float JobExpRatio = 0.6f;
        public const float HigherLevelBonusPerLevel = 0.05f;
        public const float MaxHigherLevelMultiplier = 1.5f;
        public const int LowerLevelGrace = 5;
        public const int LowerLevelTierSize = 5;
        public const float LowerLevelPenaltyPerTier = 0.3f;
        public const float MinLevelMultiplier = 0.1f;
        #endregion

        #region [Public Methods]
        public static long BaseExpForLevel(int level)
        {
            if (level <= 0) return 0;
            return Mathf.RoundToInt(10f * level + 2f * level * level);
        }

        public static long JobExpForLevel(int level)
        {
            return Mathf.RoundToInt(BaseExpForLevel(level) * JobExpRatio);
        }

        public static long FullExp(int overrideExp, long levelExp)
        {
            return overrideExp > 0 ? overrideExp : System.Math.Max(0L, levelExp);
        }

        public static float LevelDifferenceMultiplier(int killerLevel, int victimLevel)
        {
            int gap = victimLevel - killerLevel;
            if (gap >= 0) return Mathf.Min(MaxHigherLevelMultiplier, 1f + gap * HigherLevelBonusPerLevel);

            int below = -gap;
            if (below <= LowerLevelGrace) return 1f;
            int tier = Mathf.CeilToInt((below - LowerLevelGrace) / (float)LowerLevelTierSize);
            return Mathf.Max(MinLevelMultiplier, 1f - tier * LowerLevelPenaltyPerTier);
        }

        public static float Share(float part, float total)
        {
            if (total <= 0f || part <= 0f) return 0f;
            return Mathf.Clamp01(part / total);
        }

        public static long Reward(long fullExp, float share, float levelMultiplier, float expMultiplier)
        {
            if (fullExp <= 0 || share <= 0f) return 0;
            return (long)Mathf.Round(fullExp * share * Mathf.Max(0f, levelMultiplier) * Mathf.Max(0f, expMultiplier));
        }
        #endregion
    }
}
