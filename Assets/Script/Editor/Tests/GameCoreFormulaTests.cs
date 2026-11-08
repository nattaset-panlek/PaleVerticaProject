using System.Collections.Generic;
using CoreSystem.World;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class GameCoreFormulaTests
    {
        #region [Variables/Fields]
        private static readonly List<string> Excluded = new List<string> { "Title" };
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [TestCase("Town", true)]
        [TestCase("Title", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void SceneUsesCore_FollowsExclusionList(string scene, bool expected)
        {
            Assert.AreEqual(expected, GameCoreFormula.SceneUsesCore(scene, Excluded));
        }

        [Test]
        public void SceneUsesCore_NullListMeansEveryScene()
        {
            Assert.IsTrue(GameCoreFormula.SceneUsesCore("Town", null));
        }

        [TestCase("Field", "Town", "Field")]
        [TestCase("", "Town", "Town")]
        [TestCase(null, "Town", "Town")]
        public void ContinueScene_UsesSavedMapOrFallback(string saved, string fallback, string expected)
        {
            Assert.AreEqual(expected, GameCoreFormula.ContinueScene(saved, fallback));
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
