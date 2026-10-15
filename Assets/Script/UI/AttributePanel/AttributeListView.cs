using CoreSystem;
using CoreSystem.Data;
using CoreSystem.UI.Utility;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.UI.AttributePanel;

namespace CoreSystem.UI.AttributePanel
{
    public class AttributeListView : MonoBehaviour
    {
        public GameObject AttributeListBlockPrefab;

        List<AttributeType> BasicAttributes = new List<AttributeType>
        { 
            // 1. Core Resources
            AttributeType.MaxHP,
            AttributeType.HPRecovery, AttributeType.HPRecoveryPercent,
            AttributeType.MaxMP,
            AttributeType.MPRecovery, AttributeType.MPRecoveryPercent,

            // 2. Base Attributes
            AttributeType.STR, AttributeType.AGI, AttributeType.VIT,
            AttributeType.INT, AttributeType.DEX, AttributeType.LUK,

            // 3. Primary Combat Values
            AttributeType.ATK,         // Logic: Physical ATK
            AttributeType.MATK,        // Logic: Magic ATK
            AttributeType.WeaponATK,   // Weapon Physic
            AttributeType.WeaponMATK,  // Weapon Magic
            AttributeType.StatusDMG,   // Status DMG (Poison/Burn)
            AttributeType.Defense, AttributeType.StatusDEF, AttributeType.StatusDEF,
            AttributeType.Hit, AttributeType.Flee,
            AttributeType.ASPD, AttributeType.MoveSpeed,
            AttributeType.WeightLimit
        };

        List<AttributeType> AdvanceAttributes = new List<AttributeType>
        {
            // 1. Critical & Precision
            AttributeType.CriticalRate, AttributeType.CriticalDamage, AttributeType.CritResistance,
            AttributeType.PerfectDodge,
            AttributeType.AOE,

            AttributeType.CooldownReduc, AttributeType.CooldownReducPercent,

            // 2. Casting Modifiers (Consolidated)
            AttributeType.VariableCastTimeReduc,         // Marker for Variable Flat
            AttributeType.VariableCastTimeReducPercent,  // Marker for Variable %
            AttributeType.FixedCastTimeReduc,         // Marker for Fixed Flat
            AttributeType.FixedCastTimeReducPercent,


            // 3. Elemental Offense
            AttributeType.NeutralDamage, AttributeType.WaterDamage, AttributeType.EarthDamage,
            AttributeType.FireDamage, AttributeType.ElectricDamage, AttributeType.GhostDamage,
            AttributeType.HolyDamage,

            // 4. Elemental Defense
            AttributeType.NeutralResist, AttributeType.WaterResist, AttributeType.EarthResist,
            AttributeType.FireResist, AttributeType.ElectricResist, AttributeType.GhostResist,
            AttributeType.HolyResist,

            // 5. Size Multipliers (Damage)
            AttributeType.SmallDamage, AttributeType.MediumDamage, AttributeType.LargeDamage,
            // 6. Size Multipliers (Resistance)
            AttributeType.SmallResist, AttributeType.MediumResist, AttributeType.LargeResist,

            // 7. Race Multipliers (Damage)
            AttributeType.FormlessDamage, AttributeType.UndeadDamage, AttributeType.BruteDamage,
            AttributeType.PlantDamage, AttributeType.InsectDamage, AttributeType.FishDamage,
            AttributeType.DemonDamage, AttributeType.DemiHumanDamage, AttributeType.AngelDamage, AttributeType.DragonDamage,

            // 8. Race Multipliers (Resistance)
            AttributeType.FormlessResist, AttributeType.UndeadResist, AttributeType.BruteResist,
            AttributeType.PlantResist, AttributeType.InsectResist, AttributeType.FishResist,
            AttributeType.DemonResist, AttributeType.DemiHumanResist, AttributeType.AngelResist, AttributeType.DragonResist
        };

        public void Awake()
        {
            GameData.Player.Stats.OnAttributesChanged += OnAttributesChanged;
        }

        public void OnDestroy()
        {
            GameData.Player.Stats.OnAttributesChanged -= OnAttributesChanged;
        }

        private void OnAttributesChanged()
        {
            RefreshValues();
        }

        private void OnEnable()
        {
            LoadBasic();
        }

        public void LoadBasic()
        {
            SetAttributeList(BasicAttributes);
        }
        public void LoadAdvance()
        {
            SetAttributeList(AdvanceAttributes);
        }

        private List<AttributeType> _currentAttributes = new List<AttributeType>();
        private List<AttributeListBlock> _activeBlocks = new List<AttributeListBlock>();

        public void SetAttributeList(List<AttributeType> attributes)
        {
            _currentAttributes = attributes;

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            _activeBlocks.Clear();

            foreach (var attribute in attributes)
            {
                // Filter logic: Hide Special Hidden Attributes
                if (attribute == AttributeType.TrueDamage) continue;

                // Handle Consolidated Cast Time Attributes specially
                if (IsConsolidatedCastAttribute(attribute))
                {
                    CreateConsolidatedBlock(attribute);
                    continue;
                }

                if (attribute == AttributeType.ASPD)
                {
                    CreateASPDBlock();
                    continue;
                }

                var template = Database.GetAttribute(attribute);
                var attrData = GameData.Player.Stats.GetAttribute(attribute);

                var go = Instantiate(AttributeListBlockPrefab, transform);

                // Setup Hover/Tooltip
                go.AddComponent<HoverableUI>().Setup(
                    $"<color=#25E3B7>{template.FullName}</color>",
                    template.Description
                );

                var blockScript = go.GetComponent<AttributeListBlock>();
                blockScript.Setup(template, attrData);
                _activeBlocks.Add(blockScript);
            }
        }

        private bool IsConsolidatedCastAttribute(AttributeType type)
        {
            return type == AttributeType.VariableCastTimeReduc || 
                   type == AttributeType.VariableCastTimeReducPercent ||
                   type == AttributeType.FixedCastTimeReduc ||
                   type == AttributeType.FixedCastTimeReducPercent;
        }

        private void CreateConsolidatedBlock(AttributeType type)
        {
            var go = Instantiate(AttributeListBlockPrefab, transform);
            var blockScript = go.GetComponent<AttributeListBlock>();

            string label = "";
            string valueStr = "";
            string tooltipTitle = "";
            string tooltipDesc = "";

            CalculateConsolidatedValue(type, out label, out valueStr, out tooltipTitle, out tooltipDesc);

            go.AddComponent<HoverableUI>().Setup(tooltipTitle, tooltipDesc);
            blockScript.Setup(label, valueStr);
            _activeBlocks.Add(blockScript);
        }

        private void CreateASPDBlock()
        {
            var go = Instantiate(AttributeListBlockPrefab, transform);
            var blockScript = go.GetComponent<AttributeListBlock>();
            string label = "";
            string valueStr = "";
            string tooltipTitle = "";
            string tooltipDesc = "";
            CalculateASPDValue(out label, out valueStr, out tooltipTitle, out tooltipDesc);
            go.AddComponent<HoverableUI>().Setup(tooltipTitle, tooltipDesc);
            blockScript.Setup(label, valueStr);
            _activeBlocks.Add(blockScript);
        }

        private void CalculateASPDValue(out string label, out string valueStr, out string tooltipTitle, out string tooltipDesc)
        {
            var manager = GameData.Player.Stats;
            var template = Database.GetAttribute(AttributeType.ASPD);
            label = template != null ? template.Name : "ASPD";
            tooltipTitle = template != null ? $"<color=#25E3B7>{template.FullName}</color>" : "Attack Speed";
            tooltipDesc = template != null ? template.Description : "";
            var attr = manager.GetAttribute(AttributeType.ASPD);
            if (attr == null)
            {
                valueStr = "0.0 Hit/s";
                return;
            }
            int AttactSpeedMinPreHit = 1;
            int AttactSpeedMaxPreHit = 100;
            float finalValue = attr.CurrentValue;
            float baseValue = attr.BaseValue;
            float flatBonus = attr.FlatBonusSum;
            float percentBonus = attr.PercentBonusSum;
            float preFinal = (baseValue + flatBonus) * (1f + percentBonus);
            float addBonus = finalValue - preFinal;
            float finalDelay = (200f - finalValue) / 50f;
            float minDelay = 1f / AttactSpeedMaxPreHit;
            float maxDelay = 1f / AttactSpeedMinPreHit;
            finalDelay = Mathf.Clamp(finalDelay, minDelay, maxDelay);
            float finalHitsPerSecond = 1f / finalDelay;
            float baseDelay = (200f - baseValue) / 50f;
            baseDelay = Mathf.Clamp(baseDelay, minDelay, maxDelay);
            float baseHitsPerSecond = 1f / baseDelay;
            string finalText = $"<b>{finalHitsPerSecond:F1} Hit/s</b>";
            string breakdownText = "";
            if (finalValue != baseValue)
            {
                string basePart = $"{baseHitsPerSecond:F1}";
                float diff = finalHitsPerSecond - baseHitsPerSecond;
                string diffSign = diff >= 0 ? "+" : "";
                breakdownText = $" <size=85%><color=#A0A0A0>({basePart}<color=#25E3B7>{diffSign}{diff:F1}</color> Hit/s)</color></size>";
            }
            valueStr = $"{finalText}{breakdownText}";
            tooltipDesc += $"\n\nRaw ASPD Breakdown:\nBase: {baseValue:F0}\nFlat Bonus: {flatBonus:F0}\nPercent Bonus: {percentBonus * 100f:F0}%\nTotal Raw ASPD: {finalValue:F0}";
        }

        private void CalculateConsolidatedValue(AttributeType type, out string label, out string valueStr, out string tooltipTitle, out string tooltipDesc)
        {
            var manager = GameData.Player.Stats;
            label = "";
            valueStr = "";
            tooltipTitle = "";
            tooltipDesc = "";

            if (type == AttributeType.VariableCastTimeReduc)
            {
                label = "ReductionVariableCastTimeFlat";
                float val = manager.GetAttribute(AttributeType.VariableCastTimeReduc).CurrentValue;
                valueStr = $"<b>{val:F2}s</b>";
                tooltipTitle = "<color=#25E3B7>Variable Cast Time Reduction (Flat)</color>";
                tooltipDesc = "Total seconds subtracted directly from variable cast time.";
            }
            else if (type == AttributeType.VariableCastTimeReducPercent)
            {
                label = "TotalVariableCastTimeReduc";

                // Calculate effective % reduction (copied logic from SkillInstance)
                float dex = manager.GetAttribute(AttributeType.DEX)?.CurrentValue ?? 0;
                float intel = manager.GetAttribute(AttributeType.INT)?.CurrentValue ?? 0;

                // Use Percentage attributes instead of flat (seconds) ones
                float gearReduc = manager.GetAttribute(AttributeType.GearVariableCastTimePercent)?.CurrentValue ?? 0;
                float skillReduc = manager.GetAttribute(AttributeType.SkillVariableCastTimePercent)?.CurrentValue ?? 0;
                float otherReduc = manager.GetAttribute(AttributeType.VariableCastTimeReducPercent)?.CurrentValue ?? 0;

                float statTerm = Mathf.Clamp01((dex * 2f + intel) / 530f);
                float statReduc = Mathf.Sqrt(statTerm);

                // Formula: 1 - (1-stat)*(1-gear)*(1-skill)*(1-other)
                float effectiveRatio = 1f - ( (1f - statReduc) * (1f - (gearReduc/100f)) * (1f - (skillReduc/100f)) * (1f - (otherReduc/100f)) );
                float percentVal = effectiveRatio * 100f;

                valueStr = $"<b>{percentVal:F0}%</b>";
                tooltipTitle = "<color=#25E3B7>Variable Cast Time Reduction (Total %)</color>";
                tooltipDesc = $"Total effective reduction percentage.\nBreakdown:\n- Stats (DEX/INT): {statReduc*100:F0}%\n- Gear: {gearReduc:F0}%\n- Skills: {skillReduc:F0}%\n- Other (Buffs): {otherReduc:F0}%";
            }
            else if (type == AttributeType.FixedCastTimeReduc)
            {
                label = "FixedCastTimeReduc";
                float val = manager.GetAttribute(AttributeType.FixedCastTimeReduc)?.CurrentValue ?? 0;
                valueStr = $"<b>{val:F2}s</b>";
                tooltipTitle = "<color=#25E3B7>Fixed Cast Time Reduction (Flat)</color>";
                tooltipDesc = "Total seconds subtracted directly from fixed cast time.";
            }
            else if (type == AttributeType.FixedCastTimeReducPercent)
            {
                label = "FixedCastTimeReducPercent";
                float val = manager.GetAttribute(AttributeType.FixedCastTimeReducPercent)?.CurrentValue ?? 0;
                valueStr = $"<b>{val:F0}%</b>";
                tooltipTitle = "<color=#25E3B7>Fixed Cast Time Reduction (%)</color>";
                tooltipDesc = "Percentage reduction to fixed cast time (only takes the highest value usually, but here showing total for generic info).";
            }
        }

        private void RefreshValues()
        {
            if (_activeBlocks.Count == 0 || _currentAttributes == null || _currentAttributes.Count == 0) return;

            int blockIndex = 0;
            foreach (var attribute in _currentAttributes)
            {
                if (attribute == AttributeType.TrueDamage) continue;

                if (blockIndex >= _activeBlocks.Count) break;

                if (IsConsolidatedCastAttribute(attribute))
                {
                    string label, valueStr, tooltipTitle, tooltipDesc;
                    CalculateConsolidatedValue(attribute, out label, out valueStr, out tooltipTitle, out tooltipDesc);

                    var block = _activeBlocks[blockIndex];
                    block.Setup(label, valueStr);

                    // Update tooltip if possible (HoverableUI doesn't have an update method easily visible, 
                    // but we can re-add or ignore if description doesn't change much)
                    var hover = block.GetComponent<HoverableUI>();
                    if (hover != null) hover.Setup(tooltipTitle, tooltipDesc);
                }
                else if (attribute == AttributeType.ASPD)
                {
                    string label, valueStr, tooltipTitle, tooltipDesc;
                    CalculateASPDValue(out label, out valueStr, out tooltipTitle, out tooltipDesc);

                    var block = _activeBlocks[blockIndex];
                    block.Setup(label, valueStr);

                    var hover = block.GetComponent<HoverableUI>();
                    if (hover != null) hover.Setup(tooltipTitle, tooltipDesc);
                }
                else
                {
                    var template = Database.GetAttribute(attribute);
                    var attrData = GameData.Player.Stats.GetAttribute(attribute);
                    _activeBlocks[blockIndex].Setup(template, attrData);
                }

                blockIndex++;
            }

            Debug.Log(blockIndex + " attributes refreshed.");
        }
    }
}
