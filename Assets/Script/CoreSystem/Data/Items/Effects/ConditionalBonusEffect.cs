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

        public override void Apply(Character player, ItemInstance instance = null)
        {
            if (player == null || player.attributeSystem == null) return;
            
            _lastCharacter = player;
            Evaluate(player);
            
            player.attributeSystem.OnAttributesChanged += OnAttributesChanged;
        }

        public override void Remove(Character player, ItemInstance instance = null)
        {
            if (player != null && player.attributeSystem != null)
            {
                player.attributeSystem.OnAttributesChanged -= OnAttributesChanged;
            }

            if (_isActive && BonusEffect != null)
            {
                BonusEffect.Remove(player);
                _isActive = false;
            }
            _lastCharacter = null;
        }

        private Character _lastCharacter;
        private void OnAttributesChanged()
        {
            if (_lastCharacter != null) Evaluate(_lastCharacter);
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
            string condStr = Condition != null ? $"{Condition.GetDescription()}" : "No Condition";
            return $"{condStr} {BonusEffect.GetDescription()}";
        }
    }
}
