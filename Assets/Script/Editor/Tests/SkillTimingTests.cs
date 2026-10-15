using CoreSystem;
using CoreSystem.Skills;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class SkillTimingTests
    {
        #region [Variables/Fields]
        private const float Tolerance = 0.0001f;
        #endregion

        #region [Public Methods]
        [Test]
        public void CastTime_WithoutStats_ReturnsBaseValues()
        {
            Assert.AreEqual(3f, new SkillInstance().CalculateCastTimeModified(null, 2f, 1f), Tolerance);
        }

        [Test]
        public void VariableCastTime_ReachesZeroAtStatCap()
        {
            var stats = TestStats.Create(dex: 265f, @int: 0f);

            Assert.AreEqual(0f, new SkillInstance().CalculateCastTimeModified(stats, 5f, 0f), Tolerance);
        }

        [Test]
        public void VariableCastTime_NeverGoesNegative()
        {
            var stats = TestStats.Create();
            TestStats.SetBase(stats, AttributeType.VariableCastTimeReduc, 10f);

            Assert.AreEqual(0f, new SkillInstance().CalculateCastTimeModified(stats, 2f, 0f), Tolerance);
        }

        [Test]
        public void FixedCastTime_IsNotReducedByDexOrInt()
        {
            var stats = TestStats.Create(dex: 265f, @int: 265f);

            Assert.AreEqual(1.5f, new SkillInstance().CalculateCastTimeModified(stats, 0f, 1.5f), Tolerance);
        }

        [Test]
        public void VariableCastTime_UsesSquareRootStatCurve()
        {
            var stats = TestStats.Create(dex: 66.25f, @int: 0f);
            float expected = 4f * (1f - UnityEngine.Mathf.Sqrt(132.5f / 530f));

            Assert.AreEqual(expected, new SkillInstance().CalculateCastTimeModified(stats, 4f, 0f), Tolerance);
        }

        [Test]
        public void Cooldown_AppliesFlatThenPercentAndClampsAtZero()
        {
            var stats = TestStats.Create();
            TestStats.SetBase(stats, AttributeType.CooldownReduc, 1f);
            TestStats.SetBase(stats, AttributeType.CooldownReducPercent, 50f);
            var skill = new SkillInstance();

            Assert.AreEqual(4.5f, skill.CalculateCooldownModified(stats, 10f), Tolerance);
            Assert.AreEqual(0f, skill.CalculateCooldownModified(stats, 0.5f), Tolerance);
        }

        [Test]
        public void AreaOfEffect_ScalesByPercentBonus()
        {
            var stats = TestStats.Create();
            TestStats.SetBase(stats, AttributeType.AOE, 50f);

            Assert.AreEqual(6f, new SkillInstance().CalculateAOERadiusModified(stats, 4f), Tolerance);
        }
        #endregion
    }
}
