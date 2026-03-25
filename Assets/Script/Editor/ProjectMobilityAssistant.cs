using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using CoreSystem;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Attributes;
using CoreSystem.Data.Skills;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Logic.Effects;
using CoreSystem.Skills.Modifiers;
using CoreSystem.Skills.Visuals;
using UnityEditor.IMGUI.Controls;

public partial class ProjectMobilityAssistant : EditorWindow
{
    private enum ToolTab
    {
        Dashboard,
        Attributes,
        Items,
        Skills,
        SupportSkills,
        Characters,
        IdManager,
        Sets
    }

    private ToolTab _currentTab = ToolTab.Dashboard;
    private string[] _tabNames = { "Dashboard", "Attributes", "Items", "Skills", "Support Skills", "Characters", "ID Manager", "Item Sets" };

    // JSON Paths
    private string _itemJsonPath = "Assets/Resources/Data/Item.json";
    private string _attributeJsonPath = "Assets/Resources/Data/Attribute.json";
    private string _skillJsonPath = "Assets/Resources/Data/Skill.json";
    private string _supportSkillJsonPath = "Assets/Resources/Data/SupportSkill.json";
    private string _characterJsonPath = "Assets/Resources/Data/Character.json";
    private string _setJsonPath = "Assets/Resources/Data/ItemSet.json";

    // Data Lists
    private AttributeDataList _attributeData;
    private ItemDataList _itemData;
    private SkillDataList _skillData;
    private SupportModifierDataList _supportSkillData;
    private CharacterDataList _characterData;
    private ItemSetList _itemSetData;

    // --- Selection State (Per-Tab) ---
    private int _itemSelectedIndex = -1;
    private int _skillSelectedIndex = -1;
    private int _supportSelectedIndex = -1;
    private int _attributeSelectedIndex = -1;
    private int _itemSetSelectedIndex = -1;
    private int _characterSelectedIndex = -1;

    // --- Common UI State ---
    private string _searchQuery = "";
    private Vector2 _listScroll;
    private Vector2 _editorScroll;
    private Vector2 _contentScroll;
    
    // --- Filters ---
    private int _itemTypeFilterIndex = 0;
    private string[] _itemTypeNames;
    private int _skillElementFilterIndex = 0;
    private string[] _skillElementFilterNames;
    private string[] _tabNamesForItems = { "All", "Currency", "Consumable", "Equip", "Frag", "Res", "Rune", "Cos" };
    
    private Dictionary<string, int> _itemPrefixes = new Dictionary<string, int> {
        { "Currency", 1 }, { "Consumable", 2 }, { "Equipment", 3 }, { "Fragment", 4 },
        { "Resource", 5 }, { "SkillRune", 6 }, { "Costume", 8 }
    };

    // --- ID Manager State ---
    private Dictionary<string, int> _startIds = new Dictionary<string, int> {
        { "Attributes", 1 },
        { "Active Skills", 101 },
        { "Support Mods", 1 },
        { "Characters", 1 }
    };

    // --- Skill Preview / Simulator ---
    private float _previewZoom = 20f;
    private Vector2 _previewOffset = Vector2.zero;
    private Vector2 _previewTargetPoint = Vector2.zero;
    private int _simSkillLevel = 1;
    private Dictionary<int, int> _mockStats = new Dictionary<int, int>();
    private bool _showSimulator = true;

    // --- Discovery ---
    private List<System.Type> _modifierTypes = new List<System.Type>();
    private string[] _modifierTypeNames;
    
    // --- Other Flags ---
    private bool _useManualAttributeInput = false;
    private bool _isValidated = false;
    private List<string> _validationErrors = new List<string>();

    [MenuItem("Window/Realm of Hero/Mobility Assistant")]
    public static void ShowWindow()
    {
        GetWindow<ProjectMobilityAssistant>("Mobility Assistant");
    }

    private void OnEnable()
    {
        LoadAllData();
        InitializeFilters();
        DiscoverModifiers();
    }

    private void DiscoverModifiers()
    {
        _modifierTypes = TypeCache.GetTypesDerivedFrom<SupportModifier>().ToList();
        _modifierTypeNames = _modifierTypes.Select(t => t.Name).ToArray();
    }

    private void InitializeFilters()
    {
        var types = System.Enum.GetNames(typeof(ItemType)).ToList();
        types.Insert(0, "All");
        _itemTypeNames = types.ToArray();

        var elements = System.Enum.GetNames(typeof(ElementType)).ToList();
        elements.Insert(0, "All");
        _skillElementFilterNames = elements.ToArray();
    }

    private void LoadAllData()
    {
        _attributeData = LoadData<AttributeDataList>(_attributeJsonPath) ?? new AttributeDataList { Attributes = new List<AttributeEntry>() };
        _itemData = LoadData<ItemDataList>(_itemJsonPath) ?? new ItemDataList { Items = new List<ItemEntry>() };
        _skillData = LoadData<SkillDataList>(_skillJsonPath) ?? new SkillDataList { ActiveSkills = new List<ActiveSkillJsonData>(), SupportSkills = new List<SupportSkillJsonData>() };
        _supportSkillData = LoadData<SupportModifierDataList>(_supportSkillJsonPath) ?? new SupportModifierDataList { SupportModifiers = new List<SupportSkillJsonData>() };
        _characterData = LoadData<CharacterDataList>(_characterJsonPath) ?? new CharacterDataList { Characters = new List<CharacterModel>() };
        _itemSetData = LoadData<ItemSetList>(_setJsonPath) ?? new ItemSetList { Sets = new List<ItemSetEntry>() };
    }

    private void SaveAllData()
    {
        SaveData(_attributeJsonPath, _attributeData);
        SaveData(_itemJsonPath, _itemData);
        SaveData(_skillJsonPath, _skillData);
        SaveData(_supportSkillJsonPath, _supportSkillData);
        SaveData(_characterJsonPath, _characterData);
        SaveData(_setJsonPath, _itemSetData);
        AssetDatabase.Refresh();
    }

    private T LoadData<T>(string path) where T : class
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        return null;
    }

    private void SaveData<T>(string path, T data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    private void OnGUI()
    {
        // ── Toolbar ──────────────────────────────────────────────────────────
        Rect toolbarRect = EditorGUILayout.GetControlRect(false, 28);
        GUI.Box(toolbarRect, GUIContent.none, EditorStyles.toolbar);

        float tabWidth = toolbarRect.width - 160;
        Rect tabsRect = new Rect(toolbarRect.x, toolbarRect.y, tabWidth, toolbarRect.height);
        _currentTab = (ToolTab)GUI.Toolbar(tabsRect, (int)_currentTab, _tabNames, EditorStyles.toolbarButton);

        float btnX = toolbarRect.xMax - 155;
        if (GUI.Button(new Rect(btnX, toolbarRect.y, 74, toolbarRect.height), "↺ Reload", EditorStyles.toolbarButton)) LoadAllData();
        if (GUI.Button(new Rect(btnX + 76, toolbarRect.y, 74, toolbarRect.height), "💾 Save", EditorStyles.toolbarButton)) SaveAllData();

        switch (_currentTab)
        {
            case ToolTab.Dashboard:     DrawDashboard(); break;
            case ToolTab.Attributes:    DrawAttributeEditor(); break;
            case ToolTab.Items:         DrawItemEditor(); break;
            case ToolTab.Skills:        DrawSkillEditor(); break;
            case ToolTab.SupportSkills: DrawSupportSkillEditor(); break;
            case ToolTab.Characters:    DrawCharacterEditor(); break;
            case ToolTab.IdManager:     DrawIdManager(); break;
            case ToolTab.Sets:          DrawItemSetEditor(); break;
        }
    }

    private Sprite ResolveResourceSprite(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        return Resources.Load<Sprite>(path);
    }
}
