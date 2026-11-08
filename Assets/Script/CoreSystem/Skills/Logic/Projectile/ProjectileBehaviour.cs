using System.Collections.Generic;
using CoreSystem.Combat;
using CoreSystem.Data;
using CoreSystem.Entity;
using CoreSystem.Skills.Visuals;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ProjectileBehaviour : MonoBehaviour, ISkillVisual
    {
        #region [Variables/Fields]
        [Header("Movement")]
        public float Speed = 10f;
        public float Lifetime = 5f;

        [Header("Hit Behavior")]
        public bool DestroyOnHit = true;
        public bool IsContinuousDamage = false;
        public float DamageTickRate = 0.5f;

        [Header("Homing and Chain")]
        public float HomingRange = 6f;
        public float HomingTurnRate = 360f;
        public float HomingRetargetInterval = 0.2f;
        public float ChainLifetimeBonus = 2f;

        private SkillPayload payload;
        private EffectLogic damageEffect;
        private bool isInitialized = false;
        private float _speed;
        private float _lifeLeft;
        private int _pierceLeft;
        private int _chainLeft;
        private float _retargetTimer;
        private IEntity _homingTarget;
        private readonly HashSet<IEntity> _alreadyHitEntities = new HashSet<IEntity>();
        private readonly Dictionary<IEntity, float> _continuousHitTimers = new Dictionary<IEntity, float>();
        #endregion

        #region [Properties]
        public int PierceLeft => _pierceLeft;
        public int ChainLeft => _chainLeft;
        public float CurrentSpeed => _speed;
        #endregion

        #region [Unity Methods]
        private void Update()
        {
            if (!isInitialized) return;

            _lifeLeft -= Time.deltaTime;
            if (_lifeLeft <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            if (payload != null && payload.IsHoming) SteerTowardsTarget(Time.deltaTime);
            transform.Translate(Vector3.up * _speed * Time.deltaTime, Space.Self);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isInitialized || payload == null || payload.Caster == null) return;
            if (other.gameObject == payload.Caster.gameObject) return;

            IEntity targetEntity = other.GetComponentInParent<IEntity>();
            if (targetEntity == null) return;
            if (!TeamRegistry.IsHostile(payload.Caster, targetEntity)) return;

            if (IsContinuousDamage)
            {
                TryApplyDamage(targetEntity, true);
                return;
            }

            if (_alreadyHitEntities.Contains(targetEntity)) return;
            TryApplyDamage(targetEntity, false);
            _alreadyHitEntities.Add(targetEntity);
            if (ReferenceEquals(_homingTarget, targetEntity)) _homingTarget = null;
            HandleAfterHit(targetEntity);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!isInitialized || !IsContinuousDamage || payload == null || payload.Caster == null) return;
            if (other.gameObject == payload.Caster.gameObject) return;

            IEntity targetEntity = other.GetComponentInParent<IEntity>();
            if (targetEntity == null) return;
            if (!TeamRegistry.IsHostile(payload.Caster, targetEntity)) return;
            TryApplyDamage(targetEntity, true);
        }
        #endregion

        #region [Public Methods]
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
            payload = payloadData;
            damageEffect = effectToApply;
            isInitialized = true;
            _lifeLeft = Lifetime;
            _speed = payload != null ? SupportHitFormula.ProjectileSpeed(Speed, payload.ProjectileSpeedMultiplier) : Speed;
            _pierceLeft = payload != null ? Mathf.Max(0, payload.PierceCount) : 0;
            _chainLeft = payload != null ? Mathf.Max(0, payload.ChainCount) : 0;

            if (payload != null && payload.AOERadius > 0f)
            {
                transform.localScale += new Vector3(payload.AOERadius, payload.AOERadius, 0f);
            }
        }

        public static ProjectileHitStep NextStep(bool destroyOnHit, int pierceLeft, int chainLeft, bool hasChainTarget)
        {
            if (!destroyOnHit) return ProjectileHitStep.Continue;
            if (chainLeft > 0 && hasChainTarget) return ProjectileHitStep.Chain;
            if (pierceLeft > 0) return ProjectileHitStep.Pierce;
            return ProjectileHitStep.Destroy;
        }
        #endregion

        #region [Private Methods]
        private void HandleAfterHit(IEntity hitEntity)
        {
            IEntity chainTarget = _chainLeft > 0 ? FindNearestHostile(hitEntity.transform.position, payload.ChainRange) : null;

            switch (NextStep(DestroyOnHit, _pierceLeft, _chainLeft, chainTarget != null))
            {
                case ProjectileHitStep.Chain:
                    _chainLeft--;
                    _lifeLeft = Mathf.Max(_lifeLeft, ChainLifetimeBonus);
                    transform.position = hitEntity.transform.position;
                    FaceTowards(chainTarget.transform.position);
                    _homingTarget = chainTarget;
                    break;
                case ProjectileHitStep.Pierce:
                    _pierceLeft--;
                    break;
                case ProjectileHitStep.Destroy:
                    Destroy(gameObject);
                    break;
            }
        }

        private void SteerTowardsTarget(float deltaTime)
        {
            _retargetTimer -= deltaTime;
            if (_homingTarget == null || IsGone(_homingTarget))
            {
                if (_retargetTimer > 0f) return;
                _retargetTimer = HomingRetargetInterval;
                _homingTarget = FindNearestHostile(transform.position, HomingRange);
            }
            if (_homingTarget == null) return;

            Vector3 toTarget = _homingTarget.transform.position - transform.position;
            toTarget.z = 0f;
            if (toTarget.sqrMagnitude < 0.0001f) return;
            Quaternion desired = Quaternion.LookRotation(Vector3.forward, toTarget.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, HomingTurnRate * deltaTime);
        }

        private void FaceTowards(Vector3 position)
        {
            Vector3 direction = position - transform.position;
            direction.z = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.LookRotation(Vector3.forward, direction.normalized);
        }

        private IEntity FindNearestHostile(Vector3 center, float radius)
        {
            if (payload == null || payload.Caster == null || radius <= 0f) return null;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius);
            IEntity best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < colliders.Length; i++)
            {
                IEntity entity = colliders[i].GetComponentInParent<IEntity>();
                if (entity == null || ReferenceEquals(entity, payload.Caster)) continue;
                if (_alreadyHitEntities.Contains(entity) || IsGone(entity)) continue;
                if (!TeamRegistry.IsHostile(payload.Caster, entity)) continue;
                float distance = (entity.transform.position - center).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = entity;
                }
            }
            return best;
        }

        private static bool IsGone(IEntity entity)
        {
            if (entity == null) return true;
            if (entity is Object unityObject && unityObject == null) return true;
            return entity is Character character && character.IsDead;
        }

        private void TryApplyDamage(IEntity targetEntity, bool isContinuous)
        {
            if (damageEffect == null) return;

            if (isContinuous)
            {
                if (_continuousHitTimers.TryGetValue(targetEntity, out float lastHitTime) && Time.time - lastHitTime < DamageTickRate) return;
                _continuousHitTimers[targetEntity] = Time.time;
            }

            if (payload.AOERadius > 0f)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, payload.AOERadius);
                HashSet<IEntity> hitEntities = new HashSet<IEntity>();
                foreach (var col in colliders)
                {
                    if (payload.Caster != null && col.gameObject == payload.Caster.gameObject) continue;
                    IEntity hitEntity = col.GetComponentInParent<IEntity>();
                    if (hitEntity != null && hitEntities.Add(hitEntity) && TeamRegistry.IsHostile(payload.Caster, hitEntity))
                    {
                        damageEffect.Apply(payload, hitEntity);
                    }
                }
                return;
            }

            damageEffect.Apply(payload, targetEntity);
        }
        #endregion
    }
}
