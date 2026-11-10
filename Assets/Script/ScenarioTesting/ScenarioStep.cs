#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ScenarioTesting
{
    public sealed class ScenarioStep
    {
        #region [Variables/Fields]
        public const float DefaultTimeout = 120f;
        private const string ActionKey = "do";

        private readonly JObject _args;
        #endregion

        #region [Properties]
        public int Index { get; }
        public string Action { get; }
        public string Note => String("note");
        public float Timeout => Float("timeout", DefaultTimeout);
        public bool ContinueOnFail => Bool("continueOnFail");
        public JObject Raw => _args;
        #endregion

        #region [Public Methods]
        public ScenarioStep(int index, JObject args)
        {
            Index = index;
            _args = args ?? new JObject();
            Action = String(ActionKey, string.Empty);
        }

        public bool Has(string key)
        {
            return _args.TryGetValue(key, System.StringComparison.OrdinalIgnoreCase, out JToken token) && token.Type != JTokenType.Null;
        }

        public JToken Token(string key)
        {
            return _args.TryGetValue(key, System.StringComparison.OrdinalIgnoreCase, out JToken token) ? token : null;
        }

        public string String(string key, string fallback = null)
        {
            JToken token = Token(key);
            return token == null || token.Type == JTokenType.Null ? fallback : token.ToString();
        }

        public int Int(string key, int fallback = 0)
        {
            JToken token = Token(key);
            return token != null && (token.Type == JTokenType.Integer || token.Type == JTokenType.Float) ? token.Value<int>() : fallback;
        }

        public float Float(string key, float fallback = 0f)
        {
            JToken token = Token(key);
            return token != null && (token.Type == JTokenType.Integer || token.Type == JTokenType.Float) ? token.Value<float>() : fallback;
        }

        public bool Bool(string key, bool fallback = false)
        {
            JToken token = Token(key);
            return token != null && token.Type == JTokenType.Boolean ? token.Value<bool>() : fallback;
        }

        public bool IsNumber(string key)
        {
            JToken token = Token(key);
            return token != null && (token.Type == JTokenType.Integer || token.Type == JTokenType.Float);
        }

        public List<string> Strings(string key)
        {
            var values = new List<string>();
            JToken token = Token(key);
            if (token == null) return values;
            if (token is JArray array) foreach (JToken item in array) values.Add(item.ToString());
            else values.Add(token.ToString());
            return values;
        }

        public List<int> Ints(string key)
        {
            var values = new List<int>();
            foreach (string value in Strings(key)) if (int.TryParse(value, out int number)) values.Add(number);
            return values;
        }

        public string Describe()
        {
            var copy = (JObject)_args.DeepClone();
            copy.Remove(ActionKey);
            copy.Remove("note");
            string from = (string)copy["from"];
            copy.Remove("from");
            string text = copy.Count == 0 ? Action : $"{Action} {copy.ToString(Formatting.None)}";
            return from == null ? text : $"[{from}] {text}";
        }
        #endregion
    }
}
#endif
