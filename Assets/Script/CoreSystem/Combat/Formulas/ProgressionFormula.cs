using UnityEngine;

namespace CoreSystem.Combat
{
    public static class ProgressionFormula
    {
        #region [Variables/Fields]
        public static readonly float[] RefineTable = {
            0f, 0.05f, 0.10f, 0.15f, 0.20f, 0.30f, 0.40f, 0.50f, 0.65f, 0.80f,
            1.00f, 1.20f, 1.45f, 1.70f, 2.00f, 2.30f, 2.65f, 3.00f, 3.50f, 4.00f, 5.00f
        };

        public const float ReforgeValuePerLevel = 5f;
        #endregion

        #region [Public Methods]
        public static float RefineMultiplier(int level)
        {
            if (level <= 0) return 0f;
            return RefineTable[Mathf.Clamp(level, 0, RefineTable.Length - 1)];
        }

        public static float ReforgeBonus(int level)
        {
            return level * ReforgeValuePerLevel;
        }

        public static float EquipmentBaseline(float baseValue, int refineLevel, int reforgeLevel)
        {
            float value = Mathf.Max(1f, baseValue);
            return (value * (1f + RefineMultiplier(refineLevel))) + ReforgeBonus(reforgeLevel);
        }

        public static float EquipmentUpgradeBonus(float baseValue, int refineLevel, int reforgeLevel)
        {
            return (baseValue * RefineMultiplier(refineLevel)) + ReforgeBonus(reforgeLevel);
        }

        public static long FallbackBaseExp(int level)
        {
            return (long)(100f * Mathf.Pow(level, 1.5f));
        }

        public static long FallbackJobExp(int level)
        {
            return (long)(50f * Mathf.Pow(level, 1.6f));
        }
        #endregion
    }
}
