using System.Collections.Generic;
using CoreSystem.Combat;
using CoreSystem.Skills;
using NUnit.Framework;

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
