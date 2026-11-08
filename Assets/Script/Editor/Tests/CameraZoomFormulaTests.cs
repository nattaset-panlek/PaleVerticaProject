using CoreSystem.Controllers;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class CameraZoomFormulaTests
    {
        #region [Variables/Fields]
        private const float Tolerance = 0.0001f;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [TestCase(1f, 0.1f)]
        [TestCase(120f, 0.1f)]
        [TestCase(-1f, -0.1f)]
        [TestCase(-120f, -0.1f)]
        public void ScrollOverWorld_ReturnsSignedStep(float rawScroll, float expected)
        {
            Assert.AreEqual(expected, CameraZoomFormula.ScrollStep(rawScroll, false), Tolerance);
        }

        [TestCase(1f)]
        [TestCase(-1f)]
        [TestCase(120f)]
        public void ScrollOverUI_ReturnsNoStep(float rawScroll)
        {
            Assert.AreEqual(0f, CameraZoomFormula.ScrollStep(rawScroll, true));
        }

        [TestCase(0f)]
        [TestCase(0.005f)]
        [TestCase(-0.005f)]
        public void ScrollInsideDeadZone_ReturnsNoStep(float rawScroll)
        {
            Assert.IsFalse(CameraZoomFormula.HasScroll(rawScroll));
            Assert.AreEqual(0f, CameraZoomFormula.ScrollStep(rawScroll, false));
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
