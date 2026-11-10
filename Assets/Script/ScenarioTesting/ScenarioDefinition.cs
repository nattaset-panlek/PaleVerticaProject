#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ScenarioTesting
{
    public sealed class ScenarioDefinition
    {
        #region [Variables/Fields]
        public const string DefaultAgent = "claude";
        public const string IncludeAction = "Include";
        #endregion

        #region [Properties]
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string File { get; private set; }
        public string Agent { get; private set; } = DefaultAgent;
        public bool ClearSaves { get; private set; }
        public bool ContinueOnFail { get; private set; }
        public int Seed { get; private set; }
        public List<string> Tags { get; } = new List<string>();
        public List<ScenarioStep> Steps { get; } = new List<ScenarioStep>();
        public string Error { get; private set; }
        public bool IsValid => string.IsNullOrEmpty(Error);
        #endregion

        #region [Public Methods]
        public static ScenarioDefinition Load(string path)
        {
            try
            {
                return Parse(System.IO.File.ReadAllText(path), path);
            }
            catch (System.Exception exception)
            {
                return new ScenarioDefinition { Name = Path.GetFileNameWithoutExtension(path), File = path, Error = exception.Message };
            }
        }

        public static ScenarioDefinition Parse(string json, string file = null)
        {
            var definition = new ScenarioDefinition { File = file };
            JObject root = JObject.Parse(json);

            definition.Name = (string)root["name"] ?? (file != null ? Path.GetFileNameWithoutExtension(file) : "scenario");
            definition.Description = (string)root["description"] ?? string.Empty;
            definition.Agent = (string)root["agent"] ?? DefaultAgent;
            definition.ClearSaves = (bool?)root["clearSaves"] ?? false;
            definition.ContinueOnFail = (bool?)root["continueOnFail"] ?? false;
            definition.Seed = (int?)root["seed"] ?? 1337;

            if (root["tags"] is JArray tags) foreach (JToken tag in tags) definition.Tags.Add(tag.ToString());

            var visiting = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            if (file != null) visiting.Add(Path.GetFullPath(file));
            var flat = new List<JObject>();
            definition.Error = Expand(root, file, visiting, flat);
            if (!definition.IsValid) return definition;

            for (int i = 0; i < flat.Count; i++) definition.Steps.Add(new ScenarioStep(i, flat[i]));
            return definition;
        }

        public static string ResolvePath(string relative, string fromFile)
        {
            if (Path.IsPathRooted(relative)) return relative;
            string fromScenarios = Path.Combine(ScenarioLibrary.ScenariosFolder, relative);
            if (System.IO.File.Exists(fromScenarios) || fromFile == null) return fromScenarios;
            return Path.Combine(Path.GetDirectoryName(fromFile) ?? string.Empty, relative);
        }
        #endregion

        #region [Private Methods]
        private static string Expand(JObject root, string file, HashSet<string> visiting, List<JObject> flat)
        {
            if (root["path"] is JArray path)
            {
                foreach (JToken entry in path)
                {
                    string error = Include(entry.ToString(), file, visiting, flat);
                    if (error != null) return error;
                }
            }

            if (!(root["steps"] is JArray steps)) return "missing \"steps\" array";
            for (int i = 0; i < steps.Count; i++)
            {
                if (!(steps[i] is JObject step)) return $"step {i + 1} is not an object";
                if (string.Equals((string)step["do"], IncludeAction, System.StringComparison.OrdinalIgnoreCase))
                {
                    string error = Include((string)step["file"], file, visiting, flat);
                    if (error != null) return error;
                    continue;
                }

                var copy = (JObject)step.DeepClone();
                if (file != null && visiting.Count > 1 && copy["from"] == null) copy["from"] = ScenarioLibrary.RelativePath(file);
                flat.Add(copy);
            }
            return null;
        }

        private static string Include(string relative, string fromFile, HashSet<string> visiting, List<JObject> flat)
        {
            if (string.IsNullOrEmpty(relative)) return "an Include or path entry has no file";
            string full = Path.GetFullPath(ResolvePath(relative, fromFile));
            if (!System.IO.File.Exists(full)) return $"included scenario {relative} does not exist";
            if (!visiting.Add(full)) return $"scenario path loops through {relative}";

            JObject included = JObject.Parse(System.IO.File.ReadAllText(full));
            string error = Expand(included, full, visiting, flat);
            visiting.Remove(full);
            return error != null ? $"{relative}: {error}" : null;
        }
        #endregion
    }
}
#endif
