using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using CoreSystem.Data;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Variables/Fields]
    [SerializeField] private int _factionSelectedIndex = -1;
    #endregion

    #region [Private Methods]
    private void DrawFactionEditor()
    {
        if (_factionData == null) _factionData = new FactionList { Factions = new List<FactionEntry>() };

        DrawEditor(_factionData.Factions, _factionSelectedIndex, (idx) => _factionSelectedIndex = idx, (entry) => {
            BeginInspectorColumns();
            if (BeginGroup("Faction Info", ThemeAccent))
            {
                entry.Id = EditorGUILayout.IntField("ID", entry.Id);
                entry.Name = EditorGUILayout.TextField("Name", entry.Name);
            }
            EndGroup();
            NextInspectorColumn();
            if (BeginGroup("Positive (Allies)", ThemeOk, $"{entry.Positive?.Count ?? 0} links"))
            {
                if (entry.Positive == null) entry.Positive = new List<int>();
                DrawFactionListEditor(entry.Positive);
            }
            EndGroup();
            if (BeginGroup("Negative (Hostiles)", ThemeError, $"{entry.Negative?.Count ?? 0} links"))
            {
                if (entry.Negative == null) entry.Negative = new List<int>();
                DrawFactionListEditor(entry.Negative);
            }
            EndGroup();
            EndInspectorColumns();
        }, (entry) => new EntryView {
            Id = entry.Id.ToString(),
            Name = entry.Name,
            Meta = $"+{entry.Positive?.Count ?? 0} / -{entry.Negative?.Count ?? 0}",
            MetaColor = ThemeMuted,
            Accent = ThemeAccent,
            Subtitle = "Faction"
        },
        () => new FactionEntry { Id = _factionData.Factions.Count > 0 ? _factionData.Factions.Max(x => x.Id) + 1 : 1, Name = "New Faction", Positive = new List<int>(), Negative = new List<int>() },
        filter: FactionFilter, drawHeader: DrawFactionFilters, canModify: (entry) => entry.Id > 3, sorts: GetFactionSorts(), columns: GetFactionColumns());
    }

    private void DrawFactionListEditor(List<int> factionList)
    {
        if (factionList == null) return;

        for (int i = 0; i < factionList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(GetFactionLabel(factionList[i]), EditorStyles.popup))
            {
                GenericMenu menu = new GenericMenu();
                int captureIndex = i;
                menu.AddItem(new GUIContent("None"), factionList[captureIndex] == 0, () => { factionList[captureIndex] = 0; MarkDirty(); });
                foreach (var f in _factionData.Factions)
                {
                    int fid = f.Id;
                    menu.AddItem(new GUIContent($"[{f.Id}] {f.Name}"), factionList[captureIndex] == fid, () => { factionList[captureIndex] = fid; MarkDirty(); });
                }
                menu.ShowAsContext();
            }
            if (GUILayout.Button("X", GUILayout.Width(24)))
            {
                factionList.RemoveAt(i);
                MarkDirty();
                EditorGUILayout.EndHorizontal();
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Add Faction Link", EditorStyles.miniButton, GUILayout.Width(140)))
        {
            factionList.Add(0);
            MarkDirty();
        }
    }
    #endregion
}
