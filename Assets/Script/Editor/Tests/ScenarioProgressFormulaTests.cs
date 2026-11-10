using NUnit.Framework;
using ScenarioTesting;

namespace CoreSystem.Tests
{
    public class ScenarioProgressFormulaTests
    {
        #region [Public Methods]
        [TestCase(-1, 10, false, 0f)]
        [TestCase(0, 10, false, 0f)]
        [TestCase(5, 10, false, 0.5f)]
        [TestCase(9, 10, false, 0.9f)]
        [TestCase(3, 10, true, 1f)]
        [TestCase(0, 0, false, 0f)]
        [TestCase(12, 10, false, 1f)]
        public void RunFraction_CountsFinishedSteps(int stepIndex, int stepCount, bool finished, float expected)
        {
            Assert.AreEqual(expected, ScenarioProgressFormula.RunFraction(stepIndex, stepCount, finished), 0.0001f);
        }

        [TestCase(0, 4, 0f, 0f)]
        [TestCase(1, 4, 0.5f, 0.375f)]
        [TestCase(4, 4, 0f, 1f)]
        [TestCase(0, 0, 0.5f, 0f)]
        [TestCase(6, 4, 1f, 1f)]
        public void QueueFraction_AddsTheCurrentRun(int finishedRuns, int totalRuns, float currentRun, float expected)
        {
            Assert.AreEqual(expected, ScenarioProgressFormula.QueueFraction(finishedRuns, totalRuns, currentRun), 0.0001f);
        }

        [TestCase(0f, 0)]
        [TestCase(0.999f, 99)]
        [TestCase(1f, 100)]
        [TestCase(1.5f, 100)]
        [TestCase(-0.2f, 0)]
        public void Percent_RoundsDown(float fraction, int expected)
        {
            Assert.AreEqual(expected, ScenarioProgressFormula.Percent(fraction));
        }
        #endregion
    }
}
