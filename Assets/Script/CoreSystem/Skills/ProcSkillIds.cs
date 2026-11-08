namespace CoreSystem.Skills
{
    public static class ProcSkillIds
    {
        #region [Variables/Fields]
        public const int ThornReflect = -999;
        public const int ChaosBurst = -998;
        public const int BleedTick = -997;
        public const int BurnTick = -996;
        public const int ParryCounter = -995;
        public const int PoisonTick = -994;
        public const int StatusApply = -993;
        #endregion

        #region [Public Methods]
        public static bool IsProc(int skillId)
        {
            return skillId == ThornReflect || skillId == ChaosBurst || skillId == BleedTick || skillId == BurnTick || skillId == ParryCounter || skillId == PoisonTick || skillId == StatusApply;
        }
        #endregion
    }
}
