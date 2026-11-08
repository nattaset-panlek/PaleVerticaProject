using System.Linq;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Skills;
using UnityEditor;
using UnityEngine;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Private Methods]
    private (string, float, System.Func<AttributeEntry, string>, System.Func<AttributeEntry, Color?>)[] GetAttributeColumns() => new (string, float, System.Func<AttributeEntry, string>, System.Func<AttributeEntry, Color?>)[]
    {
        ("ID", 60, e => e.Id.ToString(), null),
        ("Name", 150, e => e.Name, null),
        ("Full Name", 210, e => e.FullName, null),
        ("Icon Name", 150, e => e.Icon, null),
        ("Mapping", 170, e => e.Mapping, null),
        ("Is Percent", 90, e => e.IsPercent ? "Yes" : "No", null),
        ("Description", 420, e => e.Description, null)
    };

    private (string, float, System.Func<ItemEntry, string>, System.Func<ItemEntry, Color?>)[] GetItemColumns() => new (string, float, System.Func<ItemEntry, string>, System.Func<ItemEntry, Color?>)[]
    {
        ("ID", 66, e => e.ItemId.ToString(), null),
        ("Name", 180, e => e.Name, null),
        ("Item Type", 100, e => e.ItemType, null),
        ("Rarity", 96, e => e.Rarity, e => System.Enum.TryParse(e.Rarity, true, out ItemRarity r) ? r.GetColor() : (Color?)ThemeMuted),
        ("Equipment Slot", 150, e => e.ItemType == "Equipment" ? e.EquipmentSlot : "", null),
        ("Max Stack", 80, e => e.MaxStack.ToString(), null),
        ("Max Sockets", 86, e => e.ItemType == "Equipment" ? e.MaxSockets.ToString() : "", null),
        ("Item Set", 120, e => string.IsNullOrEmpty(e.SetID) ? "" : (_itemSetData.Sets.FirstOrDefault(s => s.SetID == e.SetID)?.SetName ?? e.SetID), null),
        ("Effects", 340, e => DescribeEffects(e), null)
    };

    private (string, float, System.Func<FactionEntry, string>, System.Func<FactionEntry, Color?>)[] GetFactionColumns() => new (string, float, System.Func<FactionEntry, string>, System.Func<FactionEntry, Color?>)[]
    {
        ("ID", 60, e => e.Id.ToString(), null),
        ("Name", 200, e => e.Name, null),
        ("Type", 90, e => e.Id <= 3 ? "Built-in" : "Custom", e => e.Id <= 3 ? ThemeMuted : ThemeAccent),
        ("Allies", 330, e => string.Join(", ", (e.Positive ?? new System.Collections.Generic.List<int>()).Select(GetFactionName)), null),
        ("Hostiles", 330, e => string.Join(", ", (e.Negative ?? new System.Collections.Generic.List<int>()).Select(GetFactionName)), null)
    };

    private (string, float, System.Func<SupportSkillJsonData, string>, System.Func<SupportSkillJsonData, Color?>)[] GetSupportColumns() => new (string, float, System.Func<SupportSkillJsonData, string>, System.Func<SupportSkillJsonData, Color?>)[]
    {
        ("ID", 70, e => e.SkillID.ToString(), null),
        ("Name", 190, e => e.ModifierName, null),
        ("Description", 560, e => e.Description, null),
        ("Icon", 220, e => e.IconPath, null)
    };

    private (string, float, System.Func<UniquePassiveEntry, string>, System.Func<UniquePassiveEntry, Color?>)[] GetPassiveColumns() => new (string, float, System.Func<UniquePassiveEntry, string>, System.Func<UniquePassiveEntry, Color?>)[]
    {
        ("ID", 180, e => e.Id, null),
        ("Name", 200, e => e.Name, null),
        ("Description", 640, e => e.Description, null)
    };

    private string DescribeEffects(ItemEntry e)
    {
        if (e.Effects == null || e.Effects.Count == 0) return "";
        return string.Join(", ", e.Effects.Take(3).Select(fx =>
        {
            string attr = fx.AttributeID > 0 ? _attributeData.Attributes.FirstOrDefault(a => a.Id == fx.AttributeID)?.Name : null;
            string value = fx.Value != 0 ? " +" + fx.Value : "";
            return attr != null ? $"{fx.EffectType} {attr}{value}" : fx.EffectType;
        })) + (e.Effects.Count > 3 ? $" (+{e.Effects.Count - 3})" : "");
    }
    #endregion
}
