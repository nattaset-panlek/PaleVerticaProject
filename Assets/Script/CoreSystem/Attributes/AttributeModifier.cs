using CoreSystem;

namespace CoreSystem.Attributes
{
    public class AttributeModifier
    {
        public float Value { get; private set; }
        public ModifierType Type { get; private set; }

        public AttributeModifier(float value, ModifierType type)
        {
            Value = value;
            Type = type;
        }
    }
}
