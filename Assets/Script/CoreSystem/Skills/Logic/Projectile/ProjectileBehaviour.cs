using System.Collections.Generic;
using CoreSystem.Entity;
using CoreSystem.Skills.Visuals;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    /// <summary>
    /// Attach this to a Projectile Prefab so it knows how to deal damage using its Skill Payload.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ProjectileBehaviour : MonoBehaviour, ISkillVisual
    {
        private SkillPayload payload;
        private EffectLogic damageEffect;
        private bool isInitialized = false;

        [Header("Movement")]
        public float Speed = 10f;

        [Header("Hit Behavior")]
        [Tooltip("Destroy this object after hitting an entity? (Useful for standard projectiles)")]
        public bool DestroyOnHit = true;
        
        [Tooltip("If checked, damages entities repeatedly over time while they stay inside the trigger.")]
        public bool IsContinuousDamage = false;
        public float DamageTickRate = 0.5f;

        private HashSet<IEntity> _alreadyHitEntities = new HashSet<IEntity>();
        private Dictionary<IEntity, float> _continuousHitTimers = new Dictionary<IEntity, float>();

        public void Initialize(SkillPayload payloadData)
        {
            EffectLogic dmgEffect = null;
            if (payloadData != null && payloadData.SkillEffects != null && payloadData.SkillEffects.Count > 0)
            {
                dmgEffect = payloadData.SkillEffects[0];
            }
            Initialize(payloadData, dmgEffect);
        }

        public void Initialize(SkillPayload payloadData, EffectLogic effectToApply)
        {
            this.payload = payloadData;
            this.damageEffect = effectToApply;
            this.isInitialized = true;

            if (payload.AOERadius > 0f)
            {
                transform.localScale += new Vector3(payload.AOERadius, payload.AOERadius, 0f);
            }
            
            // Optional: Destroy projectile after a timeout to prevent infinite floating
            Destroy(gameObject, 5f);
        }

        private void Update()
        {
            if (isInitialized)
            {
                transform.Translate(Vector3.up * Speed * Time.deltaTime, Space.Self);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isInitialized || payload == null || payload.Caster == null)
                return;

            if (other.gameObject == payload.Caster.gameObject)
                return;

            IEntity targetEntity = other.GetComponentInParent<IEntity>();

            if (targetEntity != null)
            {
                if (!CoreSystem.Data.FactionDatabase.IsHostile(payload.Caster.Faction, targetEntity.Faction))
                {
                    return;
                }

                if (IsContinuousDamage)
                {
                    TryApplyDamage(targetEntity, true);
                }
                else
                {
                    if (!_alreadyHitEntities.Contains(targetEntity))
                    {
                        TryApplyDamage(targetEntity, false);
                        _alreadyHitEntities.Add(targetEntity);

                        if (DestroyOnHit)
                        {
                            Destroy(gameObject);
                        }
                    }
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!isInitialized || !IsContinuousDamage || payload == null || payload.Caster == null)
                return;

            if (other.gameObject == payload.Caster.gameObject)
                return;

            IEntity targetEntity = other.GetComponentInParent<IEntity>();

            if (targetEntity != null)
            {
                if (!CoreSystem.Data.FactionDatabase.IsHostile(payload.Caster.Faction, targetEntity.Faction))
                {
                    return;
                }
                TryApplyDamage(targetEntity, true);
            }
        }

        private void TryApplyDamage(IEntity targetEntity, bool isContinuous)
        {
            if (damageEffect == null) return;

            if (isContinuous)
            {
                float lastHitTime = 0f;
                if (_continuousHitTimers.TryGetValue(targetEntity, out lastHitTime))
                {
                    if (Time.time - lastHitTime < DamageTickRate)
                    {
                        return;
                    }
                }
                _continuousHitTimers[targetEntity] = Time.time;
            }

            if (payload.AOERadius > 0f)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, payload.AOERadius);
                HashSet<IEntity> hitEntities = new HashSet<IEntity>();
                
                foreach (var col in colliders)
                {
                    if (payload.Caster != null && col.gameObject == payload.Caster.gameObject)
                        continue;
                        
                    IEntity hitEntity = col.GetComponentInParent<IEntity>();
                    if (hitEntity != null && hitEntities.Add(hitEntity))
                    {
                        if (CoreSystem.Data.FactionDatabase.IsHostile(payload.Caster.Faction, hitEntity.Faction))
                        {
                            damageEffect.Apply(payload, hitEntity);
                        }
                    }
                }
            }
            else
            {
                damageEffect.Apply(payload, targetEntity);
            }
        }
    }
}
