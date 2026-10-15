namespace CoreSystem.Skills
{
    public static class ProcSkillIds
    {
        #region [Variables/Fields]
        public const int ThornReflect = -999;
        public const int ChaosBurst = -998;
        public const int BleedTick = -997;
        #endregion

        #region [Public Methods]
        public static bool IsProc(int skillId)
        {
            return skillId == ThornReflect || skillId == ChaosBurst || skillId == BleedTick;
        }
        #endregion
    }
}
