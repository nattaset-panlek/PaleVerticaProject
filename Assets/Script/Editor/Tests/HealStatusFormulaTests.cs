using CoreSystem.Combat;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class HealStatusFormulaTests
    {
        #region [Variables/Fields]
        private const float Tolerance = 0.0001f;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [Test]
        public void Amount_AddsScaledMatk()
        {
            Assert.AreEqual(150f, HealFormula.Amount(100f, 100f, HealFormula.DefaultMatkScale), Tolerance);
        }

        [Test]
        public void Amount_ZeroScaleIsBaseOnly()
        {
            Assert.AreEqual(50f, HealFormula.Amount(50f, 999f, 0f), Tolerance);
        }

        [Test]
        public void Amount_NeverNegative()
        {
            Assert.AreEqual(0f, HealFormula.Amount(-10f, 0f, 0.5f), Tolerance);
        }

        [Test]
        public void Clamp_StopsAtMax()
        {
            Assert.AreEqual(100f, HealFormula.Clamp(90f, 50f, 100f), Tolerance);
        }

        [Test]
        public void ExtendedDuration_AddsToRemaining()
        {
            Assert.AreEqual(5f, StatusFormula.ExtendedDuration(2f, 3f), Tolerance);
        }

        [Test]
        public void ExtendedDuration_IgnoresNegativeRemaining()
        {
            Assert.AreEqual(3f, StatusFormula.ExtendedDuration(-1f, 3f), Tolerance);
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
