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

public partial class GameDataWorkbench : EditorWindow
{
    private static bool _showCharacterBasic = true;
    private static bool _showCharacterPrefab = true;

    private void DrawCharacterEditor()
    {
        DrawEditor(_characterData.Characters, _characterSelectedIndex, (idx) => _characterSelectedIndex = idx, (entry) => {
            _showCharacterBasic = EditorGUILayout.Foldout(_showCharacterBasic, "Character Info", true, EditorStyles.foldoutHeader);
            if (_showCharacterBasic)
            {
                EditorGUILayout.BeginVertical(SectionStyle);
                entry.Id = EditorGUILayout.IntField("ID", entry.Id);
                entry.Name = EditorGUILayout.TextField("Name", entry.Name);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Faction");
                
                string currentFactionName = "None";
                if (entry.Faction > 0)
                {
                    TextAsset factionJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Data/Faction.json");
                    if (factionJson != null)
                    {
                        var list = JsonUtility.FromJson<CoreSystem.Data.FactionList>(factionJson.text);
                        var f = list?.Factions?.Find(x => x.Id == entry.Faction);
                        if (f != null) currentFactionName = $"[{f.Id}] {f.Name}";
                    }
                }
                
                if (GUILayout.Button(currentFactionName, EditorStyles.popup))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("None"), entry.Faction == 0, () => { entry.Faction = 0; });
                    
                    TextAsset factionJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Data/Faction.json");
                    if (factionJson != null)
                    {
                        var list = JsonUtility.FromJson<CoreSystem.Data.FactionList>(factionJson.text);
                        if (list != null && list.Factions != null)
                        {
                            foreach (var f in list.Factions)
                            {
                                int fid = f.Id;
                                menu.AddItem(new GUIContent($"[{f.Id}] {f.Name}"), entry.Faction == fid, () => { entry.Faction = fid; });
                            }
                        }
                    }
                    menu.ShowAsContext();
                }
                EditorGUILayout.EndHorizontal();
                entry.BaseHP = EditorGUILayout.IntField("Base HP", entry.BaseHP);
                entry.BaseMP = EditorGUILayout.IntField("Base MP", entry.BaseMP);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(10);
            _showCharacterPrefab = EditorGUILayout.Foldout(_showCharacterPrefab, "Prefab Settings", true, EditorStyles.foldoutHeader);
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
        }, (entry) => $"[{entry.Id}] {entry.Name}", 
        () => new CharacterModel { Id = _characterData.Characters.Count > 0 ? _characterData.Characters.Max(x => x.Id) + 1 : 1 },
        null);
    }
}
