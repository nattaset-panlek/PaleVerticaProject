using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Skills;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Logic.Effects;
using CoreSystem.Data.Items.Model;

public partial class ProjectMobilityAssistant : EditorWindow
{
    private void DrawItemEditor()
    {
        DrawEditor(_itemData.Items, _itemSelectedIndex, (idx) => _itemSelectedIndex = idx, (entry) => {
            EditorGUILayout.BeginVertical(SectionStyle);
            GUILayout.Label("Basic Item Info", EditorStyles.boldLabel);
            entry.ItemId = EditorGUILayout.IntField("Item ID", entry.ItemId);
            entry.Name = EditorGUILayout.TextField("Name", entry.Name);
            entry.Description.TH = EditorGUILayout.TextField("Description (TH)", entry.Description.TH);
            entry.Description.EN = EditorGUILayout.TextField("Description (EN)", entry.Description.EN);
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(SectionStyle);
            GUILayout.Label("Item Attributes", EditorStyles.boldLabel);
            
            // Robust Enum parsing with ignoreCase
            if (!System.Enum.TryParse(entry.ItemType, true, out ItemType type)) type = ItemType.Consumable;
            entry.ItemType = ((ItemType)EditorGUILayout.EnumPopup("Item Type", type)).ToString();
            
            if (!System.Enum.TryParse(entry.Rarity, true, out ItemRarity rarity)) rarity = ItemRarity.Common;
            entry.Rarity = ((ItemRarity)EditorGUILayout.EnumPopup("Rarity", rarity)).ToString();

            // Item Set ID
            EditorGUILayout.Space();
            DrawSectionHeader("\uD83D\uDC8E Item Set");
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Item Set");
            string setID = string.IsNullOrEmpty(entry.SetID) ? "None" : entry.SetID;
            if (GUILayout.Button(setID, EditorStyles.popup))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(entry.SetID), () => entry.SetID = "");
                foreach (var s in _itemSetData.Sets)
                    menu.AddItem(new GUIContent(s.SetName), entry.SetID == s.SetID, () => entry.SetID = s.SetID);
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(entry.SetID))
            {
                var set = _itemSetData.Sets.FirstOrDefault(s => s.SetID == entry.SetID);
                if (set == null)
                {
                    EditorGUILayout.HelpBox($"Set ID '{entry.SetID}' not found in Item Sets.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.LabelField("Set Name:", set.SetName);
                    if (GUILayout.Button("Go to Item Set Editor")) 
                    {
                        _currentTab = ToolTab.Sets;
                        _itemSetSelectedIndex = _itemSetData.Sets.IndexOf(set);
                    }
                }
            }

            if (entry.ItemType == "Equipment")
            {
                // Slot Dropdown
                if (!System.Enum.TryParse(entry.EquipmentSlot, true, out EquipmentSlot slot)) slot = EquipmentSlot.Armor;
                entry.EquipmentSlot = ((EquipmentSlot)EditorGUILayout.EnumPopup("Equipment Slot", slot)).ToString();
                
                entry.MaxStack = EditorGUILayout.IntField("Max Stack", entry.MaxStack);
                entry.BasePhysical = EditorGUILayout.FloatField("Base Phys", entry.BasePhysical);
                entry.BaseMagic    = EditorGUILayout.FloatField("Base Magic", entry.BaseMagic);
                entry.MaxSockets   = EditorGUILayout.IntField("Max Sockets", entry.MaxSockets);
            }
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(SectionStyle);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Visuals", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("\uD83D\uDCC1  Resources/Sprite/Item", EditorStyles.miniButton))
                EditorUtility.RevealInFinder("Assets/Resources/Sprite/Item");
            EditorGUILayout.EndHorizontal();
            
            entry.IconPath = EditorGUILayout.TextField("Path", entry.IconPath ?? "");

            // Preview + Open Folder
            EditorGUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(entry.IconPath))
            {
                var sprite = ResolveResourceSprite(entry.IconPath);
                if (sprite != null)
                {
                    Texture2D preview = AssetPreview.GetAssetPreview(sprite) ?? sprite.texture;
                    if (preview != null)
                        GUILayout.Label(new GUIContent(preview), GUILayout.Width(52), GUILayout.Height(52));
                }
                else
                {
                    EditorGUILayout.HelpBox("Sprite not found", MessageType.Warning);
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("\uD83D\uDCC1  Open Sprite Folder", GUILayout.Height(28)))
                EditorUtility.RevealInFinder("Assets/Resources/Sprite/Item");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            // ── Effects ─────────────────────────────────────────────────────────
            DrawEffectSection(entry.Effects ?? (entry.Effects = new List<EffectEntry>()), false);
        }, 
        (entry) => $"[{entry.ItemId}] {entry.Name}", 
        () => {
            var e = new ItemEntry { Description = new LocalizedString(), ItemType = ItemType.Consumable.ToString(), Rarity = ItemRarity.Common.ToString() };
            e.ItemId = GetNextItemId(e.ItemType);
            return e;
        },
        (rect, entry) => {
            if (System.Enum.TryParse(entry.Rarity, true, out ItemRarity rarity)) {
                Rect colorRect = new Rect(rect.x + 2, rect.y + 2, 4, rect.height - 4);
                EditorGUI.DrawRect(colorRect, rarity.GetColor());
            }
        },
        (entry) => {
            if (_itemTypeFilterIndex == 0) return true;
            return entry.ItemType == _itemTypeNames[_itemTypeFilterIndex];
        },
        () => {
            EditorGUILayout.LabelField("Filter by Type:");
            _itemTypeFilterIndex = GUILayout.SelectionGrid(_itemTypeFilterIndex, _tabNamesForItems, 4, EditorStyles.miniButton); 
            EditorGUILayout.Space(5);
        });
    }

    private void DrawEffectSection(List<EffectEntry> effects, bool isSkill)
    {
        DrawSectionHeader("\u26A1  Effects");
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        _useManualAttributeInput = EditorGUILayout.ToggleLeft("Manual ID Input", _useManualAttributeInput, GUILayout.Width(120));
        EditorGUILayout.EndHorizontal();

        if (effects == null) return;

        for (int i = 0; i < effects.Count; i++)
        {
            EditorGUILayout.BeginVertical(SectionStyle);
            var effect = effects[i];
            
            EditorGUILayout.BeginHorizontal();
            
            string[] effectTypes;
            if (isSkill)
            {
                effectTypes = new string[] { 
                    "DamageEffect",
                    "HealEffect",
                    "ApplyBuffEffect", 
                    "ApplyTemporaryBuffEffect", 
                    "MovementEffectLogic",
                    "SummonObjectEffect"
                };
            }
            else
            {
                effectTypes = new string[] { 
                    "AttributeBonusEffect",
                    "ScalingBonusEffect",
                    "SkillUnlockEffect",
                    "AutoCastEffect",
                    "EquipmentEffect",
                    "StatBonus"
                };
            }

            int typeIdx = System.Array.IndexOf(effectTypes, effect.EffectType);
            if (typeIdx == -1) typeIdx = effectTypes.Length - 1; 
            effect.EffectType = effectTypes[EditorGUILayout.Popup(typeIdx, effectTypes, GUILayout.Width(180))];
            
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", GUILayout.Width(20))) { effects.RemoveAt(i); break; }
            EditorGUILayout.EndHorizontal();

            // ─── Context Sensitive Fields ───
            if (isSkill)
            {
                DrawSkillEffectFields(effect);
            }
            else
            {
                DrawItemEffectFields(effect);
            }

            EditorGUILayout.EndVertical();
        }
        
        string defaultType = isSkill ? "DamageEffect" : "AttributeBonusEffect";
        if (GUILayout.Button("\uFF0B Add Effect")) effects.Add(new EffectEntry { EffectType = defaultType });
    }

    private void DrawItemEffectFields(EffectEntry effect)
    {
        if (effect.EffectType == "AttributeBonusEffect" || effect.EffectType == "StatBonus")
        {
            if (_useManualAttributeInput)
                effect.AttributeID = EditorGUILayout.IntField("Attr ID", effect.AttributeID);
            else
                DrawAttributeSelector(effect.AttributeID, (id) => effect.AttributeID = id);

            effect.Value = EditorGUILayout.FloatField("Value", effect.Value);
        }
        else if (effect.EffectType == "ScalingBonusEffect")
        {
            DrawSectionHeader("Scaling Logic");
            
            // Target Attribute (What is being boosted)
            if (_useManualAttributeInput)
                effect.AttributeID = EditorGUILayout.IntField("Target Attr ID", effect.AttributeID);
            else
                DrawAttributeSelector(effect.AttributeID, (id) => effect.AttributeID = id);

            effect.ScalingThreshold = EditorGUILayout.FloatField("Threshold", effect.ScalingThreshold);
            effect.Value = EditorGUILayout.FloatField("Bonus per Threshold", effect.Value);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Scaling Source Attributes (Summed)");
            if (effect.ScalingSourceIDs == null) effect.ScalingSourceIDs = new List<int>();
            
            for (int i = 0; i < effect.ScalingSourceIDs.Count; i++)
            {
                int index = i;
                EditorGUILayout.BeginHorizontal();
                DrawAttributeSelector(effect.ScalingSourceIDs[index], (id) => effect.ScalingSourceIDs[index] = id);
                if (GUILayout.Button("X", GUILayout.Width(20))) { effect.ScalingSourceIDs.RemoveAt(index); break; }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Source Attribute")) effect.ScalingSourceIDs.Add(0);
        }
        else if (effect.EffectType == "AutoCastEffect")
        {
            effect.SkillID = EditorGUILayout.IntField("Skill ID", effect.SkillID);
            effect.TriggerType = EditorGUILayout.TextField("Trigger Type", effect.TriggerType);
            effect.TriggerChance = EditorGUILayout.Slider("Chance", effect.TriggerChance, 0, 100);
        }
        else if (effect.EffectType == "SkillUnlockEffect")
        {
            effect.SkillID = EditorGUILayout.IntField("Skill ID", effect.SkillID);
        }
    }

    private int GetNextItemId(string itemType)
    {
        int prefix = GetItemTypePrefix(itemType);
        var ids = _itemData.Items
            .Where(i => i.ItemId / 1000 == prefix)
            .Select(i => i.ItemId);
        return ids.Any() ? ids.Max() + 1 : prefix * 1000 + 1;
    }

    private int GetItemTypePrefix(string itemType)
    {
        if (_itemPrefixes != null && _itemPrefixes.ContainsKey(itemType))
        {
            return _itemPrefixes[itemType];
        }
        switch (itemType)
        {
            case "Currency":   return 1;
            case "Consumable": return 2;
            case "Equipment":  return 3;
            case "Fragment":   return 4;
            case "Resource":   return 5;
            case "SkillRune":  return 6;
            case "Costume":    return 8;
            default:           return 9;
        }
    }
}
