using System.Collections.Generic;
using CoreSystem.Combat;
using CoreSystem.Entity;
using CoreSystem.Skills;
using NUnit.Framework;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class CombatCoreTests
    {
        #region [Variables/Fields]
        private const float Tolerance = 0.0001f;
        private const int FreeForAllFaction = 1;
        private const int NeutralFaction = 3;
        private const int PlainFaction = 5;
        private readonly List<FakeCombatant> _combatants = new List<FakeCombatant>();
        #endregion

        #region [Unity Methods]
        [TearDown]
        public void TearDown()
        {
            foreach (var combatant in _combatants) combatant.Dispose();
            _combatants.Clear();
        }
        #endregion

        #region [Public Methods]
        [Test]
        public void Resolve_DamageMatchesFormulaAndDoesNotTouchHp()
        {
            var user = Create(FreeForAllFaction, TestStats.Create(str: 50f, dex: 10f));
            var target = Create(FreeForAllFaction);
            var payload = Payload(20f);
            float hpBefore = target.Hp;

            var result = CombatCore.Resolve(CombatAction.Damage(payload), user, target, new FixedCombatRandom(99f));

            float expected = DamageFormula.Calculate(DamageFormula.BuildInput(payload, user, target), 99f).Damage;
            Assert.IsFalse(result.IsBlocked);
            Assert.AreEqual(1, result.Outcomes.Count);
            Assert.AreEqual(expected, result.Outcomes[0].Amount, Tolerance);
            Assert.AreEqual(AttributeType.HP, result.Outcomes[0].Attribute);
            Assert.AreEqual(hpBefore, target.Hp, Tolerance);
        }

        [Test]
        public void Execute_AppliesDamageAndRaisesEvents()
        {
            var user = Create(FreeForAllFaction, TestStats.Create(str: 50f));
            var target = Create(FreeForAllFaction);
            float hpBefore = target.Hp;

            var result = CombatCore.Execute(CombatAction.Damage(Payload(20f)), user, target, new FixedCombatRandom(99f));

            Assert.AreEqual(hpBefore - result.Total(EffectKind.Damage), target.Hp, Tolerance);
            Assert.AreEqual(1, user.HitEvents);
            Assert.AreEqual(1, target.DamageTakenEvents);
        }

        [Test]
        public void Crit_UsesInjectedRandom()
        {
            var user = Create(FreeForAllFaction, TestStats.Create(luk: 100f));
            var target = Create(FreeForAllFaction);

            var crit = CombatCore.Resolve(CombatAction.Damage(Payload(50f)), user, target, new FixedCombatRandom(0f));
            var normal = CombatCore.Resolve(CombatAction.Damage(Payload(50f)), user, target, new FixedCombatRandom(99f));

            Assert.IsTrue(crit.HasCritical());
            Assert.IsFalse(normal.HasCritical());
            Assert.Greater(crit.Total(EffectKind.Damage), normal.Total(EffectKind.Damage));
        }

        [Test]
        public void Gate_BlocksNonHostileTarget()
        {
            var user = Create(NeutralFaction);
            var target = Create(FreeForAllFaction);
            float hpBefore = target.Hp;

            var result = CombatCore.Execute(CombatAction.Damage(Payload(100f)), user, target);

            Assert.IsTrue(result.Outcomes[0].IsBlocked);
            Assert.AreEqual(BlockReason.NotHostile, result.Outcomes[0].BlockReason);
            Assert.AreEqual(hpBefore, target.Hp, Tolerance);
            Assert.AreEqual(0, target.DamageTakenEvents);
        }

        [Test]
        public void Gate_BlocksMissingTargetWithoutThrowing()
        {
            var user = Create(FreeForAllFaction);

            var result = CombatCore.Execute(CombatAction.Damage(Payload(10f)), user, null);

            Assert.AreEqual(BlockReason.NoTarget, result.BlockReason);
            Assert.AreEqual(0, result.Outcomes.Count);
        }

        [Test]
        public void Gate_AllowsSelfProcButNotOtherSelfDamage()
        {
            var unit = Create(PlainFaction);

            var procPayload = Payload(10f);
            procPayload.SkillID = ProcSkillIds.BleedTick;
            var proc = CombatCore.Resolve(CombatAction.Damage(procPayload), unit, unit);
            var normal = CombatCore.Resolve(CombatAction.Damage(Payload(10f)), unit, unit);

            Assert.IsFalse(proc.Outcomes[0].IsBlocked);
            Assert.AreEqual(BlockReason.NotHostile, normal.Outcomes[0].BlockReason);
        }

        [Test]
        public void NoUser_SkipsHostilityAndUserStats()
        {
            var target = Create(NeutralFaction);
            var payload = Payload(100f);

            var result = CombatCore.Resolve(CombatAction.Damage(payload), null, target, new FixedCombatRandom(0f));

            float expected = DamageFormula.Calculate(DamageFormula.BuildInput(payload, null, target), 0f).Damage;
            Assert.IsFalse(result.Outcomes[0].IsBlocked);
            Assert.IsFalse(result.Outcomes[0].IsCritical);
            Assert.AreEqual(expected, result.Outcomes[0].Amount, Tolerance);
        }

        [Test]
        public void Dispatch_ResolvesRegisteredHandlerAndBlocksUnknownKinds()
        {
            var user = Create(FreeForAllFaction);
            var target = Create(FreeForAllFaction);
            var action = new CombatAction(Payload(10f)).WithEffect(new DamageSpec()).WithEffect(new UnhandledSpec());

            var result = CombatCore.Resolve(action, user, target, new FixedCombatRandom(99f));

            Assert.IsInstanceOf<DamageHandler>(EffectHandlers.For(EffectKind.Damage));
            Assert.AreEqual(2, result.Outcomes.Count);
            Assert.IsFalse(result.Outcomes[0].IsBlocked);
            Assert.AreEqual(BlockReason.NoHandler, result.Outcomes[1].BlockReason);
        }

        [Test]
        public void Rider_ReentersResolveAndAppliesAfterMainHit()
        {
            var user = Create(FreeForAllFaction);
            var target = Create(FreeForAllFaction);
            float hpBefore = target.Hp;
            var action = CombatAction.Damage(Payload(10f)).WithRider(new ExtraHitRider(5f));

            var result = CombatCore.Execute(action, user, target, new FixedCombatRandom(99f));

            Assert.AreEqual(1, result.SubResults.Count);
            Assert.IsTrue(result.SubResults[0].Action.IsProc);
            Assert.AreEqual(1, result.SubResults[0].Depth);
            float total = result.Total(EffectKind.Damage) + result.SubResults[0].Total(EffectKind.Damage);
            Assert.AreEqual(hpBefore - total, target.Hp, Tolerance);
            Assert.AreEqual(2, target.DamageTakenEvents);
        }

        [Test]
        public void Rider_StopsAtMaxDepth()
        {
            var user = Create(FreeForAllFaction);
            var target = Create(FreeForAllFaction);
            var action = CombatAction.Damage(Payload(1f)).WithRider(new RecursiveRider());

            var result = CombatCore.Resolve(action, user, target, new FixedCombatRandom(99f));

            int depth = 0;
            var current = result;
            while (current.SubResults.Count > 0)
            {
                current = current.SubResults[0];
                depth++;
            }
            Assert.AreEqual(CombatCore.MaxDepth, depth);
        }

        [Test]
        public void Rider_RespectsChanceAndTrigger()
        {
            var user = Create(FreeForAllFaction);
            var target = Create(FreeForAllFaction);
            var neverRider = new ExtraHitRider(5f) { ChancePercent = 0f };
            var critRider = new ExtraHitRider(5f) { Trigger = RiderTrigger.OnCrit };
            var action = CombatAction.Damage(Payload(10f)).WithRider(neverRider).WithRider(critRider);

            var result = CombatCore.Resolve(action, user, target, new FixedCombatRandom(99f));

            Assert.IsFalse(result.HasCritical());
            Assert.AreEqual(0, result.SubResults.Count);
        }

        [Test]
        public void Result_PredictsKillFromHpDamage()
        {
            var user = Create(FreeForAllFaction, TestStats.Create(str: 200f));
            var target = Create(FreeForAllFaction);
            target.SetHp(5f);

            var result = CombatCore.Resolve(CombatAction.Damage(Payload(50f)), user, target, new FixedCombatRandom(99f));

            Assert.IsTrue(result.PredictsKill());
        }

        [Test]
        public void CreditTo_GetsHitEventWithoutAddingItsStats()
        {
            var burner = Create(FreeForAllFaction, TestStats.Create(str: 200f));
            var target = Create(FreeForAllFaction);
            float before = target.Hp;
            var tick = new SkillPayload { SkillID = ProcSkillIds.BurnTick, SkillLevel = 1, BaseDamage = 7f, DamageType = DamageType.Status, SkillElement = ElementType.Neutral };
            var action = CombatAction.Damage(tick);
            action.CreditTo = burner;

            CombatCore.Execute(action, null, target, new FixedCombatRandom(99f));

            Assert.AreEqual(1, burner.HitEvents);
            Assert.AreEqual(1, target.DamageTakenEvents);
            Assert.AreEqual(before - DamageFormula.Mitigate(7f, target.attributeSystem.GetAttribute(AttributeType.StatusDEF)), target.Hp, 0.001f);
        }

        [Test]
        public void KillingBlow_RaisesKilledOnceForTheCreditedKiller()
        {
            var burner = Create(FreeForAllFaction);
            var target = Create(FreeForAllFaction);
            target.SetHp(5f);
            int kills = 0;
            IEntity killer = null;
            System.Action<IEntity, IEntity> listener = (k, v) => { kills++; killer = k; };
            CombatEvents.OnKilled += listener;
            try
            {
                var action = CombatAction.Damage(new SkillPayload { SkillID = ProcSkillIds.BurnTick, BaseDamage = 50f, DamageType = DamageType.Status });
                action.CreditTo = burner;
                CombatCore.Execute(action, null, target, new FixedCombatRandom(99f));
                var second = CombatAction.Damage(new SkillPayload { SkillID = ProcSkillIds.BurnTick, BaseDamage = 50f, DamageType = DamageType.Status });
                second.CreditTo = burner;
                CombatCore.Execute(second, null, target, new FixedCombatRandom(99f));
            }
            finally
            {
                CombatEvents.OnKilled -= listener;
            }

            Assert.AreEqual(1, kills);
            Assert.AreSame(burner, killer);
        }

        [Test]
        public void KillReward_LevelCurveAndOverride()
        {
            Assert.AreEqual(0L, KillRewardFormula.BaseExpForLevel(0));
            Assert.AreEqual(12L, KillRewardFormula.BaseExpForLevel(1));
            Assert.Greater(KillRewardFormula.BaseExpForLevel(10), KillRewardFormula.BaseExpForLevel(9));
            Assert.AreEqual(Mathf.RoundToInt(KillRewardFormula.BaseExpForLevel(10) * 0.6f), KillRewardFormula.JobExpForLevel(10));
            Assert.AreEqual(50L, KillRewardFormula.FullExp(50, 999));
            Assert.AreEqual(999L, KillRewardFormula.FullExp(0, 999));
        }

        [Test]
        public void KillReward_LevelGap()
        {
            Assert.AreEqual(1f, KillRewardFormula.LevelDifferenceMultiplier(10, 10), 0.0001f);
            Assert.AreEqual(1.25f, KillRewardFormula.LevelDifferenceMultiplier(10, 15), 0.0001f);
            Assert.AreEqual(1.5f, KillRewardFormula.LevelDifferenceMultiplier(10, 40), 0.0001f);
            Assert.AreEqual(1f, KillRewardFormula.LevelDifferenceMultiplier(10, 5), 0.0001f);
            Assert.AreEqual(0.7f, KillRewardFormula.LevelDifferenceMultiplier(20, 12), 0.0001f);
            Assert.AreEqual(0.4f, KillRewardFormula.LevelDifferenceMultiplier(20, 7), 0.0001f);
            Assert.AreEqual(0.1f, KillRewardFormula.LevelDifferenceMultiplier(30, 1), 0.0001f);
        }

        [Test]
        public void KillReward_ShareAndReward()
        {
            Assert.AreEqual(0.25f, KillRewardFormula.Share(25f, 100f), 0.0001f);
            Assert.AreEqual(0f, KillRewardFormula.Share(25f, 0f), 0.0001f);
            Assert.AreEqual(30L, KillRewardFormula.Reward(100, 0.25f, 1.2f, 1f));
            Assert.AreEqual(0L, KillRewardFormula.Reward(100, 0f, 1f, 1f));
            Assert.AreEqual(50L, KillRewardFormula.Reward(100, 1f, 1f, 0.5f));
            Assert.IsTrue(CombatEvents.IsKillingBlow(5f, 0f));
            Assert.IsFalse(CombatEvents.IsKillingBlow(0f, 0f));
            Assert.IsFalse(CombatEvents.IsKillingBlow(5f, 1f));
        }

        [Test]
        public void KillCredit_SplitsByHpRemovedAndCountsSummonForOwner()
        {
            var owner = Create(FreeForAllFaction);
            var summon = Create(FreeForAllFaction);
            var other = Create(FreeForAllFaction);
            var victim = Create(FreeForAllFaction);
            TeamRegistry.Bind(summon, owner);
            try
            {
                KillCreditRegistry.Record(owner, victim, 30f);
                KillCreditRegistry.Record(summon, victim, 30f);
                KillCreditRegistry.Record(other, victim, 40f);
                KillCreditRegistry.Record(summon, owner, 99f);

                Assert.AreEqual(0.6f, KillCreditRegistry.ShareOf(victim, owner), 0.0001f);
                Assert.AreEqual(0f, KillCreditRegistry.ShareOf(victim, summon), 0.0001f);
                Assert.AreEqual(0.4f, KillCreditRegistry.ShareOf(victim, other), 0.0001f);
                Assert.AreEqual(0f, KillCreditRegistry.Total(owner), 0.0001f);
            }
            finally
            {
                TeamRegistry.Unbind(summon);
                KillCreditRegistry.Forget(victim);
                KillCreditRegistry.Forget(owner);
            }
        }
        #endregion

        #region [Private Methods]
        private FakeCombatant Create(int faction, Attributes.AttributeSystem stats = null)
        {
            var combatant = new FakeCombatant(faction, stats);
            _combatants.Add(combatant);
            return combatant;
        }

        private static SkillPayload Payload(float baseDamage)
        {
            return new SkillPayload { SkillID = 1, SkillLevel = 1, BaseDamage = baseDamage, DamageType = DamageType.Physical, SkillElement = ElementType.Neutral };
        }
        #endregion

        private sealed class UnhandledSpec : EffectSpec
        {
            #region [Properties]
            public override EffectKind Kind => EffectKind.Custom;
            #endregion
        }

        private sealed class ExtraHitRider : ActionRider
        {
            #region [Variables/Fields]
            private readonly float _damage;
            #endregion

            #region [Public Methods]
            public ExtraHitRider(float damage)
            {
                _damage = damage;
            }
            #endregion

            #region [Private Methods]
            protected override void Execute(ResolveContext context)
            {
                context.Spawn(CombatAction.Damage(Payload(_damage)), context.User, context.Target);
            }
            #endregion
        }

        private sealed class RecursiveRider : ActionRider
        {
            #region [Private Methods]
            protected override void Execute(ResolveContext context)
            {
                context.Spawn(CombatAction.Damage(Payload(1f)).WithRider(this), context.User, context.Target);
            }
            #endregion
        }
    }
}
