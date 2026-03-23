using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Skills.Visuals
{
    public enum VisualAppearance
    {
        TargetPosition,
        CasterPosition,
        FollowCaster
    }
    
    /// <summary>
    /// Component attached to visual effect prefabs that get spawned on skill execution.
    /// It can automatically scale based on the skill's AOE radius and destroy itself after playing.
    /// </summary>
    public class SkillVisualEffect : MonoBehaviour
    {
        [Header("Setup")]
        public bool autoScaleToAOE = true;
        public float scaleMultiplier = 2f; // e.g., if AOE is 3, localScale becomes (6,6,1) for a diameter of 6.
        public bool destroyAfterDuration = true;
        public float defaultDuration = 2f;
        
        [Header("Optional Particle System Feedback")]
        public ParticleSystem[] linkedParticles;
        public SpriteRenderer[] linkedRenderers;

        private SkillPayload currentPayload;

        /// <summary>
        /// Called by the SkillCaster immediately after spawning this prefab.
        /// </summary>
        public void Initialize(SkillPayload payload)
        {
            this.currentPayload = payload;

            // 1. Scale automatically based on the skill's AOE Radius
            if (autoScaleToAOE && payload.AOERadius > 0)
            {
                float diameter = payload.AOERadius * scaleMultiplier;
                // Preserve the Z scale for 2D setups if needed, or scale uniformly depending on visuals.
                transform.localScale = new Vector3(diameter, diameter, transform.localScale.z);
            }

            // 2. Schedule destruction
            if (destroyAfterDuration)
            {
                // If it has particles, we could use the longest particle duration
                float maxDuration = defaultDuration;
                if (linkedParticles != null && linkedParticles.Length > 0)
                {
                    foreach (var ps in linkedParticles)
                    {
                        if (ps != null && ps.main.duration + ps.main.startLifetime.constantMax > maxDuration)
                        {
                            maxDuration = ps.main.duration + ps.main.startLifetime.constantMax;
                        }
                    }
                }
                
                Destroy(gameObject, maxDuration);
            }
            
            // Note: You can expand this to read payload.DamageType or payload.BaseDamage 
            // to change colours (e.g. Red for Fire, Blue for Ice).
        }
    }
}
