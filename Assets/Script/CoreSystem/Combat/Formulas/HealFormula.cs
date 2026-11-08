using UnityEngine;

namespace CoreSystem.Combat
{
    public static class HealFormula
    {
        #region [Variables/Fields]
        public const float DefaultMatkScale = 0.5f;
        #endregion

        #region [Public Methods]
        public static float Amount(float baseHeal, float casterMatk, float matkScale)
        {
            return Mathf.Max(0f, baseHeal + casterMatk * matkScale);
        }

        public static float Clamp(float current, float amount, float max)
        {
            return Mathf.Min(current + Mathf.Max(0f, amount), max);
        }
        #endregion
    }
}
