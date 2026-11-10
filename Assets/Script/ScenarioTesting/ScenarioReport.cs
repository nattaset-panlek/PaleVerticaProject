#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace ScenarioTesting
{
    [Serializable]
    public sealed class ScenarioReport
    {
        #region [Variables/Fields]
        public string Scenario;
        public string File;
        public string Agent;
        public string SaveProfile;
        public string StartedAt;
        public float DurationSeconds;
        [JsonConverter(typeof(StringEnumConverter))] public ScenarioStatus Status = ScenarioStatus.Running;
        public int ChecksPassed;
        public int ChecksReview;
        public int ChecksFailed;
        public List<ScenarioStepResult> StepResults = new List<ScenarioStepResult>();
        public List<ScenarioCheck> Checks = new List<ScenarioCheck>();
        public Dictionary<string, float> Metrics = new Dictionary<string, float>();
        public Dictionary<string, float> Variables = new Dictionary<string, float>();
        public List<string> Log = new List<string>();
        public List<string> Errors = new List<string>();
        public List<CombatRecordEntry> Combat = new List<CombatRecordEntry>();
        #endregion

        #region [Properties]
        [JsonIgnore] public bool Passed => Status == ScenarioStatus.Passed;
        #endregion

        #region [Public Methods]
        public static ScenarioReport Read(string path)
        {
            try
            {
                return System.IO.File.Exists(path) ? JsonConvert.DeserializeObject<ScenarioReport>(System.IO.File.ReadAllText(path)) : null;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Scenario] Could not read report {path}: {exception.Message}");
                return null;
            }
        }

        public ScenarioStatus Evaluate()
        {
            bool stepFailed = StepResults.Exists(step => step.Status == ScenarioStatus.Failed);
            if (ChecksFailed > 0 || stepFailed) return ScenarioStatus.Failed;
            return ChecksReview > 0 ? ScenarioStatus.NeedsReview : ScenarioStatus.Passed;
        }

        public string Summary()
        {
            var builder = new StringBuilder();
            int total = ChecksPassed + ChecksReview + ChecksFailed;
            builder.AppendLine($"[Scenario] {Scenario} ({Agent}) {Status}: {ChecksPassed} passed, {ChecksReview} to review, {ChecksFailed} failed of {total} checks in {DurationSeconds:0.0}s");
            foreach (ScenarioStepResult step in StepResults)
            {
                if (step.Status == ScenarioStatus.Failed) builder.AppendLine($"  STEP {step.Index + 1} {step.Action} failed: {step.Message}");
            }
            foreach (ScenarioCheck check in Checks)
            {
                if (!check.Passed) builder.AppendLine("  " + check);
            }
            return builder.ToString().TrimEnd();
        }

        public string Write(string folder)
        {
            return Write(folder, Scenario + ".json");
        }

        public string Write(string folder, string fileName)
        {
            try
            {
                Directory.CreateDirectory(folder);
                string path = Path.Combine(folder, fileName);
                System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
                return path;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Scenario] Could not write report: {exception.Message}");
                return null;
            }
        }
        #endregion
    }
}
#endif
