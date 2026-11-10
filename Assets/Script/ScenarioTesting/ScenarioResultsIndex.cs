#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScenarioTesting
{
    public static class ScenarioResultsIndex
    {
        #region [Variables/Fields]
        public const string FileName = "_summary.md";
        private const int MaxIssueLength = 400;
        #endregion

        #region [Public Methods]
        public static string PathFor(string agent)
        {
            return Path.Combine(ScenarioLibrary.ReportsFolder, agent, FileName);
        }

        public static string Rebuild(string agent)
        {
            string folder = Path.Combine(ScenarioLibrary.ReportsFolder, agent);
            if (!Directory.Exists(folder)) return null;

            List<ScenarioReport> reports = Directory.GetFiles(folder, "*.json")
                .Select(ScenarioReport.Read)
                .Where(report => report != null)
                .OrderBy(report => Rank(report.Status))
                .ThenBy(report => report.File)
                .ToList();

            var text = new StringBuilder();
            text.AppendLine($"# Scenario results: {agent}");
            text.AppendLine();
            text.AppendLine($"Updated {DateTime.Now:yyyy-MM-dd HH:mm:ss}. Failed {reports.Count(r => r.Status == ScenarioStatus.Failed)}, needs review {reports.Count(r => r.Status == ScenarioStatus.NeedsReview)}, passed {reports.Count(r => r.Status == ScenarioStatus.Passed)}.");
            text.AppendLine();
            text.AppendLine("| Status | Scenario | File | Pass | Review | Fail | Last run |");
            text.AppendLine("|---|---|---|---|---|---|---|");
            foreach (ScenarioReport report in reports)
            {
                text.AppendLine($"| {report.Status} | {report.Scenario} | {report.File} | {report.ChecksPassed} | {report.ChecksReview} | {report.ChecksFailed} | {report.StartedAt} |");
            }

            foreach (ScenarioReport report in reports.Where(r => r.Status != ScenarioStatus.Passed))
            {
                text.AppendLine();
                text.AppendLine($"## {report.Status}: {report.Scenario}");
                text.AppendLine();
                text.AppendLine($"Report: `{report.Scenario}.json`, scenario `Tests/Scenarios/{report.File}`");
                text.AppendLine();
                foreach (ScenarioCheck check in report.Checks.Where(c => c.Status != ScenarioCheckStatus.Passed))
                {
                    text.AppendLine($"- **{check.Status}** step {check.Step + 1}: {check.Name}. Expected `{Clip(check.Expected)}`, got `{Clip(check.Actual)}`{(string.IsNullOrEmpty(check.Note) ? "" : $" ({check.Note})")}");
                }
                foreach (string error in report.Errors.Take(5)) text.AppendLine($"- **Error**: {Clip(error.Split('\n')[0])}");
            }

            string path = PathFor(agent);
            try
            {
                File.WriteAllText(path, text.ToString());
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Scenario] Could not write {path}: {exception.Message}");
                return null;
            }
            return path;
        }
        #endregion

        #region [Private Methods]
        private static int Rank(ScenarioStatus status)
        {
            switch (status)
            {
                case ScenarioStatus.Failed: return 0;
                case ScenarioStatus.NeedsReview: return 1;
                case ScenarioStatus.Passed: return 3;
                default: return 2;
            }
        }

        private static string Clip(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            value = value.Replace("`", "'").Replace("\n", " ");
            return value.Length > MaxIssueLength ? value.Substring(0, MaxIssueLength) + "..." : value;
        }
        #endregion
    }
}
#endif
