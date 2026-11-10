using CoreSystem.Combat;
using CoreSystem.Entity.UniquePassives;
using NUnit.Framework;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class MarkedThroatCriticalTests
    {
        #region [Public Methods]
        [TestCase(1f, 0f, 1f, 0f, true)]
        [TestCase(1f, 0f, -1f, 0f, false)]
        [TestCase(1f, 0f, 0f, 1f, false)]
        [TestCase(0f, 0f, 1f, 0f, false)]
        [TestCase(0f, -1f, 0.2f, -1f, true)]
        public void IsBehind_OnlyWhenTheTargetFacesAway(float fx, float fy, float dx, float dy, bool expected)
        {
            Assert.AreEqual(expected, MarkedThroatUniquePassive.IsBehind(new Vector2(fx, fy), new Vector2(dx, dy)));
        }

        [Test]
        public void ForcedRoll_CritsEvenWithZeroCritRate()
        {
            DamageInput input = Input(0f, 0f);
            DamageOffense offense = DamageFormula.Offense(input, DamageFormula.ForcedCriticalRoll);
            Assert.IsTrue(offense.IsCritical);
            Assert.AreEqual(100f * DamageFormula.BaseCriticalMultiplier, offense.RawDamage, 1e-3f);
        }

        [Test]
        public void ForcedRoll_CritsThroughCritResist()
        {
            DamageOffense offense = DamageFormula.Offense(Input(10f, 50f), DamageFormula.ForcedCriticalRoll);
            Assert.IsTrue(offense.IsCritical);
        }

        [Test]
        public void ForcedRoll_UsesTheNormalCritDamage()
        {
            DamageInput input = Input(0f, 0f);
            input.CritDamage = 0.5f;
            DamageOffense offense = DamageFormula.Offense(input, DamageFormula.ForcedCriticalRoll);
            Assert.AreEqual(100f * (DamageFormula.BaseCriticalMultiplier + 0.5f), offense.RawDamage, 1e-3f);
        }

        [Test]
        public void NormalRoll_ZeroCritRateNeverCrits()
        {
            Assert.IsFalse(DamageFormula.Offense(Input(0f, 0f), 0f).IsCritical);
        }

        [Test]
        public void MarkedThroat_DoesNotForceWithoutAnOwnHit()
        {
            Assert.IsFalse(new MarkedThroatUniquePassive().ForcesCritical(null, null));
        }
        #endregion

        #region [Private Methods]
        private static DamageInput Input(float critRate, float critResist)
        {
            return new DamageInput
            {
                BaseDamage = 100f,
                ElementAdvantage = 1f,
                HasCasterStats = true,
                CritRate = critRate,
                CritResist = critResist
            };
        }
        #endregion
    }
}
