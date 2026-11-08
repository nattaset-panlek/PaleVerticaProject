using System.Collections.Generic;
using System.Linq;
using CoreSystem.Loot;
using UnityEditor;
using UnityEngine;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Variables/Fields]
    private string _dropTableJsonPath = "Assets/Resources/Data/DropTable.json";
    private DropTableDataList _dropTableData;
    private int _dropTableSelectedIndex = -1;
    #endregion

    #region [Private Methods]
    private void DrawDropTableEditor()
    {
        DrawEditor(_dropTableData.Tables, _dropTableSelectedIndex, idx => _dropTableSelectedIndex = idx, DrawDropTableFields, DescribeDropTable, CreateDropTable);
    }

    private void DrawDropTableFields(DropTableModel entry)
    {
        if (entry.Entries == null) entry.Entries = new List<DropEntry>();
        foreach (string problem in DropTableDataValidator.Validate(entry, _dropTableData.Tables, ItemExists))
        {
            EditorGUILayout.HelpBox(problem, MessageType.Warning);
        }

        BeginInspectorColumns();
        if (BeginGroup("Drop Table", ThemeAccent))
        {
            entry.Id = EditorGUILayout.IntField("ID", entry.Id);
            entry.Name = EditorGUILayout.TextField("Name", entry.Name);
            DrawDropTableUsers(entry);
        }
        EndGroup();

        if (BeginGroup("All Drop Tables", ThemeAccent))
        {
            _dropTableData.GroundLifetimeSeconds = EditorGUILayout.FloatField("Seconds On Ground", _dropTableData.GroundLifetimeSeconds);
            _dropTableData.OwnerLockSeconds = EditorGUILayout.FloatField("Owner Lock Seconds", _dropTableData.OwnerLockSeconds);
        }
        EndGroup();

        NextInspectorColumn();
        if (BeginGroup("Items", ThemeAccent, entry.Entries.Count.ToString())) DrawDropEntries(entry);
        EndGroup();
        EndInspectorColumns();
    }

    private void DrawDropTableUsers(DropTableModel entry)
    {
        var users = _characterData.Characters.Where(c => c.DropTableId == entry.Id).Select(c => c.Name).ToList();
        EditorGUILayout.LabelField(users.Count > 0 ? "Used by: " + string.Join(", ", users) : "Not used by any character.", EditorStyles.wordWrappedMiniLabel);
    }

    private void DrawDropEntries(DropTableModel entry)
    {
        EditorGUILayout.LabelField("Each item rolls on its own, so one kill can drop several items.", EditorStyles.wordWrappedMiniLabel);

        int removeIndex = -1;
        for (int i = 0; i < entry.Entries.Count; i++)
        {
            DropEntry drop = entry.Entries[i];
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawItemPicker("Item", drop.ItemId, id => drop.ItemId = id);
            float percent = EditorGUILayout.Slider("Drop Chance %", drop.Chance * 100f, 0f, 100f);
            drop.Chance = percent / 100f;
            drop.MinQty = EditorGUILayout.IntField("Min Quantity", drop.MinQty);
            drop.MaxQty = EditorGUILayout.IntField("Max Quantity", drop.MaxQty);
            if (GUILayout.Button("Remove Item", EditorStyles.miniButton)) removeIndex = i;
            EditorGUILayout.EndVertical();
        }

        if (removeIndex >= 0)
        {
            entry.Entries.RemoveAt(removeIndex);
            MarkDirty();
        }

        if (GUILayout.Button("+ Item"))
        {
            entry.Entries.Add(new DropEntry());
            MarkDirty();
        }
    }

    private void DrawDropTablePicker(string label, int currentId, System.Action<int> onSelected)
    {
        DropTableModel current = _dropTableData.Tables.Find(t => t.Id == currentId);
        string text = current != null ? $"[{current.Id}] {current.Name}" : currentId > 0 ? $"Missing {currentId}" : "None";

        DrawPickerButton(label, text, menu =>
        {
            menu.AddItem(new GUIContent("None"), currentId == 0, () => Pick(onSelected, 0));
            foreach (DropTableModel table in _dropTableData.Tables.OrderBy(t => t.Id))
            {
                int id = table.Id;
                menu.AddItem(new GUIContent($"[{id}] {table.Name}"), id == currentId, () => Pick(onSelected, id));
            }
        });
    }

    private bool ItemExists(int id)
    {
        return _itemData.Items.Any(item => item.ItemId == id);
    }

    private EntryView DescribeDropTable(DropTableModel entry)
    {
        int problems = DropTableDataValidator.Validate(entry, _dropTableData.Tables, ItemExists).Count;
        return new EntryView
        {
            Id = entry.Id.ToString(),
            Name = entry.Name,
            Meta = problems > 0 ? $"{problems} issue(s)" : string.Empty,
            MetaColor = ThemeMuted,
            Accent = ThemeAccent,
            Subtitle = $"{entry.Entries?.Count ?? 0} item(s)"
        };
    }

    private DropTableModel CreateDropTable()
    {
        return new DropTableModel
        {
            Id = _dropTableData.Tables.Count > 0 ? _dropTableData.Tables.Max(t => t.Id) + 1 : 1,
            Name = "New Drop Table"
        };
    }
    #endregion
}
