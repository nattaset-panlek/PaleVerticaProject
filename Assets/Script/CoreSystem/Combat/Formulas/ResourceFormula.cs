using UnityEngine;

namespace CoreSystem.Combat
{
    public static class ResourceFormula
    {
        #region [Variables/Fields]
        public const float MinimumHpAfterCost = 1f;
        public const float RegenTickInterval = 1f;
        #endregion

        #region [Public Methods]
        public static bool CanAfford(in ResourceCost cost, float currentHp, float currentMp)
        {
            return cost.IsHpCost ? !(currentHp < cost.Hp) : !(currentMp < cost.Mana);
        }

        public static float HpAfterCost(float currentHp, float hpCost)
        {
            return Mathf.Max(MinimumHpAfterCost, currentHp - hpCost);
        }

        public static float MpAfterCost(float currentMp, float manaCost)
        {
            return Mathf.Max(0f, currentMp - manaCost);
        }

        public static bool TryRegen(float current, float max, float regenPerTick, bool ownerAlive, out float newValue)
        {
            newValue = current;
            if (!ownerAlive || !(current < max) || !(regenPerTick > 0f)) return false;

            newValue = Mathf.Min(max, current + regenPerTick);
            return true;
        }
        #endregion
    }
}
