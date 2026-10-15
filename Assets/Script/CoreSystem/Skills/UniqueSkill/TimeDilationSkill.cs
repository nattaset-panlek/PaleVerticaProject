using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills.Visuals;
using CoreSystem.Controllers;

namespace CoreSystem.Skills.UniqueSkill
{
    [CreateAssetMenu(fileName = "TimeDilationSkill", menuName = "Skill System/Unique/TimeDilation Skill")]
    public class TimeDilationSkill : ActiveSkill
    {
        [Header("Time Dilation Settings")]
        [Tooltip("The total duration to track entities before rewinding them.")]
        public float RecordDuration = 10f;
        
        [Tooltip("How often to sample the entity's position and health.")]
        public float RecordInterval = 2f;

        [Tooltip("If true, the caster will also be affected by the rewind.")]
        public bool AffectCaster = false;

        [Tooltip("How fast the entities rewind back. Lerp duration in seconds.")]
        public float RewindLerpDuration = 0.5f;

        [Tooltip("If true, dead entities will be resurrected if they have health records from the rewind period.")]
        public bool CanResurrect = true;

        [Header("Ghost Trail")]
        [Tooltip("If true, spawns a ghost clone of the target's model at each record interval to show the path.")]
        public bool ShowGhostTrail = false;

        [Tooltip("Optional material override for ghost clones. Leave empty to use original material.")]
        public Material GhostMaterial;

        private static HashSet<IEntity> _activeAffectedEntities = new HashSet<IEntity>();

        public override void Execute(SkillPayload payload, SkillInstance instance, SkillCaster caster)
        {
            Debug.Log($"<color=orange>[TimeDilationSkill]</color> Execute started for caster: {caster.CharacterRef.name}");

            // Standard execution - BLOCKED to handle per-target visuals manually
            // base.Execute(payload, instance, caster); 

            // Aggregate valid targets
            if ((payload.Targets == null || payload.Targets.Count == 0) && this.BaseTargeting != null)
            {
                payload.Targets = this.BaseTargeting.GetTargets(payload);
                Debug.Log($"<color=orange>[TimeDilationSkill]</color> Forced BaseTargeting check: Found {payload.Targets.Count} targets.");
            }

            List<IEntity> targetsToTrack = new List<IEntity>();
            if (payload.Targets != null)
            {
                foreach (var target in payload.Targets)
                {
                    if (target is IEntity e && !IsEntityDead(e))
                    {
                        if (_activeAffectedEntities.Contains(e))
                        {
                            Debug.Log($"[TimeDilation] Skipping {e.transform.name} - already tracked.");
                            continue;
                        }
                        targetsToTrack.Add(e);
                    }
                }
            }

            if (AffectCaster && payload.Caster != null && !targetsToTrack.Contains(payload.Caster) && !IsEntityDead(payload.Caster))
            {
                if (!_activeAffectedEntities.Contains(payload.Caster))
                {
                    targetsToTrack.Add(payload.Caster);
                    Debug.Log($"<color=orange>[TimeDilationSkill]</color> Added Caster to tracking.");
                }
            }

            if (targetsToTrack.Count > 0)
            {
                Debug.Log($"<color=orange>[TimeDilationSkill]</color> Tracking {targetsToTrack.Count} entities. Spawning tracker...");
                GameObject trackerObj = new GameObject("TimeDilationTracker_" + Time.time);
                var tracker = trackerObj.AddComponent<TimeDilationTracker>();
                tracker.Initialize(targetsToTrack, RecordDuration, RecordInterval, RewindLerpDuration, AuraVfxPrefab, _activeAffectedEntities, ShowGhostTrail, GhostMaterial, CanResurrect);
            }
            else
            {
                Debug.LogWarning($"<color=red>[TimeDilationSkill]</color> No valid targets found to track!");
            }
        }

        public static bool IsEntityDestroyed(IEntity target)
        {
            return target == null || (target is Object obj && obj == null);
        }

        // Helper method to resolve lack of IsDead on IEntity
        public static bool IsEntityDead(IEntity target)
        {
            if (IsEntityDestroyed(target)) return true;
            if (target.transform == null || !target.transform.gameObject.activeInHierarchy) return true;
            if (target.attributeSystem != null)
            {
                var hpAttr = target.attributeSystem.GetAttribute(AttributeType.HP);
                if (hpAttr != null) return hpAttr.CurrentValue <= 0;
            }
            return false;
        }

        /// <summary>
        /// Find the "Model" child of a target, clone it, destroy the Animator,
        /// then apply GhostMaterial to all SpriteRenderers.
        /// </summary>
        public static GameObject CreateGhostClone(IEntity target, Material ghostMaterial)
        {
            if (target == null || (target is Object obj && obj == null) || target.transform == null) return null;

            // Find the "Model" child - this is the visual root with Animator
            Transform modelTransform = target.transform.Find("Model");
            if (modelTransform == null)
            {
                // Fallback: look one level deep
                for (int i = 0; i < target.transform.childCount; i++)
                {
                    var child = target.transform.GetChild(i);
                    if (child.GetComponent<Animator>() != null)
                    {
                        modelTransform = child;
                        break;
                    }
                }
            }

            if (modelTransform == null) return null;

            // Clone the entire Model at its current world position/rotation (snapshot this frame)
            GameObject ghost = Object.Instantiate(modelTransform.gameObject);
            ghost.name = $"Ghost_{target.transform.name}";
            ghost.transform.position = modelTransform.position;
            ghost.transform.rotation = modelTransform.rotation;
            ghost.transform.localScale = modelTransform.lossyScale;

            // Destroy Animator so it freezes in place
            var animator = ghost.GetComponent<Animator>();
            if (animator != null) Object.Destroy(animator);

            // Destroy entire GameObjects that contain non-visual components
            // (Camera can't be removed alone because URP's UniversalAdditionalCameraData requires it)
            foreach (var cam in ghost.GetComponentsInChildren<Camera>()) Object.Destroy(cam.gameObject);
            foreach (var al in ghost.GetComponentsInChildren<AudioListener>()) Object.Destroy(al.gameObject);
            foreach (var col in ghost.GetComponentsInChildren<Collider2D>()) Object.Destroy(col);
            foreach (var col in ghost.GetComponentsInChildren<Collider>()) Object.Destroy(col);
            foreach (var rb in ghost.GetComponentsInChildren<Rigidbody2D>()) Object.Destroy(rb);

            // Apply GhostMaterial to every SpriteRenderer in the clone
            if (ghostMaterial != null)
            {
                foreach (var sr in ghost.GetComponentsInChildren<SpriteRenderer>())
                {
                    sr.material = ghostMaterial;
                }
            }

            return ghost;
        }
    }
}
