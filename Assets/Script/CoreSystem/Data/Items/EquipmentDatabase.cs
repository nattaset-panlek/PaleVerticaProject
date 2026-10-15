using CoreSystem.Combat;

namespace CoreSystem.Data.Items
{
    public static class EquipmentDatabase
    {
        #region [Variables/Fields]
        public static readonly float[] RefineTable = ProgressionFormula.RefineTable;

        public const float ReforgeValuePerLevel = ProgressionFormula.ReforgeValuePerLevel;
        #endregion

        #region [Public Methods]
        public static float GetRefineMultiplier(int level)
        {
            return ProgressionFormula.RefineMultiplier(level);
        }
        #endregion
    }
}
