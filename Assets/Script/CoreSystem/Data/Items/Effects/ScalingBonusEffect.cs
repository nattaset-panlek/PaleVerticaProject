using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Entity;
using UnityEngine;
using System.Collections.Generic;

namespace CoreSystem.Data.Items.Effects
{
    [System.Serializable]
    public class ScalingBonusEffect : EquipmentEffect
    {
        public List<AttributeType> SourceAttributes = new List<AttributeType>();
        public float Threshold = 10f;
        public float BonusPerThreshold = 1f;
        public AttributeType TargetAttribute;
        public ModifierType ModType = ModifierType.Flat;

        private Character _appliedPlayer;
        private AttributeModifier _activeModifier;
        private float _lastAppliedBonus = 0f;
        private bool _isRecalculating = false;

        public override void Apply(Character player, ItemInstance instance = null)
        {
            if (player == null || player.attributeSystem == null) return;
            _appliedPlayer = player;
            
            // Subscribe to context changes
            player.attributeSystem.OnAttributesChanged += RecalculateScaling;
            
            // Initial calculation
            RecalculateScaling();
        }

        public override void Remove(Character player, ItemInstance instance = null)
        {
            if (player == null || player.attributeSystem == null) return;
            
            player.attributeSystem.OnAttributesChanged -= RecalculateScaling;
            
            if (_activeModifier != null)
            {
                player.attributeSystem.RemoveModifier(TargetAttribute, _activeModifier);
                _activeModifier = null;
            }
            
            _lastAppliedBonus = 0f;
            _appliedPlayer = null;
        }

        private void RecalculateScaling()
        {
            if (_appliedPlayer == null || _appliedPlayer.attributeSystem == null) return;

            float totalSourceValue = 0;
            foreach (var attrType in SourceAttributes)
            {
                var attr = _appliedPlayer.attributeSystem.GetAttribute(attrType);
                if (attr != null) totalSourceValue += attr.CurrentValue;
            }

            if (Threshold <= 0) return;

            int multiplier = Mathf.FloorToInt(totalSourceValue / Threshold);
            float bonusAmount = multiplier * BonusPerThreshold;

            if (_isRecalculating) return;

            // CHECK: only update if the bonus value has actually changed to prevent infinite loops
            // since AddModifier/RemoveModifier triggers UpdateDerivedAttributes -> OnAttributesChanged
            if (Mathf.Approximately(bonusAmount, _lastAppliedBonus) && _activeModifier != null)
            {
                return;
            }

            _isRecalculating = true;
            try
            {
                // Remove old
                if (_activeModifier != null)
                {
                    _appliedPlayer.attributeSystem.RemoveModifier(TargetAttribute, _activeModifier);
                }

                _lastAppliedBonus = bonusAmount;

                if (bonusAmount != 0)
                {
                    _activeModifier = new AttributeModifier(bonusAmount, ModType);
                    _appliedPlayer.attributeSystem.AddModifier(TargetAttribute, _activeModifier);
                }
                else
                {
                    _activeModifier = null;
                }
            }
            finally
            {
                _isRecalculating = false;
            }
        }

        public override string GetDescription()
        {
            try
            {
                var targetTempl = CoreSystem.Data.Database.GetAttribute(TargetAttribute);
                string targetName = targetTempl != null ? targetTempl.FullName : TargetAttribute.ToString();
                string bonusSign = BonusPerThreshold >= 0 ? "+" : "";
                string bonusUnit = (targetTempl != null && targetTempl.IsPercent) ? "%" : "";

                List<string> sourceNames = new List<string>();
                if (SourceAttributes != null)
                {
                    foreach (var st in SourceAttributes)
                    {
                        var stTempl = CoreSystem.Data.Database.GetAttribute(st);
                        sourceNames.Add(stTempl != null ? stTempl.Name : st.ToString());
                    }
                }

                string sourceStr = sourceNames.Count > 0 ? string.Join(" & ", sourceNames) : "Stats";
                return $"Every {Threshold} {sourceStr} gives {bonusSign}{BonusPerThreshold}{bonusUnit} {targetName}";
            }
            catch (System.Exception)
            {
                return $"Bonus for {TargetAttribute} (Scaling)";
            }
        }

        public override string ToString() => GetDescription();
    }
}
