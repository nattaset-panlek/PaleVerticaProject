using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Data.Items;
using CoreSystem.Data.Skills;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Logic.Effects;

public partial class GameDataWorkbench : EditorWindow
{
    private static bool _showOverview = true;
    private static bool _showRarityDistribution = true;
    private static bool _showElementDistribution = true;
    private static bool _showDataIntegrity = true;

    private void DrawDashboard()
    {
        EditorGUILayout.BeginVertical(PaddingStyle);
        
        _showOverview = EditorGUILayout.BeginFoldoutHeaderGroup(_showOverview, "Project Overview");
        if (_showOverview)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            DrawSummaryCard("Attributes", _attributeData.Attributes.Count.ToString(), new Color(0.3f, 0.8f, 0.9f));
            DrawSummaryCard("Items",      _itemData.Items.Count.ToString(),             new Color(1f, 0.6f, 0.1f));
            DrawSummaryCard("Active Skills", _skillData.ActiveSkills.Count.ToString(),  new Color(0.9f, 0.3f, 0.3f));
            DrawSummaryCard("Support Mods", _supportSkillData.SupportModifiers.Count.ToString(), new Color(0.3f, 0.85f, 0.5f));
            DrawSummaryCard("Item Sets",     _itemSetData.Sets.Count.ToString(),             new Color(0.8f, 0.5f, 1f));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(20);

        _contentScroll = EditorGUILayout.BeginScrollView(_contentScroll);

        _showRarityDistribution = EditorGUILayout.BeginFoldoutHeaderGroup(_showRarityDistribution, "Item Rarity Distribution");
        if (_showRarityDistribution)
        {
            DrawRarityChart();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(20);

        _showElementDistribution = EditorGUILayout.BeginFoldoutHeaderGroup(_showElementDistribution, "Active Skill Element Distribution");
        if (_showElementDistribution)
        {
            DrawElementChart();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(30);

        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.35f, 0.85f, 0.45f);
        if (GUILayout.Button("Run Full Validation", GUILayout.Height(36))) RunValidation();
        GUI.backgroundColor = new Color(0.35f, 0.6f, 0.95f);
        if (GUILayout.Button("Reload Everything", GUILayout.Height(36))) { LoadAllData(); Repaint(); }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        if (_isValidated)
        {
            EditorGUILayout.Space(20);
            _showDataIntegrity = EditorGUILayout.BeginFoldoutHeaderGroup(_showDataIntegrity, "Data Integrity Health");
            if (_showDataIntegrity)
            {
                var itemErrors = _validationErrors.Where(e => e.Contains("[Item")).ToList();
                var skillErrors = _validationErrors.Where(e => e.Contains("[Skill")).ToList();
                var attrErrors = _validationErrors.Where(e => e.Contains("[Attribute")).ToList();
                var setErrors = _validationErrors.Where(e => e.Contains("[Set")).ToList();

                if (_validationErrors.Count == 0)
                {
                    EditorGUILayout.HelpBox("All data checks passed!", MessageType.Info);
                }
                else
                {
                    if (itemErrors.Count > 0) DrawValidationErrorGroup("Items", itemErrors);
                    if (skillErrors.Count > 0) DrawValidationErrorGroup("Skills", skillErrors);
                    if (attrErrors.Count > 0) DrawValidationErrorGroup("Attributes", attrErrors);
                    if (setErrors.Count > 0) DrawValidationErrorGroup("Item Sets", setErrors);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawRarityChart()
    {
        var rarities = _itemData.Items.GroupBy(i => i.Rarity).Select(g => new { Rarity = g.Key, Count = g.Count() }).ToList();
        float total = _itemData.Items.Count;
        if (total == 0) return;

        EditorGUILayout.BeginHorizontal(SectionStyle, GUILayout.Height(24));
        foreach (var r in rarities)
        {
            float width = (r.Count / total) * (Screen.width - 60);
            if (System.Enum.TryParse(r.Rarity, out ItemRarity rarity))
            {
                Rect rect = GUILayoutUtility.GetRect(width, 24);
                EditorGUI.DrawRect(rect, rarity.GetColor());
                GUI.Label(rect, $"{r.Rarity} ({r.Count})", new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } });
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawElementChart()
    {
        var elements = _skillData.ActiveSkills.GroupBy(s => s.SkillElement).Select(g => new { Element = g.Key, Count = g.Count() }).ToList();
        float total = _skillData.ActiveSkills.Count;
        if (total == 0) return;

        EditorGUILayout.BeginHorizontal(SectionStyle, GUILayout.Height(24));
        foreach (var e in elements)
        {
            float width = (e.Count / total) * (Screen.width - 60);
            if (System.Enum.TryParse(e.Element, out ElementType element))
            {
                Rect rect = GUILayoutUtility.GetRect(width, 24);
                EditorGUI.DrawRect(rect, GetElementColor(element));
                GUI.Label(rect, $"{e.Element} ({e.Count})", new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.black } });
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawValidationErrorGroup(string title, List<string> errors)
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        foreach (var err in errors)
            EditorGUILayout.LabelField($"\u2022 {err}");
        EditorGUILayout.Space(5);
    }

    private void RunValidation()
    {
        _validationErrors.Clear();
        _isValidated = true;

        // --- Attribute Validation ---
        var attrIds = _attributeData.Attributes.Select(a => a.Id).ToList();
        var dupAttr = attrIds.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        foreach (var id in dupAttr) _validationErrors.Add($"[Attribute] Duplicate ID detected: {id}");

        // --- Item Validation ---
        var itemIds = _itemData.Items.Select(i => i.ItemId).ToList();
        var dupItems = itemIds.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        foreach (var id in dupItems) _validationErrors.Add($"[Item] Duplicate ID detected: {id}");

        foreach (var item in _itemData.Items)
        {
            if (item.Effects != null)
            {
                foreach (var effect in item.Effects)
                {
                    if (effect.EffectType == "AttributeBonusEffect" || effect.EffectType == "StatBonus")
                    {
                        if (!attrIds.Contains(effect.AttributeID))
                            _validationErrors.Add($"[Item {item.ItemId}] Invalid Attribute ID {effect.AttributeID} in effects.");
                    }
                }
            }
            // Check SetID
            if (!string.IsNullOrEmpty(item.SetID))
            {
                if (!_itemSetData.Sets.Any(s => s.SetID == item.SetID))
                    _validationErrors.Add($"[Item {item.ItemId}] References unknown SetID: {item.SetID}");
            }
        }

        // --- Skill Validation ---
        var skillIds = _skillData.ActiveSkills.Select(s => s.SkillID).ToList();
        var dupSkills = skillIds.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        foreach (var id in dupSkills) _validationErrors.Add($"[Skill] Duplicate ID detected: {id}");

        foreach (var skill in _skillData.ActiveSkills)
        {
            if (skill.Effects != null)
            {
                foreach (var effect in skill.Effects)
                {
                    if (effect.AttributeID > 0 && !attrIds.Contains(effect.AttributeID))
                         _validationErrors.Add($"[Skill {skill.SkillID}] Invalid Attribute ID {effect.AttributeID} in effects.");
                }
            }
        }

        // --- Item Set Validation ---
        foreach(var set in _itemSetData.Sets)
        {
            if (set.BonusLevels != null)
            {
                foreach(var level in set.BonusLevels)
                {
                    if (level.Effects != null)
                    {
                        foreach(var effect in level.Effects)
                        {
                            if (effect.EffectType == "StatBonus" && !attrIds.Contains(effect.AttributeID))
                                _validationErrors.Add($"[Set {set.SetID}] Bonus Level {level.RequiredPieces}: Invalid Attribute ID {effect.AttributeID}");
                            if ((effect.EffectType == "SkillUnlockEffect" || effect.EffectType == "AutoCastEffect") && !skillIds.Contains(effect.SkillID))
                                _validationErrors.Add($"[Set {set.SetID}] Bonus Level {level.RequiredPieces}: Invalid Skill ID {effect.SkillID}");
                        }
                    }
                }
            }
        }

        if (_validationErrors.Count == 0)
            EditorUtility.DisplayDialog("Validation", "All checks passed!", "OK");
        else
            EditorUtility.DisplayDialog("Validation", $"Found {_validationErrors.Count} issues.", "OK");
    }
}
