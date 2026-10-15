using System.Collections.Generic;
using CoreSystem.Attributes;

namespace CoreSystem.Combat
{
    public static class StatFormula
    {
        #region [Public Methods]
        public static StatResult Compute(float baseValue, float initialValue, float statFlatBonus, float statPercentBonus, List<AttributeModifier> modifiers, AdditionalChain additional)
        {
            float flatSum = 0f;
            float percentAddSum = 0f;

            for (int i = 0; i < modifiers.Count; i++)
            {
                AttributeModifier modifier = modifiers[i];
                if (modifier.Type == ModifierType.Flat)
                    flatSum += modifier.Value;
                else if (modifier.Type == ModifierType.PercentAdd)
                    percentAddSum += modifier.Value;
            }

            float baseWithPrimary = baseValue * (1f + statPercentBonus) + statFlatBonus;
            float value = baseWithPrimary + initialValue;
            value += flatSum;
            value = value * (1f + percentAddSum);

            for (int i = 0; i < modifiers.Count; i++)
            {
                AttributeModifier modifier = modifiers[i];
                if (modifier.Type == ModifierType.PercentMult)
                    value *= modifier.Value;
            }

            if (additional != null) value = additional.Apply(value);

            return new StatResult { FlatSum = flatSum, PercentAddSum = percentAddSum, Value = value };
        }
        #endregion
    }
}
