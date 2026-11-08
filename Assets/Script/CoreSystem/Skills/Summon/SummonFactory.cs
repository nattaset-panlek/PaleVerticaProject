using CoreSystem.Agent;
using CoreSystem.Enums;
using CoreSystem.Entity;
using CoreSystem.Utility;
using UnityEngine;

namespace CoreSystem.Skills.Summon
{
    public static class SummonFactory
    {
        #region [Variables/Fields]
        public const string NamePrefix = "Summon ";
        #endregion

        #region [Public Methods]
        public static SummonedUnit Create(Character owner, GameObject prefab, Vector3 position, int skillId, float lifetime, float leashRadius)
        {
            if (owner == null || prefab == null) return null;

            GameObject instance = Object.Instantiate(prefab, position, Quaternion.identity, RuntimeRoot.Get());
            instance.name = "[Runtime] " + NamePrefix + prefab.name;

            Character body = instance.GetComponent<Character>();
            if (body == null)
            {
                Debug.LogWarning($"[Summon] {prefab.name} has no Character component; it cannot fight.");
                Object.Destroy(instance);
                return null;
            }

            body.faction = owner.faction;
            ConfigureProfile(body.AgentProfile, owner, leashRadius);

            SummonedUnit unit = instance.AddComponent<SummonedUnit>();
            unit.Bind(owner, body, skillId, lifetime);
            return unit;
        }

        public static Vector3 SpawnPoint(Vector3 casterPosition, Vector3 aimDirection, float distance, int index)
        {
            Vector3 forward = aimDirection.sqrMagnitude > 0.0001f ? aimDirection.normalized : Vector3.up;
            float angle = index * 50f * (index % 2 == 0 ? 1f : -1f);
            return casterPosition + Quaternion.Euler(0f, 0f, angle) * forward * distance;
        }
        #endregion

        #region [Private Methods]
        private static void ConfigureProfile(AgentProfile profile, Character owner, float leashRadius)
        {
            if (profile == null) return;
            profile.Behavior = AIBehaviorType.Aggressive;
            profile.GroupID = 0;
            profile.UseHomeZone = true;
            profile.HomeZoneRadius = Mathf.Max(1f, leashRadius);
            profile.RoamRadius = Mathf.Max(1f, leashRadius * 0.5f);
            profile.CanRoam = false;
            profile.HomeAnchor = owner.transform;
        }
        #endregion
    }
}
