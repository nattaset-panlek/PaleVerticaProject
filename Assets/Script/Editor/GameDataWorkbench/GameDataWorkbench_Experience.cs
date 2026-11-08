using System.IO;
using CoreSystem.Combat;
using CoreSystem.Data;
using UnityEditor;
using UnityEngine;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Variables/Fields]
    private const string ExperienceTablePath = "Assets/Resources/Data/ExperienceTable.asset";
    private const int ExperiencePageSize = 10;
    private ExperienceTable _experienceTable;
    [SerializeField] private int _experiencePage;
    [SerializeField] private int _experienceKillerLevel = 10;
    private enum ExperiencePreset { Linear, Exponential, Logarithmic, SCurve }
    #endregion

    #region [Private Methods]
    private void DrawExperienceEditor()
    {
        if (_experienceTable == null) _experienceTable = AssetDatabase.LoadAssetAtPath<ExperienceTable>(ExperienceTablePath);

        _contentScroll = EditorGUILayout.BeginScrollView(_contentScroll);
        EditorGUILayout.BeginVertical(PaddingStyle);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Experience", BigTitleStyle);
        GUILayout.FlexibleSpace();
        if (_experienceTable != null && GUILayout.Button("Ping Asset", GUILayout.Height(24), GUILayout.Width(100))) EditorGUIUtility.PingObject(_experienceTable);
        if (_experienceTable != null && PrimaryButton("Save Table", GUILayout.Height(24), GUILayout.Width(110))) SaveExperienceTable();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(8);

        if (_experienceTable == null)
        {
            EditorGUILayout.HelpBox($"No Experience Table at {ExperienceTablePath}.", MessageType.Info);
            if (PrimaryButton("Create Experience Table", GUILayout.Height(28), GUILayout.Width(200))) CreateExperienceTable();
        }
        else
        {
            BeginInspectorColumns();
            DrawLevelCurveGroups();
            NextInspectorColumn();
            DrawLevelPreviewGroup();
            EndInspectorColumns();
        }

        GUILayout.Space(8);
        DrawKillRewardGroup();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    private void DrawLevelCurveGroups()
    {
        if (BeginGroup("Level Settings", ThemeAccent))
        {
            EditorGUI.BeginChangeCheck();
            _experienceTable.MaxLevel = EditorGUILayout.IntSlider("Max Level", _experienceTable.MaxLevel, 1, 200);
            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(_experienceTable);
        }
        EndGroup();

        if (BeginGroup("Base Level Curve (EXP needed)", ThemeAccent, $"{_experienceTable.BaseExpAtMax:N0} at max"))
        {
            EditorGUI.BeginChangeCheck();
            _experienceTable.BaseExpAtMax = EditorGUILayout.LongField("EXP at Max Level", _experienceTable.BaseExpAtMax);
            _experienceTable.BaseExpCurve = EditorGUILayout.CurveField("Curve", _experienceTable.BaseExpCurve, Color.green, new Rect(0, 0, 1, 1), GUILayout.Height(60));
            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(_experienceTable);
        }
        EndGroup();

        if (BeginGroup("Job Level Curve (EXP needed)", ThemeAccent, $"{_experienceTable.JobExpAtMax:N0} at max"))
        {
            EditorGUI.BeginChangeCheck();
            _experienceTable.JobExpAtMax = EditorGUILayout.LongField("EXP at Max Level", _experienceTable.JobExpAtMax);
            _experienceTable.JobExpCurve = EditorGUILayout.CurveField("Curve", _experienceTable.JobExpCurve, Color.cyan, new Rect(0, 0, 1, 1), GUILayout.Height(60));
            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(_experienceTable);
        }
        EndGroup();

        if (BeginGroup("Curve Presets", ThemeAccent))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Linear")) ApplyExperiencePreset(ExperiencePreset.Linear);
            if (GUILayout.Button("Exponential")) ApplyExperiencePreset(ExperiencePreset.Exponential);
            if (GUILayout.Button("Logarithmic")) ApplyExperiencePreset(ExperiencePreset.Logarithmic);
            if (GUILayout.Button("S-Curve")) ApplyExperiencePreset(ExperiencePreset.SCurve);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Reset to Defaults") && EditorUtility.DisplayDialog("Reset Curves", "Overwrite the curves with the defaults?", "Reset", "Cancel"))
            {
                _experienceTable.BaseExpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                _experienceTable.JobExpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                _experienceTable.MaxLevel = 99;
                _experienceTable.BaseExpAtMax = 1000000;
                _experienceTable.JobExpAtMax = 500000;
                EditorUtility.SetDirty(_experienceTable);
            }
        }
        EndGroup();
    }

    private void DrawLevelPreviewGroup()
    {
        int totalLevels = Mathf.Min(_experienceTable.MaxLevel, 500);
        int totalPages = Mathf.Max(1, Mathf.CeilToInt(totalLevels / (float)ExperiencePageSize));
        _experiencePage = Mathf.Clamp(_experiencePage, 0, totalPages - 1);

        if (BeginGroup("EXP Needed Per Level", ThemeAccent, $"page {_experiencePage + 1}/{totalPages}"))
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Level", EditorStyles.boldLabel, GUILayout.Width(50));
            GUILayout.Label("Base", EditorStyles.boldLabel, GUILayout.Width(90));
            GUILayout.Label("Job", EditorStyles.boldLabel, GUILayout.Width(90));
            GUILayout.Label("Total Base", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            int start = _experiencePage * ExperiencePageSize;
            int end = Mathf.Min(start + ExperiencePageSize, totalLevels);
            long cumulative = 0;
            for (int i = 1; i <= start; i++) cumulative += _experienceTable.GetBaseExp(i);
            for (int level = start + 1; level <= end; level++)
            {
                long baseExp = _experienceTable.GetBaseExp(level);
                cumulative += baseExp;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(level.ToString(), GUILayout.Width(50));
                GUILayout.Label($"{baseExp:N0}", GUILayout.Width(90));
                GUILayout.Label($"{_experienceTable.GetJobExp(level):N0}", GUILayout.Width(90));
                GUILayout.Label($"{cumulative:N0}");
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = _experiencePage > 0;
            if (GUILayout.Button("Previous")) _experiencePage--;
            GUI.enabled = _experiencePage < totalPages - 1;
            if (GUILayout.Button("Next")) _experiencePage++;
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        EndGroup();
    }

    private void DrawKillRewardGroup()
    {
        if (BeginGroup("Kill Reward (EXP given by monsters)", ThemeAccent))
        {
            EditorGUILayout.HelpBox("A monster gives its Base/Job EXP Override from the Characters tab, or the level curve below when the override is 0, times its EXP Multiplier. The reward is split by damage share and scaled by the level gap.", MessageType.None);
            _experienceKillerLevel = EditorGUILayout.IntSlider("Preview Player Level", _experienceKillerLevel, 1, 200);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Monster Lv", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Base (curve)", EditorStyles.boldLabel, GUILayout.Width(90));
            GUILayout.Label("Job (curve)", EditorStyles.boldLabel, GUILayout.Width(90));
            GUILayout.Label("Level Gap x", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Solo Kill Base / Job", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            int[] offsets = { -20, -10, -6, -5, 0, 5, 10 };
            for (int i = 0; i < offsets.Length; i++)
            {
                int monsterLevel = Mathf.Max(1, _experienceKillerLevel + offsets[i]);
                long baseExp = KillRewardFormula.BaseExpForLevel(monsterLevel);
                long jobExp = KillRewardFormula.JobExpForLevel(monsterLevel);
                float gap = KillRewardFormula.LevelDifferenceMultiplier(_experienceKillerLevel, monsterLevel);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(monsterLevel.ToString(), GUILayout.Width(80));
                GUILayout.Label($"{baseExp:N0}", GUILayout.Width(90));
                GUILayout.Label($"{jobExp:N0}", GUILayout.Width(90));
                GUILayout.Label($"{gap:0.00}", GUILayout.Width(80));
                GUILayout.Label($"{KillRewardFormula.Reward(baseExp, 1f, gap, 1f):N0} / {KillRewardFormula.Reward(jobExp, 1f, gap, 1f):N0}");
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(6);
            EditorGUILayout.LabelField("Monsters", EditorStyles.boldLabel);
            if (_characterData != null && _characterData.Characters != null)
            {
                foreach (var entry in _characterData.Characters)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"[{entry.Id}] {entry.Name}", GUILayout.Width(180));
                    EditorGUI.BeginChangeCheck();
                    GUILayout.Label("Base", GUILayout.Width(34));
                    entry.BaseExp = EditorGUILayout.IntField(entry.BaseExp, GUILayout.Width(70));
                    GUILayout.Label("Job", GUILayout.Width(28));
                    entry.JobExp = EditorGUILayout.IntField(entry.JobExp, GUILayout.Width(70));
                    GUILayout.Label("x", GUILayout.Width(12));
                    entry.ExpMultiplier = EditorGUILayout.FloatField(entry.ExpMultiplier, GUILayout.Width(50));
                    if (EditorGUI.EndChangeCheck()) MarkDirty();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.LabelField("0 = use the level curve. Save with Ctrl+S (writes Character.json).", EditorStyles.miniLabel);
            }
        }
        EndGroup();
    }

    private void ApplyExperiencePreset(ExperiencePreset preset)
    {
        switch (preset)
        {
            case ExperiencePreset.Linear:
                _experienceTable.BaseExpCurve = AnimationCurve.Linear(0, 0, 1, 1);
                _experienceTable.JobExpCurve = AnimationCurve.Linear(0, 0, 1, 1);
                break;
            case ExperiencePreset.Exponential:
                _experienceTable.BaseExpCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 0.1f), new Keyframe(1, 1));
                _experienceTable.JobExpCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 0.08f), new Keyframe(1, 1));
                break;
            case ExperiencePreset.Logarithmic:
                _experienceTable.BaseExpCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 0.7f), new Keyframe(1, 1));
                _experienceTable.JobExpCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 0.65f), new Keyframe(1, 1));
                break;
            case ExperiencePreset.SCurve:
                _experienceTable.BaseExpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                _experienceTable.JobExpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                break;
        }
        EditorUtility.SetDirty(_experienceTable);
    }

    private void SaveExperienceTable()
    {
        if (_experienceTable == null) return;
        EditorUtility.SetDirty(_experienceTable);
        AssetDatabase.SaveAssetIfDirty(_experienceTable);
    }

    private void CreateExperienceTable()
    {
        string dir = Path.GetDirectoryName(ExperienceTablePath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        _experienceTable = CreateInstance<ExperienceTable>();
        AssetDatabase.CreateAsset(_experienceTable, ExperienceTablePath);
        AssetDatabase.SaveAssets();
    }
    #endregion
}
