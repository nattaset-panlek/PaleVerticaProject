using System.Collections.Generic;
using CoreSystem.Entity;
using CoreSystem.Enums;
using CoreSystem.Pathfinding;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using UnityEngine;

namespace CoreSystem.Agent
{
    public sealed class AgentHandle
    {
        #region [Variables/Fields]
        private const float DecisionTickRate = 0.2f;
        private const float DeathDestroyDelay = 5f;

        private readonly AgentCore _core;
        private readonly Character _character;
        private readonly AgentProfile _profile;
        private readonly Dictionary<int, float> _aiSkillIndividualTimers = new Dictionary<int, float>();
        private readonly List<int> _timerKeys = new List<int>();

        private AIState _currentState = AIState.Idle;
        private Transform _target;
        private Vector3 _homePosition;
        private Vector3 _roamDestination;
        private float _stateTimer;
        private float _orbitAngle;
        private float _orbitFlipTimer;
        private float _orbitSpeed;
        private float _aiSkillCooldownTimer;
        private float _nextDecisionTime;
        private float _deathTimer = -1f;

        private Vector3 _commandMoveDestination;
        private Transform _commandFollowTarget;

        private List<Vector2> _currentPath;
        private int _pathIndex;
        private float _nextPathTime;
        #endregion

        #region [Properties]
        public Character Character => _character;
        public AgentProfile Profile => _profile;
        public int RuntimeId => _character != null ? _character.RuntimeId : 0;
        public AIState CurrentState => _currentState;
        public Transform Target => _target;
        public int GroupID => _profile.GroupID;
        public string AgentName => _profile.AgentName;
        public AIBehaviorType Behavior => _profile.Behavior;
        public CombatStyle Combat => _profile.Combat;
        public List<AgentSkillEntry> SkillPool => _profile.SkillPool;
        public float AISkillCooldown => _profile.AISkillCooldown;
        public float GlobalAICooldownTimer => _aiSkillCooldownTimer;
        public IReadOnlyDictionary<int, float> IndividualAIDelayTimers => _aiSkillIndividualTimers;
        public bool HasControl => _character != null && _character.Control.IsHeldBy(_core);
        public float AttackRange => _character != null ? _character.AutoAttackRange : _profile.FallbackAttackRange;
        public Vector3 HomePosition => _homePosition;
        public Vector3 RoamDestination => _roamDestination;
        public IReadOnlyList<Vector2> CurrentPath => _currentPath;
        public int PathIndex => _pathIndex;
        private Vector3 Position => _character.transform.position;
        #endregion

        #region [Public Methods]
        public AgentHandle(AgentCore core, Character character)
        {
            _core = core;
            _character = character;
            _profile = character.AgentProfile;
            _orbitSpeed = _profile.OrbitSpeed;
            _homePosition = character.transform.position;
            if (string.IsNullOrEmpty(_profile.AgentName)) _profile.AgentName = character.gameObject.name;
        }

        public void Start()
        {
            SyncEquippedSkills(_character, _profile);
            _character.OnTakeDamage += HandleTakeDamage;
            AgentRegistry.Register(this);
        }

        public void Dispose()
        {
            if (_character != null) _character.OnTakeDamage -= HandleTakeDamage;
            AgentRegistry.Unregister(this);
        }

        public void Tick(float deltaTime)
        {
            if (_character == null) return;

            if (CheckDeath())
            {
                if (_deathTimer > 0f)
                {
                    _deathTimer -= deltaTime;
                    if (_deathTimer <= 0f) Object.Destroy(_character.gameObject);
                }
                return;
            }

            if (!HasControl) return;

            switch (_currentState)
            {
                case AIState.Idle:
                case AIState.Roam:
                    UpdateIdleRoam(deltaTime);
                    break;
                case AIState.Chase:
                    UpdateChase();
                    break;
                case AIState.Attack:
                    UpdateAttack(deltaTime);
                    break;
                case AIState.Flee:
                    UpdateFlee();
                    break;
                case AIState.CommandMove:
                    UpdateCommandMove();
                    break;
                case AIState.CommandFollow:
                    UpdateCommandFollow();
                    break;
            }

            if (Time.time >= _nextDecisionTime)
            {
                _nextDecisionTime = Time.time + DecisionTickRate;
                Think();
            }

            if (_aiSkillCooldownTimer > 0) _aiSkillCooldownTimer -= deltaTime;

            _timerKeys.Clear();
            _timerKeys.AddRange(_aiSkillIndividualTimers.Keys);
            foreach (var key in _timerKeys)
            {
                if (_aiSkillIndividualTimers[key] > 0) _aiSkillIndividualTimers[key] -= deltaTime;
            }
        }

        public void SetCommandMove(Vector3 destination)
        {
            _commandMoveDestination = destination;
            SetState(AIState.CommandMove);
        }

        public void SetCommandFollow(Transform target)
        {
            _commandFollowTarget = target;
            SetState(AIState.CommandFollow);
        }

        public void ClearCommand()
        {
            if (_currentState == AIState.CommandMove || _currentState == AIState.CommandFollow) SetState(AIState.Idle);
        }

        public void Revive(float hp)
        {
            if (_currentState != AIState.Dead) return;

            _deathTimer = -1f;
            _currentState = AIState.Idle;
            _stateTimer = 0f;
            _target = null;

            if (_character.attributeSystem != null)
            {
                var hpAttr = _character.attributeSystem.GetAttribute(AttributeType.HP);
                hpAttr.BaseValue = hp;
                hpAttr.Recalculate();
            }

            _character.Presenter.ResetAnimator();
            AgentRegistry.Register(this);
            _nextDecisionTime = Time.time + 0.5f;
        }

        public void ReceiveGroupAlert(Transform newTarget)
        {
            if (_currentState == AIState.Dead) return;
            if (_profile.Behavior == AIBehaviorType.Passive) return;

            Vector3 position = _character.transform.position;
            if (_target == null || (newTarget != null &&
                Vector3.Distance(position, newTarget.position) < Vector3.Distance(position, _target.position)))
            {
                _target = newTarget;
                if (_currentState != AIState.Attack) SetState(AIState.Chase);
                _nextDecisionTime = Time.time;
            }
        }

        public void ForceReset()
        {
            _target = null;
            SetState(AIState.Idle);
            StopMovement();
            _nextDecisionTime = Time.time;
        }

        public void ForceThink()
        {
            _nextDecisionTime = Time.time;
            Think();
        }

        public static void SyncEquippedSkills(Character character, AgentProfile profile)
        {
            if (character == null || character.skillCaster == null) return;

            character.skillCaster.EquippedSkills.Clear();

            if (character.DefaultSkillIds != null)
            {
                foreach (int id in character.DefaultSkillIds)
                {
                    var skillData = CoreSystem.Data.Database.GetSynchronizedSkill(id);
                    if (skillData != null) character.skillCaster.EquippedSkills.Add(new SkillInstance(skillData));
                }
            }

            foreach (var entry in profile.SkillPool)
            {
                if (entry != null && entry.Skill != null && entry.IsEquipped)
                {
                    if (character.DefaultSkillIds != null && character.DefaultSkillIds.Contains(entry.Skill.Id)) continue;
                    character.skillCaster.EquippedSkills.Add(new SkillInstance(entry.Skill));
                }
            }
        }
        #endregion

        #region [Private Methods]
        private void Think()
        {
            if (_currentState == AIState.Dead || _currentState == AIState.Flee || _currentState == AIState.CommandMove || _currentState == AIState.CommandFollow || _character.IsStunned) return;

            if (_target == null)
            {
                if (_profile.Behavior == AIBehaviorType.Aggressive) FindTarget();

                if (_profile.UseHomeZone && _target == null)
                {
                    float homeDist = Vector3.Distance(Position, _homePosition);
                    if (homeDist > _profile.HomeZoneRadius && _currentState != AIState.Roam) ReturnHome();
                }
            }
            else
            {
                float dist = Vector3.Distance(Position, _target.position);
                if (dist > _profile.LoseSightRadius || !IsTargetAliveAndValid(_target))
                {
                    _target = null;
                    SetState(AIState.Idle);
                    return;
                }

                SetState(dist <= GetEngageRange() ? AIState.Attack : AIState.Chase);
            }
        }

        private void FindTarget()
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(Position, _profile.DetectionRadius);
            Transform closest = null;
            float closestDist = float.MaxValue;

            foreach (var col in cols)
            {
                var c = col.GetComponent<Character>();
                if (c == null || c == _character) continue;
                if (!CoreSystem.Data.FactionDatabase.IsHostile(_character.Faction, c.Faction)) continue;

                float d = Vector3.Distance(Position, c.transform.position);
                if (d < closestDist) { closest = c.transform; closestDist = d; }
            }

            if (closest != null)
            {
                _target = closest;
                SetState(AIState.Chase);
                AgentRegistry.AlertGroup(this, _target, _profile.AlertRadius);
            }
        }

        private void UpdateIdleRoam(float deltaTime)
        {
            _stateTimer += deltaTime;

            if (_currentState == AIState.Idle)
            {
                if (_profile.CanRoam && _stateTimer > _profile.RoamWaitDuration) SetState(AIState.Roam);
            }
            else
            {
                NavigateTo(_roamDestination);
                bool arrived = Vector3.Distance(Position, _roamDestination) < 0.5f;
                bool timeout = _stateTimer > 8f;
                if (arrived || timeout) SetState(AIState.Idle);
            }
        }

        private void UpdateChase()
        {
            if (_target == null) { SetState(AIState.Idle); return; }
            NavigateTo(_target.position);
        }

        private void UpdateAttack(float deltaTime)
        {
            if (_target == null) { SetState(AIState.Idle); return; }

            Vector3 dir = (_target.position - Position).normalized;
            UpdateFacingDirection(dir);
            _character.Presenter.FaceWithCooldown(dir);

            Vector3 desiredPosition = ComputeCombatPosition(deltaTime);
            if (Vector3.Distance(Position, desiredPosition) > 0.4f) MoveTowardsLocation(desiredPosition);
            else StopMovement();

            TryCastSkill();

            float dist = Vector3.Distance(Position, _target.position);
            if (_character.skillCaster.CurrentState == SkillCasterState.Idle && dist <= AttackRange && _character.BasicAttackCooldownTimer <= 0)
            {
                var targetEntity = _target.GetComponent<IEntity>();
                if (targetEntity != null)
                {
                    _character.Control.CommandBasicAttack(targetEntity, _core);
                    _character.Presenter.PlayAttack();
                }
            }
        }

        private void UpdateFlee()
        {
            if (_target == null) { SetState(AIState.Idle); return; }

            Vector3 fleeDir = (Position - _target.position).normalized;
            MoveTowardsLocation(Position + fleeDir * 3f);

            if (Vector3.Distance(Position, _target.position) > _profile.LoseSightRadius) { _target = null; SetState(AIState.Idle); }
        }

        private void UpdateCommandMove()
        {
            NavigateTo(_commandMoveDestination);
            if (Vector3.Distance(Position, _commandMoveDestination) < 0.5f) SetState(AIState.Idle);
        }

        private void UpdateCommandFollow()
        {
            if (_commandFollowTarget == null) { SetState(AIState.Idle); return; }

            if (Vector3.Distance(Position, _commandFollowTarget.position) > 1.5f) NavigateTo(_commandFollowTarget.position);
            else StopMovement();
        }

        private Vector3 ComputeCombatPosition(float deltaTime)
        {
            if (_target == null) return Position;

            float dist = Vector3.Distance(Position, _target.position);
            switch (_profile.Combat)
            {
                case CombatStyle.Melee:
                    return _target.position + (_target.position - Position).normalized * -AttackRange;

                case CombatStyle.Ranged:
                    if (dist < _profile.PreferredRangeMin) return Position + (Position - _target.position).normalized * 1.5f;
                    if (dist > _profile.PreferredRangeMax) return _target.position + (_target.position - Position).normalized * -(_profile.PreferredRangeMax - 0.5f);
                    return Position;

                case CombatStyle.Circler:
                    _orbitFlipTimer -= deltaTime;
                    if (_orbitFlipTimer <= 0f)
                    {
                        _orbitFlipTimer = Random.Range(2f, 5f);
                        _orbitSpeed *= -1;
                    }
                    _orbitAngle += _orbitSpeed * deltaTime;
                    float rad = _orbitAngle * Mathf.Deg2Rad;
                    return _target.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * AttackRange;

                case CombatStyle.Supporter:
                    return _target.position + (_target.position - Position).normalized * -(AttackRange * 0.5f);
            }

            return Position;
        }

        private void NavigateTo(Vector3 dest)
        {
            if (_profile.UsePathfinding && PathfinderGrid.Instance != null)
            {
                if (Time.time >= _nextPathTime)
                {
                    _nextPathTime = Time.time + _profile.PathRefreshRate;
                    var path = Pathfinder2D.FindPath(Position, dest);
                    if (path != null && path.Count > 0)
                    {
                        _currentPath = path;
                        _pathIndex = 0;
                    }
                }

                if (_currentPath != null && _pathIndex < _currentPath.Count)
                {
                    Vector3 waypoint = _currentPath[_pathIndex];
                    if (Vector2.Distance(Position, waypoint) < 0.45f) _pathIndex++;

                    if (_pathIndex < _currentPath.Count)
                    {
                        MoveTowardsLocation(_currentPath[_pathIndex]);
                        return;
                    }
                }
            }

            MoveTowardsLocation(dest);
        }

        private void MoveTowardsLocation(Vector3 dest)
        {
            Vector3 dir = (dest - Position).normalized;
            _character.Control.CommandMove(dir.normalized, _core);

            if (_currentState != AIState.Attack)
            {
                _character.Presenter.FaceWithCooldown(dir);
                UpdateFacingDirection(dir);
            }
            _character.Presenter.SetMoving(true);
        }

        private void UpdateFacingDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.001f) return;
            _character.FaceDirection.UpdateDirection(direction);
        }

        private void StopMovement()
        {
            _character.Presenter.SetMoving(false);
        }

        private void ReturnHome()
        {
            _roamDestination = _homePosition + (Vector3)(Random.insideUnitCircle * _profile.RoamRadius * 0.3f);
            SetState(AIState.Roam);
        }

        private void TryCastSkill()
        {
            if (_character.skillCaster == null || _character.skillCaster.CurrentState != SkillCasterState.Idle) return;

            var readyPool = new List<SkillInstance>();
            foreach (var skill in _character.skillCaster.EquippedSkills)
            {
                if (skill == null || !skill.IsReady() || skill.SkillData == null) continue;

                float range = skill.SkillData.BaseAOERadius > 0 ? skill.SkillData.BaseAOERadius : AttackRange;
                if (skill.SkillData.BaseTargeting is SelfTargeting) range = _profile.DetectionRadius;

                if (_target == null || Vector3.Distance(Position, _target.position) <= range)
                {
                    float currentMp = _character.attributeSystem.GetAttribute(AttributeType.MP).CurrentValue;
                    if (currentMp >= skill.SkillData.BaseManaCost)
                    {
                        if (_aiSkillIndividualTimers.TryGetValue(skill.SkillData.Id, out float delay) && delay > 0) continue;
                        readyPool.Add(skill);
                    }
                }
            }

            if (readyPool.Count == 0) return;
            if (_aiSkillCooldownTimer > 0) return;

            var selected = readyPool[Random.Range(0, readyPool.Count)];
            Vector3 targetPos = _target != null ? _target.position : Position;
            var entry = _profile.SkillPool.Find(e => e.Skill != null && e.Skill.Id == selected.SkillData.Id);

            var payload = _character.skillCaster.BuildPayload(selected, targetPos, _target != null);
            if (entry != null)
            {
                payload.TotalCastTime += entry.ExtraCastTime;
                payload.CastDelay += entry.ExtraCastDelay;
                payload.Cooldown += entry.ExtraCooldown;
            }

            _character.Control.CommandCastSkill(selected, payload, _core);

            if (entry != null) _aiSkillIndividualTimers[selected.SkillData.Id] = entry.AIDelay;
            _aiSkillCooldownTimer = _profile.AISkillCooldown;
        }

        private float GetEngageRange()
        {
            float maxRange = AttackRange;
            if (_character.skillCaster == null) return maxRange;

            foreach (var skill in _character.skillCaster.EquippedSkills)
            {
                if (skill == null || skill.SkillData == null) continue;
                if (skill.SkillData.BaseTargeting is SelfTargeting) continue;
                float r = skill.SkillData.BaseAOERadius > 0 ? skill.SkillData.BaseAOERadius : AttackRange;
                if (r > maxRange) maxRange = r;
            }
            return maxRange;
        }

        private void SetState(AIState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
            _stateTimer = 0f;

            if (newState == AIState.Idle || newState == AIState.Attack) StopMovement();
            else if (newState == AIState.Roam) PickRoamDestination();
        }

        private void PickRoamDestination()
        {
            Vector2 rnd = Random.insideUnitCircle * _profile.RoamRadius;
            Vector3 candidate = _homePosition + new Vector3(rnd.x, rnd.y, 0);

            if (_profile.UseHomeZone)
            {
                Vector3 offset = candidate - _homePosition;
                if (offset.magnitude > _profile.HomeZoneRadius) candidate = _homePosition + offset.normalized * _profile.HomeZoneRadius;
            }

            _roamDestination = candidate;
            _currentPath = null;
            _nextPathTime = 0f;
        }

        private static bool IsTargetAliveAndValid(Transform t)
        {
            if (t == null) return false;
            var c = t.GetComponent<Character>();
            if (c?.attributeSystem != null) return c.attributeSystem.GetAttribute(AttributeType.HP).CurrentValue > 0;
            return true;
        }

        private void HandleTakeDamage(IEntity attacker, float damage)
        {
            if (_currentState == AIState.Dead) return;

            _character.Presenter.ShowWorldStatusBar();

            var atk = attacker as Character;
            if (atk == null) return;

            if (_profile.Behavior == AIBehaviorType.Defensive)
            {
                _target = atk.transform;
                SetState(AIState.Chase);
                AgentRegistry.AlertGroup(this, _target, _profile.AlertRadius);
                _nextDecisionTime = Time.time;
            }
            else if (_profile.Behavior == AIBehaviorType.Coward)
            {
                _target = atk.transform;
                SetState(AIState.Flee);
                AgentRegistry.AlertGroup(this, _target, _profile.AlertRadius);
            }
            else if (_profile.Behavior == AIBehaviorType.Aggressive && _target == null)
            {
                _target = atk.transform;
                SetState(AIState.Chase);
                AgentRegistry.AlertGroup(this, _target, _profile.AlertRadius);
            }
        }

        private bool CheckDeath()
        {
            if (_currentState == AIState.Dead) return true;
            if (_character.attributeSystem == null) return false;

            float hp = _character.attributeSystem.GetAttribute(AttributeType.HP).CurrentValue;
            if (hp > 0) return false;

            SetState(AIState.Dead);
            StopMovement();
            _character.Presenter.PlayDeath();
            AgentRegistry.Unregister(this);
            _character.Presenter.HideWorldStatusBar();
            _deathTimer = DeathDestroyDelay;
            return true;
        }
        #endregion
    }
}
