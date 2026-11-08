using CoreSystem.UI.Loading;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class LoadingFormulaTests
    {
        #region [Variables/Fields]
        private const float Tolerance = 0.0001f;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [TestCase(0f, 0f)]
        [TestCase(0.45f, 0.5f)]
        [TestCase(0.9f, 1f)]
        [TestCase(1f, 1f)]
        public void NormalizeSceneProgress_MapsUnityRange(float raw, float expected)
        {
            Assert.AreEqual(expected, LoadingFormula.NormalizeSceneProgress(raw), Tolerance);
        }

        [Test]
        public void StepToward_MovesBySpeedTimesDelta()
        {
            Assert.AreEqual(0.25f, LoadingFormula.StepToward(0f, 1f, 2.5f, 0.1f), Tolerance);
        }

        [Test]
        public void StepToward_DoesNotOvershoot()
        {
            Assert.AreEqual(0.5f, LoadingFormula.StepToward(0.45f, 0.5f, 10f, 1f), Tolerance);
        }

        [Test]
        public void StepToward_ClampsTarget()
        {
            Assert.AreEqual(1f, LoadingFormula.StepToward(0.9f, 5f, 10f, 1f), Tolerance);
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
