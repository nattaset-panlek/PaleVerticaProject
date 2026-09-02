using UnityEngine;
using UnityEditor;
using CoreSystem.Data;

public partial class GameDataWorkbench : EditorWindow
{
    private static bool _showPassiveDetails = true;

    #region [Private Methods]
    private void DrawUniquePassiveEditor()
    {
        if (_uniquePassiveData == null || _uniquePassiveData.UniquePassives == null) return;

        DrawEditor(
            _uniquePassiveData.UniquePassives,
            _uniquePassiveSelectedIndex,
            idx => _uniquePassiveSelectedIndex = idx,
            entry => {
                _showPassiveDetails = EditorGUILayout.BeginFoldoutHeaderGroup(_showPassiveDetails, "Unique Passive Details");
                if (_showPassiveDetails)
                {
                    EditorGUILayout.BeginVertical(SectionStyle);
                    EditorGUI.BeginChangeCheck();
                    string id = EditorGUILayout.TextField("Passive ID", entry.Id);
                    string name = EditorGUILayout.TextField("Name", entry.Name);
                    EditorGUILayout.LabelField("Description");
                    string desc = EditorGUILayout.TextArea(entry.Description, GUILayout.Height(60));
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        entry.Id = id;
                        entry.Name = name;
                        entry.Description = desc;
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            },
            entry => $"{entry.Id} - {entry.Name}",
            () => new UniquePassiveEntry { Id = "NewPassive", Name = "New Passive", Description = "" }
        );
    }
    #endregion
}
