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
                tracker.Initialize(targetsToTrack, RecordDuration, RecordInterval, RewindLerpDuration, VisualPrefab, _activeAffectedEntities, ShowGhostTrail, GhostMaterial, CanResurrect);
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

    /// <summary>
    /// Background tracker that runs over the specified duration and applies the rewind effect.
    /// </summary>
    public class TimeDilationTracker : MonoBehaviour
    {
        private class EntityRecord
        {
            public List<Vector3> RecordedPositions = new List<Vector3>();
            // Ghost clones matching each recorded position (index 0 = first, last = most recent)
            public List<GameObject> GhostClones = new List<GameObject>();
            public float MaxHealthObserved;
        }

        private List<IEntity> _targets;
        private Dictionary<IEntity, EntityRecord> _records = new Dictionary<IEntity, EntityRecord>();
        private Dictionary<IEntity, GameObject> _targetVisuals = new Dictionary<IEntity, GameObject>();
        private HashSet<IEntity> _globalRegistry;
        
        private float _duration;
        private float _interval;
        private float _rewindLerpDuration;
        private bool _showGhostTrail;
        private Material _ghostMaterial;
        private bool _canResurrect;

        public void Initialize(List<IEntity> targets, float duration, float interval, float lerpDuration,
            GameObject visualPrefab, HashSet<IEntity> registry, bool showGhostTrail, Material ghostMaterial, bool canResurrect)
        {
            _targets = targets;
            _duration = duration;
            _interval = interval > 0.1f ? interval : 0.1f; 
            _rewindLerpDuration = lerpDuration > 0 ? lerpDuration : 0.1f;
            _globalRegistry = registry;
            _showGhostTrail = showGhostTrail;
            _ghostMaterial = ghostMaterial;
            _canResurrect = canResurrect;

            // Record initial state
            foreach (var target in _targets)
            {
                if (!TimeDilationSkill.IsEntityDead(target))
                {
                    _globalRegistry.Add(target);

                    // Spawn Visual Indicator attached to target
                    if (visualPrefab != null)
                    {
                        GameObject vfx = Instantiate(visualPrefab, target.transform);
                        vfx.transform.localPosition = Vector3.zero;
                        _targetVisuals[target] = vfx;
                    }

                    float currentHp = 0f;
                    if (target.attributeSystem != null)
                    {
                        var hpAttr = target.attributeSystem.GetAttribute(AttributeType.HP);
                        if (hpAttr != null) currentHp = hpAttr.CurrentValue;
                    }

                    var record = new EntityRecord { MaxHealthObserved = currentHp };
                    record.RecordedPositions.Add(target.transform.position);

                    // Spawn first ghost at starting position
                    if (_showGhostTrail)
                    {
                        var ghost = TimeDilationSkill.CreateGhostClone(target, _ghostMaterial);
                        record.GhostClones.Add(ghost); // index 0 matches position index 0
                    }
                    else
                    {
                        record.GhostClones.Add(null); // keep indices aligned
                    }

                    _records[target] = record;
                }
            }

            StartCoroutine(TrackingRoutine());
        }

        private IEnumerator TrackingRoutine()
        {
            float elapsed = 0f;

            while (elapsed < _duration)
            {
                yield return new WaitForSeconds(_interval);
                elapsed += _interval;

                foreach (var target in _targets)
                {
                    if (!TimeDilationSkill.IsEntityDead(target) && target.attributeSystem != null && _records.ContainsKey(target))
                    {
                        var record = _records[target];
                        record.RecordedPositions.Add(target.transform.position);

                        // Spawn ghost at this snapshot position
                        if (_showGhostTrail)
                        {
                            var ghost = TimeDilationSkill.CreateGhostClone(target, _ghostMaterial);
                            record.GhostClones.Add(ghost); // index matches RecordedPositions
                        }
                        else
                        {
                            record.GhostClones.Add(null);
                        }

                        float currentHp = 0f;
                        var hpAttr = target.attributeSystem.GetAttribute(AttributeType.HP);
                        if (hpAttr != null) currentHp = hpAttr.CurrentValue;

                        if (currentHp > record.MaxHealthObserved)
                        {
                            record.MaxHealthObserved = currentHp;
                        }
                    }
                }
            }

            StartCoroutine(RewindRoutine());
        }

        private IEnumerator RewindRoutine()
        {
            int maxPathLength = 0;

            // 1. Time-Restoration Healing & Setup
            foreach (var kvp in _records)
            {
                IEntity target = kvp.Key;
                EntityRecord record = kvp.Value;

                if (record.RecordedPositions.Count > maxPathLength) maxPathLength = record.RecordedPositions.Count;

                if (!TimeDilationSkill.IsEntityDestroyed(target) && target.transform != null)
                {
                    if (target.attributeSystem != null)
                    {
                        float currentHp = 0f;
                        var hpAttr = target.attributeSystem.GetAttribute(AttributeType.HP);
                        if (hpAttr != null) currentHp = hpAttr.CurrentValue;

                        float lostHp = record.MaxHealthObserved - currentHp;

                        if (lostHp > 0 && hpAttr != null)
                        {
                            var maxHpAttr = target.attributeSystem.GetAttribute(AttributeType.MaxHP);
                            float newValue = currentHp + lostHp;
                            if (maxHpAttr != null) newValue = Mathf.Min(newValue, maxHpAttr.CurrentValue);

                            hpAttr.BaseValue = newValue;
                            hpAttr.Recalculate();

                            // Resurrection logic: if they were dead, bring them back!
                            if (_canResurrect && currentHp <= 0 && newValue > 0)
                            {
                                var agent = (target as MonoBehaviour)?.GetComponent<AgentCore>();
                                if (agent != null) agent.Revive(newValue);
                            }
                        }
                    }
                }
            }

            // 2. Sequential Position Rewind (most recent -> oldest): index N-1 -> 0
            // As target passes each point, destroy the ghost at that index
            if (maxPathLength > 1)
            {
                float segmentDuration = _rewindLerpDuration / (maxPathLength - 1);

                // i goes from most-recent (N-1) back to 1
                for (int i = maxPathLength - 1; i > 0; i--)
                {
                    // Destroy the ghost that corresponds to position i (most recent first)
                    foreach (var kvp in _records)
                    {
                        var ghosts = kvp.Value.GhostClones;
                        if (i < ghosts.Count && ghosts[i] != null)
                        {
                            Destroy(ghosts[i]);
                            ghosts[i] = null;
                        }
                    }

                    float t = 0f;
                    while (t < segmentDuration)
                    {
                        t += Time.deltaTime;
                        float normalizedTime = Mathf.Clamp01(t / segmentDuration);

                        foreach (var kvp in _records)
                        {
                            IEntity target = kvp.Key;
                            if (TimeDilationSkill.IsEntityDestroyed(target) || target.transform == null) continue;

                            var positions = kvp.Value.RecordedPositions;
                            if (i < positions.Count && (i - 1) >= 0)
                            {
                                target.transform.position = Vector3.Lerp(positions[i], positions[i - 1], normalizedTime);
                            }
                        }
                        yield return null;
                    }
                }

                // Destroy the ghost at index 0 (oldest position) last
                foreach (var kvp in _records)
                {
                    var ghosts = kvp.Value.GhostClones;
                    if (ghosts.Count > 0 && ghosts[0] != null)
                    {
                        Destroy(ghosts[0]);
                        ghosts[0] = null;
                    }
                }
            }
            else
            {
                yield return null;
            }

            // 3. Clean up Registry, Target Visuals and any remaining ghosts
            foreach (var target in _targets)
            {
                if (_targetVisuals.TryGetValue(target, out GameObject vfx))
                {
                    if (vfx != null) Destroy(vfx);
                }

                if (_records.TryGetValue(target, out EntityRecord rec))
                {
                    foreach (var ghost in rec.GhostClones)
                    {
                        if (ghost != null) Destroy(ghost);
                    }
                }

                _globalRegistry.Remove(target);
            }

            Destroy(gameObject, 0.5f);
        }
    }
}
