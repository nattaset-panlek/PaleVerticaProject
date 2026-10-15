using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Combat
{
    public static class BasicAttackFormula
    {
        #region [Variables/Fields]
        public const int MinHitsPerSecond = 1;
        public const int MaxHitsPerSecond = 100;
        public const float AspdCeiling = 200f;
        public const float AspdDivisor = 50f;
        #endregion

        #region [Public Methods]
        public static float BonusDamage(DamageType damageType, AutoAttackType attackType, float str, float dex, float @int)
        {
            if (damageType == DamageType.Magic) return @int;
            if (attackType == AutoAttackType.Melee) return str;
            if (attackType == AutoAttackType.Range) return dex;
            return 0f;
        }

        public static float AttackDelay(float aspd)
        {
            float delay = (AspdCeiling - aspd) / AspdDivisor;
            return Mathf.Clamp(delay, 1f / MaxHitsPerSecond, 1f / MinHitsPerSecond);
        }
        #endregion
    }
}
