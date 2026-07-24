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

public partial class ProjectMobilityAssistant : EditorWindow
{
    private Vector2 _idMgrScroll;
    private static bool _showIdItems = true;
    private static bool _showIdAttributes = true;
    private static bool _showIdActiveSkills = true;
    private static bool _showIdSupportMods = true;
    private static bool _showIdCharacters = true;

    private void DrawIdManager()
    {
        _idMgrScroll = EditorGUILayout.BeginScrollView(_idMgrScroll);
        EditorGUILayout.BeginVertical(PaddingStyle);
        GUILayout.Label("ID Manager", new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 });
        EditorGUILayout.Space(4);

        _showIdItems = EditorGUILayout.BeginFoldoutHeaderGroup(_showIdItems, "Items");
        if (_showIdItems)
        {
            EditorGUILayout.BeginVertical(SectionStyle);
            DrawItemIdSection();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        _showIdAttributes = EditorGUILayout.BeginFoldoutHeaderGroup(_showIdAttributes, "Attributes");
        if (_showIdAttributes)
        {
            EditorGUILayout.BeginVertical(SectionStyle);
            DrawGenericIdSection(
                "Attributes",
                _attributeData.Attributes.Select(a => a.Id).ToList(),
                "Attributes",
                (start) => { RenumberGeneric(_attributeData.Attributes, (e, id) => e.Id = id, start); SaveAllData(); }
            );
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        _showIdActiveSkills = EditorGUILayout.BeginFoldoutHeaderGroup(_showIdActiveSkills, "Active Skills");
        if (_showIdActiveSkills)
        {
            EditorGUILayout.BeginVertical(SectionStyle);
            DrawGenericIdSection(
                "Active Skills",
                _skillData.ActiveSkills.Select(s => s.SkillID).ToList(),
                "Active Skills",
                (start) => { RenumberGeneric(_skillData.ActiveSkills, (e, id) => e.SkillID = id, start); SaveAllData(); }
            );
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        _showIdSupportMods = EditorGUILayout.BeginFoldoutHeaderGroup(_showIdSupportMods, "Support Mods");
        if (_showIdSupportMods)
        {
            EditorGUILayout.BeginVertical(SectionStyle);
            DrawGenericIdSection(
                "Support Mods",
                _supportSkillData.SupportModifiers.Select(s => s.SkillID).ToList(),
                "Support Mods",
                (start) => { RenumberGeneric(_supportSkillData.SupportModifiers, (e, id) => e.SkillID = id, start); SaveAllData(); }
            );
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        _showIdCharacters = EditorGUILayout.BeginFoldoutHeaderGroup(_showIdCharacters, "Characters");
        if (_showIdCharacters)
        {
            EditorGUILayout.BeginVertical(SectionStyle);
            DrawGenericIdSection(
                "Characters",
                _characterData.Characters.Select(c => c.Id).ToList(),
                "Characters",
                (start) => { RenumberGeneric(_characterData.Characters, (e, id) => e.Id = id, start); SaveAllData(); }
            );
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    private void DrawItemIdSection()
    {
        var allIds = _itemData.Items.Select(i => i.ItemId).ToList();
        var duplicates = allIds.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

        // Summary row
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"Total: {_itemData.Items.Count}", GUILayout.Width(100));
        GUI.color = duplicates.Count > 0 ? Color.red : Color.green;
        GUILayout.Label($"Duplicates: {duplicates.Count}", GUILayout.Width(120));
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();

        // Per-type group table
        EditorGUILayout.Space(4);
        var keys = _itemPrefixes.Keys.ToList();
        
        foreach (var typeName in keys)
        {
            int prefix = _itemPrefixes[typeName];
            var ids = _itemData.Items.Where(i => i.ItemId / 1000 == prefix).Select(i => i.ItemId).ToList();
            int nextId = ids.Count > 0 ? ids.Max() + 1 : prefix * 1000 + 1;
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(typeName, GUILayout.Width(100));
            _itemPrefixes[typeName] = EditorGUILayout.IntField(_itemPrefixes[typeName], GUILayout.Width(40));
            GUILayout.Label("x 1000", GUILayout.Width(50));
            GUILayout.Label($"Count: {ids.Count}", GUILayout.Width(80));
            GUILayout.Label($"Next: {nextId}", EditorStyles.miniLabel, GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(4);
        if (duplicates.Count > 0)
        {
            GUI.color = Color.red;
            GUILayout.Label("Duplicate IDs: " + string.Join(", ", duplicates));
            GUI.color = Color.white;
        }

        EditorGUILayout.Space(4);
        GUI.backgroundColor = new Color(1f, 0.7f, 0.2f);
        if (GUILayout.Button("Auto-renumber Items (by type prefix)", GUILayout.Height(28)))
        {
            if (EditorUtility.DisplayDialog("Auto-renumber Items",
                "\u0E0C\u0E30 reassign ItemId \u0E43\u0E2B\u0E21\u0E48\u0E17\u0E31\u0E49\u0E07\u0E21\u0E14\u0E15\u0E32\u0E21 type prefix (1xxx, 2xxx...)\n\u0E14\u0E33\u0E40\u0E19\u0E34\u0E19\u0E01\u0E32\u0E23\u0E15\u0E48\u0E2D?", "\u0E43\u0E0A\u0E48", "\u0E22\u0E01\u0E40\u0E25\u0E34\u0E01"))
            {
                RenumberItems();
                SaveAllData();
            }
        }
        GUI.backgroundColor = Color.white;
    }

    private void DrawGenericIdSection(string label, List<int> ids, string dictKey, System.Action<int> onRenumber)
    {
        int startId = _startIds.ContainsKey(dictKey) ? _startIds[dictKey] : 1;

        var duplicates = ids.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        int minId = ids.Count > 0 ? ids.Min() : startId;
        int maxId = ids.Count > 0 ? ids.Max() : startId;
        var gaps   = ids.Count > 1
            ? Enumerable.Range(minId, maxId - minId + 1).Except(ids).ToList()
            : new List<int>();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"Count: {ids.Count}", GUILayout.Width(100));
        GUI.color = duplicates.Count > 0 ? Color.red : Color.green;
        GUILayout.Label($"Duplicates: {duplicates.Count}", GUILayout.Width(120));
        GUI.color = gaps.Count > 0 ? new Color(1f, 0.7f, 0f) : Color.white;
        GUILayout.Label($"Gaps: {gaps.Count}", GUILayout.Width(80));
        GUI.color = Color.white;
        int nextId = ids.Count > 0 ? ids.Max() + 1 : startId;
        GUILayout.Label($"Next: {nextId}", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Start ID:", GUILayout.Width(80));
        startId = EditorGUILayout.IntField(startId, GUILayout.Width(100));
        _startIds[dictKey] = startId;
        EditorGUILayout.EndHorizontal();

        if (duplicates.Count > 0)
        {
            GUI.color = Color.red;
            GUILayout.Label("Duplicates: " + string.Join(", ", duplicates));
            GUI.color = Color.white;
        }
        if (gaps.Count > 0 && gaps.Count <= 20)
        {
            GUI.color = new Color(1f, 0.65f, 0f);
            GUILayout.Label("Gaps: " + string.Join(", ", gaps));
            GUI.color = Color.white;
        }

        EditorGUILayout.Space(4);
        GUI.backgroundColor = new Color(1f, 0.7f, 0.2f);
        if (GUILayout.Button($"Auto-renumber {label}", GUILayout.Height(26)))
        {
            if (EditorUtility.DisplayDialog($"Auto-renumber {label}",
                $"\u0E0C\u0E30 reassign ID \u0E43\u0E2B\u0E21\u0E48\u0E14\u0E48\u0E2D\u0E40\u0E19\u0E37\u0E48\u0E2D\u0E07\u0E15\u0E31\u0E49\u0E07\u0E41\u0E15\u0E48 {startId}\n\u0E14\u0E33\u0E40\u0E19\u0E34\u0E19\u0E01\u0E32\u0E23\u0E15\u0E48\u0E2D?", "\u0E43\u0E0A\u0E48", "\u0E22\u0E01\u0E40\u0E25\u0E34\u0E01"))
                onRenumber(startId);
        }
        GUI.backgroundColor = Color.white;
    }

    private void RenumberItems()
    {
        foreach (var kvp in _itemPrefixes)
        {
            string typeName = kvp.Key;
            int prefix = kvp.Value;

            var group = _itemData.Items
                .Where(i => i.ItemType == typeName)
                .OrderBy(i => i.ItemId)
                .ToList();
            int counter = prefix * 1000 + 1;
            foreach (var item in group)
                item.ItemId = counter++;
        }
        // Items whose type didn't match any prefix get renumbered from 9001
        int fallback = 9001;
        foreach (var item in _itemData.Items)
        {
            bool matched = _itemPrefixes.ContainsKey(item.ItemType);
            if (!matched) item.ItemId = fallback++;
        }
    }

    private void RenumberGeneric<T>(List<T> list, System.Action<T, int> setter, int startId = 1)
    {
        var sorted = list.OrderBy(e => {
            // Try get current ID via reflection for sort
            var p = e.GetType().GetProperty("Id") ?? e.GetType().GetProperty("SkillID") ?? e.GetType().GetProperty("ItemId");
            return p != null ? (int)p.GetValue(e) : 0;
        }).ToList();
        int counter = startId;
        for (int i = 0; i < sorted.Count; i++)
            setter(sorted[i], counter++);
    }
}
