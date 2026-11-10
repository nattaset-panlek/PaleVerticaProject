using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioTesting;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Editor.ScenarioTesting
{
    [InitializeOnLoad]
    public static class ScenarioBatchRunner
    {
        #region [Variables/Fields]
        private const string ActiveKey = "PaleVertica.Scenario.BatchActive";
        private const string FilesKey = "PaleVertica.Scenario.BatchFiles";
        private const string FilterArgument = "-scenarios";
        private const string SpeedArgument = "-scenarioSpeed";
        private const int NothingToRunCode = 2;
        private const int FailedCode = 1;
        #endregion

        #region [Unity Methods]
        static ScenarioBatchRunner()
        {
            if (SessionState.GetBool(ActiveKey, false)) EditorApplication.playModeStateChanged += HandlePlayModeChanged;
        }
        #endregion

        #region [Public Methods]
        public static void RunBatch()
        {
            string filter = Argument(FilterArgument);
            List<string> files = ScenarioLibrary.AllFiles()
                .Where(file => string.IsNullOrEmpty(filter) || filter.Split(',').Any(part => ScenarioLibrary.RelativePath(file).IndexOf(part.Trim(), StringComparison.OrdinalIgnoreCase) >= 0))
                .ToList();

            if (files.Count == 0)
            {
                Debug.LogError($"[ScenarioBatch] No scenario matches '{filter}' in {ScenarioLibrary.ScenariosFolder}");
                EditorApplication.Exit(NothingToRunCode);
                return;
            }

            float speed = float.TryParse(Argument(SpeedArgument), out float parsed) ? parsed : 1f;
            Debug.Log($"[ScenarioBatch] Running {files.Count} scenario(s) at x{speed}: {string.Join(", ", files.Select(ScenarioLibrary.RelativePath))}");

            SessionState.SetString(ScenarioManager.PendingKey, string.Join("\n", files));
            SessionState.SetBool(ScenarioManager.ExitKey, true);
            SessionState.SetFloat(ScenarioManager.SpeedKey, speed);
            SessionState.SetString(FilesKey, string.Join("\n", files));
            SessionState.SetBool(ActiveKey, true);
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeChanged;
            EditorApplication.isPlaying = true;
        }
        #endregion

        #region [Private Methods]
        private static void HandlePlayModeChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
            SessionState.EraseBool(ActiveKey);

            string[] files = SessionState.GetString(FilesKey, string.Empty).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            SessionState.EraseString(FilesKey);

            int failed = 0;
            foreach (string file in files)
            {
                ScenarioDefinition definition = ScenarioDefinition.Load(file);
                ScenarioReport report = ScenarioReport.Read(ScenarioLibrary.ReportPath(definition));
                ScenarioStatus status = report != null ? report.Status : ScenarioStatus.NotRun;
                if (status == ScenarioStatus.Failed || status == ScenarioStatus.NotRun || status == ScenarioStatus.Running) failed++;
                Debug.Log($"[ScenarioBatch] {ScenarioLibrary.RelativePath(file)}: {status}");
            }

            Debug.Log($"[ScenarioBatch] Done: {files.Length - failed}/{files.Length} without failures.");
            if (Application.isBatchMode) EditorApplication.Exit(failed > 0 ? FailedCode : 0);
        }

        private static string Argument(string name)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(arguments, name);
            return index >= 0 && index + 1 < arguments.Length ? arguments[index + 1] : null;
        }
        #endregion
    }
}
