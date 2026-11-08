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
    [SerializeField] private ToolTab _currentTab = ToolTab.Dashboard;
    private bool _isDirty;
    private readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
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
        Factions,
        Experience
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

    [SerializeField] private int _itemSelectedIndex = -1;
    [SerializeField] private int _skillSelectedIndex = -1;
    [SerializeField] private int _supportSelectedIndex = -1;
    [SerializeField] private int _attributeSelectedIndex = -1;
    [SerializeField] private int _itemSetSelectedIndex = -1;
    [SerializeField] private int _characterSelectedIndex = -1;
    [SerializeField] private int _uniquePassiveSelectedIndex = -1;

    private string _searchQuery = "";
    private Vector2 _listScroll;
    private Vector2 _editorScroll;
    private Vector2 _contentScroll;
    
    private int _itemTypeFilterIndex = 0;
    private string[] _itemTypeNames;
    private int _skillElementFilterIndex = 0;
    private string[] _skillElementFilterNames;
    private string[] _tabNamesForItems = { "All", "Currency", "Consumable", "Equip", "Frag", "Res", "SkillRune", "SupportRune", "Cos", "Add", "Prog" };
    private bool _showOnlyWithEffects = false;
    
    private Dictionary<string, int> _itemPrefixes = new Dictionary<string, int> {
        { "Currency", 1 }, { "Consumable", 2 }, { "Equipment", 3 }, { "Fragment", 4 },
        { "Resource", 5 }, { "SkillRune", 6 }, { "SupportRune", 7 }, { "Costume", 8 }, { "Additional", 9 }, { "Progression", 10 }
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

    private List<System.Type> _modifierTypes = new List<System.Type>();
    private string[] _modifierTypeNames;
    
    private bool _useManualAttributeInput = false;
    private bool _isValidated = false;
    private List<string> _validationErrors = new List<string>();
    #endregion

    #region [Unity Methods]
    [MenuItem("Window/Pale Vertica/Pale Vertica GameDataWorkbench")]
    public static void ShowWindow()
    {
        GetWindow<GameDataWorkbench>("Pale Vertica GameDataWorkbench");
    }

    private void OnEnable()
    {
        minSize = new Vector2(980, 560);
        wantsMouseMove = true;
        InitializeFilters();
        DiscoverModifiers();
        LoadAllData();
    }

    private void OnGUI()
    {
        HandleShortcuts();
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), ThemeWindow);
        DrawHeaderBanner(new Rect(0, 0, position.width, BannerHeight));

        Rect body = new Rect(0, BannerHeight, position.width, position.height - BannerHeight);
        DrawNavigator(new Rect(body.x, body.y, NavigatorWidth, body.height));

        GUILayout.BeginArea(new Rect(body.x + NavigatorWidth, body.y, body.width - NavigatorWidth, body.height));
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
            case ToolTab.Experience:      DrawExperienceEditor(); break;
        }
        GUILayout.EndArea();

        if (Event.current.type == EventType.MouseMove) Repaint();
    }
    #endregion

    #region [Private Methods]
    private void DrawHeaderBanner(Rect rect)
    {
        EditorGUI.DrawRect(rect, ThemeBanner);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), ThemeLine);
        GUI.Label(new Rect(rect.x + 16, rect.y, 260, rect.height), "Pale Vertica GameDataWorkbench", TitleStyle);

        float x = rect.xMax - 16;
        Rect save = new Rect(x - 70, rect.y + 8, 70, 24);
        Color prevBg = GUI.backgroundColor;
        GUI.backgroundColor = ThemePrimary * 1.6f;
        if (GUI.Button(save, new GUIContent("Save", "Save all tables to JSON (Ctrl+S)"))) SaveAllData();
        GUI.backgroundColor = prevBg;
        x = save.x - 6;

        Rect reload = new Rect(x - 70, rect.y + 8, 70, 24);
        if (GUI.Button(reload, new GUIContent("Reload", "Reload all tables from JSON")))
        {
            if (!_isDirty || EditorUtility.DisplayDialog("Reload Data", "You have unsaved changes. Reload and lose them?", "Reload", "Cancel"))
                LoadAllData();
        }
        x = reload.x - 10;

        if (_isDirty)
        {
            const string unsaved = "Unsaved changes";
            float w = ChipWidth(unsaved);
            DrawChip(new Rect(x - w, rect.y + 11, w, 18), unsaved, ThemeWarn);
            x -= w + 10;
        }

        GUI.Label(new Rect(x - 220, rect.y, 220, rect.height), $"{GetTotalEntries()} entries loaded", MutedRightStyle);
    }

    private void HandleShortcuts()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.S && (e.control || e.command))
        {
            SaveAllData();
            e.Use();
        }
    }

    private int GetTotalEntries()
    {
        int total = 0;
        foreach (ToolTab tab in System.Enum.GetValues(typeof(ToolTab)))
        {
            int count = GetTabCount(tab);
            if (count > 0) total += count;
        }
        return total;
    }

    private void MarkDirty()
    {
        _isDirty = true;
    }

    private Sprite GetCachedSprite(string keyOrPath)
    {
        if (string.IsNullOrEmpty(keyOrPath)) return null;
        if (_spriteCache.TryGetValue(keyOrPath, out Sprite sprite)) return sprite;
        sprite = ResolveEditorSprite(keyOrPath);
        _spriteCache[keyOrPath] = sprite;
        return sprite;
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
        _spriteCache.Clear();
        _isDirty = false;
        RunValidation(false);
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
        _isDirty = false;
        RunValidation(false);
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
