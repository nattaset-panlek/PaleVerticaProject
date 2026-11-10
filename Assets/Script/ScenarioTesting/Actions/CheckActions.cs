#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System;
using System.Collections;
using System.Linq;
using CoreSystem;
using CoreSystem.Entity;
using UnityEngine;

namespace ScenarioTesting.Actions
{
    [ScenarioAction("Record", "Check", "{\"do\":\"Record\",\"name\":\"hp0\",\"value\":\"target.HP\"}  read later as var.hp0")]
    internal sealed class RecordAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            if (run.TryEvaluate(step.String("value"), out float value))
            {
                run.SetVariable(step.String("name"), value);
                run.Note($"var.{step.String("name")} = {value:0.###}");
            }
            yield break;
        }
        #endregion
    }

    [ScenarioAction("Measure", "Check", "{\"do\":\"Measure\",\"name\":\"damage per hit\",\"value\":\"hits.avg\"}  kept in the report")]
    internal sealed class MeasureAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            if (run.TryEvaluate(step.String("value"), out float value)) run.Measure(step.String("name"), value);
            yield break;
        }
        #endregion
    }

    [ScenarioAction("Expect", "Check", "{\"do\":\"Expect\",\"name\":\"...\",\"value\":\"hit.0 / var.base\",\"op\":\"~=\",\"to\":2,\"tolerance\":0.03,\"review\":0.1}  op: == != < <= > >= ~= between(min,max); outside tolerance but inside review = Review; \"severity\":\"review\" turns a miss into Review")]
    internal sealed class ExpectAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            string name = step.String("name", step.String("value"));
            if (!run.TryEvaluate(step.String("value"), out float actual)) yield break;

            string op = step.String("op", step.Has("tolerance") ? "~=" : step.Has("min") ? "between" : "==");
            float tolerance = step.Float("tolerance", op == "==" || op == "!=" ? 0.0001f : 0f);
            float review = step.Float("review", 0f);
            float target = run.Number(step, "to", 0f);
            float min = run.Number(step, "min", float.MinValue);
            float max = run.Number(step, "max", float.MaxValue);

            float miss = Miss(op, actual, target, min, max, tolerance, out string expected);
            if (float.IsNaN(miss))
            {
                run.FailStep($"unknown op '{op}'");
                yield break;
            }

            ScenarioCheckStatus status = miss <= 0f ? ScenarioCheckStatus.Passed
                : miss <= review ? ScenarioCheckStatus.Review
                : step.String("severity", "fail") == "review" ? ScenarioCheckStatus.Review
                : ScenarioCheckStatus.Failed;
            run.Record(name, status, expected, actual.ToString("0.####"), step.Note);
        }
        #endregion

        #region [Private Methods]
        private static float Miss(string op, float actual, float target, float min, float max, float tolerance, out string expected)
        {
            switch (op)
            {
                case "==":
                    expected = $"== {target:0.####}";
                    return Math.Abs(actual - target) - tolerance;
                case "~=":
                    expected = $"{target:0.####} +/- {tolerance:0.####}";
                    return Math.Abs(actual - target) - tolerance;
                case "!=":
                    expected = $"!= {target:0.####}";
                    return Math.Abs(actual - target) > tolerance ? 0f : 1f;
                case "<":
                    expected = $"< {target:0.####}";
                    return actual < target ? 0f : actual - target + 0.0001f;
                case "<=":
                    expected = $"<= {target:0.####}";
                    return actual <= target ? 0f : actual - target;
                case ">":
                    expected = $"> {target:0.####}";
                    return actual > target ? 0f : target - actual + 0.0001f;
                case ">=":
                    expected = $">= {target:0.####}";
                    return actual >= target ? 0f : target - actual;
                case "between":
                    expected = $"{min:0.####}..{max:0.####}";
                    return actual < min ? min - actual : actual > max ? actual - max : 0f;
                default:
                    expected = op;
                    return float.NaN;
            }
        }
        #endregion
    }

    [ScenarioAction("ExpectNoErrors", "Check", "{\"do\":\"ExpectNoErrors\",\"name\":\"no errors so far\"}  fails if anything logged an error or exception")]
    internal sealed class ExpectNoErrorsAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            int since = (int)run.Number(step, "since", 0f);
            int count = run.ErrorCount - since;
            run.Record(step.String("name", "no errors"), count == 0 ? ScenarioCheckStatus.Passed : ScenarioCheckStatus.Failed, "0 errors", $"{count}", "errors are listed in the report");
            yield break;
        }
        #endregion
    }

    [ScenarioAction("Snapshot", "Check", "{\"do\":\"Snapshot\",\"name\":\"before\",\"of\":\"player\"}  every stat except HP and MP")]
    internal sealed class SnapshotAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Character character = run.Entity(step.String("of", ScenarioRun.PlayerName));
            run.Snapshots[step.String("name", "snapshot")] = run.Lab.StableSnapshot(character);
            yield break;
        }
        #endregion
    }

    [ScenarioAction("ExpectSameStats", "Check", "{\"do\":\"ExpectSameStats\",\"name\":\"no leaked stats\",\"since\":\"before\",\"of\":\"player\"}")]
    internal sealed class ExpectSameStatsAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            string since = step.String("since", "snapshot");
            if (!run.Snapshots.TryGetValue(since, out var before))
            {
                run.FailStep($"no snapshot called {since}");
                yield break;
            }
            Character character = run.Entity(step.String("of", ScenarioRun.PlayerName));
            var changed = AttributeProbe.Diff(before, run.Lab.StableSnapshot(character));
            string actual = changed.Count == 0 ? "no change" : string.Join(", ", changed.Select(pair => $"{pair.Key} {pair.Value:+0.##;-0.##}"));
            run.Record(step.String("name", "stats unchanged"), changed.Count == 0 ? ScenarioCheckStatus.Passed : ScenarioCheckStatus.Failed, "no change", actual);
        }
        #endregion
    }

    [ScenarioAction("Review", "Check", "{\"do\":\"Review\",\"name\":\"slash VFX plays\",\"note\":\"what a person should look at\"}  takes a screenshot and waits for a human verdict")]
    internal sealed class ReviewAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            string path = null;
            if (step.Bool("screenshot", true) && !Application.isBatchMode)
            {
                path = ScenarioLibrary.ScreenshotPath(run.Definition, step.Index);
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                ScreenCapture.CaptureScreenshot(path);
                yield return new WaitForEndOfFrame();
                yield return null;
            }
            run.Record(step.String("name", "look at this"), ScenarioCheckStatus.Review, "a person approves", "waiting for review", step.Note, path);
        }
        #endregion
    }
}
#endif
