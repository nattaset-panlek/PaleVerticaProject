using UnityEngine;
using UnityEditor;
using System.Linq;
using CoreSystem.Data;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Private Methods]
    private void DrawCharacterEditor()
    {
        DrawEditor(_characterData.Characters, _characterSelectedIndex, (idx) => _characterSelectedIndex = idx, (entry) => {
            BeginInspectorColumns();
            if (BeginGroup("Character Info", ThemeAccent))
            {
                entry.Id = EditorGUILayout.IntField("ID", entry.Id);
                entry.Name = EditorGUILayout.TextField("Name", entry.Name);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Faction");
                if (GUILayout.Button(GetFactionLabel(entry.Faction), EditorStyles.popup))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("None"), entry.Faction == 0, () => { entry.Faction = 0; MarkDirty(); });
                    foreach (var f in _factionData.Factions)
                    {
                        int fid = f.Id;
                        menu.AddItem(new GUIContent($"[{f.Id}] {f.Name}"), entry.Faction == fid, () => { entry.Faction = fid; MarkDirty(); });
                    }
                    menu.ShowAsContext();
                }
                EditorGUILayout.EndHorizontal();

                entry.BaseHP = EditorGUILayout.IntField("Base HP", entry.BaseHP);
                entry.BaseMP = EditorGUILayout.IntField("Base MP", entry.BaseMP);
                entry.ExpMultiplier = EditorGUILayout.FloatField("EXP Multiplier", entry.ExpMultiplier);
                DrawDropTablePicker("Drop Table", entry.DropTableId, id => entry.DropTableId = id);
                DrawDialoguePicker("Talk Dialogue", entry.DialogueId, id => entry.DialogueId = id, false);
                entry.BaseExp = EditorGUILayout.IntField("Base EXP Override (0 = by level)", entry.BaseExp);
                entry.JobExp = EditorGUILayout.IntField("Job EXP Override (0 = by level)", entry.JobExp);
            }
            EndGroup();
            NextInspectorColumn();
            if (BeginGroup("Prefab Settings", ThemeAccent))
            {
                entry.PrefabPath = EditorGUILayout.TextField("Prefab Path (Addressable)", entry.PrefabPath);
                GameObject dropPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Drag Prefab Here", "Drag a Character Prefab here to automatically resolve and assign its Addressable key."), null, typeof(GameObject), false);
                if (dropPrefab != null)
                {
                    entry.PrefabPath = GetAddressableKey(dropPrefab);
                    GUI.FocusControl(null);
                }
                DrawPrefabLink(entry);
            }
            EndGroup();
            EndInspectorColumns();
        }, (entry) => new EntryView {
            Id = entry.Id.ToString(),
            Name = entry.Name,
            Meta = GetFactionName(entry.Faction),
            MetaColor = ThemeMuted,
            Accent = ThemeAccent,
            Subtitle = GetFactionName(entry.Faction),
            Chips = new[] { new EntryChip($"HP {entry.BaseHP}", ThemeMuted), new EntryChip($"MP {entry.BaseMP}", ThemeMuted) }
        },
        () => new CharacterModel { Id = _characterData.Characters.Count > 0 ? _characterData.Characters.Max(x => x.Id) + 1 : 1 },
        CharacterFilter, DrawCharacterFilters, sorts: GetCharacterSorts());
    }

    private void DrawPrefabLink(CharacterModel entry)
    {
        if (string.IsNullOrEmpty(entry.PrefabPath)) return;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(entry.PrefabPath);
        var character = prefab != null ? prefab.GetComponent<CoreSystem.Entity.Character>() : null;
        if (character == null)
        {
            EditorGUILayout.HelpBox("Prefab Path does not point to a prefab with a Character component.", MessageType.Warning);
            return;
        }

        var sharedIds = _characterData.Characters.Where(c => c.PrefabPath == entry.PrefabPath).Select(c => c.Id);
        if (CoreSystem.Entity.CharacterPrefabLinkFormula.IsLinked(character.CharacterId, sharedIds)) return;

        EditorGUILayout.HelpBox($"The prefab points to Character Id {character.CharacterId}, not this entry.", MessageType.Warning);
        if (GUILayout.Button("Set Prefab To This Character")) SetPrefabCharacter(entry);
    }

    private static void SetPrefabCharacter(CharacterModel entry)
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(entry.PrefabPath);
        try
        {
            var serialized = new SerializedObject(contents.GetComponent<CoreSystem.Entity.Character>());
            serialized.FindProperty("characterId").intValue = entry.Id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(contents, entry.PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    private string GetFactionName(int factionId)
    {
        if (factionId <= 0) return "None";
        var f = _factionData?.Factions?.Find(x => x.Id == factionId);
        return f != null ? f.Name : $"Missing {factionId}";
    }

    private string GetFactionLabel(int factionId)
    {
        if (factionId <= 0) return "None";
        var f = _factionData?.Factions?.Find(x => x.Id == factionId);
        return f != null ? $"[{f.Id}] {f.Name}" : $"Missing faction {factionId}";
    }
    #endregion
}
