#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreSystem;
using CoreSystem.Entity;
using CoreSystem.Entity.UniquePassives;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScenarioTesting.Actions
{
    internal static class PassiveRoutine
    {
        #region [Variables/Fields]
        public const float IncomingDamage = 40f;
        private const float SettleTime = 0.2f;
        #endregion

        #region [Public Methods]
        public static IEnumerator Standard(ScenarioRun run, string label)
        {
            int errors = run.ErrorCount;
            Character player = ScenarioProbe.Player;
            yield return run.Lab.EnsureTarget();

            yield return run.Lab.Attack(run.Lab.Target, 4, 6f, true);
            run.Measure($"{label} basic damage", run.Lab.LastHitTotal);
            yield return run.Lab.Cast(0, run.Lab.Target, 1.5f);
            yield return run.Lab.TakeHits(run.Lab.Target, 3, IncomingDamage, 0.15f);
            yield return run.Lab.UseItem(ScenarioLab.DefaultPotionId);
            yield return new WaitForSeconds(0.5f);
            yield return run.Lab.HoldKeys(new[] { Key.A }, 0.5f);

            CheckHealthy(run, $"{label}: player", player);
            CheckHealthy(run, $"{label}: target", run.Lab.Target);
            run.Record($"{label}: no errors", run.ErrorCount == errors ? ScenarioCheckStatus.Passed : ScenarioCheckStatus.Failed, "0 errors", $"{run.ErrorCount - errors}");
        }

        public static IEnumerator WithLeakCheck(ScenarioRun run, string label, IEnumerable<string> passives)
        {
            Character player = ScenarioProbe.Player;
            run.Lab.ClearPassives(player);
            ScenarioLab.Refill(player);
            yield return run.Lab.EnsureTarget();
            Random.InitState(run.Definition.Seed);

            Dictionary<AttributeType, float> before = run.Lab.StableSnapshot(player);
            run.Lab.Give(player, passives);
            yield return Standard(run, label);
            run.Lab.ClearPassives(player);
            yield return new WaitForSeconds(SettleTime);

            var leaked = AttributeProbe.Diff(before, run.Lab.StableSnapshot(player));
            run.Record($"{label}: removing restores every stat", leaked.Count == 0 ? ScenarioCheckStatus.Passed : ScenarioCheckStatus.Failed, "no change",
                leaked.Count == 0 ? "no change" : string.Join(", ", leaked.Select(pair => $"{pair.Key} {pair.Value:+0.##;-0.##}")));
        }

        public static void CheckHealthy(ScenarioRun run, string label, Character character)
        {
            if (character == null) return;
            float hp = AttributeProbe.Read(character, AttributeType.HP);
            float mp = AttributeProbe.Read(character, AttributeType.MP);
            float maxHp = AttributeProbe.Read(character, AttributeType.MaxHP);
            bool valid = IsFinite(hp) && IsFinite(mp) && hp >= 0f && mp >= 0f && IsFinite(maxHp) && maxHp > 0f;
            run.Record($"{label} hp, mp and max hp are valid", valid ? ScenarioCheckStatus.Passed : ScenarioCheckStatus.Failed, "finite, not negative", $"hp {hp}, mp {mp}, max {maxHp}");
        }
        #endregion

        #region [Private Methods]
        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
        #endregion
    }

    [ScenarioAction("StandardRoutine", "Script", "{\"do\":\"StandardRoutine\",\"label\":\"vampire\"}  attack, cast slot 0, take hits, drink, idle, walk, then sanity checks")]
    internal sealed class StandardRoutineAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            yield return PassiveRoutine.Standard(run, step.String("label", "routine"));
        }
        #endregion
    }

    [ScenarioAction("SmokeAllPassives", "Script", "{\"do\":\"SmokeAllPassives\",\"skip\":[\"Bleeding\"],\"timeout\":3600}  every registered Unique Passive through the routine and a stat leak check")]
    internal sealed class SmokeAllPassivesAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            var skip = new HashSet<string>(step.Strings("skip"));
            List<string> ids = UniquePassiveSystem.RegisteredTypes.Keys.Where(id => !skip.Contains(id)).OrderBy(id => id).ToList();
            run.Measure("passives tested", ids.Count);
            foreach (string id in ids) yield return PassiveRoutine.WithLeakCheck(run, id, new[] { id });
        }
        #endregion
    }

    [ScenarioAction("RandomLoadouts", "Script", "{\"do\":\"RandomLoadouts\",\"count\":12,\"size\":5,\"timeout\":3600}  seeded random sets of passives through the routine")]
    internal sealed class RandomLoadoutsAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            List<string> ids = UniquePassiveSystem.RegisteredTypes.Keys.OrderBy(id => id).ToList();
            var random = new System.Random(run.Definition.Seed);
            int count = step.Int("count", 12);
            int size = step.Int("size", 5);
            for (int i = 0; i < count; i++)
            {
                string[] loadout = ids.OrderBy(_ => random.Next()).Take(size).ToArray();
                yield return PassiveRoutine.WithLeakCheck(run, $"loadout {i + 1}: {string.Join("+", loadout)}", loadout);
            }
        }
        #endregion
    }
}
#endif
