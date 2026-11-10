#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System.Collections;
using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Data;
using CoreSystem.Entity;
using CoreSystem.Player;
using CoreSystem.Settings.Keybinds;
using CoreSystem.UI.ClassSelect;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ScenarioTesting.Actions
{
    internal static class Progression
    {
        #region [Variables/Fields]
        public const string RestKeyword = "rest";
        public const int MaxLevel = 999;
        public static readonly AttributeType[] Primaries = { AttributeType.STR, AttributeType.AGI, AttributeType.VIT, AttributeType.INT, AttributeType.DEX, AttributeType.LUK };
        #endregion

        #region [Public Methods]
        public static bool RaiseBase(Character character, int level)
        {
            int guard = 0;
            while (character.BaseLevel < level && guard++ < MaxLevel)
            {
                long need = character.NextBaseExp - character.CurrentBaseExp;
                if (need <= 0) return false;
                character.expSystem.AddBaseExp(need);
            }
            return character.BaseLevel >= level;
        }

        public static bool RaiseJob(Character character, int level)
        {
            int guard = 0;
            while (character.JobLevel < level && guard++ < MaxLevel)
            {
                long need = character.NextJobExp - character.CurrentJobExp;
                if (need <= 0) return false;
                character.expSystem.AddJobExp(need);
            }
            return character.JobLevel >= level;
        }

        public static int Spend(AttributeType type, int points)
        {
            int spent = 0;
            while (spent < points && GameData.Player.AttributePoints.Spend(type)) spent++;
            return spent;
        }
        #endregion
    }

    [ScenarioAction("Level", "Progression", "{\"do\":\"Level\",\"base\":20,\"job\":10}  gives the EXP to reach these levels, so the game hands out the stat and passive points itself")]
    internal sealed class LevelAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Character player = PlayerCore.GetPlayer();
            if (player == null)
            {
                run.FailStep("no player");
                yield break;
            }

            if (step.Has("base") && !Progression.RaiseBase(player, step.Int("base"))) run.FailStep($"base level stopped at {player.BaseLevel}");
            if (step.Has("job") && !Progression.RaiseJob(player, step.Int("job"))) run.FailStep($"job level stopped at {player.JobLevel}");
            ScenarioLab.Refill(player);
            run.Note($"base {player.BaseLevel}, job {player.JobLevel}, stat points {player.AttributePoints}, passive points {player.PassivePoints}");
            yield return null;
        }
        #endregion
    }

    [ScenarioAction("SpendStats", "Progression", "{\"do\":\"SpendStats\",\"VIT\":20,\"STR\":\"rest\"}  spends earned stat points like the + buttons; fails when there are not enough")]
    internal sealed class SpendStatsAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Character player = PlayerCore.GetPlayer();
            if (player == null)
            {
                run.FailStep("no player");
                yield break;
            }

            AttributeType? rest = null;
            var spentLines = new List<string>();
            foreach (AttributeType type in Progression.Primaries)
            {
                string key = type.ToString();
                if (!step.Has(key)) continue;
                if (string.Equals(step.String(key), Progression.RestKeyword, System.StringComparison.OrdinalIgnoreCase))
                {
                    rest = type;
                    continue;
                }

                int wanted = step.Int(key);
                int spent = Progression.Spend(type, wanted);
                spentLines.Add($"{key} +{spent}");
                if (spent < wanted)
                {
                    run.FailStep($"only {spent} of {wanted} points went into {key}, {player.AttributePoints} left; raise the level first");
                    yield break;
                }
            }

            if (rest.HasValue) spentLines.Add($"{rest.Value} +{Progression.Spend(rest.Value, player.AttributePoints)}");
            ScenarioLab.Refill(player);
            run.Note($"{string.Join(", ", spentLines)}; {player.AttributePoints} points left");
            yield return null;
        }
        #endregion
    }

    [ScenarioAction("SetBase", "Progression", "{\"do\":\"SetBase\",\"of\":\"player\",\"STR\":50,\"VIT\":40}  free mode: writes base stats directly, no points needed")]
    internal sealed class SetBaseAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Character character = SetupInput.Of(run, step);
            if (character == null) yield break;

            var lines = new List<string>();
            foreach (AttributeType type in Progression.Primaries)
            {
                string key = type.ToString();
                if (!step.Has(key)) continue;
                character.attributeSystem.GetAttribute(type).BaseValue = run.Number(step, key, 1f);
                lines.Add($"{key} {character.attributeSystem.GetAttribute(type).BaseValue}");
            }
            character.RecalculateStats();
            ScenarioLab.Refill(character);
            run.Note(string.Join(", ", lines));
            yield return null;
        }
        #endregion
    }
    [ScenarioAction("ChooseClass", "Progression", "{\"do\":\"ChooseClass\",\"class\":\"Warrior\"}  opens the Passive Tree with its key, clicks the class card, then Confirm")]
    internal sealed class ChooseClassAction : ScenarioAction
    {
        #region [Variables/Fields]
        private const string ConfirmLabel = "Confirm";
        #endregion

        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            string wanted = step.String("class");
            if (GameData.Player.Class.HasClass)
            {
                run.Note("the player already has a class");
                yield break;
            }
            if (!KeybindSetting.TryGetWindowKeys(ClassSelectGate.SkillTreeWindowName, out KeybindPair pair) || pair.Get(0) == Key.None)
            {
                run.FailStep($"no key opens {ClassSelectGate.SkillTreeWindowName}");
                yield break;
            }
            Key key = pair.Get(0);

            yield return new WaitForSeconds(0.3f);
            ClassCardSlot card = FindCard(wanted);
            if (card == null) yield return run.Input.Tap(key);
            else run.Log("the class screen is already open");
            yield return run.WaitFor(() => (card = FindCard(wanted)) != null, step.Float("max", 5f),
                $"the class card {wanted} after pressing {key} (job level {PlayerCore.GetPlayer()?.JobLevel})");
            if (run.StepFailed) yield break;

            yield return ScenarioButtonPress.Press(run, card.GetComponentInChildren<Button>());
            yield return DialogueInput.ClickLabel(run, ConfirmLabel);
            if (run.StepFailed) yield break;
            yield return run.WaitFor(() => GameData.Player.Class.HasClass, 3f, $"the class {wanted} to be chosen");
            if (run.StepFailed) yield break;

            yield return new WaitForSeconds(0.3f);
            yield return run.Input.Tap(key);
            yield return new WaitForSeconds(0.3f);
            ScenarioLab.Refill(PlayerCore.GetPlayer());
            run.Note($"class {wanted}, origin node {GameData.Player.Class.OriginNodeId}");
        }
        #endregion

        #region [Private Methods]
        private static ClassCardSlot FindCard(string label)
        {
            foreach (ClassCardSlot card in Object.FindObjectsByType<ClassCardSlot>(FindObjectsSortMode.None))
            {
                if (card.isActiveAndEnabled && string.Equals(card.ClassLabel, label, System.StringComparison.OrdinalIgnoreCase)) return card;
            }
            return null;
        }
        #endregion
    }
}
#endif
