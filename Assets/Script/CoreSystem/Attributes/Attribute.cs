using System.Collections.Generic;
using CoreSystem;

namespace CoreSystem.Attributes
{
    [System.Serializable]
    public class Attribute
    {
        #region Variables/Fields
        public AttributeType Type;
        public float BaseValue;
        public float CurrentValue { get; private set; }
        public System.Action OnValueUpdated;

        public float StatFlatBonus { get; set; }
        public float StatPercentBonus { get; set; }

        public readonly AdditionalChain Additional = new AdditionalChain();

        public float FlatBonusSum { get; private set; }
        public float PercentBonusSum { get; private set; }

        private readonly List<AttributeModifier> modifiers = new List<AttributeModifier>();
        #endregion

        #region Properties
        public float AddedValue => CurrentValue - BaseValue;
        public string Name => Type.ToString();
        public string TotalValue => CurrentValue.ToString("F0");

        public static implicit operator float(Attribute attr) => attr?.CurrentValue ?? 0f;
        public override string ToString() => CurrentValue.ToString();
        #endregion

        #region Public Methods
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
            float flatSum = StatFlatBonus;
            float percentAddSum = StatPercentBonus;

            for (int i = 0; i < modifiers.Count; i++)
            {
                AttributeModifier mod = modifiers[i];
                if (mod.Type == ModifierType.Flat)
                    flatSum += mod.Value;
                else if (mod.Type == ModifierType.PercentAdd)
                    percentAddSum += mod.Value;
            }

            FlatBonusSum = flatSum;
            PercentBonusSum = percentAddSum;

            float finalValue = BaseValue;
            finalValue += flatSum;
            finalValue = finalValue * (1f + percentAddSum);

            for (int i = 0; i < modifiers.Count; i++)
            {
                AttributeModifier mod = modifiers[i];
                if (mod.Type == ModifierType.PercentMult)
                    finalValue *= mod.Value;
            }

            finalValue = Additional.Apply(finalValue);

            CurrentValue = finalValue;
            OnValueUpdated?.Invoke();
        }
        #endregion
    }
}
