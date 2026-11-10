#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CoreSystem.Utility;

namespace ScenarioTesting
{
    public abstract class ScenarioAction
    {
        #region [Variables/Fields]
        private static readonly AttributedTypeRegistry<string, ScenarioAction, ScenarioActionAttribute> Registry =
            new AttributedTypeRegistry<string, ScenarioAction, ScenarioActionAttribute>(attribute => attribute.Name.ToLowerInvariant());
        #endregion

        #region [Properties]
        public static IReadOnlyDictionary<string, Type> RegisteredTypes => Registry.Types;
        #endregion

        #region [Public Methods]
        public abstract IEnumerator Run(ScenarioRun run, ScenarioStep step);

        public static ScenarioAction Create(string name)
        {
            return string.IsNullOrEmpty(name) ? null : Registry.Create(name.ToLowerInvariant());
        }

        public static List<ScenarioActionAttribute> Catalog()
        {
            var list = new List<ScenarioActionAttribute>();
            foreach (Type type in Registry.Types.Values) list.Add(type.GetCustomAttribute<ScenarioActionAttribute>());
            list.Sort((a, b) => string.Compare(a.Category + a.Name, b.Category + b.Name, StringComparison.Ordinal));
            return list;
        }
        #endregion
    }
}
#endif
