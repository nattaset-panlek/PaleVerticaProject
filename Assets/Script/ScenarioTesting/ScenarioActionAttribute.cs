#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System;

namespace ScenarioTesting
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ScenarioActionAttribute : Attribute
    {
        #region [Properties]
        public string Name { get; }
        public string Usage { get; }
        public string Category { get; }
        #endregion

        #region [Public Methods]
        public ScenarioActionAttribute(string name, string category, string usage)
        {
            Name = name;
            Category = category;
            Usage = usage;
        }
        #endregion
    }
}
#endif
