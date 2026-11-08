using CoreSystem.Entity;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class CharacterPrefabLinkFormulaTests
    {
        #region [Variables/Fields]
        private static readonly int[] SharedIds = { 1001, 1002, 1003 };
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [TestCase(1001, true)]
        [TestCase(1003, true)]
        [TestCase(2004, false)]
        [TestCase(0, false)]
        public void IsLinked_AcceptsAnyEntryUsingThePrefab(int prefabId, bool expected)
        {
            Assert.AreEqual(expected, CharacterPrefabLinkFormula.IsLinked(prefabId, SharedIds));
        }

        [Test]
        public void IsLinked_NullListIsNotLinked()
        {
            Assert.IsFalse(CharacterPrefabLinkFormula.IsLinked(1001, null));
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
