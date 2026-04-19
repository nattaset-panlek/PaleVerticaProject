using UnityEngine;
using CoreSystem;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Attributes;
using System.Collections.Generic;
using CoreSystem.Data.Skills;
using CoreSystem.Skills;
using CoreSystem.Skills.Modifiers;
using CoreSystem.SkillTree;

namespace CoreSystem.Data
{
    public class Database : MonoBehaviour
    {
        private static Database _instance;

        public static Database Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Database>();
                    
                    if (_instance == null)
                    {
                        var prefab = Resources.Load<GameObject>("Prefab/Database");
                        if (prefab != null)
                        {
                            var go = Instantiate(prefab);
                            go.name = "Database";
                            _instance = go.GetComponent<Database>();
                        }
                        else
                        {
                            var go = new GameObject("Database");
                            _instance = go.AddComponent<Database>();
                            Debug.LogWarning("Database prefab not found in Resources/Prefab/. A temporary GameObject was created.");
                        }
                        if (Application.isPlaying)
                        {
                            DontDestroyOnLoad(_instance.gameObject);
                        }
                    }
                }

                if (_instance != null && !_instance._isInitialized)
                {
                    _instance.Initialize();
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
                Initialize();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private Dictionary<int, CharacterModel> _characterDatabase = new Dictionary<int, CharacterModel>();
        
        private Dictionary<int, ActiveSkillJsonData> _activeSkillJsonData = new Dictionary<int, ActiveSkillJsonData>();
        private Dictionary<int, SupportSkillJsonData> _supportSkillJsonData = new Dictionary<int, SupportSkillJsonData>();
        private Dictionary<int, ActiveSkill> _activeSkillAssets = new Dictionary<int, ActiveSkill>();
        private Dictionary<int, SupportModifier> _supportSkillAssets = new Dictionary<int, SupportModifier>();

        private bool _isInitialized = false;

        [ContextMenu("Initialize Load")]
        public void Initialize()
        {
            if (_isInitialized) return;

            // 1. Initialize Items
            Debug.Log("Database: Initializing ItemDatabase...");
            ItemDatabase.Initialize();

            // 2. Initialize Attributes
            Debug.Log("Database: Initializing AttributeDatabase...");
            AttributeDatabase.Initialize();

            // 3. Initialize Skills
            Debug.Log("Database: Initializing Skills...");
            InitializeSkills();

            // 4. Initialize SkillTree
            Debug.Log("Database: Initializing SkillTree...");
            SkillTreeDatabase.Initialize();

            // 5. Initialize Characters (Placeholder loading)
            Debug.Log("Database: Loading Characters...");
            LoadCharacters();

            _isInitialized = true;
            Debug.Log("Global Database Initialized.");
        }


        private void LoadCharacters()
        {
            _characterDatabase.Clear();
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/Character");
            if (jsonFile != null)
            {
                Debug.Log($"Database: Parsing Character.json ({jsonFile.text.Length} chars)");
                CharacterDataList dataList = JsonUtility.FromJson<CharacterDataList>(jsonFile.text);
                if (dataList != null && dataList.Characters != null)
                {
                    foreach (var character in dataList.Characters)
                    {
                        _characterDatabase[character.Id] = character;
                    }
                    Debug.Log($"Database: Loaded {_characterDatabase.Count} characters.");
                }
                else
                {
                    Debug.LogError("Database: Failed to parse Character.json or Characters list is null.");
                }
            }
            else
            {
                Debug.LogError("Database: Character.json not found in Resources/Data/");
            }
        }

        private void InitializeSkills()
        {
            _activeSkillJsonData.Clear();
            _supportSkillJsonData.Clear();
            _activeSkillAssets.Clear();
            _supportSkillAssets.Clear();

            // Load JSON Data
            TextAsset skillJsonFile = Resources.Load<TextAsset>("Data/Skill");
            if (skillJsonFile != null)
            {
                SkillDataList dataList = JsonUtility.FromJson<SkillDataList>(skillJsonFile.text);
                if (dataList != null)
                {
                    if (dataList.ActiveSkills != null)
                    {
                        foreach (var data in dataList.ActiveSkills)
                            _activeSkillJsonData[data.SkillID] = data;
                    }
                    if (dataList.SupportSkills != null)
                    {
                        foreach (var data in dataList.SupportSkills)
                            _supportSkillJsonData[data.SkillID] = data;
                    }
                }
            }

            // Load SupportSkill JSON Data (if it exists separately)
            TextAsset supportSkillJsonFile = Resources.Load<TextAsset>("Data/SupportSkill");
            if (supportSkillJsonFile != null)
            {
                SupportModifierDataList supportDataList = JsonUtility.FromJson<SupportModifierDataList>(supportSkillJsonFile.text);
                if (supportDataList != null && supportDataList.SupportModifiers != null)
                {
                    foreach (var data in supportDataList.SupportModifiers)
                        _supportSkillJsonData[data.SkillID] = data;
                }
            }

            // Load ActiveSkill Assets
            ActiveSkill[] activeAssets = Resources.LoadAll<ActiveSkill>("Skill/ActiveSkill");
            foreach (var asset in activeAssets)
            {
                _activeSkillAssets[asset.Id] = asset;
            }

            // Load SupportSkill Assets
            SupportModifier[] supportAssets = Resources.LoadAll<SupportModifier>("Skill/SupportSkill");
            foreach (var asset in supportAssets)
            {
                _supportSkillAssets[asset.ID] = asset;
            }
        }

        public ActiveSkill GetSynchronizedActiveSkill(int id)
        {
            if (!_activeSkillAssets.TryGetValue(id, out ActiveSkill templateAsset))
            {
                return null;
            }

            // Clone the asset safely
            ActiveSkill clonedSkill = ScriptableObject.Instantiate(templateAsset);
            clonedSkill.name = templateAsset.name; // Keep naming clean

            // Merge JSON Data if exists
            if (_activeSkillJsonData.TryGetValue(id, out ActiveSkillJsonData jsonData))
            {
                clonedSkill.Name = jsonData.SkillName;
                if (System.Enum.TryParse(jsonData.SkillElement, out ElementType element))
                {
                    clonedSkill.SkillElement = element;
                }

                if (System.Enum.TryParse(jsonData.DamageType, out DamageType damageType))
                {
                    clonedSkill.SkillDamageType = damageType;
                }
                
                clonedSkill.BaseDamage = jsonData.BaseDamage;
                clonedSkill.BaseManaCost = jsonData.BaseManaCost;
                clonedSkill.BaseVCT = jsonData.BaseVCT;
                clonedSkill.BaseFCT = jsonData.BaseFCT;
                clonedSkill.BaseCastDelay = jsonData.BaseCastDelay;
                clonedSkill.BaseCooldown = jsonData.BaseCooldown;
                clonedSkill.BaseProjectileCount = jsonData.BaseProjectileCount;
                clonedSkill.IsToggle = jsonData.IsToggle;
                
                if (jsonData.BaseTargeting != null)
                {
                    clonedSkill.BaseAOERadius = jsonData.BaseTargeting.Radius;
                }

                if (jsonData.Scalings != null && jsonData.Scalings.Count > 0)
                {
                    clonedSkill.Scalings = new List<OutputScaling>(jsonData.Scalings);
                }
                
                // Inject Heal value into inner HealEffect without breaking structure
                if (jsonData.Heal > 0 && clonedSkill.Effects != null)
                {
                    foreach (var effect in clonedSkill.Effects)
                    {
                        if (effect is CoreSystem.Skills.Logic.HealEffect healEffect)
                        {
                            healEffect.BaseHealAmount = jsonData.Heal;
                        }
                    }
                }
            }

            return clonedSkill;
        }

        public SupportModifier GetSynchronizedSupportModifier(int id)
        {
            if (!_supportSkillAssets.TryGetValue(id, out SupportModifier templateAsset))
            {
                return null;
            }

            // Clone the asset safely
            SupportModifier clonedModifier = ScriptableObject.Instantiate(templateAsset);
            clonedModifier.name = templateAsset.name;

            // Merge JSON Data if exists
            if (_supportSkillJsonData.TryGetValue(id, out SupportSkillJsonData jsonData))
            {
                if (clonedModifier is GenericSupportModifier genericMod)
                {
                    genericMod.ModifierName = jsonData.ModifierName;
                    
                    if (jsonData.Effects != null)
                    {
                        genericMod.Effects.Clear();
                        foreach (var eData in jsonData.Effects)
                        {
                            var effect = new SupportEffect
                            {
                                FlatValue = eData.FlatValue,
                                PercentValue = eData.PercentValue,
                                UsePayloadReference = eData.UsePayloadReference,
                                PayloadMultiplier = eData.PayloadMultiplier,
                                UseAttributeReference = eData.UseAttributeReference,
                                AttributeMultiplier = eData.AttributeMultiplier
                            };
                            
                            if (System.Enum.TryParse(eData.TargetField, out PayloadTarget target)) effect.TargetField = target;
                            if (System.Enum.TryParse(eData.Operation, out ModifierOperation op)) effect.Operation = op;
                            if (System.Enum.TryParse(eData.ReferencePayloadField, out PayloadTarget refP)) effect.ReferencePayloadField = refP;
                            if (System.Enum.TryParse(eData.ReferenceAttribute, out AttributeType attr)) effect.ReferenceAttribute = attr;
                            
                            genericMod.Effects.Add(effect);
                        }
                    }

                    if (jsonData.Conditions != null)
                    {
                        genericMod.Conditions.Clear();
                        foreach (var cData in jsonData.Conditions)
                        {
                            var cond = new SupportCondition();
                            if (System.Enum.TryParse(cData.Type, out SupportConditionType cType)) cond.Type = cType;
                            if (System.Enum.TryParse(cData.RequiredElement, out ElementType el)) cond.RequiredElement = el;
                            cond.RequiredSkillID = cData.RequiredSkillID;
                            cond.MinSkillLevel = cData.MinSkillLevel;
                            genericMod.Conditions.Add(cond);
                        }
                    }
                }
            }

            return clonedModifier;
        }

        // --- API METHODS ---

        public Item GetItemInternal(int id)
        {
            return ItemDatabase.GetItem(id); // Routes to ItemDatabase
        }


        public CharacterModel GetCharacterInternal(int id)
        {
            if (_characterDatabase.TryGetValue(id, out CharacterModel character))
            {
                return character;
            }
            return null;
        }

        public List<Item> GetItemsInternal() => ItemDatabase.GetAllItems();
        public List<CharacterModel> GetCharactersInternal() => new List<CharacterModel>(_characterDatabase.Values);


        public List<ActiveSkill> GetAllSynchronizedSkillsInternal()
        {
            List<ActiveSkill> allSkills = new List<ActiveSkill>();
            foreach (int id in _activeSkillAssets.Keys)
            {
                var skill = GetSynchronizedActiveSkill(id);
                if (skill != null) allSkills.Add(skill);
            }
            return allSkills;
        }

        // --- STATIC WRAPPERS ---
        public static Item GetItem(int id) => Instance.GetItemInternal(id);
        public static CharacterModel GetCharacter(int id) => Instance.GetCharacterInternal(id);
        public static AttributeTemplate GetAttribute(AttributeType type) => AttributeDatabase.GetAttribute(type);
        public static AttributeTemplate GetAttributeById(int id) => AttributeDatabase.GetAttribute(id);
        public static ActiveSkill GetSynchronizedSkill(int id) => Instance.GetSynchronizedActiveSkill(id);
        public static SupportModifier GetSynchronizedSupport(int id) => Instance.GetSynchronizedSupportModifier(id);

        public static List<Item> GetItems => Instance.GetItemsInternal();
        public static List<CharacterModel> Characters => Instance.GetCharactersInternal();
        public static List<AttributeTemplate> Attributes => AttributeDatabase.GetAllAttributes();
        public static List<ActiveSkill> Skills => Instance.GetAllSynchronizedSkillsInternal();

        // --- SkillTree Static API ---
        public static SkillTree.Models.SkillTreeNode GetSkillTreeNode(int id) => SkillTreeDatabase.GetNode(id);
        public static System.Collections.Generic.List<SkillTree.Models.SkillTreeNode> GetAllSkillNodes() => SkillTreeDatabase.GetAllNodes();
        public static SkillTree.Models.SkillTreeStats GetSkillTreeStats() => SkillTreeDatabase.GetActiveStats();
        public static bool CanUnlockSkillTreeNode(int id) => SkillTreeDatabase.CanUnlock(id);

        public static int GetDefaultSlotForType(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.WeaponOneHandLeft:  return 0;
                case EquipmentSlot.WeaponTwoHand:      return 0;   
                case EquipmentSlot.WeaponOneHandRight: return 7;
                case EquipmentSlot.Helm:               return 1;
                case EquipmentSlot.Armor:              return 2;
                case EquipmentSlot.Glove:              return 3;
                case EquipmentSlot.Talisman:           return 4;
                case EquipmentSlot.Accessory:          return 5;   
                case EquipmentSlot.AccessoryLeft:      return 5;
                case EquipmentSlot.AccessoryRight:     return 6;
                default: return 0;
            }
        }
    }


    [System.Serializable]
    public class CharacterModel
    {
        public int Id;
        public string Name;
        public CoreSystem.Entity.FactionType Faction;
        public int BaseHP;
        public int BaseMP;
        public string PrefabPath;
    }

    [System.Serializable]
    public class CharacterDataList
    {
        public List<CharacterModel> Characters;
    }
}
