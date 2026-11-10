using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ScenarioTesting;

namespace CoreSystem.Editor.ScenarioTesting
{
    public sealed class ScenarioReviewStore
    {
        #region [Variables/Fields]
        public const string Approved = "approved";
        public const string Rejected = "rejected";

        [JsonProperty] private Dictionary<string, Dictionary<string, ScenarioReviewVerdict>> _verdicts = new Dictionary<string, Dictionary<string, ScenarioReviewVerdict>>();
        #endregion

        #region [Public Methods]
        public static ScenarioReviewStore Load()
        {
            try
            {
                string path = ScenarioLibrary.ReviewsPath;
                return File.Exists(path) ? JsonConvert.DeserializeObject<ScenarioReviewStore>(File.ReadAllText(path)) ?? new ScenarioReviewStore() : new ScenarioReviewStore();
            }
            catch (Exception)
            {
                return new ScenarioReviewStore();
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(ScenarioLibrary.ScenariosFolder);
            File.WriteAllText(ScenarioLibrary.ReviewsPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public ScenarioReviewVerdict Get(ScenarioReport report, ScenarioCheck check)
        {
            if (report == null || !_verdicts.TryGetValue(report.Scenario, out var checks)) return null;
            return checks.TryGetValue(check.Name, out ScenarioReviewVerdict verdict) && verdict.Run == report.StartedAt ? verdict : null;
        }

        public void Set(ScenarioReport report, ScenarioCheck check, string verdict, string note)
        {
            if (!_verdicts.TryGetValue(report.Scenario, out var checks))
            {
                checks = new Dictionary<string, ScenarioReviewVerdict>();
                _verdicts[report.Scenario] = checks;
            }
            checks[check.Name] = new ScenarioReviewVerdict { Verdict = verdict, Note = note, Run = report.StartedAt, Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm") };
            Save();
        }

        public ScenarioStatus EffectiveStatus(ScenarioReport report)
        {
            if (report == null) return ScenarioStatus.NotRun;
            if (report.Status != ScenarioStatus.NeedsReview) return report.Status;

            bool waiting = false;
            foreach (ScenarioCheck check in report.Checks)
            {
                if (check.Status != ScenarioCheckStatus.Review) continue;
                ScenarioReviewVerdict verdict = Get(report, check);
                if (verdict == null) waiting = true;
                else if (verdict.Verdict == Rejected) return ScenarioStatus.Failed;
            }
            return waiting ? ScenarioStatus.NeedsReview : ScenarioStatus.Passed;
        }
        #endregion
    }
}
