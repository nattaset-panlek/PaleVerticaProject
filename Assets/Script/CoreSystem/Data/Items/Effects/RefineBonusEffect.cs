using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Data.Items.Effects
{
    [System.Serializable]
    public class RefineBonusEffect : EquipmentEffect
    {
        public int RequiredRefine;
        public EquipmentEffect BonusEffect;
        
        private bool _wasApplied = false;

        public override void Apply(Character player, ItemInstance instance = null)
        {
            if (player == null || BonusEffect == null) return;
            
            bool conditionMet = false;
            
            // Check refine condition
            if (instance is EquipmentInstance equipInst)
            {
                if (equipInst.Refine >= RequiredRefine)
                {
                    conditionMet = true;
                }
            }
            // If condition isn't an equipment instance, it's false unless RequiredRefine is 0
            else if (RequiredRefine <= 0)
            {
                conditionMet = true;
            }

            if (conditionMet && !_wasApplied)
            {
                BonusEffect.Apply(player, instance);
                _wasApplied = true;
            }
        }

        public override void Remove(Character player, ItemInstance instance = null)
        {
            if (player == null || BonusEffect == null) return;

            if (_wasApplied)
            {
                // Always remove if it was applied, regardless of current condition state
                // This handles cases where Refine is lost or the item is completely unequipped
                BonusEffect.Remove(player, instance);
                _wasApplied = false;
            }
        }

        public override string GetDescription()
        {
            string bonusDesc = BonusEffect != null ? BonusEffect.GetDescription() : "Unknown Bonus";
            return $"If Refine {RequiredRefine} {bonusDesc}";
        }
    }
}
