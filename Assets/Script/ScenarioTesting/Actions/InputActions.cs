#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using CoreSystem.Entity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace ScenarioTesting.Actions
{
    [ScenarioAction("Key", "Input", "{\"do\":\"Key\",\"key\":\"Escape\"}  or {\"key\":\"E\",\"hold\":0.5}")]
    internal sealed class KeyAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            if (!Enum.TryParse(step.String("key"), true, out Key key))
            {
                run.FailStep($"unknown key '{step.String("key")}'");
                yield break;
            }
            if (step.Has("hold")) yield return run.Lab.HoldKeys(new[] { key }, step.Float("hold"));
            else yield return run.Input.Tap(key);
        }
        #endregion
    }

    [ScenarioAction("Walk", "Input", "{\"do\":\"Walk\",\"dir\":\"left\",\"seconds\":1}  holds W/A/S/D")]
    internal sealed class WalkAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            var keys = new List<Key>();
            string direction = step.String("dir", "right").ToLowerInvariant();
            if (direction.Contains("up")) keys.Add(Key.W);
            if (direction.Contains("down")) keys.Add(Key.S);
            if (direction.Contains("left")) keys.Add(Key.A);
            if (direction.Contains("right")) keys.Add(Key.D);
            yield return run.Lab.HoldKeys(keys.ToArray(), run.Number(step, "seconds", 1f));
        }
        #endregion
    }

    [ScenarioAction("WalkTo", "Input", "{\"do\":\"WalkTo\",\"to\":\"target\",\"range\":1.5}  or {\"x\":3,\"y\":-2} steering with W/A/S/D")]
    internal sealed class WalkToAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Vector3 destination;
            if (step.Has("to") || step.Has("character"))
            {
                Character target = step.Has("character") ? run.EntityFrom(step, "character", null) : run.Entity(step.String("to"));
                if (target == null)
                {
                    run.FailStep("nothing to walk to");
                    yield break;
                }
                destination = target.transform.position;
            }
            else destination = new Vector3(run.Number(step, "x", 0f), run.Number(step, "y", 0f));

            yield return run.Lab.Approach(destination, step.Float("range", 0.5f), step.Float("max", 30f));
        }
        #endregion
    }

    [ScenarioAction("Idle", "Input", "{\"do\":\"Idle\",\"seconds\":1}  touches nothing")]
    internal sealed class IdleAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            run.Input.ReleaseAll();
            yield return new WaitForSeconds(run.Number(step, "seconds", 1f));
        }
        #endregion
    }

    [ScenarioAction("Click", "Input", "{\"do\":\"Click\",\"at\":\"target\",\"button\":\"left\"}  or {\"x\":1,\"y\":2} world, or {\"screenX\":100,\"screenY\":200}")]
    internal sealed class ClickAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            MouseButton button = step.String("button", "left").Equals("right", StringComparison.OrdinalIgnoreCase) ? MouseButton.Right : MouseButton.Left;
            if (step.Has("screenX"))
            {
                yield return run.Input.Click(new Vector2(step.Float("screenX"), step.Float("screenY")), button);
                yield break;
            }

            if (step.Has("at"))
            {
                Character target = run.EntityFrom(step, "at", null);
                if (target == null)
                {
                    run.FailStep($"nothing called {step.String("at")}");
                    yield break;
                }
                yield return run.Lab.ClickCharacter(target, button);
                yield break;
            }

            var world = new Vector3(run.Number(step, "x", 0f), run.Number(step, "y", 0f));
            if (!ScenarioInput.TryScreenOf(world, out Vector2 screen))
            {
                run.FailStep($"{world} is not on screen");
                yield break;
            }
            yield return run.Input.Click(screen, button);
        }
        #endregion
    }

    [ScenarioAction("ClickUI", "Input", "{\"do\":\"ClickUI\",\"label\":\"Accept\"}  clicks a visible button by its text")]
    internal sealed class ClickUIAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            string label = step.String("label");
            Button button = null;
            yield return run.WaitFor(() => (button = ScenarioInput.FindButton(label)) != null, step.Float("max", 5f), $"a button \"{label}\" (visible: {string.Join(", ", ScenarioInput.VisibleButtonLabels())})");
            if (button == null) yield break;
            yield return ScenarioButtonPress.Press(run, button);
            yield return null;
        }
        #endregion
    }
}
#endif
