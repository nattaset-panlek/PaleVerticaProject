#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System;

namespace ScenarioTesting
{
    [Serializable]
    public sealed class CombatRecordEntry
    {
        #region [Variables/Fields]
        public string Kind;
        public string Source;
        public string Target;
        public float Amount;
        public float Time;
        #endregion
    }
}
#endif
