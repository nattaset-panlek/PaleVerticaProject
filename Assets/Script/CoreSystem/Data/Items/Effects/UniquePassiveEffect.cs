using CoreSystem;
using CoreSystem.Entity;

namespace CoreSystem.Data.Items.Effects
{
    [System.Serializable]
    public class UniquePassiveEffect : EquipmentEffect
    {
        #region [Variables/Fields]
        public string PassiveId;
        #endregion

        #region [Public Methods]
        public override void Apply(Character player, ItemInstance instance = null)
        {
            if (player != null && player.UniquePassives != null)
            {
                player.UniquePassives.AddPassive(PassiveId);
                player.RecalculateStats();
            }
        }

        public override void Remove(Character player, ItemInstance instance = null)
        {
            if (player != null && player.UniquePassives != null)
            {
                player.UniquePassives.RemovePassive(PassiveId);
                player.RecalculateStats();
            }
        }

        public override string GetDescription()
        {
            return "Grants unique passive: " + PassiveId;
        }
        #endregion
    }
}
