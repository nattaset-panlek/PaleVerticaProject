using System;
using CoreSystem.Entity;
using CoreSystem.Player;

namespace CoreSystem.Data
{
    public class PlayerAttributePoints
    {
        #region [Variables/Fields]
        public event Action OnChanged;
        #endregion

        #region [Properties]
        public int Available
        {
            get
            {
                Character player = PlayerCore.GetPlayer();
                return player != null ? player.AttributePoints : 0;
            }
        }
        #endregion

        #region [Public Methods]
        public bool Spend(AttributeType type)
        {
            Character player = PlayerCore.GetPlayer();
            if (player == null || player.AttributePoints <= 0 || !IsPrimary(type)) return false;

            player.AttributePoints--;
            player.attributeSystem.GetAttribute(type).BaseValue += 1f;
            player.RecalculateStats();
            OnChanged?.Invoke();
            return true;
        }

        public static bool IsPrimary(AttributeType type)
        {
            return type == AttributeType.STR || type == AttributeType.VIT || type == AttributeType.AGI
                || type == AttributeType.INT || type == AttributeType.DEX || type == AttributeType.LUK;
        }
        #endregion
    }
}
