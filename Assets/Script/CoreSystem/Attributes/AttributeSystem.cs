using UnityEngine;
using System.Collections.Generic;
using CoreSystem;
using CoreSystem.SkillTree.Models;
using CoreSystem.Combat;

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
            this.owner = owner;
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
                    existing.InitialValue = attr.BaseValue;
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

            if (owner != null && owner.UniquePassives != null)
            {
                owner.UniquePassives.ModifyCoreAttributes(ref str, ref agi, ref vit, ref @int, ref dex, ref luk);
            }

            DerivedStats derived = DerivedStatFormula.Compute(new CoreStats { Str = str, Agi = agi, Vit = vit, Int = @int, Dex = dex, Luk = luk });

            attributes[AttributeType.ATK].StatFlatBonus = derived.AtkFlat;
            attributes[AttributeType.MATK].StatFlatBonus = derived.MatkFlat;
            attributes[AttributeType.WeightLimit].StatFlatBonus = derived.WeightLimitFlat;
            attributes[AttributeType.Flee].StatFlatBonus = derived.FleeFlat;
            attributes[AttributeType.Hit].StatFlatBonus = derived.HitFlat;
            attributes[AttributeType.StatusDEF].StatFlatBonus = derived.StatusDefFlat;
            attributes[AttributeType.ASPD].StatFlatBonus = derived.AspdFlat;
            attributes[AttributeType.MaxHP].StatPercentBonus = derived.MaxHpPercent;
            attributes[AttributeType.MaxMP].StatPercentBonus = derived.MaxMpPercent;
            attributes[AttributeType.HealingItems].StatPercentBonus = derived.HealingItemsPercent;
            attributes[AttributeType.HPRecovery].StatFlatBonus = derived.HpRecoveryFlat;
            attributes[AttributeType.MPRecovery].StatFlatBonus = derived.MpRecoveryFlat;
            attributes[AttributeType.StatusDMG].StatFlatBonus = derived.StatusDmgFlat;
            attributes[AttributeType.CriticalRate].StatFlatBonus = derived.CriticalRateFlat;
            attributes[AttributeType.CritResistance].StatFlatBonus = derived.CritResistanceFlat;
            attributes[AttributeType.PerfectDodge].StatFlatBonus = derived.PerfectDodgeFlat;

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
