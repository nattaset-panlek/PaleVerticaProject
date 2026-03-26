namespace CoreSystem.Data.Items
{
    public static class EquipmentDatabase
    {
        public static readonly float[] RefineTable = {
            0f, 0.05f, 0.10f, 0.15f, 0.20f, 0.30f, 0.40f, 0.50f, 0.65f, 0.80f,
            1.00f, 1.20f, 1.45f, 1.70f, 2.00f, 2.30f, 2.65f, 3.00f, 3.50f, 4.00f, 5.00f
        };

        public const float ReforgeValuePerLevel = 5f;

        public static float GetRefineMultiplier(int level)
        {
            if (level <= 0) return 0f;
            return RefineTable[UnityEngine.Mathf.Clamp(level, 0, RefineTable.Length - 1)];
        }
    }
}