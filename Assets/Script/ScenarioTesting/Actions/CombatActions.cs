#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System.Collections;
using CoreSystem;
using CoreSystem.Entity;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace ScenarioTesting.Actions
{
    [ScenarioAction("SpawnTarget", "Combat", "{\"do\":\"SpawnTarget\",\"character\":1001,\"tank\":true}  frozen dummy next to the player, named \"target\"")]
    internal sealed class SpawnTargetAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            yield return run.Lab.EnsureTarget(step.Int("character", ScenarioLab.DefaultTargetId), step.Bool("tank", true));
        }
        #endregion
    }

    [ScenarioAction("Spawn", "Combat", "{\"do\":\"Spawn\",\"character\":1001,\"x\":-1.5,\"y\":0,\"as\":\"victim\",\"freeze\":true,\"tank\":false}  x/y are offsets from the player")]
    internal sealed class SpawnAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Character player = ScenarioProbe.Player;
            Vector3 position = player.transform.position + new Vector3(run.Number(step, "x", 1.5f), run.Number(step, "y", 0f));
            string name = step.String("as", $"spawn{step.Index + 1}");
            yield return run.Lab.Spawn(step.Int("character"), position, step.Bool("freeze", true), step.Bool("tank"), character => run.Name(name, character));
        }
        #endregion
    }

    [ScenarioAction("Attack", "Combat", "{\"do\":\"Attack\",\"target\":\"target\",\"hits\":3}  clicks the target and waits for hits; read them with hit.0, hits.count, hits.total; fails with a diagnosis when nothing lands unless \"allowNone\":true")]
    internal sealed class AttackAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            yield return run.Lab.Attack(run.Entity(step.String("target", ScenarioRun.TargetName)), step.Int("hits", 1), step.Float("max", 8f), step.Bool("allowNone"));
            run.Note($"{run.Lab.LastHits.Count} hits, {run.Lab.LastHitTotal:0.##} damage");
        }
        #endregion
    }

    [ScenarioAction("Kill", "Combat", "{\"do\":\"Kill\",\"character\":1001,\"count\":3,\"radius\":200,\"revives\":5}  walks to the nearest one, clicks it until it dies, then the next; on death presses Revive and keeps going until the step timeout (\"revives\" caps it)")]
    internal sealed class KillAction : ScenarioAction
    {
        #region [Variables/Fields]
        private const float ReclickAfter = 6f;
        private const float GiveUpAfter = 45f;
        private const float SearchTimeout = 30f;
        #endregion

        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            int characterId = step.Int("character");
            int wanted = step.Int("count", 1);
            float radius = step.Float("radius", 200f);
            int revivesLeft = step.Has("revives") ? step.Int("revives") : int.MaxValue;
            int kills = 0;
            float searchSince = -1f;
            while (kills < wanted)
            {
                Character player = ScenarioProbe.Player;
                Character enemy = player != null ? ScenarioProbe.FindCharacter(characterId, player.transform.position, true, radius) : null;
                if (enemy == null || !TeamRegistry.IsHostile(player, enemy))
                {
                    if (searchSince < 0f) searchSince = Time.time;
                    if (Time.time - searchSince > SearchTimeout)
                    {
                        run.FailStep($"no hostile {characterId} within {radius} in {ScenarioProbe.SceneName} after {kills} kills");
                        yield break;
                    }
                    yield return null;
                    continue;
                }
                searchSince = -1f;

                float giveUp = Time.time + GiveUpAfter;
                float lastHp = AttributeProbe.Read(enemy, AttributeType.HP);
                float reclick = 0f;
                while (enemy != null && !enemy.IsDead && player != null && !player.IsDead && Time.time < giveUp)
                {
                    float hp = AttributeProbe.Read(enemy, AttributeType.HP);
                    if (hp < lastHp)
                    {
                        lastHp = hp;
                        reclick = Time.time + ReclickAfter;
                    }
                    if (Time.time >= reclick)
                    {
                        yield return run.Lab.ClickCharacter(enemy, MouseButton.Left);
                        if (run.StepFailed) yield break;
                        reclick = Time.time + ReclickAfter;
                    }
                    yield return null;
                }

                if (player != null && player.IsDead && revivesLeft > 0)
                {
                    revivesLeft--;
                    yield return DeathFlow.Revive(run, $"after {kills} kills, fighting {(enemy != null ? enemy.name : "nothing")}");
                    if (run.StepFailed) yield break;
                    continue;
                }
                if (player == null || player.IsDead)
                {
                    run.FailStep($"the player died after {kills} kills and has no revives left");
                    yield break;
                }
                if (enemy != null && !enemy.IsDead)
                {
                    run.Log($"{enemy.name} is still alive after {GiveUpAfter}s, picking again ({run.Lab.LastClickReport})");
                    continue;
                }
                kills++;
                run.Log($"kill {kills}/{wanted}");
            }
            run.Note($"{kills} kills");
        }
        #endregion
    }

    internal static class DeathFlow
    {
        #region [Variables/Fields]
        public const string ReviveLabel = "Revive";
        public const string DeathsMetric = "deaths";
        #endregion

        #region [Public Methods]
        public static IEnumerator Revive(ScenarioRun run, string context)
        {
            run.Report.Metrics.TryGetValue(DeathsMetric, out float deaths);
            run.Measure(DeathsMetric, deaths + 1f);
            run.Log($"the player died {context}, pressing {ReviveLabel}");
            yield return DialogueInput.ClickLabel(run, ReviveLabel, 10f);
            if (run.StepFailed) yield break;
            yield return run.WaitFor(() => ScenarioProbe.Player != null && !ScenarioProbe.Player.IsDead && ScenarioProbe.IsGameReady, 20f, "the player to revive");
            yield return new WaitForSeconds(1f);
            run.Log($"revived in {ScenarioProbe.SceneName}");
        }
        #endregion
    }

    [ScenarioAction("Revive", "Combat", "{\"do\":\"Revive\"}  presses Revive on the death screen and waits for the player to stand up")]
    internal sealed class ReviveAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Character player = ScenarioProbe.Player;
            if (player == null || !player.IsDead)
            {
                run.Note("the player is alive");
                yield break;
            }
            yield return DeathFlow.Revive(run, "before this step");
        }
        #endregion
    }

    [ScenarioAction("Cast", "Combat", "{\"do\":\"Cast\",\"slot\":0,\"at\":\"target\",\"wait\":1.5}  presses the hotbar key, then clicks the target")]
    internal sealed class CastAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            yield return run.Lab.Cast(step.Int("slot"), run.Entity(step.String("at", ScenarioRun.TargetName)), step.Float("wait", 1.5f));
            run.Note($"cast started {run.Lab.LastCastStarted}, {run.Lab.LastHits.Count} hits");
        }
        #endregion
    }

    [ScenarioAction("TakeHits", "Combat", "{\"do\":\"TakeHits\",\"count\":3,\"amount\":40,\"from\":\"target\"}  the attacker hits the player through CombatCore")]
    internal sealed class TakeHitsAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            yield return run.Lab.TakeHits(run.Entity(step.String("from", ScenarioRun.TargetName)), step.Int("count", 1), run.Number(step, "amount", 40f), step.Float("gap", 0.15f));
        }
        #endregion
    }

    [ScenarioAction("Impulse", "Combat", "{\"do\":\"Impulse\",\"of\":\"player\",\"x\":1,\"y\":0,\"distance\":3}  pushes a character")]
    internal sealed class ImpulseAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Character character = run.Entity(step.String("of", ScenarioRun.PlayerName));
            character.ApplyImpulse(new Vector3(step.Float("x", 1f), step.Float("y", 0f)).normalized, step.Float("distance", 3f), step.Float("duration", 0.2f));
            yield return new WaitForSeconds(0.5f);
        }
        #endregion
    }
}
#endif
