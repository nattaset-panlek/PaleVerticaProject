#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ScenarioTesting
{
    [Serializable]
    public sealed class ScenarioStepResult
    {
        #region [Variables/Fields]
        public int Index;
        public string Action;
        public string Arguments;
        [JsonConverter(typeof(StringEnumConverter))] public ScenarioStatus Status;
        public string Message;
        public float StartTime;
        public float Duration;
        #endregion
    }
}
#endif
