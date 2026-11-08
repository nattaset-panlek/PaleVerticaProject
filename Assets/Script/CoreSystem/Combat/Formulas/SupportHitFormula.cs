using UnityEngine;

namespace CoreSystem.Combat
{
    public static class SupportHitFormula
    {
        #region [Variables/Fields]
        public const float MaxChance = 100f;
        #endregion

        #region [Public Methods]
        public static float Leech(float damageDealt, float leechPercent)
        {
            if (damageDealt <= 0f || leechPercent <= 0f) return 0f;
            return damageDealt * leechPercent / 100f;
        }

        public static float StatusChance(float baseChance, float bonusChance)
        {
            if (baseChance <= 0f) return 0f;
            return Mathf.Clamp(baseChance + Mathf.Max(0f, bonusChance), 0f, MaxChance);
        }

        public static bool Rolls(float chance, float roll)
        {
            return chance > 0f && roll < chance;
        }

        public static float Duration(float baseDuration, float durationMultiplier)
        {
            return Mathf.Max(0f, baseDuration) * Mathf.Max(0f, durationMultiplier);
        }

        public static float TickDamage(float hitDamage, float percentPerTick)
        {
            if (hitDamage <= 0f || percentPerTick <= 0f) return 0f;
            return hitDamage * percentPerTick / 100f;
        }

        public static float ProjectileSpeed(float baseSpeed, float speedMultiplier)
        {
            return Mathf.Max(0f, baseSpeed) * Mathf.Max(0f, speedMultiplier);
        }

        public static float AddPercent(float multiplier, float percent)
        {
            return multiplier * (1f + percent / 100f);
        }
        #endregion
    }
}
