using CoreSystem.Entity.UniquePassives;
using CoreSystem.Visuals;
using NUnit.Framework;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class DirectionHintFormulaTests
    {
        #region [Public Methods]
        [Test]
        public void Pivot_IsTheFigmaCenterElement()
        {
            Assert.AreEqual(169.5f / 338f, DirectionHintFormula.Pivot.x, 1e-4f);
            Assert.AreEqual(1f - 183f / 344f, DirectionHintFormula.Pivot.y, 1e-4f);
        }

        [TestCase(1f, 0f, -90f)]
        [TestCase(0f, 1f, 0f)]
        [TestCase(-1f, 0f, 90f)]
        [TestCase(0f, -1f, -180f)]
        public void AngleDegrees_TurnsTheArrowToTheFacing(float x, float y, float expected)
        {
            Assert.AreEqual(expected, DirectionHintFormula.AngleDegrees(new Vector2(x, y)), 1e-3f);
        }

        [Test]
        public void AngleDegrees_KeepsUpWhenThereIsNoFacing()
        {
            Assert.AreEqual(0f, DirectionHintFormula.AngleDegrees(Vector2.zero));
        }

        [TestCase(1f, 0.5f, 1.6f, 0.6f, 1.6f)]
        [TestCase(0.2f, 0.1f, 1.6f, 0.6f, 0.6f)]
        [TestCase(-2f, 1f, 1f, 0f, 2f)]
        public void Diameter_FollowsTheLongerColliderSide(float w, float h, float multiplier, float minimum, float expected)
        {
            Assert.AreEqual(expected, DirectionHintFormula.Diameter(new Vector2(w, h), multiplier, minimum), 1e-4f);
        }

        [Test]
        public void Offset_AddsWorldAndSizeParts()
        {
            Vector2 offset = DirectionHintFormula.Offset(new Vector2(2f, 4f), new Vector2(0.1f, -0.2f), new Vector2(0f, -0.25f));
            Assert.AreEqual(0.1f, offset.x, 1e-4f);
            Assert.AreEqual(-1.2f, offset.y, 1e-4f);
        }

        [TestCase(true, false, false, true, true)]
        [TestCase(false, false, false, true, false)]
        [TestCase(true, true, false, true, false)]
        [TestCase(true, false, true, true, false)]
        [TestCase(true, false, false, false, false)]
        public void ShouldShow_OnlyOnLivingEnemiesWithAFacingPassive(bool uses, bool isPlayer, bool isDead, bool hostile, bool expected)
        {
            Assert.AreEqual(expected, DirectionHintFormula.ShouldShow(uses, isPlayer, isDead, hostile));
        }

        [Test]
        public void MarkedThroat_UsesFacing()
        {
            Assert.IsTrue(new MarkedThroatUniquePassive().UsesFacing);
            Assert.IsFalse(new VampireUniquePassive().UsesFacing);
        }
        #endregion
    }
}
