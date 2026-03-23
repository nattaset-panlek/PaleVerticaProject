using UnityEngine;
using System.Collections.Generic;
using CoreSystem;

namespace CoreSystem.Attributes
{
    [System.Serializable]
    public class AttributeSystem
    {
        [SerializeField] private List<Attribute> initialAttributes = new List<Attribute>();
        
        private Dictionary<AttributeType, Attribute> attributes = new Dictionary<AttributeType, Attribute>();

        public IReadOnlyDictionary<AttributeType, Attribute> ActiveAttributes => attributes;
        private Entity.Character owner;
        private bool _isUpdating = false;

        public event System.Action OnAttributesChanged;
        public void InvokeOnAttributesChanged() 
        {
            if (_isUpdating) return;
            OnAttributesChanged?.Invoke();
        }


        public void Initialize(Entity.Character owner)
        {
            foreach (AttributeType type in System.Enum.GetValues(typeof(AttributeType)))
            {
                var attr = new Attribute { Type = type, BaseValue = 0f };
                attr.OnValueUpdated += InvokeOnAttributesChanged;
                attributes[type] = attr;
            }

            attributes[AttributeType.MaxHP].BaseValue = 100f;
            attributes[AttributeType.MaxMP].BaseValue = 50f;

            attributes[AttributeType.HP].BaseValue = attributes[AttributeType.MaxHP].CurrentValue;
            attributes[AttributeType.MP].BaseValue = attributes[AttributeType.MaxMP].CurrentValue;
            attributes[AttributeType.HPRecovery].BaseValue = 1f;
            attributes[AttributeType.MPRecovery].BaseValue = 1f;

            attributes[AttributeType.BaseLevel].BaseValue = 1f;
            attributes[AttributeType.JobLevel].BaseValue = 1f;
            
            // Core Stats starts at 1
            attributes[AttributeType.STR].BaseValue = 1f;
            attributes[AttributeType.AGI].BaseValue = 1f;
            attributes[AttributeType.VIT].BaseValue = 1f;
            attributes[AttributeType.INT].BaseValue = 1f;
            attributes[AttributeType.DEX].BaseValue = 1f;
            attributes[AttributeType.LUK].BaseValue = 1f;

            // Movement & Combat basics
            attributes[AttributeType.MoveSpeed].BaseValue = 5f;
            attributes[AttributeType.ASPD].BaseValue = 150f; 
            attributes[AttributeType.WeightLimit].BaseValue = 2000f;
            
            // Baseline Attack (Physical Only)
            attributes[AttributeType.ATK].BaseValue = 10f; 
            attributes[AttributeType.StatusDMG].BaseValue = 10f;

            // Apply specific overrides from InitialAttributes List if any (set in Inspector)
            foreach (var attr in initialAttributes)
            {
                if (attributes.TryGetValue(attr.Type, out var existing))
                {
                    existing.BaseValue = attr.BaseValue;
                    // Note: modifiers might also be in the initialAttributes if they were serialized, 
                    // but usually it's just BaseValue that we care about from the inspector.
                }
            }

            UpdateDerivedAttributes();

            // Initialize current HP / MP to max
            attributes[AttributeType.HP].BaseValue = attributes[AttributeType.MaxHP].CurrentValue;
            attributes[AttributeType.HP].Recalculate();
            
            attributes[AttributeType.MP].BaseValue = attributes[AttributeType.MaxMP].CurrentValue;
            attributes[AttributeType.MP].Recalculate();
        }
        public void UpdateDerivedAttributes()
        {
            if (_isUpdating) return;

            _isUpdating = true;
            try
            {
                InnerUpdateDerivedAttributes();
            }
            finally
            {
                _isUpdating = false;
            }

            OnAttributesChanged?.Invoke();
        }

        private void InnerUpdateDerivedAttributes()
        {
            // Fetch core stats (using CurrentValue to include equipment/buff modifiers)
            float str = attributes[AttributeType.STR].CurrentValue;
            float agi = attributes[AttributeType.AGI].CurrentValue;
            float vit = attributes[AttributeType.VIT].CurrentValue;
            float @int = attributes[AttributeType.INT].CurrentValue;
            float dex = attributes[AttributeType.DEX].CurrentValue;
            float luk = attributes[AttributeType.LUK].CurrentValue;

            // --- STR (Strength) ---
            // Physical ATK Influence
            attributes[AttributeType.ATK].StatFlatBonus = str + (dex / 5f) + (luk / 3f);
            attributes[AttributeType.WeightLimit].StatFlatBonus = str * 30f; 

            // --- AGI (Agility) ---
            attributes[AttributeType.Flee].StatFlatBonus = agi + (luk / 5f);
            attributes[AttributeType.StatusDEF].StatFlatBonus = (vit / 2f) + (agi / 5f) + (luk / 5f); 
            attributes[AttributeType.ASPD].StatFlatBonus = (agi * 0.5f) + (dex * 0.15f);

            // --- VIT (Vitality) ---
            attributes[AttributeType.MaxHP].StatPercentBonus = vit * 0.01f;
            attributes[AttributeType.HealingItems].StatPercentBonus = vit * 0.02f;
            attributes[AttributeType.HPRecovery].StatFlatBonus = vit / 5f;
            attributes[AttributeType.StatusDEF].StatFlatBonus = (vit / 5f) + (dex / 5f); 

            // --- INT (Intelligence) ---
            // MATK Influence
            attributes[AttributeType.MATK].StatFlatBonus = (@int * 1.5f) + (dex / 5f) + (luk / 3f);
            attributes[AttributeType.MaxMP].StatPercentBonus = @int * 0.01f;
            attributes[AttributeType.StatusDEF].StatFlatBonus += @int; 
            
            float spRec = @int / 6f;
            if (@int >= 120) spRec += (@int - 120) / 2f; 
            attributes[AttributeType.MPRecovery].StatFlatBonus = spRec;

            // --- DEX (Dexterity) ---
            attributes[AttributeType.Hit].StatFlatBonus = dex + (luk / 3f);

            // --- LUK (Luck) ---
            // Status ATK (Poison, Burn Damage) Influence - Adding LUK to StatusATK
            attributes[AttributeType.StatusDMG].StatFlatBonus = (luk * 0.5f) + (@int / 4f); 
            
            attributes[AttributeType.CriticalRate].StatFlatBonus = luk * 0.3f;
            attributes[AttributeType.CritResistance].StatFlatBonus = luk / 5f;
            attributes[AttributeType.PerfectDodge].StatFlatBonus = luk / 10f;

            // Recalculate all to apply any modifiers
            foreach (var kvp in attributes)
            {
                // Skip Special Hidden Attributes or Handle them separately
                if (kvp.Key == AttributeType.TrueDamage) 
                {
                    kvp.Value.Recalculate();
                    continue;
                }
                
                kvp.Value.Recalculate();
            }
        }

        public Attribute GetAttribute(AttributeType type)
        {
            if (attributes.TryGetValue(type, out Attribute attr))
            {
                return attr;
            }
            return null;
        }

        public void AddModifier(AttributeType type, AttributeModifier modifier)
        {
            if (attributes.TryGetValue(type, out Attribute attr))
            {
                attr.AddModifier(modifier);
                UpdateDerivedAttributes();
            }
        }

        public void RemoveModifier(AttributeType type, AttributeModifier modifier)
        {
            if (attributes.TryGetValue(type, out Attribute attr))
            {
                attr.RemoveModifier(modifier);
                UpdateDerivedAttributes();
            }
        }
    }
}
