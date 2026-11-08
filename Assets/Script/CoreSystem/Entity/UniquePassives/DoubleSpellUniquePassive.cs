using CoreSystem.Skills;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("DoubleSpell")]
    public class DoubleSpellUniquePassive : UniquePassive, ISkillCastRepeater
    {
        #region [Variables/Fields]
        public const int RequiredSkillLevel = 1;
        public const float SecondCastDelay = 0.2f;
        public const float SecondCastDamageMultiplier = 0.4f;
        #endregion

        #region [Properties]
        public float RepeatDelay => SecondCastDelay;
        #endregion

        #region [Public Methods]
        public bool CanRepeat(SkillInstance instance)
        {
            return instance != null && instance.SkillData != null && instance.CurrentLevel == RequiredSkillLevel;
        }

        public SkillPayload CreateRepeatPayload(SkillPayload original)
        {
            SkillPayload repeat = original.Clone();
            repeat.DamageMultiplier *= SecondCastDamageMultiplier;
            if (Owner != null) repeat.OriginPosition = Owner.transform.position;
            return repeat;
        }
        #endregion
    }
}
