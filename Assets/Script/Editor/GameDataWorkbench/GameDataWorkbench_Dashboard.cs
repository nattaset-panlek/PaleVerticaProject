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
    #region [Private Methods]
    private void DrawDashboard()
    {
        _contentScroll = EditorGUILayout.BeginScrollView(_contentScroll);
        EditorGUILayout.BeginVertical(PaddingStyle);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Project Overview", BigTitleStyle);
        GUILayout.FlexibleSpace();
        if (PrimaryButton("Run Full Validation", GUILayout.Height(24), GUILayout.Width(150))) RunValidation(true);
        if (GUILayout.Button("Reload Everything", GUILayout.Height(24), GUILayout.Width(130)))
        {
            if (!_isDirty || EditorUtility.DisplayDialog("Reload Data", "You have unsaved changes. Reload and lose them?", "Reload", "Cancel"))
                LoadAllData();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(12);

        EditorGUILayout.BeginHorizontal();
        DrawDashboardStat("Attributes", _attributeData.Attributes.Count, ToolTab.Attributes);
        DrawDashboardStat("Items", _itemData.Items.Count, ToolTab.Items);
        DrawDashboardStat("Active Skills", _skillData.ActiveSkills.Count, ToolTab.Skills);
        DrawDashboardStat("Support Mods", _supportSkillData.SupportModifiers.Count, ToolTab.SupportSkills);
        DrawDashboardStat("Item Sets", _itemSetData.Sets.Count, ToolTab.Sets);
        DrawDashboardStat("Characters", _characterData.Characters.Count, ToolTab.Characters);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(12);

        if (BeginGroup("Distribution", ThemeAccent))
        {
            GUILayout.Label("Item Rarity Distribution", EditorStyles.boldLabel);
            DrawRarityChart();
            EditorGUILayout.Space(8);
            GUILayout.Label("Active Skill Element Distribution", EditorStyles.boldLabel);
            DrawElementChart();
        }
        EndGroup();

        string healthLabel = !_isValidated ? "not run" : _validationErrors.Count == 0 ? "all checks passed" : $"{_validationErrors.Count} issues";
        if (BeginGroup("Data Integrity Health", _validationErrors.Count > 0 ? ThemeWarn : ThemeOk, healthLabel))
        {
            if (!_isValidated)
                DrawStatusLine("Validation has not run yet.", MessageType.Info);
            else if (_validationErrors.Count == 0)
                DrawStatusLine("All data checks passed.", MessageType.Info);
            else
            {
                DrawValidationErrorGroup("Items", _validationErrors.Where(e => e.Contains("[Item")).ToList());
                DrawValidationErrorGroup("Skills", _validationErrors.Where(e => e.Contains("[Skill")).ToList());
                DrawValidationErrorGroup("Attributes", _validationErrors.Where(e => e.Contains("[Attribute")).ToList());
                DrawValidationErrorGroup("Item Sets", _validationErrors.Where(e => e.Contains("[Set")).ToList());
            }
        }
        EndGroup();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    private void DrawDashboardStat(string label, int value, ToolTab target)
    {
        Rect r = GUILayoutUtility.GetRect(80, 58, GUILayout.ExpandWidth(true));
        r.width -= 8;
        EditorGUI.DrawRect(r, ThemeGroup);
        DrawBorder(r, ThemeLine);
        GUI.Label(new Rect(r.x + 10, r.y + 6, r.width - 16, 14), label, MutedStyle);
        GUI.Label(new Rect(r.x + 10, r.y + 22, r.width - 16, 28), value.ToString(), StatValueStyle);
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && r.Contains(Event.current.mousePosition))
        {
            SelectTab(target);
            Event.current.Use();
        }
    }

    private void DrawDistributionBar(List<(string label, int count, Color color)> parts, Color textColor)
    {
        float total = parts.Sum(p => p.count);
        Rect bar = GUILayoutUtility.GetRect(10, 24, GUILayout.ExpandWidth(true));
        if (total <= 0)
        {
            EditorGUI.DrawRect(bar, ThemeField);
            return;
        }
        GUIStyle style = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = textColor } };
        float x = bar.x;
        foreach (var p in parts)
        {
            float w = bar.width * (p.count / total);
            Rect r = new Rect(x, bar.y, w, bar.height);
            EditorGUI.DrawRect(r, p.color);
            if (w > 40) GUI.Label(r, $"{p.label} {p.count}", style);
            x += w;
        }
    }

    private void DrawRarityChart()
    {
        var parts = _itemData.Items.GroupBy(i => i.Rarity).OrderByDescending(g => g.Count())
            .Select(g => (g.Key ?? "None", g.Count(), System.Enum.TryParse(g.Key, out ItemRarity rarity) ? rarity.GetColor() : ThemeMuted)).ToList();
        DrawDistributionBar(parts, Hex(0x1E1E1E));
    }

    private void DrawElementChart()
    {
        var parts = _skillData.ActiveSkills.GroupBy(s => s.SkillElement).OrderByDescending(g => g.Count())
            .Select(g => (g.Key ?? "None", g.Count(), System.Enum.TryParse(g.Key, out ElementType element) ? GetElementColor(element) : ThemeMuted)).ToList();
        DrawDistributionBar(parts, Hex(0x1E1E1E));
    }

    private void DrawValidationErrorGroup(string title, List<string> errors)
    {
        if (errors.Count == 0) return;
        GUILayout.Label(title, EditorStyles.boldLabel);
        foreach (var err in errors) DrawStatusLine(err, MessageType.Warning);
        EditorGUILayout.Space(4);
    }

    private void RunValidation(bool showDialog)
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

        if (!showDialog) return;
        if (_validationErrors.Count == 0)
            EditorUtility.DisplayDialog("Validation", "All checks passed!", "OK");
        else
            EditorUtility.DisplayDialog("Validation", $"Found {_validationErrors.Count} issues.", "OK");
    }
    #endregion
}
