#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Data;
using CoreSystem.Entity;
using UnityEngine;

namespace ScenarioTesting.Actions
{
    internal static class SetupInput
    {
        #region [Public Methods]
        public static Character Of(ScenarioRun run, ScenarioStep step, string fallback = ScenarioRun.PlayerName)
        {
            Character character = run.Entity(step.String("of", step.String("to", fallback)));
            if (character == null) run.FailStep($"nothing called {step.String("of", fallback)}");
            return character;
        }

        public static bool TryStat(ScenarioRun run, ScenarioStep step, out AttributeType type)
        {
            if (Enum.TryParse(step.String("stat"), true, out type)) return true;
            run.FailStep($"unknown stat '{step.String("stat")}'");
            return false;
        }

        public static List<string> Passives(ScenarioStep step)
        {
            var ids = step.Strings("passives");
            ids.AddRange(step.Strings("passive"));
            return ids;
        }
        #endregion
    }

    [ScenarioAction("Give", "Setup", "{\"do\":\"Give\",\"passives\":[\"Vampire\",\"HollowCrown\"],\"to\":\"player\"}  in this order")]
    internal sealed class GiveAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Character character = SetupInput.Of(run, step);
            if (character != null) run.Lab.Give(character, SetupInput.Passives(step));
            yield return null;
        }
        #endregion
    }

    [ScenarioAction("Take", "Setup", "{\"do\":\"Take\",\"passives\":[\"Vampire\"]}  removes passives")]
    internal sealed class TakeAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Character character = SetupInput.Of(run, step);
            if (character == null) yield break;
            foreach (string id in SetupInput.Passives(step)) character.UniquePassives.RemovePassive(id);
            character.RecalculateStats();
            yield return null;
        }
        #endregion
    }

    [ScenarioAction("ClearPassives", "Setup", "{\"do\":\"ClearPassives\",\"of\":\"player\"}")]
    internal sealed class ClearPassivesAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Character character = SetupInput.Of(run, step);
            if (character != null) run.Lab.ClearPassives(character);
            yield return null;
        }
        #endregion
    }

    [ScenarioAction("SetStat", "Setup", "{\"do\":\"SetStat\",\"of\":\"player\",\"stat\":\"CriticalRate\",\"total\":50}  or \"add\":10")]
    internal sealed class SetStatAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Character character = SetupInput.Of(run, step);
            if (character == null || !SetupInput.TryStat(run, step, out AttributeType type)) yield break;
            if (step.Has("total")) ScenarioLab.SetTotal(character, type, run.Number(step, "total", 0f));
            else ScenarioLab.AddStat(character, type, run.Number(step, "add", 0f));
            yield return null;
        }
        #endregion
    }

    [ScenarioAction("ClearStat", "Setup", "{\"do\":\"ClearStat\",\"of\":\"player\",\"stat\":\"PerfectDodge\"}  removes what SetStat added")]
    internal sealed class ClearStatAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Character character = SetupInput.Of(run, step);
            if (character != null && SetupInput.TryStat(run, step, out AttributeType type)) ScenarioLab.ClearLabStat(character, type);
            yield return null;
        }
        #endregion
    }

    [ScenarioAction("SetHp", "Setup", "{\"do\":\"SetHp\",\"of\":\"target\",\"ratio\":0.04}  or \"value\":100")]
    internal sealed class SetHpAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            yield return ResourceSetter.Set(run, step, AttributeType.HP, AttributeType.MaxHP);
        }
        #endregion
    }

    [ScenarioAction("SetMp", "Setup", "{\"do\":\"SetMp\",\"of\":\"player\",\"ratio\":0}")]
    internal sealed class SetMpAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            yield return ResourceSetter.Set(run, step, AttributeType.MP, AttributeType.MaxMP);
        }
        #endregion
    }

    internal static class ResourceSetter
    {
        #region [Public Methods]
        public static IEnumerator Set(ScenarioRun run, ScenarioStep step, AttributeType resource, AttributeType max)
        {
            Character character = SetupInput.Of(run, step);
            if (character == null) yield break;
            float value = step.Has("ratio") ? AttributeProbe.Read(character, max) * run.Number(step, "ratio", 1f) : run.Number(step, "value", 0f);
            ScenarioLab.SetResource(character, resource, value);
            yield return null;
        }
        #endregion
    }

    [ScenarioAction("Refill", "Setup", "{\"do\":\"Refill\",\"of\":\"player\"}  full HP and MP")]
    internal sealed class RefillAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            ScenarioLab.Refill(SetupInput.Of(run, step));
            yield return null;
        }
        #endregion
    }

    [ScenarioAction("UseItem", "Setup", "{\"do\":\"UseItem\",\"item\":2001}  adds the item and uses it; read potion (1 = kept)")]
    internal sealed class UseItemAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            yield return run.Lab.UseItem(step.Int("item", ScenarioLab.DefaultPotionId));
        }
        #endregion
    }

    [ScenarioAction("AddItem", "Setup", "{\"do\":\"AddItem\",\"item\":2001,\"qty\":5}")]
    internal sealed class AddItemAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            GameData.Player.Inventory.AddItem(step.Int("item"), step.Int("qty", 1));
            yield return null;
        }
        #endregion
    }

    [ScenarioAction("Reset", "Setup", "{\"do\":\"Reset\"}  clears player passives, refills HP/MP, respawns the target if needed, reseeds")]
    internal sealed class ResetAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Character player = ScenarioProbe.Player;
            run.Input.ReleaseAll();
            run.Lab.ClearPassives(player);
            ScenarioLab.Refill(player);
            yield return run.Lab.EnsureTarget();
            UnityEngine.Random.InitState(run.Definition.Seed);
        }
        #endregion
    }
}
#endif
