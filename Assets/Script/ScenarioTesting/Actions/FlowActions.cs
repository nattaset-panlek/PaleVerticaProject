#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System.Collections;
using UnityEngine;

namespace ScenarioTesting.Actions
{
    [ScenarioAction("Wait", "Flow", "{\"do\":\"Wait\",\"seconds\":1}")]
    internal sealed class WaitAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            yield return new WaitForSeconds(run.Number(step, "seconds", 1f));
        }
        #endregion
    }

    [ScenarioAction("WaitUntil", "Flow", "{\"do\":\"WaitUntil\",\"until\":\"target.dead\",\"timeout\":10}")]
    internal sealed class WaitUntilAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            string expression = step.String("until");
            float deadline = Time.time + step.Float("max", step.Timeout - 1f);
            while (true)
            {
                if (!run.TryEvaluate(expression, out float value)) yield break;
                if (value != 0f) yield break;
                if (Time.time > deadline)
                {
                    run.FailStep($"\"{expression}\" never became true");
                    yield break;
                }
                yield return null;
            }
        }
        #endregion
    }

    [ScenarioAction("Log", "Flow", "{\"do\":\"Log\",\"text\":\"anything\"}")]
    internal sealed class LogAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            run.Note(step.String("text", string.Empty));
            yield break;
        }
        #endregion
    }

    [ScenarioAction("Speed", "Flow", "{\"do\":\"Speed\",\"value\":2}")]
    internal sealed class SpeedAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            ScenarioManager.Speed = step.Float("value", 1f);
            yield break;
        }
        #endregion
    }

    internal static class SubSteps
    {
        #region [Public Methods]
        public static IEnumerator Run(ScenarioRun run, ScenarioStep parent)
        {
            if (!(parent.Token("steps") is Newtonsoft.Json.Linq.JArray steps))
            {
                run.FailStep("missing \"steps\" array");
                yield break;
            }

            foreach (var token in steps)
            {
                if (!(token is Newtonsoft.Json.Linq.JObject raw))
                {
                    run.FailStep("a sub step is not an object");
                    yield break;
                }
                var step = new ScenarioStep(parent.Index, raw);
                ScenarioAction action = ScenarioAction.Create(step.Action);
                if (action == null)
                {
                    run.FailStep($"unknown action '{step.Action}'");
                    yield break;
                }
                yield return action.Run(run, step);
                if (run.StepFailed) yield break;
            }
        }
        #endregion
    }

    [ScenarioAction("Repeat", "Flow", "{\"do\":\"Repeat\",\"times\":20,\"steps\":[{\"do\":\"UseItem\"},{\"do\":\"Record\",\"name\":\"kept\",\"value\":\"var.kept + potion\"}]}  var.i counts from 0")]
    internal sealed class RepeatAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            int times = (int)run.Number(step, "times", 1f);
            for (int i = 0; i < times; i++)
            {
                run.SetVariable("i", i);
                yield return SubSteps.Run(run, step);
                if (run.StepFailed) yield break;
            }
        }
        #endregion
    }

    [ScenarioAction("Watch", "Flow", "{\"do\":\"Watch\",\"name\":\"hp\",\"value\":\"player.HP\",\"steps\":[{\"do\":\"Attack\",\"hits\":4}]}  keeps var.hp_min and var.hp_max while the steps run")]
    internal sealed class WatchAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            string name = step.String("name", "watch");
            string expression = step.String("value");
            float min = float.MaxValue;
            float max = float.MinValue;

            IEnumerator inner = SubSteps.Run(run, step);
            var stack = new System.Collections.Generic.Stack<IEnumerator>();
            stack.Push(inner);
            while (stack.Count > 0)
            {
                if (!run.TryEvaluate(expression, out float value)) yield break;
                min = Mathf.Min(min, value);
                max = Mathf.Max(max, value);

                IEnumerator top = stack.Peek();
                if (!top.MoveNext())
                {
                    stack.Pop();
                    continue;
                }
                if (top.Current is IEnumerator nested && !(top.Current is CustomYieldInstruction)) stack.Push(nested);
                else yield return top.Current;
            }

            run.SetVariable($"{name}_min", min);
            run.SetVariable($"{name}_max", max);
            run.Note($"{expression}: min {min:0.##}, max {max:0.##}");
        }
        #endregion
    }

    [ScenarioAction("Seed", "Flow", "{\"do\":\"Seed\",\"value\":1337}")]
    internal sealed class SeedAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Random.InitState(step.Int("value", run.Definition.Seed));
            yield break;
        }
        #endregion
    }
}
#endif
