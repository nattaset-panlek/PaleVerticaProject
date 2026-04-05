using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Data.Items.Effects
{
    [System.Serializable]
    public class AttributeBonusEffect : EquipmentEffect
    {
        public AttributeType Attribute;
        public float Value;
        
        // Track modifiers applied to possibly multiple attributes (e.g. self and base)
        private System.Collections.Generic.List<(AttributeType type, AttributeModifier mod)> _activeModifiers = new System.Collections.Generic.List<(AttributeType, AttributeModifier)>();

        public override void Apply(Character player, ItemInstance instance = null)
        {
            if (player == null || player.attributeSystem == null) return;

            _activeModifiers.Clear();

            // 1. Apply to the target attribute itself as a Flat bonus
            // This ensures the value appears in the attribute's own CurrentValue (useful for UI display)
            var selfMod = new AttributeModifier(this.Value, ModifierType.Flat);
            player.attributeSystem.AddModifier(Attribute, selfMod);
            _activeModifiers.Add((Attribute, selfMod));

            // 2. If this is a 'Percent' attribute, also apply it as a PercentAdd modifier to its base attribute
            // Convention: BaseAttribute = PercentAttribute - 1
            var template = AttributeDatabase.GetAttribute((int)Attribute);
            if (template != null && template.IsPercent)
            {
                AttributeType baseAttr = (AttributeType)((int)Attribute - 1);
                // Basic validation: ensures we don't go out of bounds and that the base attribute actually exists
                if (System.Enum.IsDefined(typeof(AttributeType), baseAttr))
                {
                    // For a PercentAdd modifier, we convert 5 to 0.05
                    var percentMod = new AttributeModifier(this.Value / 100f, ModifierType.PercentAdd);
                    player.attributeSystem.AddModifier(baseAttr, percentMod);
                    _activeModifiers.Add((baseAttr, percentMod));
                }
            }
        }

        public override void Remove(Character player, ItemInstance instance = null)
        {
            if (player == null || player.attributeSystem == null) return;
            
            foreach (var record in _activeModifiers)
            {
                player.attributeSystem.RemoveModifier(record.type, record.mod);
            }
            _activeModifiers.Clear();
        }

        public override string GetDescription()
        {
            var template = AttributeDatabase.GetAttribute((int)Attribute);
            if (template == null) return $"{Attribute} +{Value}";

            string sign = Value >= 0 ? "+" : "";
            string unit = template.IsPercent ? "%" : "";
            return $"{template.FullName} {sign}{Value}{unit}";
        }
    }
}
