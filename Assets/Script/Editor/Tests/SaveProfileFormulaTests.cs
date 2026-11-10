using System.IO;
using CoreSystem.Data.Save;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class SaveProfileFormulaTests
    {
        #region [Variables/Fields]
        private const string Root = "Saves";
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [TestCase(null, "")]
        [TestCase("   ", "")]
        [TestCase("claude", "claude")]
        [TestCase("agent/../x", "agent____x")]
        [TestCase("Quest Bot 2", "Quest_Bot_2")]
        public void Sanitize_KeepsSafeCharactersOnly(string profile, string expected)
        {
            Assert.AreEqual(expected, SaveProfileFormula.Sanitize(profile));
        }

        [Test]
        public void FolderFor_DefaultProfileIsTheRoot()
        {
            Assert.AreEqual(Root, SaveProfileFormula.FolderFor(Root, ""));
        }

        [Test]
        public void FolderFor_NamedProfileIsUnderProfiles()
        {
            Assert.AreEqual(Path.Combine(Root, SaveProfileFormula.ProfilesFolderName, "claude"), SaveProfileFormula.FolderFor(Root, "claude"));
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
