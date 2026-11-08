using CoreSystem.Combat;
using CoreSystem.Movement;
using CoreSystem.Skills.Logic;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class SprintTests
    {
        #region [Variables/Fields]
        private const float Duration = 10f;
        private const float Sprint = 0.6f;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [Test]
        public void BonusFollowsSprintJogWalk()
        {
            Assert.AreEqual(0.6f, MovementFormula.SprintBonus(0f, Duration, Sprint), 1e-5f);
            Assert.AreEqual(0.6f, MovementFormula.SprintBonus(3.9f, Duration, Sprint), 1e-5f);
            Assert.AreEqual(0.3f, MovementFormula.SprintBonus(5f, Duration, Sprint), 1e-5f);
            Assert.AreEqual(0.3f, MovementFormula.SprintBonus(7.9f, Duration, Sprint), 1e-5f);
            Assert.AreEqual(0.15f, MovementFormula.SprintBonus(9f, Duration, Sprint), 1e-5f);
            Assert.AreEqual(0f, MovementFormula.SprintBonus(10f, Duration, Sprint), 1e-5f);
            Assert.AreEqual(0f, MovementFormula.SprintBonus(12f, Duration, Sprint), 1e-5f);
        }

        [Test]
        public void BonusNeverRises()
        {
            float previous = float.MaxValue;
            for (int i = 0; i <= 200; i++)
            {
                float value = MovementFormula.SprintBonus(i * 0.05f, Duration, Sprint);
                Assert.LessOrEqual(value, previous + 1e-6f);
                previous = value;
            }
        }

        [Test]
        public void PhasesAreReportedInOrder()
        {
            Assert.AreEqual(SprintPhase.Sprint, MovementFormula.GetSprintPhase(1f, Duration));
            Assert.AreEqual(SprintPhase.SprintToJog, MovementFormula.GetSprintPhase(4.5f, Duration));
            Assert.AreEqual(SprintPhase.Jog, MovementFormula.GetSprintPhase(6f, Duration));
            Assert.AreEqual(SprintPhase.JogToWalk, MovementFormula.GetSprintPhase(9f, Duration));
            Assert.AreEqual(SprintPhase.Walk, MovementFormula.GetSprintPhase(10f, Duration));
        }

        [Test]
        public void ZeroDurationOrBonusGivesNothing()
        {
            Assert.AreEqual(0f, MovementFormula.SprintBonus(1f, 0f, Sprint));
            Assert.AreEqual(0f, MovementFormula.SprintBonus(1f, Duration, 0f));
        }

        [Test]
        public void SprintBuffRaisesThenRestoresMoveSpeed()
        {
            var stats = TestStats.Create();
            var buffs = new BuffSystem();
            buffs.Initialize(stats);
            float walk = stats.GetAttribute(AttributeType.MoveSpeed).CurrentValue;

            var sprint = new SprintEffect { SprintBonusPercent = 60f, Duration = Duration, TickInterval = 0.1f };
            sprint.Begin(buffs);
            Assert.AreEqual(walk * 1.6f, stats.GetAttribute(AttributeType.MoveSpeed).CurrentValue, 0.01f);

            Advance(buffs, 6f);
            Assert.AreEqual(walk * 1.3f, stats.GetAttribute(AttributeType.MoveSpeed).CurrentValue, 0.5f);

            Advance(buffs, 4.2f);
            Assert.IsFalse(buffs.HasBuff(sprint.ResolvedBuffCode));
            Assert.AreEqual(walk, stats.GetAttribute(AttributeType.MoveSpeed).CurrentValue, 0.01f);
        }

        [Test]
        public void RecastRefreshesToFullSprint()
        {
            var stats = TestStats.Create();
            var buffs = new BuffSystem();
            buffs.Initialize(stats);
            float walk = stats.GetAttribute(AttributeType.MoveSpeed).CurrentValue;

            var sprint = new SprintEffect { SprintBonusPercent = 60f, Duration = Duration, TickInterval = 0.1f };
            sprint.Begin(buffs);
            Advance(buffs, 6f);
            sprint.Begin(buffs);

            Assert.AreEqual(walk * 1.6f, stats.GetAttribute(AttributeType.MoveSpeed).CurrentValue, 0.01f);
            Assert.AreEqual(1, buffs.GetBuff(sprint.ResolvedBuffCode, AttributeType.MoveSpeed).Stacks);
        }
        #endregion

        #region [Private Methods]
        private static void Advance(BuffSystem buffs, float seconds)
        {
            const float step = 0.05f;
            for (float t = 0f; t < seconds; t += step) buffs.Tick(step);
        }
        #endregion
    }
}
