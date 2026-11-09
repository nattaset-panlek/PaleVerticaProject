using CoreSystem.SkillTree;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class SkillTreeLoaderTests
    {
        #region [Variables/Fields]
        private const float Tolerance = 0.0001f;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [TestCase(-280f, 280f)]
        [TestCase(1680f, -1680f)]
        [TestCase(0f, 0f)]
        public void ToUnityY_FlipsEditorCoordinates(float editorY, float expected)
        {
            Assert.AreEqual(expected, SkillTreeLoader.ToUnityY(editorY), Tolerance);
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
