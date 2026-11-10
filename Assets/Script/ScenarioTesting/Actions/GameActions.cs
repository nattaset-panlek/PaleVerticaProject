#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System.Collections;
using System.Reflection;
using CoreSystem.Data.Save;
using CoreSystem.Player;
using CoreSystem.World;
using UnityEngine;
using UnityEngine.UI;

namespace ScenarioTesting.Actions
{
    [ScenarioAction("StartNewGame", "Game", "{\"do\":\"StartNewGame\"}  goes back to the title if needed, then presses New Game")]
    internal sealed class StartNewGameAction : ScenarioAction
    {
        #region [Variables/Fields]
        private const string NewGameLabel = "New Game";
        #endregion

        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            yield return run.WaitFor(() => !ScenarioProbe.IsLoading, step.Float("max", 60f), "the previous loading screen to close");
            if (run.StepFailed) yield break;

            if (ScenarioProbe.SceneName != GameCoreLoader.TitleSceneName)
            {
                run.Log($"going back to the title from {ScenarioProbe.SceneName} so the old character is dropped");
                GameCoreLoader.ReturnToTitle();
                yield return run.WaitFor(() => ScenarioProbe.SceneName == GameCoreLoader.TitleSceneName && !ScenarioProbe.IsLoading, step.Float("max", 60f), "the title screen");
                if (run.StepFailed) yield break;
            }

            Button button = null;
            yield return run.WaitFor(() => (button = ScenarioInput.FindButton(NewGameLabel)) != null, 10f, $"a \"{NewGameLabel}\" button on the title (visible: {string.Join(", ", ScenarioInput.VisibleButtonLabels())})");
            if (run.StepFailed) yield break;

            bool loaded = false;
            void HandleLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode) => loaded = true;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += HandleLoaded;
            yield return ScenarioButtonPress.Press(run, button);

            string slotZero = null;
            void WatchSlotZero()
            {
                string now = ScenarioLab.SkillSlotText(0);
                if (now == slotZero) return;
                run.Log($"skill slot 1 is now {now} (frame {Time.frameCount}, scene {ScenarioProbe.SceneName}, loading {ScenarioProbe.IsLoading})");
                slotZero = now;
            }

            yield return run.WaitFor(() => { WatchSlotZero(); return loaded; }, step.Float("max", 60f), "the new game scene to load");
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleLoaded;
            if (run.StepFailed) yield break;

            yield return run.WaitFor(() => { WatchSlotZero(); return ScenarioProbe.IsGameReady; }, step.Float("max", 60f), "the player and loading screen");
            float settleUntil = Time.time + step.Float("settle", 1f);
            while (Time.time < settleUntil)
            {
                WatchSlotZero();
                yield return null;
            }
            run.Note($"in scene {ScenarioProbe.SceneName}");
            run.Log($"skills after the new game: {ScenarioLab.SkillLoadout()}");
        }
        #endregion
    }

    [ScenarioAction("Travel", "Game", "{\"do\":\"Travel\",\"scene\":\"Field\",\"spawn\":\"field_gate\"}  loads a map directly (setup, not input)")]
    internal sealed class TravelAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            string scene = step.String("scene");
            MapTravel.Travel(scene, step.String("spawn"));
            yield return run.WaitFor(() => ScenarioProbe.SceneName == scene && ScenarioProbe.IsGameReady, step.Float("max", 60f), $"scene {scene}");
            yield return new WaitForSeconds(step.Float("settle", 1f));
        }
        #endregion
    }

    [ScenarioAction("UsePortal", "Game", "{\"do\":\"UsePortal\",\"to\":\"Field\"}  walks to the portal and clicks it")]
    internal sealed class UsePortalAction : ScenarioAction
    {
        #region [Variables/Fields]
        private static readonly FieldInfo TargetField = typeof(MapPortal).GetField("_targetScene", BindingFlags.Instance | BindingFlags.NonPublic);
        #endregion

        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            string scene = step.String("to");
            MapPortal portal = null;
            foreach (MapPortal candidate in Object.FindObjectsByType<MapPortal>(FindObjectsSortMode.None))
            {
                if (TargetField != null && (string)TargetField.GetValue(candidate) == scene) portal = candidate;
            }
            if (portal == null)
            {
                run.FailStep($"no portal to {scene} in {ScenarioProbe.SceneName}");
                yield break;
            }

            yield return run.Lab.BringOnScreen(portal.transform, step.Float("walk", 30f));
            if (!ScenarioInput.TryScreenOf(portal.transform.position, out Vector2 screen))
            {
                run.FailStep("the portal is not on screen");
                yield break;
            }
            yield return run.Input.Click(screen);
            run.Log($"clicked the portal to {scene} at screen {screen.x:0},{screen.y:0}, UI under the pointer: {string.Join(", ", ScenarioInput.UIUnder(screen))}");
            yield return run.WaitFor(() => ScenarioProbe.SceneName == scene && ScenarioProbe.IsGameReady, step.Float("max", 60f), $"scene {scene}");
            yield return new WaitForSeconds(step.Float("settle", 1f));
        }
        #endregion
    }

    [ScenarioAction("Save", "Game", "{\"do\":\"Save\",\"slot\":1}  saves the player into the scenario agent's own profile")]
    internal sealed class SaveAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            int slot = step.Int("slot", 1);
            bool saved = PlayerCore.Instance != null && SaveStorage.SaveSlot(slot, PlayerCore.Instance.SaveData());
            if (!saved) run.FailStep($"could not save slot {slot}");
            else run.Note(SaveStorage.GetSlotFilePath(slot));
            yield break;
        }
        #endregion
    }

    [ScenarioAction("Load", "Game", "{\"do\":\"Load\",\"slot\":1}  loads a slot into the current player")]
    internal sealed class LoadAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            int slot = step.Int("slot", 1);
            PlayerDataModel data = SaveStorage.LoadSlot(slot);
            if (data == null || PlayerCore.Instance == null)
            {
                run.FailStep($"slot {slot} could not be loaded");
                yield break;
            }
            PlayerCore.Instance.LoadData(data);
            yield return null;
        }
        #endregion
    }

    [ScenarioAction("ContinueFromSlot", "Game", "{\"do\":\"ContinueFromSlot\",\"slot\":1}  continues from a save like the title screen does")]
    internal sealed class ContinueFromSlotAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            PlayerDataModel data = SaveStorage.LoadSlot(step.Int("slot", 1));
            if (data == null)
            {
                run.FailStep("the slot is empty");
                yield break;
            }
            GameCoreLoader.ContinueGame(data);
            yield return new WaitForSeconds(0.2f);
            yield return run.WaitFor(() => ScenarioProbe.IsGameReady, step.Float("max", 60f), "the game to continue");
        }
        #endregion
    }
}
#endif
