#if UNITY_EDITOR || PV_SCENARIO_TESTSusing System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ScenarioTesting
{
    public static class ScenarioLibrary
    {
        #region [Variables/Fields]
        public const string ScenarioExtension = "*.json";
        public const string ReviewsFileName = "_reviews.json";
        public const string HistoryLimitKey = "PaleVertica.Scenario.HistoryLimit";
        private const string HistoryFolderName = "history";
        #endregion

        #region [Properties]
        public static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        public static string ScenariosFolder => Path.Combine(ProjectRoot, "Tests", "Scenarios");
        public static string ReportsFolder => Path.Combine(ProjectRoot, "Tests", "Results");
        public static string ScreenshotsFolder => Path.Combine(ProjectRoot, "Logs", "Scenarios");
        public static string ReviewsPath => Path.Combine(ScenariosFolder, ReviewsFileName);

        public static int HistoryLimit
        {
#if UNITY_EDITOR
            get => Mathf.Max(0, UnityEditor.EditorPrefs.GetInt(HistoryLimitKey, 0));
            set => UnityEditor.EditorPrefs.SetInt(HistoryLimitKey, Mathf.Max(0, value));
#else
            get => 0;
            set { }
#endif
        }
        #endregion

        #region [Public Methods]
        public static List<string> AllFiles()
        {
            var files = new List<string>();
            if (!Directory.Exists(ScenariosFolder)) return files;
            foreach (string file in Directory.GetFiles(ScenariosFolder, ScenarioExtension, SearchOption.AllDirectories))
            {
                if (Path.GetFileName(file).StartsWith("_")) continue;
                files.Add(Path.GetFullPath(file));
            }
            files.Sort();
            return files;
        }

        public static string RelativePath(string file)
        {
            string full = Path.GetFullPath(file);
            return full.StartsWith(ScenariosFolder) ? full.Substring(ScenariosFolder.Length).TrimStart('\\', '/').Replace('\\', '/') : full;
        }

        public static string ReportPath(ScenarioDefinition definition)
        {
            return Path.Combine(ReportsFolder, definition.Agent, definition.Name + ".json");
        }

        public static string HistoryFolder(ScenarioDefinition definition)
        {
            return Path.Combine(ReportsFolder, definition.Agent, HistoryFolderName, definition.Name);
        }

        public static int TrimHistory(ScenarioDefinition definition, int keep)
        {
            return TrimFolder(HistoryFolder(definition), keep);
        }

        public static int TrimFolder(string folder, int keep)
        {
            if (!Directory.Exists(folder)) return 0;
            var files = new List<string>(Directory.GetFiles(folder, ScenarioExtension));
            files.Sort();
            files.Reverse();
            int removed = 0;
            for (int i = Mathf.Max(0, keep); i < files.Count; i++)
            {
                File.Delete(files[i]);
                removed++;
            }
            if (Directory.GetFileSystemEntries(folder).Length == 0) Directory.Delete(folder);
            return removed;
        }

        public static int TrimAllHistory(int keep)
        {
            int removed = 0;
            if (!Directory.Exists(ReportsFolder)) return removed;
            foreach (string agent in Directory.GetDirectories(ReportsFolder))
            {
                string history = Path.Combine(agent, HistoryFolderName);
                if (!Directory.Exists(history)) continue;
                foreach (string folder in Directory.GetDirectories(history)) removed += TrimFolder(folder, keep);
            }
            return removed;
        }

        public static List<string> HistoryFiles(ScenarioDefinition definition)
        {
            var files = new List<string>();
            string folder = HistoryFolder(definition);
            if (Directory.Exists(folder)) files.AddRange(Directory.GetFiles(folder, ScenarioExtension));
            files.Sort();
            files.Reverse();
            return files;
        }

        public static string ScreenshotPath(ScenarioDefinition definition, int stepIndex)
        {
            return Path.Combine(ScreenshotsFolder, definition.Agent, $"{definition.Name}_{System.DateTime.Now:yyyyMMdd_HHmmss}_step{stepIndex + 1}.png");
        }
        #endregion
    }
}
#endif
