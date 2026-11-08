using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Combat
{
    public static class BasicAttackFormula
    {
        #region [Variables/Fields]
        public const float BaseAttacksPerSecond = 1f;
        public const float BaseAspd = 100f;
        public const float MinAspd = 50f;
        public const float MaxAspd = 600f;
        #endregion

        #region [Public Methods]
        public static float BonusDamage(DamageType damageType, AutoAttackType attackType, float str, float dex, float @int)
        {
            if (damageType == DamageType.Magic) return @int;
            if (attackType == AutoAttackType.Melee) return str;
            if (attackType == AutoAttackType.Range) return dex;
            return 0f;
        }

        public static float AttacksPerSecond(float aspd)
        {
            return BaseAttacksPerSecond * Mathf.Clamp(aspd, MinAspd, MaxAspd) / BaseAspd;
        }

        public static float AttackDelay(float aspd)
        {
            return 1f / AttacksPerSecond(aspd);
        }
        #endregion
    }
}
