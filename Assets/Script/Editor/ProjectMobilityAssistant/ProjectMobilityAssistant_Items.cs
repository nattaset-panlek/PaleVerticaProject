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
            else if (entry.ItemType == "Costume")
            {
                if (System.Enum.TryParse(entry.CostumeSlot, out CoreSystem.CostumeSlot cSlot))
                     entry.CostumeSlot = ((CoreSystem.CostumeSlot)EditorGUILayout.EnumPopup("Costume Slot", cSlot)).ToString();
                else
                     entry.CostumeSlot = EditorGUILayout.TextField("Costume Slot (Raw)", entry.CostumeSlot);

                entry.LayerPriority = EditorGUILayout.IntField("Layer Priority", entry.LayerPriority);
                
                // ── Costume Sprite Layers (Addressable Keys) ──────────────────────
                DrawSectionHeader("\uD83C\uD0AD Costume Sprite Layers  \u00B7  Addressables");
                EditorGUILayout.HelpBox("ใส่ Addressable Key ของ Sprite (เช่น: \"Costume/Head_A_Sprite\")\nSet Label ใน Addressables Groups ด้วยนะ", MessageType.Info);
                if (entry.VisualSpritePaths == null) entry.VisualSpritePaths = new List<string>();
                for (int i = 0; i < entry.VisualSpritePaths.Count; i++)
                {
                    EditorGUILayout.BeginVertical(SectionStyle);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"Layer {i}", GUILayout.Width(60));
                    entry.VisualSpritePaths[i] = EditorGUILayout.TextField(entry.VisualSpritePaths[i]);
                    if (GUILayout.Button("X", GUILayout.Width(20))) { entry.VisualSpritePaths.RemoveAt(i); break; }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
                if (GUILayout.Button("\uFF0B Add Sprite Layer")) entry.VisualSpritePaths.Add("");

                // ── Costume Visual Prefabs (Addressable Keys) ──────────────────────
                DrawSectionHeader("\u2728 Costume Visual Prefabs  \u00B7  Addressables");
                EditorGUILayout.HelpBox("ใส่ Addressable Key ของ Prefab (เช่น: \"Costume/Wing_A_Prefab\")", MessageType.Info);
                if (entry.VisualPrefabEntries == null) entry.VisualPrefabEntries = new List<CostumePrefabEntry>();
                for (int i = 0; i < entry.VisualPrefabEntries.Count; i++)
                {
                    var pEntry = entry.VisualPrefabEntries[i];
                    EditorGUILayout.BeginVertical(SectionStyle);
                    
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"Prefab {i}", GUILayout.Width(60));
                    pEntry.PrefabKey = EditorGUILayout.TextField(pEntry.PrefabKey ?? "");
                    if (GUILayout.Button("X", GUILayout.Width(20))) { entry.VisualPrefabEntries.RemoveAt(i); break; }
                    EditorGUILayout.EndHorizontal();

                    if (System.Enum.TryParse(pEntry.TargetNode, out CoreSystem.CostumeSlot node))
                         pEntry.TargetNode = ((CoreSystem.CostumeSlot)EditorGUILayout.EnumPopup("Target Node", node)).ToString();
                    else
                         pEntry.TargetNode = EditorGUILayout.TextField("Node (Raw)", pEntry.TargetNode);

                    pEntry.Offset   = EditorGUILayout.Vector3Field("Offset",   pEntry.Offset);
                    pEntry.Rotation = EditorGUILayout.Vector3Field("Rotation", pEntry.Rotation);
                    pEntry.Scale    = EditorGUILayout.Vector3Field("Scale",    pEntry.Scale == Vector3.zero ? Vector3.one : pEntry.Scale);

                    EditorGUILayout.EndVertical();
                }
                if (GUILayout.Button("\uFF0B Add Visual Prefab")) entry.VisualPrefabEntries.Add(new CostumePrefabEntry { TargetNode = "Head", Scale = Vector3.one });
            }
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(SectionStyle);
            GUILayout.Label("Visuals", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("ลาก Sprite จากโฟลเดอร์ Assets/Addressable/Sprite/Item แล้วระบบจะดึง Addressable Key อัตโนมัติ", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            Sprite currentIcon = ResolveEditorSprite(entry.IconPath);
            Sprite newIcon = (Sprite)EditorGUILayout.ObjectField("Item Icon", currentIcon, typeof(Sprite), false);
            
            if (newIcon != currentIcon)
            {
                entry.IconPath = GetAddressableKey(newIcon);
            }
            EditorGUILayout.EndHorizontal();

            entry.IconPath = EditorGUILayout.TextField("Addressable Key", entry.IconPath ?? "");

            // Preview
            Sprite previewSprite = ResolveEditorSprite(entry.IconPath);
            if (previewSprite != null)
            {
                Texture2D preview = AssetPreview.GetAssetPreview(previewSprite) ?? previewSprite.texture;
                if (preview != null)
                {
                    EditorGUILayout.Space(5);
                    Rect r = EditorGUILayout.GetControlRect(false, 64);
                    r.width = 64;
                    GUI.DrawTexture(r, preview, ScaleMode.ScaleToFit);
                }
            }
            else if (!string.IsNullOrEmpty(entry.IconPath))
            {
                EditorGUILayout.HelpBox($"Key '{entry.IconPath}' ไม่พบใน Addressables\nกรุณา Register Asset ใน Addressables Groups ก่อน", MessageType.Warning);
            }
            EditorGUILayout.EndVertical();

            // ── Effects ─────────────────────────────────────────────────────────
            DrawEffectSection(entry.Effects ?? (entry.Effects = new List<EffectEntry>()), false);
        }, 
        (entry) => {
            string effectLabel = (entry.Effects != null && entry.Effects.Count > 0) ? " ⚡" : "";
            return $"[{entry.ItemId}] {entry.Name}{effectLabel}";
        }, 
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
            if (_showOnlyWithEffects && (entry.Effects == null || entry.Effects.Count == 0)) return false;
            if (_itemTypeFilterIndex == 0) return true;
            return entry.ItemType == _itemTypeNames[_itemTypeFilterIndex];
        },
        () => {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter by Type:", GUILayout.Width(100));
            _showOnlyWithEffects = EditorGUILayout.ToggleLeft("Show Only with Effects (⚡)", _showOnlyWithEffects);
            EditorGUILayout.EndHorizontal();
            
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
                    "BonusCondition"
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
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Skill");
            DrawSkillSelector(effect.SkillID, (id) => effect.SkillID = id);
            EditorGUILayout.EndHorizontal();

            effect.TriggerType = EditorGUILayout.TextField("Trigger Type", effect.TriggerType);
            effect.TriggerChance = EditorGUILayout.Slider("Chance", effect.TriggerChance, 0, 100);
        }
        else if (effect.EffectType == "SkillUnlockEffect")
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Unlock Skill");
            DrawSkillSelector(effect.SkillID, (id) => effect.SkillID = id);
            EditorGUILayout.EndHorizontal();
        }
        else if (effect.EffectType == "BonusCondition")
        {
            DrawSectionHeader("Condition Configuration");
            EditorGUILayout.LabelField("If Attribute meets criteria, grant bonus.");
            
            // The Attribute to check
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Check Attr", GUILayout.Width(80));
            DrawAttributeSelector(effect.ConditionAttributeID, (id) => effect.ConditionAttributeID = id);
            EditorGUILayout.EndHorizontal();

            // Comparison & Threshold
            EditorGUILayout.BeginHorizontal();
            effect.ConditionComparison = EditorGUILayout.TextField("Comparison", effect.ConditionComparison ?? ">=", GUILayout.Width(140));
            effect.ConditionThreshold = EditorGUILayout.FloatField("Threshold", effect.ConditionThreshold);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            DrawSectionHeader("Bonus Award");
            
            // The Attribute to grant
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Bonus Attr", GUILayout.Width(80));
            DrawAttributeSelector(effect.AttributeID, (id) => effect.AttributeID = id);
            EditorGUILayout.EndHorizontal();

            effect.Value = EditorGUILayout.FloatField("Bonus Value", effect.Value);
        }
    }

    private void DrawSkillSelector(int currentId, System.Action<int> onSelected)
    {
        string currentName = "None (0)";
        if (currentId > 0)
        {
            var skill = _skillData.ActiveSkills.FirstOrDefault(s => s.SkillID == currentId);
            currentName = skill != null ? $"[{skill.SkillID}] {skill.SkillName}" : $"Unknown ({currentId})";
        }

        if (GUILayout.Button(currentName, EditorStyles.popup))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), currentId == 0, () => onSelected(0));
            foreach (var s in _skillData.ActiveSkills.OrderBy(s => s.SkillID))
            {
                int id = s.SkillID;
                menu.AddItem(new GUIContent($"[{id}] {s.SkillName}"), currentId == id, () => onSelected(id));
            }
            menu.ShowAsContext();
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
