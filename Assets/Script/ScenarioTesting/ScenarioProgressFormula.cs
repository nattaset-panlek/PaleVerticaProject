#if UNITY_EDITOR || PV_SCENARIO_TESTS
using UnityEngine;

namespace ScenarioTesting
{
    public static class ScenarioProgressFormula
    {
        #region [Public Methods]
        public static float RunFraction(int stepIndex, int stepCount, bool finished)
        {
            if (finished) return 1f;
            if (stepCount <= 0) return 0f;
            return Mathf.Clamp01((float)Mathf.Max(stepIndex, 0) / stepCount);
        }

        public static float QueueFraction(int finishedRuns, int totalRuns, float currentRun)
        {
            if (totalRuns <= 0) return 0f;
            return Mathf.Clamp01((Mathf.Clamp(finishedRuns, 0, totalRuns) + Mathf.Clamp01(currentRun)) / totalRuns);
        }

        public static int Percent(float fraction)
        {
            return Mathf.FloorToInt(Mathf.Clamp01(fraction) * 100f);
        }

        public static float RunFraction(ScenarioRun run)
        {
            if (run == null) return 0f;
            return RunFraction(run.StepIndex, run.Definition.Steps.Count, run.Report.Status != ScenarioStatus.Running);
        }

        public static string Describe(ScenarioRun run)
        {
            if (run == null) return string.Empty;
            float current = RunFraction(run);
            string text = $"Scenario {Percent(current)}%";
            if (ScenarioManager.RunTotal > 1)
            {
                float overall = QueueFraction(ScenarioManager.RunsFinished, ScenarioManager.RunTotal, run == ScenarioManager.Current ? current : 0f);
                text += $"   Run {ScenarioManager.RunNumber}/{ScenarioManager.RunTotal}, overall {Percent(overall)}%";
            }
            return text;
        }
        #endregion
    }
}
#endif
