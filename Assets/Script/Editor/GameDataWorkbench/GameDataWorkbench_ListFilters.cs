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
    #region [Variables/Fields]
    private static readonly string[] AttributeKindFilters = { "All", "Flat", "Percent" };
    private static readonly string[] SetFilters = { "All", "Has items", "No items" };
    private static readonly string[] FactionFilters = { "All", "Built-in", "Custom" };
    private static readonly string[] IconFilters = { "All", "Has icon", "No icon" };
    private static readonly string[] DescriptionFilters = { "All", "Has description", "No description" };
    private static readonly string[] ItemSetFilters = { "Any set", "In a set", "No set" };
    private static readonly string[] SkillTargetTypes = { "All", "Single", "SingleUnit", "Sphere", "Cone", "Box", "Self", "Line", "TargetLocation" };

    private int _attributeKindFilter;
    private int _itemRarityFilter;
    private int _itemSetFilter;
    private int _setMembershipFilter;
    private int _characterFactionFilter;
    private int _factionKindFilter;
    private int _skillDamageTypeFilter;
    private int _skillTargetFilter;
    private bool _skillChainOnly;
    private int _supportIconFilter;
    private int _passiveDescriptionFilter;
    #endregion

    #region [Private Methods]
    private void DrawAttributeFilters()
    {
        DrawFilterChips(AttributeKindFilters, ref _attributeKindFilter);
    }

    private bool AttributeFilter(AttributeEntry e)
    {
        return _attributeKindFilter == 0 || (_attributeKindFilter == 2) == e.IsPercent;
    }

    private (string, System.Func<AttributeEntry, System.IComparable>)[] GetAttributeSorts() => new (string, System.Func<AttributeEntry, System.IComparable>)[]
    {
        ("ID", e => e.Id),
        ("Name", e => e.Name ?? ""),
        ("Full Name", e => e.FullName ?? ""),
        ("Mapping", e => e.Mapping ?? "")
    };

    private void DrawItemFilters()
    {
        DrawFilterChips(_tabNamesForItems, ref _itemTypeFilterIndex);
        GUILayout.Space(4);
        string[] rarities = new[] { "All" }.Concat(System.Enum.GetNames(typeof(ItemRarity))).ToArray();
        _itemRarityFilter = DrawFilterPopup("Rarity", rarities, _itemRarityFilter, 44);
        _itemSetFilter = DrawFilterPopup("Set", ItemSetFilters, _itemSetFilter, 44);
        _showOnlyWithEffects = EditorGUILayout.ToggleLeft("Only items with effects", _showOnlyWithEffects);
    }

    private bool ItemFilter(ItemEntry e)
    {
        if (_showOnlyWithEffects && (e.Effects == null || e.Effects.Count == 0)) return false;
        if (_itemTypeFilterIndex > 0 && e.ItemType != _itemTypeNames[_itemTypeFilterIndex]) return false;
        if (_itemRarityFilter > 0 && !string.Equals(e.Rarity, System.Enum.GetNames(typeof(ItemRarity))[_itemRarityFilter - 1], System.StringComparison.OrdinalIgnoreCase)) return false;
        bool inSet = !string.IsNullOrEmpty(e.SetID);
        if (_itemSetFilter == 1 && !inSet) return false;
        if (_itemSetFilter == 2 && inSet) return false;
        return true;
    }

    private (string, System.Func<ItemEntry, System.IComparable>)[] GetItemSorts() => new (string, System.Func<ItemEntry, System.IComparable>)[]
    {
        ("ID", e => e.ItemId),
        ("Name", e => e.Name ?? ""),
        ("Rarity", e => System.Enum.TryParse(e.Rarity, true, out ItemRarity r) ? (int)r : -1),
        ("Item Type", e => e.ItemType ?? ""),
        ("Equipment Slot", e => e.EquipmentSlot ?? ""),
        ("Max Sockets", e => e.MaxSockets),
        ("Max Stack", e => e.MaxStack),
        ("Effect Count", e => e.Effects?.Count ?? 0)
    };

    private void DrawSetFilters()
    {
        DrawFilterChips(SetFilters, ref _setMembershipFilter);
    }

    private bool SetFilter(ItemSetEntry e)
    {
        if (_setMembershipFilter == 0) return true;
        bool hasItems = _itemData.Items.Any(i => i.SetID == e.SetID);
        return _setMembershipFilter == 1 ? hasItems : !hasItems;
    }

    private (string, System.Func<ItemSetEntry, System.IComparable>)[] GetSetSorts() => new (string, System.Func<ItemSetEntry, System.IComparable>)[]
    {
        ("Set ID", e => e.SetID ?? ""),
        ("Name", e => e.SetName ?? ""),
        ("Pieces", e => _itemData.Items.Count(i => i.SetID == e.SetID)),
        ("Bonus Levels", e => e.BonusLevels?.Count ?? 0)
    };

    private void DrawCharacterFilters()
    {
        string[] options = new[] { "All factions" }.Concat(_factionData.Factions.Select(f => f.Name)).ToArray();
        _characterFactionFilter = DrawFilterPopup("Faction", options, _characterFactionFilter, 50);
    }

    private bool CharacterFilter(CharacterModel e)
    {
        if (_characterFactionFilter <= 0 || _characterFactionFilter > _factionData.Factions.Count) return true;
        return e.Faction == _factionData.Factions[_characterFactionFilter - 1].Id;
    }

    private (string, System.Func<CharacterModel, System.IComparable>)[] GetCharacterSorts() => new (string, System.Func<CharacterModel, System.IComparable>)[]
    {
        ("ID", e => e.Id),
        ("Name", e => e.Name ?? ""),
        ("Faction", e => GetFactionName(e.Faction)),
        ("Base HP", e => e.BaseHP),
        ("Base MP", e => e.BaseMP)
    };

    private void DrawFactionFilters()
    {
        DrawFilterChips(FactionFilters, ref _factionKindFilter);
    }

    private bool FactionFilter(FactionEntry e)
    {
        return _factionKindFilter == 0 || (_factionKindFilter == 1) == (e.Id <= 3);
    }

    private (string, System.Func<FactionEntry, System.IComparable>)[] GetFactionSorts() => new (string, System.Func<FactionEntry, System.IComparable>)[]
    {
        ("ID", e => e.Id),
        ("Name", e => e.Name ?? ""),
        ("Allies", e => e.Positive?.Count ?? 0),
        ("Hostiles", e => e.Negative?.Count ?? 0)
    };

    private void DrawSkillFilters()
    {
        DrawFilterChips(_skillElementFilterNames, ref _skillElementFilterIndex, name => System.Enum.TryParse(name, out ElementType el) ? GetElementColor(el) : ThemeAccent);
        GUILayout.Space(4);
        string[] damageTypes = new[] { "All" }.Concat(System.Enum.GetNames(typeof(DamageType))).ToArray();
        DrawFilterChips(damageTypes, ref _skillDamageTypeFilter);
        GUILayout.Space(4);
        _skillTargetFilter = DrawFilterPopup("Target", SkillTargetTypes, _skillTargetFilter, 44);
        _skillChainOnly = EditorGUILayout.ToggleLeft("Only skills with chain overrides", _skillChainOnly);
    }

    private bool SkillFilter(ActiveSkillJsonData e)
    {
        if (_skillElementFilterIndex > 0 && e.SkillElement != _skillElementFilterNames[_skillElementFilterIndex]) return false;
        if (_skillDamageTypeFilter > 0 && e.DamageType != System.Enum.GetNames(typeof(DamageType))[_skillDamageTypeFilter - 1]) return false;
        if (_skillTargetFilter > 0 && (e.BaseTargeting == null || e.BaseTargeting.Type != SkillTargetTypes[_skillTargetFilter])) return false;
        if (_skillChainOnly && (e.ChainOverrides == null || e.ChainOverrides.Count == 0)) return false;
        return true;
    }

    private (string, System.Func<ActiveSkillJsonData, System.IComparable>)[] GetSkillSorts() => new (string, System.Func<ActiveSkillJsonData, System.IComparable>)[]
    {
        ("ID", e => e.SkillID),
        ("Name", e => e.SkillName ?? ""),
        ("Element", e => e.SkillElement ?? ""),
        ("Base Damage", e => e.BaseDamage),
        ("Mana Cost", e => e.BaseManaCost),
        ("Cooldown", e => e.BaseCooldown),
        ("Cast Time (VCT)", e => e.BaseVCT),
        ("AOE Radius", e => e.BaseAOERadius)
    };

    private void DrawSupportFilters()
    {
        DrawFilterChips(IconFilters, ref _supportIconFilter);
    }

    private bool SupportFilter(SupportSkillJsonData e)
    {
        if (_supportIconFilter == 0) return true;
        bool hasIcon = !string.IsNullOrEmpty(e.IconPath);
        return _supportIconFilter == 1 ? hasIcon : !hasIcon;
    }

    private (string, System.Func<SupportSkillJsonData, System.IComparable>)[] GetSupportSorts() => new (string, System.Func<SupportSkillJsonData, System.IComparable>)[]
    {
        ("ID", e => e.SkillID),
        ("Name", e => e.ModifierName ?? "")
    };

    private void DrawPassiveFilters()
    {
        DrawFilterChips(DescriptionFilters, ref _passiveDescriptionFilter);
    }

    private bool PassiveFilter(UniquePassiveEntry e)
    {
        if (_passiveDescriptionFilter == 0) return true;
        bool has = !string.IsNullOrWhiteSpace(e.Description);
        return _passiveDescriptionFilter == 1 ? has : !has;
    }

    private (string, System.Func<UniquePassiveEntry, System.IComparable>)[] GetPassiveSorts() => new (string, System.Func<UniquePassiveEntry, System.IComparable>)[]
    {
        ("ID", e => e.Id ?? ""),
        ("Name", e => e.Name ?? "")
    };
    #endregion
}
