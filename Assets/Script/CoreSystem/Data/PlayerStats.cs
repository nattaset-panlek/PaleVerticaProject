using System.Collections.Generic;
using System.Linq;
using CoreSystem.Attributes;

namespace CoreSystem.Data
{
    public class PlayerStats
    {
        private AttributeSystem _attributeSystem;

        public event System.Action OnAttributesChanged;

        // Called when the player character is spawned or loaded
        public void Bind(AttributeSystem attributeSystem)
        {
            if (_attributeSystem != null)
            {
                _attributeSystem.OnAttributesChanged -= NotifyAttributesChanged;
            }
            
            _attributeSystem = attributeSystem;
            
            if (_attributeSystem != null)
            {
                _attributeSystem.OnAttributesChanged += NotifyAttributesChanged;
            }
        }

        private void NotifyAttributesChanged()
        {
            OnAttributesChanged?.Invoke();
        }

        // Helper to safely get an attribute value
        public Attribute GetAttribute(AttributeType type)
        {
            if (_attributeSystem != null)
            {
                return _attributeSystem.GetAttribute(type);
            }
            return null;
        }

        // Example properties for direct access
        public float Str => GetAttribute(AttributeType.STR);
        public float Agi => GetAttribute(AttributeType.AGI);
        public float Vit => GetAttribute(AttributeType.VIT);
        public float Int => GetAttribute(AttributeType.INT);
        public float Dex => GetAttribute(AttributeType.DEX);
        public float Luk => GetAttribute(AttributeType.LUK);
        
        public float Movespeed => GetAttribute(AttributeType.MoveSpeed);
        public float MaxHP => GetAttribute(AttributeType.MaxHP);
        public float MaxMP => GetAttribute(AttributeType.MaxMP);

        // Get all attributes as a list of KeyValuePairs or custom struct
        public List<KeyValuePair<AttributeType, float>> GetAllAttributes()
        {
            if (_attributeSystem != null)
            {
                return _attributeSystem.ActiveAttributes
                    .Select(kvp => new KeyValuePair<AttributeType, float>(kvp.Key, kvp.Value.CurrentValue))
                    .ToList();
            }
            
            return new List<KeyValuePair<AttributeType, float>>();
        }
    }
}
