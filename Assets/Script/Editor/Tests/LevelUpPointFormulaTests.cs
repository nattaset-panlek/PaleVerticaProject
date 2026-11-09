using CoreSystem.Combat;
using NUnit.Framework;

namespace CoreSystem.Editor.Tests
{
    public class LevelUpPointFormulaTests
    {
        #region [Public Methods]
        [TestCase(1, 5)]
        [TestCase(9, 5)]
        [TestCase(10, 6)]
        [TestCase(25, 7)]
        public void AttributePoints_GrowEveryTenLevels(int level, int expected)
        {
            Assert.AreEqual(expected, ProgressionFormula.AttributePointsForLevel(level));
        }

        [TestCase(1, 1)]
        [TestCase(9, 1)]
        [TestCase(10, 2)]
        [TestCase(20, 2)]
        public void PassivePoints_BonusOnEveryTenthJobLevel(int level, int expected)
        {
            Assert.AreEqual(expected, ProgressionFormula.PassivePointsForLevel(level));
        }

        [Test]
        public void Points_NoneAtLevelZero()
        {
            Assert.AreEqual(0, ProgressionFormula.AttributePointsForLevel(0));
            Assert.AreEqual(0, ProgressionFormula.PassivePointsForLevel(0));
        }
        #endregion
    }
}
