using System.Text;
using CoreSystem.Editor.UI;
using CoreSystem.UI.Diagnostics;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class UILayoutRegressionTests
    {
        #region [Public Methods]
        [Test]
        public void BuilderScreens_HaveNoLayoutErrors()
        {
            var results = UISnapshotRenderer.InspectAll();
            var failures = new StringBuilder();

            foreach (var screen in results)
            {
                foreach (var issue in screen.Value)
                {
                    if (issue.Severity == UIIssueSeverity.Error) failures.AppendLine($"{screen.Key}: {issue}");
                }
            }

            Assert.GreaterOrEqual(results.Count, 12, "Expected the gallery, every dev tool page, the expanded database and the death screen.");
            Assert.IsEmpty(failures.ToString(), failures.ToString());
        }
        #endregion
    }
}
