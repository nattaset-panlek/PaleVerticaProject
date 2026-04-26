using CoreSystem.Data;
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
        public SpriteRenderer SpriteRendererFillSkill;
        
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
            if (autoScaleToAOE && payload.AOERadius > 0)
            {
                float diameter = payload.AOERadius * scaleMultiplier;
                castTime = payload.TotalCastTime;
                transform.localScale = new Vector3(diameter, diameter, transform.localScale.z);
                if (SpriteRendererFillSkill != null)
                {
                    SpriteRendererFillSkill.transform.localScale = Vector3.zero;
                    StartCoroutine(ScaleRoutine());
                }
            }
            if (destroyAfterDuration)
            {
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
        }

        private float castTime;
        private System.Collections.IEnumerator ScaleRoutine()
        {
            float elapsed = 0f;
            while (elapsed < castTime)
            {
                elapsed += Time.deltaTime;
                SpriteRendererFillSkill.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(1,1,1), elapsed / castTime);
                yield return null;
            }
            SpriteRendererFillSkill.transform.localScale = new Vector3(1, 1, 1);
            yield return new WaitForSeconds(0.25f);
            Destroy(SpriteRendererFillSkill.gameObject);
        }
    }
}
