#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System.Collections.Generic;
using System.Text;

namespace ScenarioTesting
{
    public static class ScenarioReportFormula
    {
        #region [Variables/Fields]
        public const int LogTail = 60;
        public const int ErrorLength = 1500;
        public const int MaxErrors = 3;
        #endregion

        #region [Public Methods]
        public static string Detailed(ScenarioReport report, int logTail = LogTail)
        {
            if (report == null) return "No report.";
            var text = new StringBuilder();
            AppendHeader(text, report);
            AppendIssues(text, report);

            if (report.Errors != null && report.Errors.Count > 0)
            {
                text.AppendLine();
                text.AppendLine($"Errors ({report.Errors.Count}):");
                for (int i = 0; i < report.Errors.Count && i < MaxErrors; i++) text.AppendLine("- " + Clip(report.Errors[i], ErrorLength));
                if (report.Errors.Count > MaxErrors) text.AppendLine($"- ... {report.Errors.Count - MaxErrors} more in the report");
            }

            if (report.Variables != null && report.Variables.Count > 0)
            {
                text.AppendLine();
                var pairs = new List<string>();
                foreach (var pair in report.Variables) pairs.Add($"{pair.Key}={pair.Value:0.####}");
                text.AppendLine("Variables: " + string.Join(", ", pairs));
            }

            if (report.Log != null && report.Log.Count > 0)
            {
                text.AppendLine();
                int start = report.Log.Count > logTail ? report.Log.Count - logTail : 0;
                text.AppendLine(start > 0 ? $"Log (first line and last {logTail} of {report.Log.Count}):" : "Log:");
                if (start > 0)
                {
                    text.AppendLine(report.Log[0]);
                    text.AppendLine("...");
                }
                for (int i = start; i < report.Log.Count; i++) text.AppendLine(report.Log[i]);
            }
            return text.ToString().TrimEnd();
        }

        public static string Brief(IEnumerable<ScenarioReport> reports)
        {
            var text = new StringBuilder();
            int count = 0;
            foreach (ScenarioReport report in reports)
            {
                if (report == null || report.Status == ScenarioStatus.Passed) continue;
                if (count > 0) text.AppendLine();
                AppendHeader(text, report);
                AppendIssues(text, report);
                count++;
            }
            return count == 0 ? "Every scenario passed." : text.ToString().TrimEnd();
        }
        #endregion

        #region [Private Methods]
        private static void AppendHeader(StringBuilder text, ScenarioReport report)
        {
            text.AppendLine($"[{report.Status}] {report.Scenario} ({report.File})");
            text.AppendLine($"Run {report.StartedAt}, {report.DurationSeconds:0.0}s, agent {report.Agent}, checks {report.ChecksPassed} passed / {report.ChecksReview} review / {report.ChecksFailed} failed, {(report.Errors != null ? report.Errors.Count : 0)} errors");
        }

        private static void AppendIssues(StringBuilder text, ScenarioReport report)
        {
            if (report.Checks != null)
            {
                foreach (ScenarioCheck check in report.Checks)
                {
                    if (check == null || check.Passed) continue;
                    string step = check.Step >= 0 ? $"step {check.Step + 1}: " : string.Empty;
                    text.AppendLine($"- {check.Status} {step}{check.Name} | expected {check.Expected} | actual {Clip(check.Actual, ErrorLength)}");
                }
            }
            if (report.StepResults != null)
            {
                foreach (ScenarioStepResult result in report.StepResults)
                {
                    if (result == null || result.Status != ScenarioStatus.Failed) continue;
                    text.AppendLine($"- Step {result.Index + 1} {result.Arguments} failed: {Clip(result.Message, ErrorLength)}");
                }
            }
        }

        private static string Clip(string value, int length)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= length ? value : value.Substring(0, length) + " ...";
        }
        #endregion
    }
}
#endif
