using UnityEngine;
using CoreSystem.Entity;

namespace CoreSystem.Skills.Logic.Effects
{
    /// <summary>
    /// Spawns an object at the TargetPosition (ground-targeted) or OriginPosition of the payload.
    /// Used for summoning walls, minions, or spawning persistent area objects.
    /// </summary>
    [System.Serializable]
    public class SummonObjectEffect : EffectLogic
    {
        public GameObject Prefab;
        public float Lifetime = 5f; // Duration of the object. 0 means it won't be auto-destroyed.
        public bool ParentToCaster = false;

        // Ensure only one object is spawned even if multiple enemies were targeted
        public override bool ApplyOncePerCast => true;

        public override void Apply(SkillPayload payload, IEntity target)
        {
            if (Prefab == null) return;

            // Preferred location is TargetPosition (ground click); fallback to Caster (Origin)
            Vector3 spawnPos = (payload.TargetPosition != Vector3.zero) ? payload.TargetPosition : payload.OriginPosition;

            GameObject instance = Object.Instantiate(Prefab, spawnPos, Quaternion.identity);
            
            if (ParentToCaster && payload.Caster != null)
            {
                instance.transform.SetParent(payload.Caster.transform);
            }

            if (Lifetime > 0)
            {
                Object.Destroy(instance, Lifetime);
            }
        }
    }
}
