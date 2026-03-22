using CoreSystem;
using CoreSystem.Data;
using CoreSystem.UI.Utile;
using System.Collections.Generic;
using UnityEngine;


public class AttributeListManager : MonoBehaviour
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
        AttributeType.Defense, AttributeType.StatusDEF, AttributeType.StatusMDEF,
        AttributeType.Hit, AttributeType.Flee,
        AttributeType.ASPD, AttributeType.MoveSpeed,
        AttributeType.WeightLimit
    };

    List<AttributeType> AdvanceAttributes = new List<AttributeType>
    {
        // 1. Critical & Precision
        AttributeType.CriticalRate, AttributeType.CriticalDamage, AttributeType.CritResistance,
        AttributeType.PerfectDodge,

        // 2. Casting Modifiers (Consolidated)
        AttributeType.VCTReductionFlat,         // Marker for Variable Flat
        AttributeType.CastTime,                 // Marker for Variable %
        AttributeType.FCTReductionFlat,         // Marker for Fixed Flat
        AttributeType.MaxFCTReduction,          // Marker for Fixed %
        
        
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
        GameData.Player.CharacterManager.OnAttributesChanged += OnAttributesChanged;
    }

    public void OnDestroy()
    {
        GameData.Player.CharacterManager.OnAttributesChanged -= OnAttributesChanged;
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

            var template = Database.GetAttribute(attribute);
            var attrData = GameData.Player.CharacterManager.GetAttribute(attribute);

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
        return type == AttributeType.VCTReductionFlat || 
               type == AttributeType.CastTime || 
               type == AttributeType.FCTReductionFlat || 
               type == AttributeType.MaxFCTReduction;
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

    private void CalculateConsolidatedValue(AttributeType type, out string label, out string valueStr, out string tooltipTitle, out string tooltipDesc)
    {
        var manager = GameData.Player.CharacterManager;
        label = "";
        valueStr = "";
        tooltipTitle = "";
        tooltipDesc = "";

        if (type == AttributeType.VCTReductionFlat)
        {
            label = "ReductionVariableCastTimeFlat";
            float val = manager.GetAttribute(AttributeType.VCTReductionFlat).CurrentValue;
            valueStr = $"<b>{val:F2}s</b>";
            tooltipTitle = "<color=#25E3B7>Variable Cast Time Reduction (Flat)</color>";
            tooltipDesc = "Total seconds subtracted directly from variable cast time.";
        }
        else if (type == AttributeType.CastTime)
        {
            label = "ReductioVerableCastTime";
            
            // Calculate effective % reduction (copied logic from SkillInstance)
            float dex = manager.GetAttribute(AttributeType.DEX).CurrentValue;
            float intel = manager.GetAttribute(AttributeType.INT).CurrentValue;
            float gearReduc = manager.GetAttribute(AttributeType.GearVCTReduction).CurrentValue;
            float skillReduc = manager.GetAttribute(AttributeType.SkillVCTReduction).CurrentValue;

            float statTerm = Mathf.Clamp01((dex * 2f + intel) / 530f);
            float statReduc = Mathf.Sqrt(statTerm);
            
            float effectiveRatio = 1f - ( (1f - statReduc) * (1f - (gearReduc/100f)) * (1f - (skillReduc/100f)) );
            float percentVal = effectiveRatio * 100f;

            valueStr = $"<b>{percentVal:F0}%</b>";
            tooltipTitle = "<color=#25E3B7>Variable Cast Time Reduction (%)</color>";
            tooltipDesc = $"Total effective reduction percentage.\nBreakdown:\n- Stats (DEX/INT): {statReduc*100:F0}%\n- Gear: {gearReduc:F0}%\n- Skills: {skillReduc:F0}%";
        }
        else if (type == AttributeType.FCTReductionFlat)
        {
            label = "ReducFixeCastFlat";
            float val = manager.GetAttribute(AttributeType.FCTReductionFlat).CurrentValue;
            valueStr = $"<b>{val:F2}s</b>";
            tooltipTitle = "<color=#25E3B7>Fixed Cast Time Reduction (Flat)</color>";
            tooltipDesc = "Total seconds subtracted directly from fixed cast time.";
        }
        else if (type == AttributeType.MaxFCTReduction)
        {
            label = "ReducFixeCastTime";
            float val = manager.GetAttribute(AttributeType.MaxFCTReduction).CurrentValue;
            valueStr = $"<b>{val:F0}%</b>";
            tooltipTitle = "<color=#25E3B7>Fixed Cast Time Reduction (%)</color>";
            tooltipDesc = "Percentage reduction for fixed cast time.";
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
            else
            {
                var template = Database.GetAttribute(attribute);
                var attrData = GameData.Player.CharacterManager.GetAttribute(attribute);
                _activeBlocks[blockIndex].Setup(template, attrData);
            }

            blockIndex++;
        }

        Debug.Log(blockIndex + " attributes refreshed.");
    }
}
