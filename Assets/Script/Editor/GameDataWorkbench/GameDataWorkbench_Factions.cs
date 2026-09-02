using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using CoreSystem.Data;

public partial class GameDataWorkbench : EditorWindow
{
    private static bool _showFactionBasic = true;
    private int _factionSelectedIndex = -1;

    private void DrawFactionEditor()
    {
        if (_factionData == null) _factionData = new FactionList { Factions = new List<FactionEntry>() };

        DrawEditor(_factionData.Factions, _factionSelectedIndex, (idx) => _factionSelectedIndex = idx, (entry) => {
            _showFactionBasic = EditorGUILayout.Foldout(_showFactionBasic, "Faction Info", true, EditorStyles.foldoutHeader);
            if (_showFactionBasic)
            {
                EditorGUILayout.BeginVertical(SectionStyle);
                entry.Id = EditorGUILayout.IntField("ID", entry.Id);
                entry.Name = EditorGUILayout.TextField("Name", entry.Name);
                
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Positive (Allies)", EditorStyles.boldLabel);
                DrawFactionListEditor(entry.Positive);

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Negative (Hostiles)", EditorStyles.boldLabel);
                DrawFactionListEditor(entry.Negative);

                EditorGUILayout.EndVertical();
            }
        }, (entry) => $"[{entry.Id}] {entry.Name}", 
        () => new FactionEntry { Id = _factionData.Factions.Count > 0 ? _factionData.Factions.Max(x => x.Id) + 1 : 1, Name = "New Faction", Positive = new List<int>(), Negative = new List<int>() },
        drawIcon: null, filter: null, drawHeader: null, canModify: (entry) => entry.Id > 3);
    }

    private void DrawFactionListEditor(List<int> factionList)
    {
        if (factionList == null) return;

        for (int i = 0; i < factionList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            
            string currentFactionName = "None";
            if (factionList[i] > 0)
            {
                var f = _factionData?.Factions?.Find(x => x.Id == factionList[i]);
                if (f != null) currentFactionName = $"[{f.Id}] {f.Name}";
            }

            if (GUILayout.Button(currentFactionName, EditorStyles.popup))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("None"), factionList[i] == 0, () => { factionList[i] = 0; });
                
                if (_factionData != null && _factionData.Factions != null)
                {
                    foreach (var f in _factionData.Factions)
                    {
                        int fid = f.Id;
                        int captureIndex = i;
                        menu.AddItem(new GUIContent($"[{f.Id}] {f.Name}"), factionList[captureIndex] == fid, () => { factionList[captureIndex] = fid; });
                    }
                }
                menu.ShowAsContext();
            }

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                factionList.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(EditorGUIUtility.labelWidth);
        if (GUILayout.Button("+ Add Faction Link", GUILayout.Width(150)))
        {
            factionList.Add(0);
        }
        EditorGUILayout.EndHorizontal();
    }
}
