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

public partial class GameDataWorkbench : EditorWindow
{
    #region [Variables/Fields]
    private enum TabCategory { Overview, Data, Combat, System }
    private enum DataSubTab { Attributes, Items, Sets, Characters, Factions }
    private enum CombatSubTab { Skills, SupportSkills, UniquePassives }
    private enum SystemSubTab { IdManager }

    private TabCategory _category = TabCategory.Overview;
    private DataSubTab _dataTab = DataSubTab.Attributes;
    private CombatSubTab _combatTab = CombatSubTab.Skills;
    private SystemSubTab _systemTab = SystemSubTab.IdManager;

    private ToolTab _currentTab = ToolTab.Dashboard;
    private enum ToolTab
    {
        Dashboard,
        Attributes,
        Items,
        Skills,
        SupportSkills,
        Characters,
        IdManager,
        Sets,
        UniquePassives,
        Factions
    }

    private string _itemJsonPath = "Assets/Resources/Data/Item.json";
    private string _attributeJsonPath = "Assets/Resources/Data/Attribute.json";
    private string _skillJsonPath = "Assets/Resources/Data/Skill.json";
    private string _supportSkillJsonPath = "Assets/Resources/Data/SupportSkill.json";
    private string _characterJsonPath = "Assets/Resources/Data/Character.json";
    private string _setJsonPath = "Assets/Resources/Data/ItemSet.json";
    private string _uniquePassiveJsonPath = "Assets/Resources/Data/UniquePassive.json";
    private string _factionJsonPath = "Assets/Resources/Data/Faction.json";

    private AttributeDataList _attributeData;
    private ItemDataList _itemData;
    private SkillDataList _skillData;
    private SupportModifierDataList _supportSkillData;
    private CharacterDataList _characterData;
    private ItemSetList _itemSetData;
    private UniquePassiveDataList _uniquePassiveData;
    private FactionList _factionData;

    private int _itemSelectedIndex = -1;
    private int _skillSelectedIndex = -1;
    private int _supportSelectedIndex = -1;
    private int _attributeSelectedIndex = -1;
    private int _itemSetSelectedIndex = -1;
    private int _characterSelectedIndex = -1;
    private int _uniquePassiveSelectedIndex = -1;

    private string _searchQuery = "";
    private Vector2 _listScroll;
    private Vector2 _editorScroll;
    private Vector2 _contentScroll;
    
    private int _itemTypeFilterIndex = 0;
    private string[] _itemTypeNames;
    private int _skillElementFilterIndex = 0;
    private string[] _skillElementFilterNames;
    private string[] _tabNamesForItems = { "All", "Currency", "Consumable", "Equip", "Frag", "Res", "SkillRune", "SupportRune", "Cos", "Add" };
    private bool _showOnlyWithEffects = false;
    
    private Dictionary<string, int> _itemPrefixes = new Dictionary<string, int> {
        { "Currency", 1 }, { "Consumable", 2 }, { "Equipment", 3 }, { "Fragment", 4 },
        { "Resource", 5 }, { "SkillRune", 6 }, { "SupportRune", 7 }, { "Costume", 8 }, { "Additional", 9 }
    };

    private Dictionary<string, int> _startIds = new Dictionary<string, int> {
        { "Attributes", 1 },
        { "Active Skills", 101 },
        { "Support Mods", 1 },
        { "Characters", 1 }
    };

    private float _previewZoom = 20f;
    private Vector2 _previewOffset = Vector2.zero;
    private Vector2 _previewTargetPoint = Vector2.zero;
    private int _simSkillLevel = 1;
    private Dictionary<int, int> _mockStats = new Dictionary<int, int>();
    private bool _showSimulator = true;

    private List<System.Type> _modifierTypes = new List<System.Type>();
    private string[] _modifierTypeNames;
    
    private bool _useManualAttributeInput = false;
    private bool _isValidated = false;
    private List<string> _validationErrors = new List<string>();

    private static readonly Color CategoryOverview = new Color(0.55f, 0.75f, 0.95f);
    private static readonly Color CategoryData     = new Color(0.45f, 0.70f, 1.0f);
    private static readonly Color CategoryCombat   = new Color(1.0f, 0.45f, 0.45f);
    private static readonly Color CategorySystem   = new Color(0.40f, 0.85f, 0.55f);
    #endregion

    #region [Unity Methods]
    [MenuItem("Window/Realm of Hero/Game Data Workbench")]
    public static void ShowWindow()
    {
        GetWindow<GameDataWorkbench>("Game Data Workbench");
    }

    private void OnEnable()
    {
        LoadAllData();
        InitializeFilters();
        DiscoverModifiers();
    }

    private void OnGUI()
    {
        DrawHeaderBanner();
        DrawCategoryToolbar();
        DrawSubTabToolbar();

        switch (_currentTab)
        {
            case ToolTab.Dashboard:       DrawDashboard(); break;
            case ToolTab.Attributes:      DrawAttributeEditor(); break;
            case ToolTab.Items:           DrawItemEditor(); break;
            case ToolTab.Skills:          DrawSkillEditor(); break;
            case ToolTab.SupportSkills:   DrawSupportSkillEditor(); break;
            case ToolTab.Characters:      DrawCharacterEditor(); break;
            case ToolTab.Factions:        DrawFactionEditor(); break;
            case ToolTab.IdManager:       DrawIdManager(); break;
            case ToolTab.Sets:            DrawItemSetEditor(); break;
            case ToolTab.UniquePassives:  DrawUniquePassiveEditor(); break;
        }
    }
    #endregion

    #region [Private Methods]
    private void DrawHeaderBanner()
    {
        Rect bannerRect = EditorGUILayout.GetControlRect(false, 32);
        EditorGUI.DrawRect(bannerRect, new Color(0.14f, 0.14f, 0.18f));

        Rect accentLine = new Rect(bannerRect.x, bannerRect.yMax - 2, bannerRect.width, 2);
        EditorGUI.DrawRect(accentLine, CategoryData);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel) {
            fontSize = 13,
            normal = { textColor = new Color(0.85f, 0.90f, 1.0f) }
        };
        GUI.Label(new Rect(bannerRect.x + 12, bannerRect.y + 4, 250, 24), "Game Data Workbench", titleStyle);

        int totalEntries = 0;
        if (_attributeData?.Attributes != null) totalEntries += _attributeData.Attributes.Count;
        if (_itemData?.Items != null) totalEntries += _itemData.Items.Count;
        if (_skillData?.ActiveSkills != null) totalEntries += _skillData.ActiveSkills.Count;
        if (_supportSkillData?.SupportModifiers != null) totalEntries += _supportSkillData.SupportModifiers.Count;
        if (_characterData?.Characters != null) totalEntries += _characterData.Characters.Count;

        GUIStyle infoStyle = new GUIStyle(EditorStyles.miniLabel) {
            normal = { textColor = new Color(0.6f, 0.65f, 0.7f) },
            alignment = TextAnchor.MiddleRight
        };
        GUI.Label(new Rect(bannerRect.xMax - 330, bannerRect.y + 4, 160, 24), $"{totalEntries} entries loaded", infoStyle);

        float btnX = bannerRect.xMax - 160;
        GUIStyle btnStyle = new GUIStyle(EditorStyles.miniButton) { fixedHeight = 22 };
        if (GUI.Button(new Rect(btnX, bannerRect.y + 5, 74, 22), "Reload", btnStyle)) LoadAllData();
        if (GUI.Button(new Rect(btnX + 78, bannerRect.y + 5, 74, 22), "Save", btnStyle)) SaveAllData();
    }

    private void DrawCategoryToolbar()
    {
        Rect catRect = EditorGUILayout.GetControlRect(false, 28);
        EditorGUI.DrawRect(catRect, new Color(0.18f, 0.18f, 0.22f));

        float btnW = catRect.width / 4f;
        string[] catNames = { "Overview", "Data", "Combat", "System" };
        Color[] catColors = { CategoryOverview, CategoryData, CategoryCombat, CategorySystem };

        for (int i = 0; i < 4; i++)
        {
            Rect btnRect = new Rect(catRect.x + btnW * i, catRect.y, btnW, catRect.height);
            bool isActive = (int)_category == i;

            if (isActive)
            {
                EditorGUI.DrawRect(btnRect, new Color(catColors[i].r, catColors[i].g, catColors[i].b, 0.15f));
                Rect underline = new Rect(btnRect.x + 4, btnRect.yMax - 3, btnRect.width - 8, 3);
                EditorGUI.DrawRect(underline, catColors[i]);
            }

            GUIStyle catStyle = new GUIStyle(EditorStyles.miniLabel) {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal,
                normal = { textColor = isActive ? Color.white : new Color(0.65f, 0.65f, 0.70f) }
            };

            if (GUI.Button(btnRect, catNames[i], catStyle))
            {
                _category = (TabCategory)i;
                SyncTabFromCategory();
            }
        }
    }

    private void DrawSubTabToolbar()
    {
        if (_category == TabCategory.Overview) return;

        string[] subNames;
        int selectedSub;

        switch (_category)
        {
            case TabCategory.Data:
                subNames = GetDataSubTabNames();
                selectedSub = (int)_dataTab;
                break;
            case TabCategory.Combat:
                subNames = GetCombatSubTabNames();
                selectedSub = (int)_combatTab;
                break;
            case TabCategory.System:
                subNames = GetSystemSubTabNames();
                selectedSub = (int)_systemTab;
                break;
            default: return;
        }

        Rect subRect = EditorGUILayout.GetControlRect(false, 24);
        EditorGUI.DrawRect(subRect, new Color(0.20f, 0.20f, 0.24f));

        int newSub = GUI.Toolbar(subRect, selectedSub, subNames, EditorStyles.toolbarButton);

        if (newSub != selectedSub)
        {
            switch (_category)
            {
                case TabCategory.Data:   _dataTab = (DataSubTab)newSub; break;
                case TabCategory.Combat: _combatTab = (CombatSubTab)newSub; break;
                case TabCategory.System: _systemTab = (SystemSubTab)newSub; break;
            }
        }

        SyncTabFromCategory();
    }

    private string[] GetDataSubTabNames()
    {
        int attrCount = _attributeData?.Attributes?.Count ?? 0;
        int itemCount = _itemData?.Items?.Count ?? 0;
        int setCount  = _itemSetData?.Sets?.Count ?? 0;
        int charCount = _characterData?.Characters?.Count ?? 0;
        int factionCount = _factionData?.Factions?.Count ?? 0;
        return new[] { $"Attributes ({attrCount})", $"Items ({itemCount})", $"Sets ({setCount})", $"Characters ({charCount})", $"Factions ({factionCount})" };
    }

    private string[] GetCombatSubTabNames()
    {
        int skillCount   = _skillData?.ActiveSkills?.Count ?? 0;
        int supportCount = _supportSkillData?.SupportModifiers?.Count ?? 0;
        int passiveCount = _uniquePassiveData?.UniquePassives?.Count ?? 0;
        return new[] { $"Skills ({skillCount})", $"Support Skills ({supportCount})", $"Unique Passives ({passiveCount})" };
    }

    private string[] GetSystemSubTabNames()
    {
        return new[] { "ID Manager" };
    }

    private void SyncTabFromCategory()
    {
        switch (_category)
        {
            case TabCategory.Overview:
                _currentTab = ToolTab.Dashboard;
                break;
            case TabCategory.Data:
                switch (_dataTab)
                {
                    case DataSubTab.Attributes:  _currentTab = ToolTab.Attributes; break;
                    case DataSubTab.Items:        _currentTab = ToolTab.Items; break;
                    case DataSubTab.Sets:         _currentTab = ToolTab.Sets; break;
                    case DataSubTab.Characters:   _currentTab = ToolTab.Characters; break;
                    case DataSubTab.Factions:     _currentTab = ToolTab.Factions; break;
                }
                break;
            case TabCategory.Combat:
                switch (_combatTab)
                {
                    case CombatSubTab.Skills:         _currentTab = ToolTab.Skills; break;
                    case CombatSubTab.SupportSkills:   _currentTab = ToolTab.SupportSkills; break;
                    case CombatSubTab.UniquePassives:   _currentTab = ToolTab.UniquePassives; break;
                }
                break;
            case TabCategory.System:
                _currentTab = ToolTab.IdManager;
                break;
        }
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
        _uniquePassiveData = LoadData<UniquePassiveDataList>(_uniquePassiveJsonPath) ?? new UniquePassiveDataList { UniquePassives = new List<UniquePassiveEntry>() };
        _factionData = LoadData<FactionList>(_factionJsonPath) ?? new FactionList { Factions = new List<FactionEntry>() };
    }

    private void SaveAllData()
    {
        if (_itemData != null && _itemData.Items != null)
        {
            foreach (var item in _itemData.Items)
            {
                if (item.Effects != null && item.Effects.Count == 0) item.Effects = null;
                if (item.VisualSpritePaths != null && item.VisualSpritePaths.Count == 0) item.VisualSpritePaths = null;
            }
        }

        SaveData(_attributeJsonPath, _attributeData);
        SaveData(_itemJsonPath, _itemData);
        SaveData(_skillJsonPath, _skillData);
        SaveData(_supportSkillJsonPath, _supportSkillData);
        SaveData(_characterJsonPath, _characterData);
        SaveData(_setJsonPath, _itemSetData);
        SaveData(_uniquePassiveJsonPath, _uniquePassiveData);
        SaveData(_factionJsonPath, _factionData);
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

    public static Sprite ResolveEditorSprite(string addressableKeyOrPath)
    {
        if (string.IsNullOrEmpty(addressableKeyOrPath)) return null;

#if UNITY_EDITOR
        var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
        if (settings != null)
        {
            foreach (var group in settings.groups)
            {
                if (group == null) continue;
                foreach (var entry in group.entries)
                {
                    if (entry.address == addressableKeyOrPath)
                    {
                        return AssetDatabase.LoadAssetAtPath<Sprite>(entry.AssetPath);
                    }
                }
            }
        }
#endif
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(addressableKeyOrPath);
        if (sprite != null) return sprite;
        return Resources.Load<Sprite>(addressableKeyOrPath);
    }

    public static string GetAddressableKey(UnityEngine.Object obj)
    {
        if (obj == null) return "";
        string assetPath = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(assetPath)) return "";

#if UNITY_EDITOR
        var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
        if (settings != null)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = settings.FindAssetEntry(guid);
            if (entry != null) return entry.address;
        }
#endif
        Debug.LogWarning($"[GameDataWorkbench] '{obj.name}' is not explicitly registered (may be implicitly via folder). Using full path as key: '{assetPath}'.");
        return assetPath;
    }

    private Sprite ResolveResourceSprite(string path)
    {
        return ResolveEditorSprite(path);
    }

    private string GetResourcePath(UnityEngine.Object obj)
    {
        if (obj == null) return "";
        string assetPath = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(assetPath)) return "";

        assetPath = assetPath.Replace('\\', '/');
        
        int resIndex = assetPath.IndexOf("/Resources/");
        if (resIndex == -1) 
        {
            Debug.LogWarning($"Object '{obj.name}' is not in a Resources folder.");
            return "";
        }

        string resPath = assetPath.Substring(resIndex + 11);
        int extIndex = resPath.LastIndexOf(".");
        if (extIndex != -1) resPath = resPath.Substring(0, extIndex);

        if (obj is Sprite sprite)
        {
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            if (AssetDatabase.IsMainAsset(sprite) || sprite.name == fileName) return resPath;
            
            return $"{resPath}:{sprite.name}";
        }
        
        return resPath;
    }
    #endregion
}
