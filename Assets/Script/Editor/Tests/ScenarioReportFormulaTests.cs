using System.Collections.Generic;
using NUnit.Framework;
using ScenarioTesting;

namespace CoreSystem.Tests
{
    public class ScenarioReportFormulaTests
    {
        #region [Public Methods]
        [Test]
        public void Detailed_ListsOnlyWhatDidNotPass()
        {
            string text = ScenarioReportFormula.Detailed(FailedReport());
            StringAssert.Contains("[Failed] cast_test (core/cast.json)", text);
            StringAssert.Contains("Failed step 9: the cast started | expected == 1 | actual 0", text);
            StringAssert.Contains("Step 8 Cast {} failed: no hits", text);
            StringAssert.Contains("NullReferenceException", text);
            StringAssert.DoesNotContain("target lost hp", text);
        }

        [Test]
        public void Detailed_KeepsTheFirstLineAndTheLogEnd()
        {
            ScenarioReport report = FailedReport();
            for (int i = 0; i < 10; i++) report.Log.Add($"line {i}");
            string text = ScenarioReportFormula.Detailed(report, 3);
            StringAssert.Contains("begin, speed x1", text);
            StringAssert.Contains("line 9", text);
            StringAssert.DoesNotContain("line 5", text);
        }

        [Test]
        public void Brief_SkipsPassedReports()
        {
            var passed = new ScenarioReport { Scenario = "fine", Status = ScenarioStatus.Passed };
            string text = ScenarioReportFormula.Brief(new List<ScenarioReport> { passed, FailedReport(), null });
            StringAssert.Contains("cast_test", text);
            StringAssert.DoesNotContain("fine", text);
        }

        [Test]
        public void Brief_SaysWhenEverythingPassed()
        {
            var passed = new ScenarioReport { Scenario = "fine", Status = ScenarioStatus.Passed };
            Assert.AreEqual("Every scenario passed.", ScenarioReportFormula.Brief(new List<ScenarioReport> { passed }));
        }
        #endregion

        #region [Private Methods]
        private static ScenarioReport FailedReport()
        {
            var report = new ScenarioReport { Scenario = "cast_test", File = "core/cast.json", Status = ScenarioStatus.Failed, ChecksPassed = 1, ChecksFailed = 1 };
            report.Checks.Add(new ScenarioCheck { Name = "target lost hp", Status = ScenarioCheckStatus.Passed, Step = 9 });
            report.Checks.Add(new ScenarioCheck { Name = "the cast started", Status = ScenarioCheckStatus.Failed, Expected = "== 1", Actual = "0", Step = 8 });
            report.StepResults.Add(new ScenarioStepResult { Index = 7, Action = "Cast", Arguments = "Cast {}", Status = ScenarioStatus.Failed, Message = "no hits" });
            report.Errors.Add("Exception: NullReferenceException: Object reference not set");
            report.Log.Add("0.00s begin, speed x1");
            return report;
        }
        #endregion
    }
}
