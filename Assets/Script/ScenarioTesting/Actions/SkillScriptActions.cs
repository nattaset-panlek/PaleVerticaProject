#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreSystem;
using CoreSystem.Data;
using CoreSystem.Entity;
using CoreSystem.Entity.UniquePassives;
using CoreSystem.Skills;
using UnityEngine;

namespace ScenarioTesting.Actions
{
    internal static class SkillRoutine
    {
        #region [Variables/Fields]
        public const int Slot = 0;
        private const float CastWait = 2.5f;
        private const float IdleTimeout = 10f;
        private const float ManaPool = 100000f;
        private const float FarFromTarget = 2.5f;
        private const float WalkTimeout = 8f;
        private const float SettleTime = 0.3f;
        #endregion

        #region [Public Methods]
        public static List<ActiveSkill> Skills(ScenarioStep step)
        {
            var only = new HashSet<int>(step.Ints("skills"));
            var skip = new HashSet<int>(step.Ints("skip"));
            return Database.Skills
                .Where(skill => skill != null && !skip.Contains(skill.Id) && (only.Count == 0 || only.Contains(skill.Id)))
                .OrderBy(skill => skill.Id)
                .ToList();
        }

        public static List<string> Passives(ScenarioStep step)
        {
            var skip = new HashSet<string>(step.Strings("skipPassives"));
            return UniquePassiveSystem.RegisteredTypes.Keys.Where(id => !skip.Contains(id)).OrderBy(id => id).ToList();
        }

        public static void Begin(Character player)
        {
            ScenarioLab.SetTotal(player, AttributeType.MaxMP, ManaPool);
            ScenarioLab.Refill(player);
        }

        public static void End(Character player)
        {
            ScenarioLab.ClearLabStat(player, AttributeType.MaxMP);
            ScenarioLab.Refill(player);
        }

        public static string Label(ActiveSkill skill)
        {
            return $"{skill.Id} {skill.Name}";
        }

        public static IEnumerator CastOne(ScenarioRun run, ActiveSkill skill, string label)
        {
            Character player = ScenarioProbe.Player;
            if (player == null || player.skillCaster == null)
            {
                run.FailStep("no player to cast with");
                yield break;
            }

            yield return Prepare(run, player);
            if (run.StepFailed) yield break;

            int errors = run.ErrorCount;
            SkillCaster caster = player.skillCaster;
            SkillInstance previous = caster.EquippedSkills[Slot];
            bool wasGranted = caster.GrantedSkills.Exists(granted => granted.SkillData != null && granted.SkillData.Id == skill.Id);
            SkillInstance instance = caster.GrantSkill(skill);
            if (instance == null)
            {
                run.Record($"{label}: can be granted", ScenarioCheckStatus.Failed, "granted", "GrantSkill returned null");
                yield break;
            }
            caster.EquipSkill(instance, Slot);
            yield return null;

            yield return run.Lab.Cast(Slot, run.Lab.Target, CastWait);
            if (run.Lab.LastCastStarted)
            {
                run.Record($"{label}: cast started", ScenarioCheckStatus.Passed, "started", "started");
            }
            else if (BlockedByPassive(player, instance, run.Lab.Target))
            {
                run.Record($"{label}: cast started", ScenarioCheckStatus.Passed, "started, or blocked by a passive", "blocked by a passive (CanCast is false), as designed");
            }
            else
            {
                run.Record($"{label}: cast started", ScenarioCheckStatus.Review, "started", $"did not start ({ScenarioLab.SkillSlotText(Slot)}, caster {caster.CurrentState})",
                    "Some skills need a condition (an incoming hit, an ally, a free spot). Look at the log if this one should have started.");
            }
            run.Measure($"{label} damage", run.Lab.LastHitTotal);

            yield return WaitForIdle(caster);
            run.Record($"{label}: caster returns to Idle", caster.CurrentState == SkillCasterState.Idle ? ScenarioCheckStatus.Passed : ScenarioCheckStatus.Failed,
                "Idle", caster.CurrentState.ToString());

            if (skill.IsToggle && instance.IsToggledOn)
            {
                yield return run.Lab.Cast(Slot, run.Lab.Target, CastWait);
                yield return WaitForIdle(caster);
                run.Record($"{label}: toggles off", instance.IsToggledOn ? ScenarioCheckStatus.Failed : ScenarioCheckStatus.Passed, "off", instance.IsToggledOn ? "still on" : "off");
            }

            yield return new WaitForSeconds(SettleTime);
            PassiveRoutine.CheckHealthy(run, $"{label}: player", player);
            PassiveRoutine.CheckHealthy(run, $"{label}: target", run.Lab.Target);
            run.Record($"{label}: no errors", run.ErrorCount == errors ? ScenarioCheckStatus.Passed : ScenarioCheckStatus.Failed, "0 errors", $"{run.ErrorCount - errors}");

            caster.EquipSkill(previous, Slot);
            if (!wasGranted) caster.RevokeSkill(skill.Id);
            run.Input.ReleaseAll();
        }
        #endregion

        #region [Private Methods]
        private static IEnumerator Prepare(ScenarioRun run, Character player)
        {
            run.Input.ReleaseAll();
            ScenarioLab.Refill(player);
            if (run.Lab.Target != null && !run.Lab.Target.IsDead && !TeamRegistry.IsHostile(player, run.Lab.Target)) run.Lab.DropTarget();
            yield return run.Lab.EnsureTarget();
            if (run.Lab.Target == null) yield break;
            if (Vector2.Distance(player.transform.position, run.Lab.Target.transform.position) > FarFromTarget)
            {
                yield return run.Lab.Approach(run.Lab.Target.transform.position, ScenarioLab.TargetDistance, WalkTimeout);
            }
        }

        private static bool BlockedByPassive(Character player, SkillInstance instance, Character target)
        {
            if (player.UniquePassives == null || instance == null) return false;
            Vector3 at = target != null ? target.transform.position : player.transform.position;
            SkillPayload payload = player.skillCaster.BuildPayload(instance, at, true);
            return payload != null && !player.UniquePassives.CanCast(payload);
        }

        private static IEnumerator WaitForIdle(SkillCaster caster)
        {
            float deadline = Time.time + IdleTimeout;
            while (caster.CurrentState != SkillCasterState.Idle && Time.time < deadline) yield return null;
        }
        #endregion
    }

    [ScenarioAction("CastEverySkill", "Script", "{\"do\":\"CastEverySkill\",\"skip\":[1019],\"skills\":[1001],\"timeout\":3600}  puts each Active skill on slot 1, casts it at the target and checks errors, stuck casts, toggles and hp/mp")]
    internal sealed class CastEverySkillAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Character player = ScenarioProbe.Player;
            List<ActiveSkill> skills = SkillRoutine.Skills(step);
            run.Measure("skills tested", skills.Count);
            SkillRoutine.Begin(player);
            foreach (ActiveSkill skill in skills)
            {
                yield return SkillRoutine.CastOne(run, skill, SkillRoutine.Label(skill));
                if (run.StepFailed) break;
            }
            SkillRoutine.End(player);
        }
        #endregion
    }

    [ScenarioAction("SkillPassiveCombos", "Script", "{\"do\":\"SkillPassiveCombos\",\"mode\":\"all\"}  each Active skill with every Unique Passive at once, or \"mode\":\"random\",\"count\":2,\"size\":5 for seeded random sets per skill")]
    internal sealed class SkillPassiveCombosAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Character player = ScenarioProbe.Player;
            List<ActiveSkill> skills = SkillRoutine.Skills(step);
            List<string> passives = SkillRoutine.Passives(step);
            bool random = step.String("mode", "all") == "random";
            int count = random ? step.Int("count", 2) : 1;
            int size = step.Int("size", 5);
            var picker = new System.Random(run.Definition.Seed);
            run.Measure("skills tested", skills.Count);

            SkillRoutine.Begin(player);
            foreach (ActiveSkill skill in skills)
            {
                for (int i = 0; i < count; i++)
                {
                    string[] loadout = random ? passives.OrderBy(_ => picker.Next()).Take(size).ToArray() : passives.ToArray();
                    string label = random ? $"{SkillRoutine.Label(skill)} + {string.Join("+", loadout)}" : $"{SkillRoutine.Label(skill)} + every passive";
                    run.Lab.ClearPassives(player);
                    run.Lab.Give(player, loadout);
                    SkillRoutine.Begin(player);
                    yield return SkillRoutine.CastOne(run, skill, label);
                    run.Lab.ClearPassives(player);
                    if (run.StepFailed) break;
                }
                if (run.StepFailed) break;
            }
            SkillRoutine.End(player);
        }
        #endregion
    }
}
#endif
