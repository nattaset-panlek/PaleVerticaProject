using UnityEngine;
using UnityEditor;
using CoreSystem.Data;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Private Methods]
    private void DrawUniquePassiveEditor()
    {
        if (_uniquePassiveData == null || _uniquePassiveData.UniquePassives == null) return;

        DrawEditor(
            _uniquePassiveData.UniquePassives,
            _uniquePassiveSelectedIndex,
            idx => _uniquePassiveSelectedIndex = idx,
            entry => {
                BeginInspectorColumns();
                if (BeginGroup("Unique Passive Details", ThemeAccent))
                {
                    entry.Id = EditorGUILayout.TextField("Passive ID", entry.Id);
                    entry.Name = EditorGUILayout.TextField("Name", entry.Name);
                }
                EndGroup();
                NextInspectorColumn();
                if (BeginGroup("Description", ThemeAccent))
                {
                    entry.Description = EditorGUILayout.TextArea(entry.Description ?? string.Empty, EditorStyles.textArea, GUILayout.MinHeight(60));
                }
                EndGroup();
                EndInspectorColumns();
            },
            entry => new EntryView {
                Id = entry.Id,
                Name = entry.Name,
                Accent = ThemeAccent,
                Subtitle = "Unique Passive"
            },
            () => new UniquePassiveEntry { Id = "NewPassive", Name = "New Passive", Description = "" },
            PassiveFilter, DrawPassiveFilters, sorts: GetPassiveSorts(), columns: GetPassiveColumns()
        );
    }
    #endregion
}
