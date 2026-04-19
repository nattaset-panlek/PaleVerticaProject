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
    private void DrawCharacterEditor()
    {
        DrawEditor(_characterData.Characters, _characterSelectedIndex, (idx) => _characterSelectedIndex = idx, (entry) => {
            entry.Id = EditorGUILayout.IntField("ID", entry.Id);
            entry.Name = EditorGUILayout.TextField("Name", entry.Name);
            entry.Faction = (CoreSystem.Entity.FactionType)EditorGUILayout.EnumPopup("Faction", entry.Faction);
            entry.BaseHP = EditorGUILayout.IntField("Base HP", entry.BaseHP);
            entry.BaseMP = EditorGUILayout.IntField("Base MP", entry.BaseMP);
            
            GUILayout.Space(10);
            GUILayout.Label("Prefab Settings", EditorStyles.boldLabel);
            entry.PrefabPath = EditorGUILayout.TextField("Prefab Path (Addressable)", entry.PrefabPath);
            
            GameObject dropPrefab = (GameObject)EditorGUILayout.ObjectField("Drag Prefab Here", null, typeof(GameObject), false);
            if (dropPrefab != null)
            {
                entry.PrefabPath = GetAddressableKey(dropPrefab);
                // Clear focus to refresh text field
                GUI.FocusControl(null);
            }
        }, (entry) => $"[{entry.Id}] {entry.Name}", 
        () => new CharacterModel { Id = _characterData.Characters.Count > 0 ? _characterData.Characters.Max(x => x.Id) + 1 : 1 },
        null);
    }
}
