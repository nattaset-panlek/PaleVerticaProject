#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Entity;

namespace ScenarioTesting
{
    public static class AttributeProbe
    {
        #region [Public Methods]
        public static Dictionary<AttributeType, float> Snapshot(IEntity entity)
        {
            var values = new Dictionary<AttributeType, float>();
            if (entity == null || entity.attributeSystem == null) return values;
            foreach (var pair in entity.attributeSystem.ActiveAttributes) values[pair.Key] = pair.Value.CurrentValue;
            return values;
        }

        public static float Read(IEntity entity, AttributeType type)
        {
            var attribute = entity != null && entity.attributeSystem != null ? entity.attributeSystem.GetAttribute(type) : null;
            return attribute != null ? attribute.CurrentValue : 0f;
        }

        public static Dictionary<AttributeType, float> Diff(Dictionary<AttributeType, float> before, Dictionary<AttributeType, float> after)
        {
            var changes = new Dictionary<AttributeType, float>();
            foreach (var pair in after)
            {
                before.TryGetValue(pair.Key, out float old);
                float delta = pair.Value - old;
                if (System.Math.Abs(delta) > 0.0001f) changes[pair.Key] = delta;
            }
            return changes;
        }
        #endregion
    }
}
#endif
