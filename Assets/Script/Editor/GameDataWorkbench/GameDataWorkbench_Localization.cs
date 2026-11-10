using System.Collections.Generic;
using System.Linq;
using CoreSystem.Data;
using CoreSystem.Editor.Localization;
using CoreSystem.Localization;
using UnityEditor;
using UnityEngine;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Variables/Fields]
    [SerializeField] private int _localizationSelectedIndex = -1;
    [SerializeField] private bool _localizationMissingOnly;
    private LocalizationTable _localizationData;
    #endregion

    #region [Private Methods]
    private void LoadLocalizationData()
    {
        _localizationData = LocalizationCoverageFormula.LoadTable(LocalizationDatabase.AssetPath);
        if (_localizationData.Entries == null) _localizationData.Entries = new List<LocalizationEntry>();
    }

    private void SaveLocalizationData()
    {
        if (_localizationData == null) return;
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(LocalizationDatabase.AssetPath));
        SaveData(LocalizationDatabase.AssetPath, _localizationData);
        LocalizationDatabase.Reload();
    }

    private void DrawLocalizationEditor()
    {
        if (_localizationData == null) LoadLocalizationData();

        DrawEditor(_localizationData.Entries, _localizationSelectedIndex, (idx) => _localizationSelectedIndex = idx, (entry) => {
            if (entry.Text == null) entry.Text = new LocalizedString();
            List<string> missing = LocalizationFormula.MissingLanguages(entry.Text);
            if (BeginGroup("Text", missing.Count == 0 ? ThemeOk : ThemeWarn, missing.Count == 0 ? "EN and TH done" : $"missing {string.Join(", ", missing)}"))
            {
                EditorGUI.BeginChangeCheck();
                entry.Key = EditorGUILayout.TextField("Key", entry.Key);
                EditorGUILayout.LabelField("EN");
                entry.Text.EN = EditorGUILayout.TextArea(entry.Text.EN ?? string.Empty, GUILayout.MinHeight(36f));
                EditorGUILayout.LabelField("TH");
                entry.Text.TH = EditorGUILayout.TextArea(entry.Text.TH ?? string.Empty, GUILayout.MinHeight(36f));
                if (EditorGUI.EndChangeCheck()) MarkDirty();
                EditorGUILayout.HelpBox("Every key needs EN and TH. Use {0}, {1} for values the code fills in. Keys use dots: area.screen.name, for example settings.language.", MessageType.None);
            }
            EndGroup();
        }, (entry) => new EntryView {
            Id = entry.Key,
            Name = entry.Text != null ? entry.Text.EN : string.Empty,
            Meta = entry.Text != null && entry.Text.IsComplete ? "EN TH" : "missing",
            MetaColor = entry.Text != null && entry.Text.IsComplete ? ThemeMuted : ThemeWarn,
            Accent = ThemeAccent,
            Subtitle = entry.Text != null ? entry.Text.TH : string.Empty
        },
        () => new LocalizationEntry { Key = NewLocalizationKey(), Text = new LocalizedString { EN = string.Empty, TH = string.Empty } },
        filter: LocalizationFilter, drawHeader: DrawLocalizationFilters, sorts: GetLocalizationSorts(), columns: GetLocalizationColumns());
    }

    private string NewLocalizationKey()
    {
        int index = _localizationData.Entries.Count + 1;
        string key = $"new.key.{index}";
        while (_localizationData.Entries.Any(e => e.Key == key)) key = $"new.key.{++index}";
        return key;
    }

    private bool LocalizationFilter(LocalizationEntry entry)
    {
        return !_localizationMissingOnly || entry.Text == null || !entry.Text.IsComplete;
    }

    private void DrawLocalizationFilters()
    {
        int missing = _localizationData.Entries.Count(e => e.Text == null || !e.Text.IsComplete);
        _localizationMissingOnly = GUILayout.Toggle(_localizationMissingOnly, $"Missing only ({missing})", EditorStyles.toolbarButton, GUILayout.Width(120f));
    }

    private (string name, System.Func<LocalizationEntry, System.IComparable> key)[] GetLocalizationSorts()
    {
        return new (string, System.Func<LocalizationEntry, System.IComparable>)[]
        {
            ("Key", e => e.Key ?? string.Empty),
            ("Missing first", e => e.Text != null && e.Text.IsComplete ? 1 : 0)
        };
    }

    private (string header, float width, System.Func<LocalizationEntry, string> text, System.Func<LocalizationEntry, Color?> chip)[] GetLocalizationColumns()
    {
        return new (string, float, System.Func<LocalizationEntry, string>, System.Func<LocalizationEntry, Color?>)[]
        {
            ("Key", 220f, e => e.Key, e => null),
            ("EN", 240f, e => e.Text?.EN, e => string.IsNullOrWhiteSpace(e.Text?.EN) ? ThemeWarn : (Color?)null),
            ("TH", 240f, e => e.Text?.TH, e => string.IsNullOrWhiteSpace(e.Text?.TH) ? ThemeWarn : (Color?)null)
        };
    }
    #endregion
}
