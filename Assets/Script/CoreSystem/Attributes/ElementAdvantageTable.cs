using System.Collections.Generic;

namespace CoreSystem.Attributes
{
    public static class ElementAdvantageTable
    {
        // Multiplier constants
        public const float x0_0 = 0.0f;
        public const float x0_25 = 0.25f;
        public const float x0_50 = 0.50f;
        public const float x0_75 = 0.75f;
        public const float x1_0 = 1.0f;
        public const float x1_25 = 1.25f;
        public const float x1_50 = 1.50f;
        public const float x1_75 = 1.75f;
        public const float x2_0 = 2.0f;

        // Dictionary to store multipliers: [AttackerElement][DefenderElement]
        private static readonly Dictionary<ElementType, Dictionary<ElementType, float>> multiplierTable =
            new Dictionary<ElementType, Dictionary<ElementType, float>>()
            {
                // ATK = Neutral
                { ElementType.Neutral, new Dictionary<ElementType, float>
                    {
                        {ElementType.Neutral, x1_0}, {ElementType.Water, x1_0}, {ElementType.Earth, x1_0},
                        {ElementType.Fire, x1_0}, {ElementType.Electric, x1_0}, {ElementType.Ghost, x0_0},
                        {ElementType.Holy, x1_0}
                    }
                },
                
                // ATK = Water (Strong vs Fire, Weak vs Earth, Resisted by Water/Holy)
                { ElementType.Water, new Dictionary<ElementType, float>
                    {
                        {ElementType.Neutral, x1_0}, {ElementType.Water, x0_25}, {ElementType.Earth, x0_50},
                        {ElementType.Fire, x1_50}, {ElementType.Electric, x1_0}, {ElementType.Ghost, x1_0},
                        {ElementType.Holy, x0_75}
                    }
                },

                // ATK = Earth (Strong vs Electric, Weak vs Fire, Resisted by Earth/Holy/Ghost)
                { ElementType.Earth, new Dictionary<ElementType, float>
                    {
                        {ElementType.Neutral, x1_0}, {ElementType.Water, x1_0}, {ElementType.Earth, x0_25},
                        {ElementType.Fire, x0_50}, {ElementType.Electric, x1_50}, {ElementType.Ghost, x0_75},
                        {ElementType.Holy, x0_75}
                    }
                },

                // ATK = Fire (Strong vs Earth, Weak vs Water, Resisted by Fire/Holy)
                { ElementType.Fire, new Dictionary<ElementType, float>
                    {
                        {ElementType.Neutral, x1_0}, {ElementType.Water, x0_50}, {ElementType.Earth, x1_50},
                        {ElementType.Fire, x0_25}, {ElementType.Electric, x1_0}, {ElementType.Ghost, x1_0},
                        {ElementType.Holy, x0_75}
                    }
                },

                // ATK = Electric (Strong vs Water, Weak vs Earth, Resisted by Electric/Holy)
                { ElementType.Electric, new Dictionary<ElementType, float>
                    {
                        {ElementType.Neutral, x1_0}, {ElementType.Water, x1_50}, {ElementType.Earth, x0_50},
                        {ElementType.Fire, x1_0}, {ElementType.Electric, x0_25}, {ElementType.Ghost, x1_0},
                        {ElementType.Holy, x0_75}
                    }
                },

                // ATK = Ghost (Strong vs Ghost, Resisted by Neutral, Normal against others)
                { ElementType.Ghost, new Dictionary<ElementType, float>
                    {
                        {ElementType.Neutral, x0_0}, {ElementType.Water, x1_0}, {ElementType.Earth, x1_0},
                        {ElementType.Fire, x1_0}, {ElementType.Electric, x1_0}, {ElementType.Ghost, x1_50},
                        {ElementType.Holy, x0_75}
                    }
                },

                // ATK = Holy (Strong vs Ghost, Resisted by Holy/Neutral)
                { ElementType.Holy, new Dictionary<ElementType, float>
                    {
                        {ElementType.Neutral, x0_75}, {ElementType.Water, x1_0}, {ElementType.Earth, x1_0},
                        {ElementType.Fire, x1_0}, {ElementType.Electric, x1_0}, {ElementType.Ghost, x1_50},
                        {ElementType.Holy, x0_0}
                    }
                }
            };

        /// <summary>
        /// Gets the damage multiplier based on attacker and defender elements.
        /// </summary>
        /// <param name="attackerElement">The element of the attack.</param>
        /// <param name="defenderElement">The element of the defender.</param>
        /// <returns>A float multiplier (e.g., 1.0 for normal, 1.5 for weakness, 0.5 for resist).</returns>
        public static float GetMultiplier(ElementType attackerElement, ElementType defenderElement)
        {
            if (multiplierTable.TryGetValue(attackerElement, out var defenderDict))
            {
                if (defenderDict.TryGetValue(defenderElement, out var multiplier))
                {
                    return multiplier;
                }
            }
            
            // Default 1.0 (Normal Damage) if not found, though all combinations should be covered.
            return x1_0;
        }
    }
}
