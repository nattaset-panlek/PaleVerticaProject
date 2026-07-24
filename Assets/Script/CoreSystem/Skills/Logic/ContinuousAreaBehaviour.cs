using System.Collections;
using System.Collections.Generic;
using CoreSystem.Entity;
using CoreSystem.Skills.Visuals;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    /// <summary>
    /// Spawns an invisible zone that repeatedly evaluates a TargetingLogic and applies effects.
    /// Used for ActiveSkills with IsContinuousArea turned on.
    /// </summary>
    public class ContinuousAreaBehaviour : MonoBehaviour
    {
        private ActiveSkill _skillData;
        private SkillPayload _payload;
        private TargetingLogic _targeting;
        private List<EffectLogic> _effects;
        private float _tickRate;
        private bool _isInitialized = false;

        public void Initialize(ActiveSkill skillData, SkillPayload payload, TargetingLogic targeting, List<EffectLogic> effects, float duration, float tickRate)
        {
            _skillData = skillData;
            _payload = payload;
            _targeting = targeting;
            _effects = effects;
            _tickRate = tickRate;
            _isInitialized = true;

            // Start tick routine
            StartCoroutine(DamageRoutine());

            // Destroy this object when duration is over
            Destroy(gameObject, duration);
        }

        private IEnumerator DamageRoutine()
        {
            while (_isInitialized)
            {
                // If parented to caster, update origin position to follow
                if (transform.parent != null && _payload.Caster != null && transform.parent == _payload.Caster.transform)
                {
                    _payload.OriginPosition = _payload.Caster.transform.position;
                }

                if (_payload.Caster == null || _payload.Caster.IsDead)
                {
                    Destroy(gameObject);
                    yield break;
                }

                // Evaluate targets at the current origin
                if (_targeting != null)
                {
                    _payload.Targets = _targeting.GetTargets(_payload);
                    
                    if (_payload.Targets != null && _payload.Targets.Count > 0)
                    {
                        // Apply all effects to hit targets
                        for (int i = 0; i < _effects.Count; i++)
                        {
                            var effect = _effects[i];
                            if (effect == null) continue;

                            for (int j = 0; j < _payload.Targets.Count; j++)
                            {
                                effect.Apply(_payload, _payload.Targets[j]);
                            }
                        }

                        // Play visual on hit (optional: maybe only play at origin once? or repeat every tick?)
                        if (_skillData != null && VisualManager.Instance != null)
                        {
                            VisualManager.Instance.PlayVisual(_skillData, _payload);
                        }
                    }
                }

                yield return new WaitForSeconds(_tickRate > 0.05f ? _tickRate : 0.05f);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_isInitialized || _targeting == null || _payload == null || _payload.Caster == null) return;
            Vector2 forward = _payload.Caster.transform.up;
            _targeting.DrawGizmos(_payload.OriginPosition, forward, _payload);
        }
#endif
    }
}
