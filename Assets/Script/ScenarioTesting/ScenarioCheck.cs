#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ScenarioTesting
{
    [Serializable]
    public sealed class ScenarioCheck
    {
        #region [Variables/Fields]
        public string Name;
        [JsonConverter(typeof(StringEnumConverter))] public ScenarioCheckStatus Status;
        public string Expected;
        public string Actual;
        public string Note;
        public string Screenshot;
        public int Step = -1;
        public float Time;
        #endregion

        #region [Properties]
        [JsonIgnore] public bool Passed => Status == ScenarioCheckStatus.Passed;
        #endregion

        #region [Public Methods]
        public override string ToString()
        {
            string label = Status == ScenarioCheckStatus.Passed ? "PASS" : Status == ScenarioCheckStatus.Review ? "REVIEW" : "FAIL";
            string note = string.IsNullOrEmpty(Note) ? string.Empty : $" ({Note})";
            return $"{label} {Name}: expected {Expected}, actual {Actual}{note}";
        }
        #endregion
    }
}
#endif
