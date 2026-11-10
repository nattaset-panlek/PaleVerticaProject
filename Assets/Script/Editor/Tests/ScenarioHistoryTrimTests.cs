using System.IO;
using System.Linq;
using NUnit.Framework;
using ScenarioTesting;

namespace CoreSystem.Tests
{
    public class ScenarioHistoryTrimTests
    {
        #region [Variables/Fields]
        private string _folder;
        #endregion

        #region [Public Methods]
        [SetUp]
        public void CreateFolder()
        {
            _folder = Path.Combine(Path.GetTempPath(), "pv_history_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_folder);
            foreach (string name in new[] { "20261110_100000_Failed", "20261110_110000_Passed", "20261110_120000_Failed" })
            {
                File.WriteAllText(Path.Combine(_folder, name + ".json"), "{}");
            }
        }

        [TearDown]
        public void DeleteFolder()
        {
            if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
        }

        [Test]
        public void TrimFolder_KeepsTheNewestRuns()
        {
            Assert.AreEqual(2, ScenarioLibrary.TrimFolder(_folder, 1));
            string[] left = Directory.GetFiles(_folder).Select(Path.GetFileNameWithoutExtension).ToArray();
            CollectionAssert.AreEqual(new[] { "20261110_120000_Failed" }, left);
        }

        [Test]
        public void TrimFolder_KeepZeroRemovesTheFolder()
        {
            Assert.AreEqual(3, ScenarioLibrary.TrimFolder(_folder, 0));
            Assert.IsFalse(Directory.Exists(_folder));
        }

        [Test]
        public void TrimFolder_KeepMoreThanExistsDeletesNothing()
        {
            Assert.AreEqual(0, ScenarioLibrary.TrimFolder(_folder, 5));
            Assert.AreEqual(3, Directory.GetFiles(_folder).Length);
        }

        [Test]
        public void TrimFolder_MissingFolderIsIgnored()
        {
            Assert.AreEqual(0, ScenarioLibrary.TrimFolder(Path.Combine(_folder, "missing"), 1));
        }
        #endregion
    }
}
