using UnityEngine;
using System.Collections.Generic;
using CoreSystem;
using CoreSystem.SkillTree.Models;

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
            attributes[AttributeType.MoveSpeed].BaseValue = 100f;
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
            // Fetch core stats
            float str = attributes[AttributeType.STR].CurrentValue;
            float agi = attributes[AttributeType.AGI].CurrentValue;
            float vit = attributes[AttributeType.VIT].CurrentValue;
            float @int = attributes[AttributeType.INT].CurrentValue;
            float dex = attributes[AttributeType.DEX].CurrentValue;
            float luk = attributes[AttributeType.LUK].CurrentValue;

            // --- STR ---
            attributes[AttributeType.ATK].StatFlatBonus = str + (dex / 5f) + (luk / 3f);
            attributes[AttributeType.WeightLimit].StatFlatBonus = str * 30f; 

            // --- AGI ---
            attributes[AttributeType.Flee].StatFlatBonus = agi + (luk / 5f);
            attributes[AttributeType.StatusDEF].StatFlatBonus = (vit / 2f) + (agi / 5f) + (luk / 5f); 
            attributes[AttributeType.ASPD].StatFlatBonus = (agi * 0.5f) + (dex * 0.15f);

            // --- VIT ---
            attributes[AttributeType.MaxHP].StatPercentBonus = vit * 0.01f;
            attributes[AttributeType.HealingItems].StatPercentBonus = vit * 0.02f;
            attributes[AttributeType.HPRecovery].StatFlatBonus = vit / 5f;
            attributes[AttributeType.StatusDEF].StatFlatBonus += (vit / 5f) + (dex / 5f); 

            // --- INT ---
            attributes[AttributeType.MATK].StatFlatBonus = (@int * 1.5f) + (dex / 5f) + (luk / 3f);
            attributes[AttributeType.MaxMP].StatPercentBonus = @int * 0.01f;
            attributes[AttributeType.StatusDEF].StatFlatBonus += @int; 
            
            float spRec = @int / 6f;
            if (@int >= 120) spRec += (@int - 120) / 2f; 
            attributes[AttributeType.MPRecovery].StatFlatBonus = spRec;

            // --- DEX ---
            attributes[AttributeType.Hit].StatFlatBonus = dex + (luk / 3f);

            // --- LUK ---
            attributes[AttributeType.StatusDMG].StatFlatBonus = (luk * 0.5f) + (@int / 4f); 
            attributes[AttributeType.CriticalRate].StatFlatBonus = luk * 0.3f;
            attributes[AttributeType.CritResistance].StatFlatBonus = luk / 5f;
            attributes[AttributeType.PerfectDodge].StatFlatBonus = luk / 10f;

            // Recalculate all
            foreach (var kvp in attributes)
            {
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
            if (attributes.TryGetValue(type, out var attr))
            {
                attr.RemoveModifier(modifier);
                UpdateDerivedAttributes();
            }
        }

        public void AddAdditional(AttributeType type, AdditionalEntry entry)
        {
            if (attributes.TryGetValue(type, out var attr))
            {
                attr.Additional.Add(entry);
                UpdateDerivedAttributes();
            }
        }

        public AdditionalEntry AddAdditional(AttributeType type, int order, AdditionalOp op, float value, string tag = "")
        {
            var entry = new AdditionalEntry(order, op, value, tag);
            AddAdditional(type, entry);
            return entry;
        }

        public void RemoveAdditional(AttributeType type, AdditionalEntry entry)
        {
            if (attributes.TryGetValue(type, out var attr))
            {
                attr.Additional.Remove(entry);
                UpdateDerivedAttributes();
            }
        }

        public void RemoveAdditionalByTag(AttributeType type, string tag)
        {
            if (attributes.TryGetValue(type, out var attr))
            {
                attr.Additional.RemoveByTag(tag);
                UpdateDerivedAttributes();
            }
        }

        public void ClearAdditional(AttributeType type)
        {
            if (attributes.TryGetValue(type, out var attr))
            {
                attr.Additional.Clear();
                UpdateDerivedAttributes();
            }
        }

        public void ClearAllAdditional()
        {
            foreach (var attr in attributes.Values)
                attr.Additional.Clear();
            UpdateDerivedAttributes();
        }

        public void ApplyPassiveStats(SkillTreeStats stats)
        {
            foreach (var attr in attributes.Values)
                attr.Additional.RemoveByTag("Passive");

            if (stats == null)
            {
                UpdateDerivedAttributes();
                return;
            }

            foreach (var kvp in stats.RawStats)
            {
                AttributeType type = (AttributeType)kvp.Key;
                float value = kvp.Value;

                string typeName = type.ToString();

                if (typeName.EndsWith("Percent"))
                {
                    string baseName = typeName.Substring(0, typeName.Length - 7);
                    if (System.Enum.TryParse(baseName, out AttributeType baseType) && attributes.TryGetValue(baseType, out var baseAttr))
                    {
                        baseAttr.Additional.Add(new AdditionalEntry(2, AdditionalOp.Multiply, 1f + (value / 100f), "Passive"));
                        continue;
                    }
                }

                if (attributes.TryGetValue(type, out var attr))
                    attr.Additional.Add(new AdditionalEntry(2, AdditionalOp.Add, value, "Passive"));
            }

            UpdateDerivedAttributes();
        }
    }
}
