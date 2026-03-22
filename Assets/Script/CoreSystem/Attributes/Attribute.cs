using System.Collections.Generic;
using CoreSystem;

namespace CoreSystem.Attributes
{
    [System.Serializable]
    public class Attribute
    {
        public AttributeType Type;
        public float BaseValue;
        public float CurrentValue { get; private set; }
        public System.Action OnValueUpdated;
        
        // Bonuses from Primary Stats (STR, VIT, etc.)
        public float StatFlatBonus { get; set; }
        public float StatPercentBonus { get; set; }

        public float FlatBonusSum { get; private set; }
        public float PercentBonusSum { get; private set; }

        public float AddedValue => CurrentValue - BaseValue;

        public string Name => Type.ToString();
        public string TotalValue => CurrentValue.ToString("F0");

        public static implicit operator float(Attribute attr) => attr?.CurrentValue ?? 0f;

        public override string ToString() => CurrentValue.ToString();

        private readonly List<AttributeModifier> modifiers = new List<AttributeModifier>();

        public void AddModifier(AttributeModifier modifier)
        {
            modifiers.Add(modifier);
            Recalculate();
        }

        public void RemoveModifier(AttributeModifier modifier)
        {
            modifiers.Remove(modifier);
            Recalculate();
        }

        public void Recalculate()
        {
            float finalValue = BaseValue;
            float flatSum = StatFlatBonus;
            float percentAddSum = StatPercentBonus;

            for (int i = 0; i < modifiers.Count; i++)
            {
                AttributeModifier mod = modifiers[i];
                if (mod.Type == ModifierType.Flat)
                {
                    flatSum += mod.Value;
                }
                else if (mod.Type == ModifierType.PercentAdd)
                {
                    percentAddSum += mod.Value;
                }
            }

            FlatBonusSum = flatSum;
            PercentBonusSum = percentAddSum;

            finalValue += flatSum;
            finalValue *= 1f + percentAddSum;

            for (int i = 0; i < modifiers.Count; i++)
            {
                AttributeModifier mod = modifiers[i];
                if (mod.Type == ModifierType.PercentMult)
                {
                    finalValue *= mod.Value;
                }
            }

            CurrentValue = finalValue;
            OnValueUpdated?.Invoke();
        }
    }
}
