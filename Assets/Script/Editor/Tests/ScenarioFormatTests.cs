using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using ScenarioTesting;

namespace CoreSystem.Tests
{
    public class ScenarioFormatTests
    {
        #region [Variables/Fields]
        private static readonly string[] ExpressionKeys = { "value", "to", "until", "min", "max", "amount", "seconds", "times", "total", "add", "ratio", "x", "y" };
        private static readonly Regex Name = new Regex(@"[A-Za-z_][A-Za-z0-9_.]*");
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [TestCase("1 + 2 * 3", 7f)]
        [TestCase("(1 + 2) * 3", 9f)]
        [TestCase("-4 / 2", -2f)]
        [TestCase("10 / 0", 0f)]
        [TestCase("hit.0 / var.base", 2f)]
        public void Expression_Evaluates(string expression, float expected)
        {
            var values = new Dictionary<string, float> { { "hit.0", 10f }, { "var.base", 5f } };
            Assert.IsTrue(ScenarioExpression.TryEvaluate(expression, name => values.TryGetValue(name, out float value) ? value : (float?)null, out float result, out string error), error);
            Assert.AreEqual(expected, result, 0.0001f);
        }

        [TestCase("1 +")]
        [TestCase("unknown.value")]
        [TestCase("(1 + 2")]
        [TestCase("")]
        public void Expression_ReportsErrors(string expression)
        {
            Assert.IsFalse(ScenarioExpression.TryEvaluate(expression, _ => null, out _, out string error));
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void Definition_ParsesSteps()
        {
            ScenarioDefinition definition = ScenarioDefinition.Parse("{\"name\":\"x\",\"seed\":7,\"steps\":[{\"do\":\"Wait\",\"seconds\":2}]}");
            Assert.IsTrue(definition.IsValid, definition.Error);
            Assert.AreEqual(7, definition.Seed);
            Assert.AreEqual("Wait", definition.Steps[0].Action);
            Assert.AreEqual(2f, definition.Steps[0].Float("seconds"));
        }

        [Test]
        public void Definition_PathAndIncludeRunOtherFilesFirst()
        {
            string folder = Path.Combine(Path.GetTempPath(), "PaleVerticaScenarioPath");
            Directory.CreateDirectory(folder);
            string setup = Path.Combine(folder, "setup.json");
            string middle = Path.Combine(folder, "middle.json");
            File.WriteAllText(setup, "{\"steps\":[{\"do\":\"Wait\",\"seconds\":1}]}");
            File.WriteAllText(middle, "{\"steps\":[{\"do\":\"Log\",\"text\":\"m\"}]}");

            string json = $"{{\"path\":[{JToken.FromObject(setup).ToString(Newtonsoft.Json.Formatting.None)}],\"steps\":[{{\"do\":\"Include\",\"file\":{JToken.FromObject(middle).ToString(Newtonsoft.Json.Formatting.None)}}},{{\"do\":\"Wait\",\"seconds\":3}}]}}";
            ScenarioDefinition definition = ScenarioDefinition.Parse(json, Path.Combine(folder, "main.json"));

            Assert.IsTrue(definition.IsValid, definition.Error);
            CollectionAssert.AreEqual(new[] { "Wait", "Log", "Wait" }, definition.Steps.Select(step => step.Action).ToArray());
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, definition.Steps.Select(step => step.Index).ToArray());
            StringAssert.Contains("setup.json", definition.Steps[0].Describe());
        }

        [Test]
        public void Definition_PathThatLoopsIsInvalid()
        {
            string folder = Path.Combine(Path.GetTempPath(), "PaleVerticaScenarioLoop");
            Directory.CreateDirectory(folder);
            string a = Path.Combine(folder, "a.json");
            string b = Path.Combine(folder, "b.json");
            File.WriteAllText(a, $"{{\"path\":[{JToken.FromObject(b).ToString(Newtonsoft.Json.Formatting.None)}],\"steps\":[]}}");
            File.WriteAllText(b, $"{{\"path\":[{JToken.FromObject(a).ToString(Newtonsoft.Json.Formatting.None)}],\"steps\":[]}}");

            ScenarioDefinition definition = ScenarioDefinition.Load(a);

            Assert.IsFalse(definition.IsValid);
            StringAssert.Contains("loops", definition.Error);
        }

        [Test]
        public void Definition_WithoutStepsIsInvalid()
        {
            Assert.IsFalse(ScenarioDefinition.Parse("{\"name\":\"x\"}").IsValid);
        }

        [Test]
        public void EveryScenarioFile_IsValidAndUsesKnownActions()
        {
            var problems = new List<string>();
            foreach (string file in ScenarioLibrary.AllFiles())
            {
                ScenarioDefinition definition = ScenarioDefinition.Load(file);
                string relative = ScenarioLibrary.RelativePath(file);
                if (!definition.IsValid)
                {
                    problems.Add($"{relative}: {definition.Error}");
                    continue;
                }
                foreach (ScenarioStep step in definition.Steps) CheckStep(relative, step, problems);
            }
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void ScenarioNames_AreUnique()
        {
            var names = ScenarioLibrary.AllFiles().Select(ScenarioDefinition.Load).GroupBy(definition => definition.Name).Where(group => group.Count() > 1).Select(group => group.Key).ToList();
            Assert.IsEmpty(names, "duplicate scenario names: " + string.Join(", ", names));
        }
        #endregion

        #region [Private Methods]
        private static void CheckStep(string file, ScenarioStep step, List<string> problems)
        {
            if (ScenarioAction.Create(step.Action) == null)
            {
                problems.Add($"{file} step {step.Index + 1}: unknown action '{step.Action}'");
                return;
            }

            foreach (string key in ExpressionKeys)
            {
                if (!step.Has(key) || step.IsNumber(key) || step.Token(key).Type != JTokenType.String) continue;
                string expression = step.String(key);
                if (!ScenarioExpression.TryEvaluate(expression, name => 1f, out _, out string error)) problems.Add($"{file} step {step.Index + 1}: {key} \"{expression}\": {error}");
            }

            if (step.Token("steps") is JArray children)
            {
                foreach (JToken child in children) if (child is JObject raw) CheckStep(file, new ScenarioStep(step.Index, raw), problems);
            }
        }
        #endregion
    }
}
