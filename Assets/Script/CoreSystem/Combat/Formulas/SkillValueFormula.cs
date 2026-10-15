using UnityEngine;

namespace CoreSystem.Combat
{
    public static class SkillValueFormula
    {
        #region [Public Methods]
        public static float Cooldown(float baseCooldown, float flatReduction, float percentReduction)
        {
            float cooldown = (baseCooldown - flatReduction) * (1f - (percentReduction / 100f));
            return Mathf.Max(0f, cooldown);
        }

        public static float Area(float baseValue, float bonusPercent)
        {
            float area = baseValue * (1f + (bonusPercent / 100f));
            return Mathf.Max(0f, area);
        }

        public static float Scaling(float statValue, float attributeMultiplier, float flatBonus, float levelMultiplier, int skillLevel, float globalMultiplier)
        {
            float levelBonus = levelMultiplier * skillLevel;
            return (statValue * attributeMultiplier + flatBonus + levelBonus) * globalMultiplier;
        }
        #endregion
    }
}
