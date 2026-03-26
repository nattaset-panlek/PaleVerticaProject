using System.Collections.Generic;
using UnityEngine;
using CoreSystem;
using CoreSystem.Data;
using CoreSystem.Data.Items.Model;
using CoreSystem.Data.Items.Effects;
using CoreSystem.Attributes;
using CoreSystem.Data.Skills;

namespace CoreSystem.Data.Items
{
    public static class ItemDatabase
    {
        private static Dictionary<int, Item> _items = new Dictionary<int, Item>();
        private static Dictionary<string, EquipmentSetData> _sets = new Dictionary<string, EquipmentSetData>();

        public static void Initialize()
        {
            _items.Clear();
            _sets.Clear();
            
            LoadSets();

            TextAsset jsonFile = Resources.Load<TextAsset>("Data/Item");
            if (jsonFile == null)
            {
                Debug.LogError("item.json not found in Resources/Data/");
                return;
            }

            ItemDataList dataList = JsonUtility.FromJson<ItemDataList>(jsonFile.text);
            if (dataList == null || dataList.Items == null)
            {
                Debug.LogError("ItemDatabase: Failed to parse Item.json or Items list is null.");
                return;
            }

            Debug.Log($"ItemDatabase: Parsing {dataList.Items.Count} entries from Item.json");

            foreach (var entry in dataList.Items)
            {
                Item model = CreateItemFromEntry(entry);
                if (model != null)
                {
                    if (System.Enum.TryParse(entry.Rarity, out ItemRarity rarity))
                    {
                        model.Rarity = rarity;
                    }
                    else
                    {
                        model.Rarity = ItemRarity.Common;
                    }

                    if (System.Enum.TryParse(entry.ItemType, out ItemType type))
                    {
                        model.Type = type;
                    }

                    // Assign icon path from JSON (can be empty -> fallback will apply)
                    model.IconPath = entry.IconPath;

                    _items[model.Id] = model;
                }
                else
                {
                    Debug.LogWarning($"ItemDatabase: Failed to create model for item {entry.ItemId} (Type: {entry.ItemType})");
                }
            }
            
            Debug.Log($"ItemDatabase: Successfully loaded {_items.Count} items.");
        }

        public static Item GetItem(int itemId)
        {
            if (_items.TryGetValue(itemId, out Item model))
            {
                return model;
            }
            return null;
        }

        public static List<Item> GetAllItems()
        {
            return new List<Item>(_items.Values);
        }

        private static Item CreateItemFromEntry(ItemEntry entry)
        {
            switch (entry.ItemType)
            {
                case "Currency":
                    return new Currency { Id = entry.ItemId, Code = entry.ItemId.ToString(), Name = entry.Name, Description = entry.Description, IsStackable = entry.MaxStack > 1, IconPath = entry.IconPath };
                case "Consumable":
                    return new Consumable { Id = entry.ItemId, Code = entry.ItemId.ToString(), Name = entry.Name, Description = entry.Description, IsStackable = entry.MaxStack > 1, IconPath = entry.IconPath };
                case "Equipment":
                    if (System.Enum.TryParse(entry.EquipmentSlot, out EquipmentSlot slot))
                    {
                        var equip = new Equipment { 
                            Id = entry.ItemId, 
                            Code = entry.ItemId.ToString(), 
                            Name = entry.Name, 
                            Description = entry.Description, 
                            IsStackable = entry.MaxStack > 1, 
                            Slot = slot,
                            BasePhysical = entry.BasePhysical,
                            BaseMagic = entry.BaseMagic,
                            MaxSockets = entry.MaxSockets,
                            IconPath = entry.IconPath
                        };
                        PopulateEffects(equip.Effects, entry.Effects);
                        
                        if (!string.IsNullOrEmpty(entry.SetID) && _sets.TryGetValue(entry.SetID, out EquipmentSetData setData))
                        {
                            equip.SetResource = setData;
                        }
                        
                        return equip;
                    }
                    return null;
                case "Fragment":
                    if (System.Enum.TryParse(entry.FragmentKind, out FragmentType kind))
                    {
                        var fragment = new Fragment { Id = entry.ItemId, Code = entry.ItemId.ToString(), Name = entry.Name, Description = entry.Description, IsStackable = entry.MaxStack > 1, FragmentKind = kind, IconPath = entry.IconPath };
                        PopulateEffects(fragment.Effects, entry.Effects);
                        return fragment;
                    }
                    return null;
                case "Resource":
                    return new Resource { Id = entry.ItemId, Code = entry.ItemId.ToString(), Name = entry.Name, Description = entry.Description, IsStackable = entry.MaxStack > 1, IconPath = entry.IconPath };
                case "SkillRune":
                    return new SkillRune { Id = entry.ItemId, Code = entry.ItemId.ToString(), Name = entry.Name, Description = entry.Description, IsStackable = entry.MaxStack > 1, IsSupportRune = entry.IsSupportRune, LinkedSkillID = entry.SkillID, IconPath = entry.IconPath };
                case "Additional":
                    return new Additional { Id = entry.ItemId, Code = entry.ItemId.ToString(), Name = entry.Name, Description = entry.Description, IsStackable = entry.MaxStack > 1, IconPath = entry.IconPath };
                case "Costume":
                    if (System.Enum.TryParse(entry.CostumeSlot, out CostumeSlot cSlot))
                    {
                        var costume = new Costume { 
                            Id = entry.ItemId, 
                            Code = entry.ItemId.ToString(), 
                            Name = entry.Name, 
                            Description = entry.Description, 
                            IsStackable = entry.MaxStack > 1, 
                            Slot = cSlot,
                            LayerPriority = entry.LayerPriority,
                            IconPath = entry.IconPath
                        };
                        
                        // Load visual sprites if paths are provided
                        if (entry.VisualSpritePaths != null && entry.VisualSpritePaths.Count > 0)
                        {
                            costume.VisualSprites = new Sprite[entry.VisualSpritePaths.Count];
                            for (int i = 0; i < entry.VisualSpritePaths.Count; i++)
                            {
                                costume.VisualSprites[i] = LoadSprite(entry.VisualSpritePaths[i]);
                            }
                        }
                        
                        PopulateEffects(costume.Effects, entry.Effects);
                        return costume;
                    }
                    return null;
                default:
                    Debug.LogWarning($"Unknown item type {entry.ItemType} for item {entry.ItemId}");
                    return null;
            }
        }

        private static void LoadSets()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/ItemSet");
            if (jsonFile == null) return;

            ItemSetList setList = JsonUtility.FromJson<ItemSetList>(jsonFile.text);
            if (setList == null || setList.Sets == null) return;

            foreach (var sEntry in setList.Sets)
            {
                var setData = new EquipmentSetData();
                setData.SetID = sEntry.SetID;
                setData.SetName = sEntry.SetName;
                
                foreach (var bEntry in sEntry.BonusLevels)
                {
                    var bonusLevel = new SetBonusLevel { RequiredPieces = bEntry.RequiredPieces };
                    PopulateEffects(bonusLevel.BonusEffects, bEntry.Effects);
                    setData.BonusLevels.Add(bonusLevel);
                }
                
                _sets[sEntry.SetID] = setData;
            }
            Debug.Log($"ItemDatabase: Loaded {_sets.Count} equipment sets from JSON.");
        }

        private static void PopulateEffects(List<EquipmentEffect> targetList, List<EffectEntry> entries)
        {
            if (entries == null) return;

            foreach (var e in entries)
            {
                switch (e.EffectType)
                {
                    case "StatBonus":
                    case "AttributeBonusEffect":
                        targetList.Add(new AttributeBonusEffect { Attribute = (AttributeType)e.AttributeID, Value = e.Value });
                        break;
                    case "SkillUnlock":
                    case "SkillUnlockEffect":
                        // Prioritize SkillID if present, otherwise SkillName (backwards compatibility or data helper)
                        targetList.Add(new SkillUnlockEffect { SkillID = e.SkillID });
                        break;
                    case "AutoCast":
                    case "AutoCastEffect":
                        if (System.Enum.TryParse(e.TriggerType, out AutoCastTrigger trigger))
                        {
                            // In JSON/Editor it's already 0-100, no need to multiply by 100
                            targetList.Add(new AutoCastEffect { TriggerType = trigger, TriggerChance = e.TriggerChance, SkillID = e.SkillID });
                        }
                        break;
                    case "ScalingBonusEffect":
                        var scalingEffect = new ScalingBonusEffect
                        {
                            Threshold = e.ScalingThreshold,
                            BonusPerThreshold = e.Value,
                            TargetAttribute = (AttributeType)e.AttributeID,
                            ModType = ModifierType.Flat
                        };
                        
                        if (e.ScalingSourceIDs != null)
                        {
                            foreach(var id in e.ScalingSourceIDs)
                                scalingEffect.SourceAttributes.Add((AttributeType)id);
                        }
                        
                        targetList.Add(scalingEffect);
                        break;
                    case "BonusCondition":
                        var condBonus = new CoreSystem.Data.Items.Effects.ConditionalBonusEffect
                        {
                            Condition = new ConditionLogic
                            {
                                PrimaryAttribute = (AttributeType)e.ConditionAttributeID,
                                Operator = ParseOperator(e.ConditionComparison),
                                TargetValue = e.ConditionThreshold,
                                JoinOperation = ConditionOperatorJoin.None
                            },
                            BonusEffect = new AttributeBonusEffect
                            {
                                Attribute = (AttributeType)e.AttributeID,
                                Value = e.Value
                            }
                        };
                        targetList.Add(condBonus);
                        break;
                }
            }
        }
        
        private static ConditionOperator ParseOperator(string op)
        {
            switch (op)
            {
                case ">":  return ConditionOperator.GreaterThan;
                case "<":  return ConditionOperator.LessThan;
                case "=":  
                case "==": return ConditionOperator.Equal;
                case ">=": return ConditionOperator.GreaterOrEqual;
                case "<=": return ConditionOperator.LessOrEqual;
                default:   return ConditionOperator.GreaterOrEqual; // Default
            }
        }

        private static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (path.Contains(":"))
            {
                string[] parts = path.Split(':');
                string mainPath = parts[0];
                string subName = parts[1];
                Sprite[] allSprites = Resources.LoadAll<Sprite>(mainPath);
                foreach (var sprite in allSprites)
                {
                    if (sprite.name == subName) return sprite;
                }
            }
            return Resources.Load<Sprite>(path);
        }
    }

    [System.Serializable]
    public class ItemDataList
    {
        public List<ItemEntry> Items;
    }

    [System.Serializable]
    public class ItemEntry
    {
        public int ItemId;
        public string ItemType; // "Currency", "Consumable", "Equipment", etc.
        public string Name;
        public LocalizedString Description;
        public int MaxStack;

        /// <summary>Resources-relative path for the item icon, e.g. "Sprite/Item/UI_Helmet_Icon".
        /// Leave empty to use the UI_Null_Icon fallback.</summary>
        public string IconPath;

        // Specific fields
        public string EquipmentSlot; // e.g., "Weapon", "Helm"
        public bool IsSupportRune;
        public string Rarity;
        public int SkillID;
        public string FragmentKind;
        public float BasePhysical;
        public float BaseMagic;
        public int MaxSockets;
        public List<EffectEntry> Effects;

        // Costume specific
        public string CostumeSlot;
        public int LayerPriority;
        public List<string> VisualSpritePaths;
        
        public string SetID; // New: Reference to a set in ItemSet.json
    }

    [System.Serializable]
    public class ItemSetList
    {
        public List<ItemSetEntry> Sets;
    }

    [System.Serializable]
    public class ItemSetEntry
    {
        public string SetID;
        public string SetName;
        public List<SetBonusEntry> BonusLevels;
    }

    [System.Serializable]
    public class SetBonusEntry
    {
        public int RequiredPieces;
        public List<EffectEntry> Effects;
    }

}
