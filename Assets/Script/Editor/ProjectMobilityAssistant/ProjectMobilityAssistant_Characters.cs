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
using CoreSystem.Data;

public partial class ProjectMobilityAssistant : EditorWindow
{
    private static bool _showCharacterBasic = true;
    private static bool _showCharacterPrefab = true;

    private void DrawCharacterEditor()
    {
        DrawEditor(_characterData.Characters, _characterSelectedIndex, (idx) => _characterSelectedIndex = idx, (entry) => {
            _showCharacterBasic = EditorGUILayout.BeginFoldoutHeaderGroup(_showCharacterBasic, "Character Info");
            if (_showCharacterBasic)
            {
                EditorGUILayout.BeginVertical(SectionStyle);
                entry.Id = EditorGUILayout.IntField("ID", entry.Id);
                entry.Name = EditorGUILayout.TextField("Name", entry.Name);
                entry.Faction = (CoreSystem.Entity.FactionType)EditorGUILayout.EnumPopup("Faction", entry.Faction);
                entry.BaseHP = EditorGUILayout.IntField("Base HP", entry.BaseHP);
                entry.BaseMP = EditorGUILayout.IntField("Base MP", entry.BaseMP);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            EditorGUILayout.Space(10);
            _showCharacterPrefab = EditorGUILayout.BeginFoldoutHeaderGroup(_showCharacterPrefab, "Prefab Settings");
            if (_showCharacterPrefab)
            {
                EditorGUILayout.BeginVertical(SectionStyle);
                entry.PrefabPath = EditorGUILayout.TextField("Prefab Path (Addressable)", entry.PrefabPath);
                
                GameObject dropPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Drag Prefab Here", "Drag a Character Prefab here to automatically resolve and assign its Addressable key."), null, typeof(GameObject), false);
                if (dropPrefab != null)
                {
                    entry.PrefabPath = GetAddressableKey(dropPrefab);
                    GUI.FocusControl(null);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }, (entry) => $"[{entry.Id}] {entry.Name}", 
        () => new CharacterModel { Id = _characterData.Characters.Count > 0 ? _characterData.Characters.Max(x => x.Id) + 1 : 1 },
        null);
    }
}
