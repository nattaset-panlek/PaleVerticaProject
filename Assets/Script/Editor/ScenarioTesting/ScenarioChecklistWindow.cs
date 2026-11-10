using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScenarioTesting;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Editor.ScenarioTesting
{
    public sealed class ScenarioChecklistWindow : EditorWindow
    {
        #region [Variables/Fields]
        private const string MenuPath = "Pale Vertica/Testing/Scenario Checklist";
        private const float ListWidth = 330f;
        private static readonly Color PassColor = new Color(0.4f, 0.8f, 0.4f);
        private static readonly Color ReviewColor = new Color(1f, 0.75f, 0.25f);
        private static readonly Color FailColor = new Color(1f, 0.4f, 0.4f);
        private static readonly Color IdleColor = new Color(0.6f, 0.6f, 0.6f);

        private readonly List<Entry> _entries = new List<Entry>();
        private readonly HashSet<string> _checked = new HashSet<string>();
        private ScenarioReviewStore _reviews;
        private Entry _selected;
        private Vector2 _listScroll;
        private Vector2 _detailScroll;
        private string _filter = string.Empty;
        private string _reviewNote = string.Empty;
        private bool _showActions;
        private bool _exitWhenDone;
        private float _speed = 1f;
        private bool _showHistory = true;
        private string _historyFile;
        private ScenarioReport _historyReport;
        private const int HistoryRows = 15;
        private static readonly int[] HistoryLimitOptions = { 0, 1, 5, 20 };
        #endregion

        #region [Unity Methods]
        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<ScenarioChecklistWindow>("Scenario Checklist").Refresh();
        }

        private void OnEnable()
        {
            Refresh();
            ListenForRuns();
            EditorApplication.playModeStateChanged += HandlePlayModeChanged;
        }

        private void OnDisable()
        {
            ScenarioManager.OnRunFinished -= HandleRunFinished;
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
        }

        private void OnInspectorUpdate()
        {
            if (!EditorApplication.isPlaying) return;
            ListenForRuns();
            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.BeginHorizontal();
            DrawList();
            DrawDetails();
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region [Private Methods]
        private void Refresh()
        {
            _reviews = ScenarioReviewStore.Load();
            string selectedFile = _selected?.File;
            _entries.Clear();
            foreach (string file in ScenarioLibrary.AllFiles())
            {
                ScenarioDefinition definition = ScenarioDefinition.Load(file);
                _entries.Add(new Entry { File = file, Definition = definition, Report = ScenarioReport.Read(ScenarioLibrary.ReportPath(definition)) });
            }
            _selected = _entries.FirstOrDefault(entry => entry.File == selectedFile) ?? _entries.FirstOrDefault();
            Repaint();
        }

        private void ListenForRuns()
        {
            ScenarioManager.OnRunFinished -= HandleRunFinished;
            ScenarioManager.OnRunFinished += HandleRunFinished;
        }

        private void HandleRunFinished(ScenarioRun run)
        {
            EditorApplication.delayCall += RefreshAfterRun;
        }

        private void HandlePlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode) RefreshAfterRun();
        }

        private void RefreshAfterRun()
        {
            if (this == null) return;
            ClearHistoryView();
            Refresh();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60f))) Refresh();
            if (GUILayout.Button("Run checked", EditorStyles.toolbarButton, GUILayout.Width(85f))) Run(_entries.Where(entry => _checked.Contains(entry.File)));
            if (GUILayout.Button("Run selected", EditorStyles.toolbarButton, GUILayout.Width(85f)) && _selected != null) Run(new[] { _selected });
            if (GUILayout.Button("Run all", EditorStyles.toolbarButton, GUILayout.Width(55f))) Run(_entries);
            if (GUILayout.Button("Run not passed", EditorStyles.toolbarButton, GUILayout.Width(100f))) Run(_entries.Where(entry => StatusOf(entry) != ScenarioStatus.Passed));
            GUI.enabled = EditorApplication.isPlaying && ScenarioManager.IsReady;
            if (GUILayout.Button("Stop", EditorStyles.toolbarButton, GUILayout.Width(45f))) ScenarioManager.Instance.StopAll();
            GUI.enabled = true;
            GUILayout.Space(8f);
            GUILayout.Label("Speed", GUILayout.Width(40f));
            float speed = GUILayout.HorizontalSlider(_speed, ScenarioManager.MinSpeed, ScenarioManager.MaxSpeed, GUILayout.Width(90f));
            if (!Mathf.Approximately(speed, _speed))
            {
                _speed = Mathf.Round(speed * 4f) / 4f;
                if (EditorApplication.isPlaying && ScenarioManager.IsReady) ScenarioManager.Speed = _speed;
            }
            GUILayout.Label($"x{_speed:0.##}", GUILayout.Width(36f));
            _exitWhenDone = GUILayout.Toggle(_exitWhenDone, "Stop play when done", EditorStyles.toolbarButton, GUILayout.Width(125f));
            if (GUILayout.Button("History", EditorStyles.toolbarDropDown, GUILayout.Width(65f))) ShowHistoryMenu();
            if (GUILayout.Button(new GUIContent("Copy fails", "Copy the failed and review checks of every scenario"), EditorStyles.toolbarButton, GUILayout.Width(70f))) CopyToClipboard(ScenarioReportFormula.Brief(_entries.Select(entry => entry.Report)));
            GUILayout.FlexibleSpace();
            _filter = GUILayout.TextField(_filter, EditorStyles.toolbarSearchField, GUILayout.Width(160f));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(ListWidth));
            DrawSummary();
            _listScroll = EditorGUILayout.BeginScrollView(_listScroll);
            foreach (Entry entry in _entries)
            {
                if (_filter.Length > 0 && entry.File.IndexOf(_filter, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                DrawListRow(entry);
            }
            EditorGUILayout.EndScrollView();
            if (_entries.Count == 0) EditorGUILayout.HelpBox($"No scenarios in {ScenarioLibrary.ScenariosFolder}", MessageType.Info);
            EditorGUILayout.EndVertical();
        }

        private void DrawSummary()
        {
            int passed = _entries.Count(entry => StatusOf(entry) == ScenarioStatus.Passed);
            int review = _entries.Count(entry => StatusOf(entry) == ScenarioStatus.NeedsReview);
            int failed = _entries.Count(entry => StatusOf(entry) == ScenarioStatus.Failed);
            int notRun = _entries.Count(entry => StatusOf(entry) == ScenarioStatus.NotRun);
            EditorGUILayout.LabelField($"Passed {passed}   Review {review}   Failed {failed}   Not run {notRun}", EditorStyles.boldLabel);
        }

        private void DrawListRow(Entry entry)
        {
            ScenarioStatus status = StatusOf(entry);
            EditorGUILayout.BeginHorizontal(entry == _selected ? EditorStyles.helpBox : GUIStyle.none);
            bool isChecked = _checked.Contains(entry.File);
            bool nowChecked = EditorGUILayout.Toggle(isChecked, GUILayout.Width(16f));
            if (nowChecked != isChecked)
            {
                if (nowChecked) _checked.Add(entry.File);
                else _checked.Remove(entry.File);
            }

            Color previous = GUI.color;
            GUI.color = ColorOf(status);
            GUILayout.Label(Badge(status), EditorStyles.boldLabel, GUILayout.Width(58f));
            GUI.color = previous;
            if (GUILayout.Button(ScenarioLibrary.RelativePath(entry.File), EditorStyles.label)) Select(entry);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDetails()
        {
            EditorGUILayout.BeginVertical();
            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
            if (_selected == null)
            {
                EditorGUILayout.HelpBox("Pick a scenario.", MessageType.None);
            }
            else
            {
                DrawHeader(_selected);
                DrawHistory(_selected);
                DrawLive(_selected);
                DrawSteps(_selected);
                DrawChecks(_selected);
                DrawMetrics(_selected);
            }
            DrawActionCatalog();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawHeader(Entry entry)
        {
            ScenarioDefinition definition = entry.Definition;
            ScenarioStatus status = StatusOf(entry);
            Color previous = GUI.color;
            GUI.color = ColorOf(status);
            EditorGUILayout.LabelField($"{definition.Name}   {Badge(status)}", EditorStyles.largeLabel);
            GUI.color = previous;
            if (!string.IsNullOrEmpty(definition.Description)) EditorGUILayout.LabelField(definition.Description, EditorStyles.wordWrappedLabel);
            if (!definition.IsValid) EditorGUILayout.HelpBox(definition.Error, MessageType.Error);

            ScenarioReport report = entry.Report;
            if (report != null)
            {
                EditorGUILayout.LabelField($"Last run {report.StartedAt}, {report.DurationSeconds:0.0}s, agent {report.Agent}, {report.Errors.Count} logged errors", EditorStyles.miniLabel);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Run in Play mode", GUILayout.Width(130f))) Run(new[] { entry });
            if (GUILayout.Button("Open scenario", GUILayout.Width(100f))) EditorUtility.OpenWithDefaultApp(entry.File);
            GUI.enabled = report != null;
            if (GUILayout.Button("Open report", GUILayout.Width(90f))) EditorUtility.OpenWithDefaultApp(ScenarioLibrary.ReportPath(definition));
            if (GUILayout.Button(new GUIContent("Copy", "Copy the result, failed checks, errors and the log end, ready to paste"), GUILayout.Width(50f))) CopyToClipboard(ScenarioReportFormula.Detailed(entry == _selected && _historyReport != null ? _historyReport : report));
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawHistory(Entry entry)
        {
            List<string> files = ScenarioLibrary.HistoryFiles(entry.Definition);
            _showHistory = EditorGUILayout.Foldout(_showHistory, $"History ({files.Count} runs, kept in Tests/Results)", true);
            if (!_showHistory || files.Count == 0) return;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Keep latest only", GUILayout.Width(120f))) AskTrimHistory(entry, 1, $"Delete every run of {entry.Definition.Name} except the latest?");
            if (GUILayout.Button("Delete all", GUILayout.Width(80f))) AskTrimHistory(entry, 0, $"Delete all {files.Count} history runs of {entry.Definition.Name}? The latest result stays.");
            EditorGUILayout.EndHorizontal();

            if (_historyReport != null)
            {
                EditorGUILayout.HelpBox($"Showing the run from {_historyReport.StartedAt}.", MessageType.None);
                if (GUILayout.Button("Back to the latest run", GUILayout.Width(160f))) ClearHistoryView();
            }

            foreach (string file in files.Take(HistoryRows))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                ScenarioStatus status = name.EndsWith(nameof(ScenarioStatus.Failed)) ? ScenarioStatus.Failed
                    : name.EndsWith(nameof(ScenarioStatus.NeedsReview)) ? ScenarioStatus.NeedsReview : ScenarioStatus.Passed;
                EditorGUILayout.BeginHorizontal();
                Color previous = GUI.color;
                GUI.color = ColorOf(status);
                GUILayout.Label(Badge(status), GUILayout.Width(58f));
                GUI.color = previous;
                GUILayout.Label(name.Split('_').Take(2).Aggregate((a, b) => $"{a} {b}"), GUILayout.Width(130f));
                bool viewing = file == _historyFile;
                if (GUILayout.Button(viewing ? "Viewing" : "View", GUILayout.Width(65f)))
                {
                    _historyFile = file;
                    _historyReport = ScenarioReport.Read(file);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void Select(Entry entry)
        {
            _selected = entry;
            ClearHistoryView();
        }

        private void ShowHistoryMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Keep the latest run of every scenario"), false, () =>
            {
                if (Confirm("Delete every history run except the latest one of each scenario?")) TrimAllHistory(1);
            });
            menu.AddItem(new GUIContent("Delete all history"), false, () =>
            {
                if (Confirm("Delete the history of every scenario? Each scenario keeps its latest result.")) TrimAllHistory(0);
            });
            menu.AddSeparator(string.Empty);
            int limit = ScenarioLibrary.HistoryLimit;
            foreach (int option in HistoryLimitOptions)
            {
                int value = option;
                string label = value == 0 ? "After each run, keep/All runs" : $"After each run, keep/Latest {value}";
                menu.AddItem(new GUIContent(label), limit == value, () => ScenarioLibrary.HistoryLimit = value);
            }
            menu.ShowAsContext();
        }

        private void AskTrimHistory(Entry entry, int keep, string message)
        {
            EditorApplication.delayCall += () =>
            {
                if (this != null && Confirm(message)) TrimHistory(entry, keep);
            };
        }

        private void TrimHistory(Entry entry, int keep)
        {
            int removed = ScenarioLibrary.TrimHistory(entry.Definition, keep);
            ClearHistoryView();
            ShowNotification(new GUIContent($"Deleted {removed} runs"));
        }

        private void TrimAllHistory(int keep)
        {
            int removed = ScenarioLibrary.TrimAllHistory(keep);
            ClearHistoryView();
            ShowNotification(new GUIContent($"Deleted {removed} runs"));
        }

        private void CopyToClipboard(string text)
        {
            EditorGUIUtility.systemCopyBuffer = text;
            ShowNotification(new GUIContent("Copied"));
        }

        private static bool Confirm(string message)
        {
            return EditorUtility.DisplayDialog("Scenario history", message, "Delete", "Cancel");
        }

        private void ClearHistoryView()
        {
            _historyFile = null;
            _historyReport = null;
        }

        private void DrawLive(Entry entry)
        {
            ScenarioRun run = ScenarioManager.Current;
            if (!EditorApplication.isPlaying || run == null || run.Definition.File != entry.File) return;
            ScenarioStep step = run.CurrentStep;
            float fraction = ScenarioProgressFormula.RunFraction(run);
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false, 18f), fraction, ScenarioProgressFormula.Describe(run));
            EditorGUILayout.HelpBox($"Running step {(step != null ? step.Index + 1 : 0)}/{run.Definition.Steps.Count}: {step?.Describe()}\nPassed {run.Report.ChecksPassed}  Review {run.Report.ChecksReview}  Failed {run.Report.ChecksFailed}", MessageType.Info);
        }

        private void DrawSteps(Entry entry)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Steps", EditorStyles.boldLabel);
            ScenarioReport report = LiveReport(entry);
            foreach (ScenarioStep step in entry.Definition.Steps)
            {
                ScenarioStepResult result = report?.StepResults.FirstOrDefault(item => item.Index == step.Index);
                ScenarioStatus status = result != null ? result.Status : ScenarioStatus.NotRun;
                EditorGUILayout.BeginHorizontal();
                Color previous = GUI.color;
                GUI.color = ColorOf(status);
                GUILayout.Label(status == ScenarioStatus.NotRun ? "-" : Badge(status), GUILayout.Width(58f));
                GUI.color = previous;
                GUILayout.Label($"{step.Index + 1}. {step.Describe()}", EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndHorizontal();
                if (!string.IsNullOrEmpty(step.Note)) EditorGUILayout.LabelField("     " + step.Note, EditorStyles.wordWrappedMiniLabel);
                if (result != null && !string.IsNullOrEmpty(result.Message)) EditorGUILayout.LabelField("     " + result.Message, EditorStyles.wordWrappedMiniLabel);
            }
        }

        private void DrawChecks(Entry entry)
        {
            ScenarioReport report = LiveReport(entry);
            if (report == null || report.Checks.Count == 0) return;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Checks", EditorStyles.boldLabel);
            foreach (ScenarioCheck check in report.Checks)
            {
                ScenarioReviewVerdict verdict = _reviews.Get(report, check);
                ScenarioStatus status = check.Status == ScenarioCheckStatus.Passed ? ScenarioStatus.Passed
                    : check.Status == ScenarioCheckStatus.Failed ? ScenarioStatus.Failed
                    : verdict == null ? ScenarioStatus.NeedsReview
                    : verdict.Verdict == ScenarioReviewStore.Approved ? ScenarioStatus.Passed : ScenarioStatus.Failed;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                Color previous = GUI.color;
                GUI.color = ColorOf(status);
                GUILayout.Label(Badge(status), EditorStyles.boldLabel, GUILayout.Width(58f));
                GUI.color = previous;
                GUILayout.Label(check.Name, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField($"expected {check.Expected}   actual {check.Actual}", EditorStyles.wordWrappedMiniLabel);
                if (!string.IsNullOrEmpty(check.Note)) EditorGUILayout.LabelField(check.Note, EditorStyles.wordWrappedMiniLabel);
                if (verdict != null) EditorGUILayout.LabelField($"{verdict.Verdict} {verdict.Date} {verdict.Note}", EditorStyles.miniLabel);
                if (check.Status == ScenarioCheckStatus.Review && entry.Report == report) DrawReviewButtons(report, check);
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawReviewButtons(ScenarioReport report, ScenarioCheck check)
        {
            EditorGUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(check.Screenshot) && File.Exists(check.Screenshot) && GUILayout.Button("Screenshot", GUILayout.Width(85f))) EditorUtility.OpenWithDefaultApp(check.Screenshot);
            _reviewNote = GUILayout.TextField(_reviewNote, GUILayout.MinWidth(120f));
            if (GUILayout.Button("Approve", GUILayout.Width(70f))) _reviews.Set(report, check, ScenarioReviewStore.Approved, _reviewNote);
            if (GUILayout.Button("Reject", GUILayout.Width(60f))) _reviews.Set(report, check, ScenarioReviewStore.Rejected, _reviewNote);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawMetrics(Entry entry)
        {
            ScenarioReport report = LiveReport(entry);
            if (report == null) return;
            if (report.Metrics.Count + report.Variables.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
                foreach (var pair in report.Metrics) EditorGUILayout.LabelField(pair.Key, pair.Value.ToString("0.####"));
                foreach (var pair in report.Variables) EditorGUILayout.LabelField("var." + pair.Key, pair.Value.ToString("0.####"));
            }
            if (report.Errors.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Logged errors", EditorStyles.boldLabel);
                foreach (string error in report.Errors.Take(20)) EditorGUILayout.LabelField(error, EditorStyles.wordWrappedMiniLabel);
            }
        }

        private void DrawActionCatalog()
        {
            EditorGUILayout.Space();
            _showActions = EditorGUILayout.Foldout(_showActions, "Actions you can use in a scenario", true);
            if (!_showActions) return;
            foreach (ScenarioActionAttribute action in ScenarioAction.Catalog())
            {
                EditorGUILayout.LabelField($"{action.Category} / {action.Name}", EditorStyles.boldLabel);
                EditorGUILayout.SelectableLabel(action.Usage, EditorStyles.wordWrappedMiniLabel, GUILayout.Height(30f));
            }
        }

        private void Run(IEnumerable<Entry> entries)
        {
            List<string> files = entries.Where(entry => entry.Definition.IsValid).Select(entry => entry.File).ToList();
            if (files.Count == 0) return;

            if (EditorApplication.isPlaying)
            {
                ScenarioManager manager = ScenarioManager.Ensure();
                ScenarioManager.Speed = _speed;
                foreach (string file in files) manager.Enqueue(ScenarioDefinition.Load(file));
                return;
            }

            SessionState.SetString(ScenarioManager.PendingKey, string.Join("\n", files));
            SessionState.SetBool(ScenarioManager.ExitKey, _exitWhenDone);
            SessionState.SetFloat(ScenarioManager.SpeedKey, _speed);
            EditorApplication.isPlaying = true;
        }

        private ScenarioReport LiveReport(Entry entry)
        {
            ScenarioRun run = ScenarioManager.Current;
            if (EditorApplication.isPlaying && run != null && run.Definition.File == entry.File) return run.Report;
            return entry == _selected && _historyReport != null ? _historyReport : entry.Report;
        }

        private ScenarioStatus StatusOf(Entry entry)
        {
            ScenarioRun run = ScenarioManager.Current;
            if (EditorApplication.isPlaying && run != null && run.Definition.File == entry.File) return ScenarioStatus.Running;
            return _reviews.EffectiveStatus(entry.Report);
        }

        private static string Badge(ScenarioStatus status)
        {
            switch (status)
            {
                case ScenarioStatus.Passed: return "PASS";
                case ScenarioStatus.NeedsReview: return "REVIEW";
                case ScenarioStatus.Failed: return "FAIL";
                case ScenarioStatus.Running: return "RUN";
                default: return "NOT RUN";
            }
        }

        private static Color ColorOf(ScenarioStatus status)
        {
            switch (status)
            {
                case ScenarioStatus.Passed: return PassColor;
                case ScenarioStatus.NeedsReview: return ReviewColor;
                case ScenarioStatus.Failed: return FailColor;
                case ScenarioStatus.Running: return Color.cyan;
                default: return IdleColor;
            }
        }
        #endregion

        private sealed class Entry
        {
            public string File;
            public ScenarioDefinition Definition;
            public ScenarioReport Report;
        }
    }
}
