using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Editor.Localization;
using CoreSystem.Localization;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class LocalizationTests
    {
        #region [Public Methods]
        [Test]
        public void ProjectText_HasEveryLanguage()
        {
            List<string> issues = LocalizationCoverageFormula.AuditProject();
            Assert.IsEmpty(issues, "Player-facing text needs EN and TH (AGENTS.md section 9):\n" + string.Join("\n", issues));
        }

        [TestCase("TH", "สวัสดี")]
        [TestCase("th", "สวัสดี")]
        [TestCase("EN", "Hello")]
        [TestCase("JP", "Hello")]
        [TestCase("", "Hello")]
        public void Resolve_PicksTheLanguageOrEnglish(string language, string expected)
        {
            var text = new LocalizedString { EN = "Hello", TH = "สวัสดี" };
            Assert.AreEqual(expected, LocalizationFormula.Resolve(text, language));
        }

        [Test]
        public void Resolve_FallsBackToEnglishWhenThaiIsEmpty()
        {
            Assert.AreEqual("Hello", LocalizationFormula.Resolve(new LocalizedString { EN = "Hello", TH = "" }, "TH"));
        }

        [Test]
        public void Resolve_ReturnsNullWhenNothingIsWritten()
        {
            Assert.IsNull(LocalizationFormula.Resolve(new LocalizedString(), "TH"));
            Assert.IsNull(LocalizationFormula.Resolve(null, "EN"));
        }

        [Test]
        public void MissingLanguages_ListsEmptyOnes()
        {
            CollectionAssert.AreEqual(new[] { "TH" }, LocalizationFormula.MissingLanguages(new LocalizedString { EN = "Hello", TH = " " }));
            Assert.IsTrue(new LocalizedString { EN = "a", TH = "b" }.IsComplete);
        }

        [Test]
        public void Format_FillsValuesAndSurvivesBadTemplates()
        {
            Assert.AreEqual("Lv 5", LocalizationFormula.Format("Lv {0}", new object[] { 5 }));
            Assert.AreEqual("Lv {1}", LocalizationFormula.Format("Lv {1}", new object[] { 5 }));
            Assert.AreEqual("plain", LocalizationFormula.Format("plain", null));
        }

        [Test]
        public void AuditTable_FindsDuplicatesEmptyKeysAndMissingText()
        {
            var table = new LocalizationTable();
            table.Entries.Add(new LocalizationEntry { Key = "a", Text = new LocalizedString { EN = "A", TH = "เอ" } });
            table.Entries.Add(new LocalizationEntry { Key = "a", Text = new LocalizedString { EN = "A", TH = "เอ" } });
            table.Entries.Add(new LocalizationEntry { Key = "", Text = new LocalizedString { EN = "B", TH = "บี" } });
            table.Entries.Add(new LocalizationEntry { Key = "c", Text = new LocalizedString { EN = "C" } });
            List<string> issues = LocalizationCoverageFormula.AuditTable(table, "test");
            Assert.AreEqual(3, issues.Count, string.Join("\n", issues));
        }

        [Test]
        public void AuditData_FlagsHalfTranslatedTextInNestedLists()
        {
            var data = new Holder { Rows = new List<Row> { new Row { Id = 7, Text = new LocalizedString { EN = "Only English" } }, new Row { Id = 8, Text = new LocalizedString() } } };
            List<string> issues = LocalizationCoverageFormula.AuditData(data, "holder");
            Assert.AreEqual(1, issues.Count, string.Join("\n", issues));
            StringAssert.Contains("(7)", issues[0]);
        }

        [Test]
        public void Parse_ReadsTheTableFormat()
        {
            LocalizationTable table = LocalizationDatabase.Parse("{\"Entries\":[{\"Key\":\"common.ok\",\"Text\":{\"EN\":\"OK\",\"TH\":\"ตกลง\"}}]}");
            Assert.AreEqual(1, table.Entries.Count);
            Assert.AreEqual("ตกลง", table.Entries[0].Text.TH);
        }
        #endregion

        #region [Private Methods]
        [System.Serializable]
        private class Holder
        {
            public List<Row> Rows;
        }

        [System.Serializable]
        private class Row
        {
            public int Id;
            public LocalizedString Text;
        }
        #endregion
    }
}
