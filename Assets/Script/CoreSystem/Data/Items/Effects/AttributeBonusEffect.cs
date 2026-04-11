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
        private float _lastCalculatedValue;

        public override void Apply(Character player, ItemInstance instance = null)
        {
            if (player == null || player.attributeSystem == null) return;

            _activeModifiers.Clear();

            float refineLevel = (instance is EquipmentInstance equipInst) ? equipInst.Refine : 0f;
            float refineBonus = 0f;

            var template = AttributeDatabase.GetAttribute((int)Attribute);
            if (template != null)
            {
                if (template.IsPercent)
                {
                    // Rule: 1% of the base value per refine level (e.g. 50% + 20 refine = 60%, since 0.5 * 20 = 10)
                    refineBonus = refineLevel * (Value * 0.01f);
                }
                else
                {
                    // Rule: 1.0 flat per refine level
                    refineBonus = refineLevel * 1.0f;
                }
            }

            _lastCalculatedValue = this.Value + refineBonus;

            // 1. Apply to the target attribute itself as a Flat bonus
            // This ensures the value appears in the attribute's own CurrentValue (useful for UI display)
            var selfMod = new AttributeModifier(_lastCalculatedValue, ModifierType.Flat);
            player.attributeSystem.AddModifier(Attribute, selfMod);
            _activeModifiers.Add((Attribute, selfMod));

            // 2. If this is a 'Percent' attribute, also apply it as a PercentAdd modifier to its base attribute
            // Convention: BaseAttribute = PercentAttribute - 1
            if (template != null && template.IsPercent)
            {
                AttributeType baseAttr = (AttributeType)((int)Attribute - 1);
                // Basic validation: ensures we don't go out of bounds and that the base attribute actually exists
                if (System.Enum.IsDefined(typeof(AttributeType), baseAttr))
                {
                    // For a PercentAdd modifier, we convert 5 to 0.05
                    var percentMod = new AttributeModifier(_lastCalculatedValue / 100f, ModifierType.PercentAdd);
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
