using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Data.Items.Effects
{
    [System.Serializable]
    public class ConditionalBonusEffect : EquipmentEffect
    {
        public ConditionLogic Condition;
        
        // We use polymorphism here but note Unity's JSON serialization won't support it well out of the box
        [SerializeReference] 
        public EquipmentEffect BonusEffect;
        
        private bool _isActive;

        public override void Apply(Character player)
        {
            // Note: In an event-driven system, this would listen to "OnStatChanged" to re-evaluate.
            // For now, we evaluate once on equip, or if ticked during an update.
            Evaluate(player);
        }

        public override void Remove(Character player)
        {
            if (_isActive && BonusEffect != null)
            {
                BonusEffect.Remove(player);
                _isActive = false;
            }
        }

        public void Evaluate(Character player)
        {
            if (player == null || Condition == null || BonusEffect == null) return;

            bool isConditionMet = Condition.Evaluate(player);
            
            if (isConditionMet && !_isActive)
            {
                BonusEffect.Apply(player);
                _isActive = true;
            }
            else if (!isConditionMet && _isActive)
            {
                BonusEffect.Remove(player);
                _isActive = false;
            }
        }

        public override string GetDescription()
        {
            if (BonusEffect == null) return "Unknown Conditional Effect";
            string condStr = Condition != null ? Condition.GetDescription() : "No Condition";
            return $"{condStr}: {BonusEffect.GetDescription()}";
        }
    }
}
