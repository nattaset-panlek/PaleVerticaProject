using CoreSystem.Entity;
using CoreSystem.Enums;
using CoreSystem.Pathfinding;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.UI;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Controllers
{
    public enum AIState
    {
        Idle,
        Roam,
        Chase,
        Attack,
        Flee,
        Dead
    }

    [RequireComponent(typeof(Character))]
    public class AgentCore : MonoBehaviour
    {
        // ─── Identity ─────────────────────────────────────────────
        [Header("Identity")]
        public string AgentName = "Agent";
        
        // ─── AI Config ────────────────────────────────────────────
        [Header("AI Configuration")]
        public AIBehaviorType Behavior = AIBehaviorType.Aggressive;
        public CombatStyle Combat = CombatStyle.Melee;
        public float DetectionRadius = 10f;
        public float AttackRange = 3f;
        public float LoseSightRadius = 15f;

        // ─── Ranged/Circler Combat Settings ───────────────────────
        [Header("Combat Style Settings")]
        [Tooltip("Ranged: min preferred distance from target.")]
        public float PreferredRangeMin = 5f;
        [Tooltip("Ranged: max preferred distance from target.")]
        public float PreferredRangeMax = 7f;
        [Tooltip("Circler: orbit speed in degrees per second.")]
        public float OrbitSpeed = 90f;

        // ─── Roam / Home Zone ─────────────────────────────────────
        [Header("Home Zone & Roam")]
        public bool UseHomeZone = true;
        public float HomeZoneRadius = 8f;
        public float RoamRadius = 5f;
        public float RoamWaitDuration = 3f;
        public bool CanRoam = true;

        // ─── Group AI ─────────────────────────────────────────────
        [Header("Group AI")]
        [Tooltip("0 = Solo. Same GroupID agents coordinate and alert each other.")]
        public int GroupID = 0;
        public float AlertRadius = 12f;

        // ─── Skill Pool ───────────────────────────────────────────
        [System.Serializable]
        public class SkillEquipEntry
        {
            public ActiveSkill Skill;
            public bool IsEquipped = true;
            [Tooltip("AI-specific delay (seconds) before this skill can be used again by THIS agent.")]
            public float AIDelay = 2.0f;
        }

        [Header("Skill Pool")]
        [Tooltip("The list of skills this agent can use. Toggle 'IsEquipped' to enable/disable at runtime.")]
        public List<SkillEquipEntry> SkillPool = new List<SkillEquipEntry>();
        
        [Tooltip("Global delay (seconds) between AI skill casts to prevent spamming.")]
        public float AISkillCooldown = 1.5f;

        private float _aiSkillCooldownTimer;

        // ─── Animation ────────────────────────────────────────────
        [Header("Animation")]
        public Animator Anim;
        public string MovingBoolParam = "IsMoving";
        public string DeadTriggerParam = "IsDead";

        // ─── Visual Controls ──────────────────────────────────────
        [Header("Visual Controls")]
        public Transform ModelTransform;
        public bool LockModelRotation = true;

        // ─── Runtime State (Read Only) ────────────────────────────
        [Header("Debug (Read Only)")]
        [SerializeField] private AIState _currentState = AIState.Idle;
        [SerializeField] private Transform _target;

        // ─── Private Fields ───────────────────────────────────────
        private Character _character;
        private Vector3 _homePosition;
        private Vector3 _roamDestination;
        private float _stateTimer;
        private float _orbitAngle;
        private float _orbitFlipTimer;
        
        private Dictionary<int, float> _aiSkillIndividualTimers = new Dictionary<int, float>();

        private float _decisionTickRate = 0.2f;
        private float _nextDecisionTime;

        // ─── Pathfinding ───────────────────────────────────────────
        [Header("Pathfinding")]
        [Tooltip("Enable A* pathfinding. Requires a PathfinderGrid in the scene.")]
        public bool UsePathfinding = true;
        [Tooltip("How often (seconds) to re-request a new path.")]
        public float PathRefreshRate = 0.5f;

        private List<Vector2> _currentPath;
        private int           _pathIndex;
        private float         _nextPathTime;
        private Vector3       _activeMoveTarget;

        private FloatingStatusBar _spawnedStatusBar;

        // ─── Properties ───────────────────────────────────────────
        public AIState CurrentState => _currentState;
        public Transform Target => _target;
        public Character Character => _character;

        // ─── Debug Monitor ─────────────────────────────────────────
        [System.Serializable]
        public struct SkillMonitorEntry
        {
            public string SkillName;
            public string Cooldown;
            public string AIDelay;
            public string Status;
        }

        [Header("Skill Monitor (Debug)")]
        public List<SkillMonitorEntry> SkillMonitor = new List<SkillMonitorEntry>();
        public float GlobalAICooldownTimer;

        // ═══════════════════════════════════════════════════════════
        //   Lifecycle
        // ═══════════════════════════════════════════════════════════

        private void Awake()
        {
            _character = GetComponent<Character>();
            _homePosition = transform.position;
            if (string.IsNullOrEmpty(AgentName))
                AgentName = gameObject.name;

            InitializeSkills();
        }

        private void InitializeSkills()
        {
            if (_character != null && _character.skillCaster != null)
            {
                UpdateEquippedList();
            }
        }

        public void UpdateEquippedList()
        {
            if (_character == null || _character.skillCaster == null) return;
            
            _character.skillCaster.EquippedSkills.Clear();
            foreach (var entry in SkillPool)
            {
                if (entry != null && entry.Skill != null && entry.IsEquipped)
                {
                    _character.skillCaster.EquippedSkills.Add(new SkillInstance(entry.Skill));
                }
            }
        }

        private void OnValidate()
        {
            // Sync in editor
            if (!Application.isPlaying && _character == null)
            {
                _character = GetComponent<Character>();
            }
            
            if (!Application.isPlaying && _character != null && _character.skillCaster != null)
            {
                UpdateEquippedList();
            }
        }

        private void OnEnable()
        {
            AgentGroup.Register(this);
            if (_character != null)
                _character.OnTakeDamage += HandleTakeDamage;
        }

        private void OnDisable()
        {
            AgentGroup.Unregister(this);
            if (_character != null)
                _character.OnTakeDamage -= HandleTakeDamage;
        }

        // ═══════════════════════════════════════════════════════════
        //   Update
        // ═══════════════════════════════════════════════════════════

        private void Update()
        {
            if (CheckDeath())
            {
                Destroy(gameObject, 5f); // Optional: delay destroy to allow death animation
                return;
            }
            switch (_currentState)
            {
                case AIState.Idle:
                case AIState.Roam:
                    UpdateIdleRoam();
                    break;
                case AIState.Chase:
                    UpdateChase();
                    break;
                case AIState.Attack:
                    UpdateAttack();
                    break;
                case AIState.Flee:
                    UpdateFlee();
                    break;
            }

            if (Time.time >= _nextDecisionTime)
            {
                _nextDecisionTime = Time.time + _decisionTickRate;
                Think();
            }

            if (_aiSkillCooldownTimer > 0)
                _aiSkillCooldownTimer -= Time.deltaTime;
            
            GlobalAICooldownTimer = _aiSkillCooldownTimer;

            // Update individual skill timers
            List<int> keys = new List<int>(_aiSkillIndividualTimers.Keys);
            foreach (var key in keys)
            {
                if (_aiSkillIndividualTimers[key] > 0)
                    _aiSkillIndividualTimers[key] -= Time.deltaTime;
            }

            UpdateSkillMonitor();
        }

        private void UpdateSkillMonitor()
        {
#if UNITY_EDITOR
            if (_character == null || _character.skillCaster == null) return;

            SkillMonitor.Clear();
            foreach (var instance in _character.skillCaster.EquippedSkills)
            {
                if (instance == null || instance.SkillData == null) continue;

                SkillMonitorEntry entry = new SkillMonitorEntry();
                entry.SkillName = instance.SkillData.Name;
                entry.Cooldown = instance.CurrentCooldown.ToString("F1") + "s";
                
                _aiSkillIndividualTimers.TryGetValue(instance.SkillData.Id, out float aiDelay);
                entry.AIDelay = aiDelay.ToString("F1") + "s";

                if (instance.CurrentCooldown > 0) entry.Status = "On Cooldown";
                else if (aiDelay > 0) entry.Status = "AI Delay";
                else entry.Status = "Ready";

                // If currently casting this specific skill
                if (_character.skillCaster.CurrentState == SkillCasterState.Casting && _character.skillCaster.ActiveSkillInstance == instance)
                {
                    entry.Status = $"Casting ({_character.skillCaster.CurrentCastTimer:F1}/{_character.skillCaster.CurrentCastTimeTotal:F1})";
                }

                SkillMonitor.Add(entry);
            }
#endif
        }

        private void LateUpdate()
        {
            if (LockModelRotation && ModelTransform != null)
                ModelTransform.rotation = Quaternion.identity;
        }

        // ═══════════════════════════════════════════════════════════
        //   Core Decision Loop
        // ═══════════════════════════════════════════════════════════

        private void Think()
        {
            if (_currentState == AIState.Dead || _currentState == AIState.Flee) return;

            if (_target == null)
            {
                if (Behavior == AIBehaviorType.Aggressive)
                    FindTarget();

                // Return home if drifted outside home zone
                if (UseHomeZone && _target == null)
                {
                    float homeDist = Vector3.Distance(transform.position, _homePosition);
                    if (homeDist > HomeZoneRadius && _currentState != AIState.Roam)
                        ReturnHome();
                }
            }
            else
            {
                float dist = Vector3.Distance(transform.position, _target.position);
                if (dist > LoseSightRadius || !IsTargetAliveAndValid(_target))
                {
                    _target = null;
                    SetState(AIState.Idle);
                    return;
                }

                float engageRange = GetEngageRange();
                SetState(dist <= engageRange ? AIState.Attack : AIState.Chase);
            }
        }

        private void FindTarget()
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, DetectionRadius);
            Transform closest = null;
            float closestDist = float.MaxValue;

            foreach (var col in cols)
            {
                var c = col.GetComponent<Character>();
                if (c == null || c == _character) continue;
                if (!IsHostile(_character.Faction, c.Faction)) continue;

                float d = Vector3.Distance(transform.position, c.transform.position);
                if (d < closestDist) { closest = c.transform; closestDist = d; }
            }

            if (closest != null)
            {
                _target = closest;
                SetState(AIState.Chase);
                AgentGroup.AlertGroup(this, _target, AlertRadius);
            }
        }

        // ═══════════════════════════════════════════════════════════
        //   State Updates
        // ═══════════════════════════════════════════════════════════

        private void UpdateIdleRoam()
        {
            _stateTimer += Time.deltaTime;

            if (_currentState == AIState.Idle)
            {
                if (CanRoam && _stateTimer > RoamWaitDuration)
                    SetState(AIState.Roam);
            }
            else // Roam
            {
                Vector3 intendedDir = (_roamDestination - transform.position).normalized;
                NavigateTo(_roamDestination, Vector3.zero);

                bool arrived = Vector3.Distance(transform.position, _roamDestination) < 0.5f;
                bool timeout = _stateTimer > 8f;
                if (arrived || timeout) SetState(AIState.Idle);
            }
        }

        private void UpdateChase()
        {
            if (_target == null) { SetState(AIState.Idle); return; }

            Vector3 intendedDir = (_target.position - transform.position).normalized;
            NavigateTo(_target.position, Vector3.zero);
        }

        private void UpdateAttack()
        {
            if (_target == null) { SetState(AIState.Idle); return; }

            Vector3 dir = (_target.position - transform.position).normalized;
            transform.up = dir;
            UpdateVisualFlip(dir);

            // Determine desired position based on CombatStyle
            Vector3 desiredPosition = ComputeCombatPosition();
            float distToDesired = Vector3.Distance(transform.position, desiredPosition);

            if (distToDesired > 0.4f)
            {
                MoveTowardsLocation(desiredPosition, Vector3.zero);
            }
            else
            {
                StopMovement();
            }

            // Attempt skill cast
            TryCastSkill();
        }

        private void UpdateFlee()
        {
            if (_target == null) { SetState(AIState.Idle); return; }

            Vector3 fleeDir = (transform.position - _target.position).normalized;
            // Flee doesn't use pathfinding — just direct opposite direction
            MoveTowardsLocation(transform.position + fleeDir * 3f, Vector3.zero);

            if (Vector3.Distance(transform.position, _target.position) > LoseSightRadius)
            { _target = null; SetState(AIState.Idle); }
        }

        // ═══════════════════════════════════════════════════════════
        //   Combat Positioning
        // ═══════════════════════════════════════════════════════════

        private Vector3 ComputeCombatPosition()
        {
            if (_target == null) return transform.position;

            float dist = Vector3.Distance(transform.position, _target.position);

            switch (Combat)
            {
                case CombatStyle.Melee:
                    return _target.position + (_target.position - transform.position).normalized * -AttackRange;

                case CombatStyle.Ranged:
                    // Stay between min and max range
                    if (dist < PreferredRangeMin)
                    {
                        // Back away
                        return transform.position + (transform.position - _target.position).normalized * 1.5f;
                    }
                    else if (dist > PreferredRangeMax)
                    {
                        // Move closer
                        return _target.position + (_target.position - transform.position).normalized * -(PreferredRangeMax - 0.5f);
                    }
                    return transform.position; // In range, hold

                case CombatStyle.Circler:
                    _orbitFlipTimer -= Time.deltaTime;
                    if (_orbitFlipTimer <= 0f)
                    {
                        _orbitFlipTimer = Random.Range(2f, 5f);
                        OrbitSpeed *= -1; // Randomly reverse orbit direction
                    }
                    _orbitAngle += OrbitSpeed * Time.deltaTime;
                    float rad = _orbitAngle * Mathf.Deg2Rad;
                    Vector3 orbit = _target.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * AttackRange;
                    return orbit;

                case CombatStyle.Supporter:
                    // Stay near ally target; supporter's target should be set to an ally or player
                    return _target.position + (_target.position - transform.position).normalized * -(AttackRange * 0.5f);
            }

            return transform.position;
        }

        // ─── Pathfinding ───────────────────────────────────────────
        //   Movement & Pathfinding
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// High-level navigation: use A* if available, otherwise straight line.
        /// </summary>
        private void NavigateTo(Vector3 dest, Vector3 separationForce)
        {
            _activeMoveTarget = dest;

            if (UsePathfinding && PathfinderGrid.Instance != null)
            {
                // Request path refresh periodically or when destination changes significantly
                if (Time.time >= _nextPathTime)
                {
                    _nextPathTime   = Time.time + PathRefreshRate;
                    var path = Pathfinder2D.FindPath(transform.position, dest);
                    if (path != null && path.Count > 0)
                    {
                        _currentPath = path;
                        _pathIndex   = 0;
                    }
                }

                // Follow the path
                if (_currentPath != null && _pathIndex < _currentPath.Count)
                {
                    Vector3 waypoint = _currentPath[_pathIndex];
                    if (Vector2.Distance(transform.position, waypoint) < 0.45f)
                        _pathIndex++;

                    if (_pathIndex < _currentPath.Count)
                    {
                        MoveTowardsLocation(_currentPath[_pathIndex], separationForce);
                        return;
                    }
                }
            }

            // Fallback: direct movement
            MoveTowardsLocation(dest, separationForce);
        }

        private void MoveTowardsLocation(Vector3 dest, Vector3 separationForce)
        {
            Vector3 dir = (dest - transform.position).normalized;
            Vector3 combined = dir + separationForce; // We will normalize later

            if (_character.movementSystem != null)
                _character.movementSystem.Move(combined.normalized);

            // Flip visuals based on INTENDED direction, not avoidance, to prevent sprite twitching
            UpdateVisualFlip(dir);
            UpdateAnimation(true);
            transform.up = dir; // Aim toward destination not separation
        }

        private void StopMovement()
        {
            UpdateAnimation(false);
        }

        private void ReturnHome()
        {
            // Just pick a roam target toward home
            _roamDestination = _homePosition + (Vector3)(Random.insideUnitCircle * RoamRadius * 0.3f);
            SetState(AIState.Roam);
        }

        private float _lastFlipTime;

        private void UpdateVisualFlip(Vector3 direction)
        {
            if (ModelTransform == null) return;
            
            // Cooldown to prevent rapid frame-by-frame flipping
            if (Time.time - _lastFlipTime < 0.1f) return;

            if (direction.x > 0.1f && ModelTransform.localScale.x > 0)
            {
                ModelTransform.localScale = new Vector3(-1, 1, 1);
                _lastFlipTime = Time.time;
            }
            else if (direction.x < -0.1f && ModelTransform.localScale.x < 0)
            {
                ModelTransform.localScale = new Vector3(1, 1, 1);
                _lastFlipTime = Time.time;
            }
        }

        private void UpdateAnimation(bool isMoving)
        {
            if (Anim != null) Anim.SetBool(MovingBoolParam, isMoving);
        }

        // ═══════════════════════════════════════════════════════════
        //   Skill
        // ═══════════════════════════════════════════════════════════

        private void TryCastSkill()
        {
            if (_character == null || _character.skillCaster == null || _character.skillCaster.CurrentState != SkillCasterState.Idle) return;

            // 1. Filter Ready Pool
            var readyPool = new List<SkillInstance>();
            foreach (var skill in _character.skillCaster.EquippedSkills)
            {
                if (skill == null || !skill.IsReady() || skill.SkillData == null) continue;

                // Range Check
                float range = skill.SkillData.BaseAOERadius > 0 ? skill.SkillData.BaseAOERadius : AttackRange;
                if (skill.SkillData.BaseTargeting is SelfTargeting) range = DetectionRadius;

                if (_target == null || Vector3.Distance(transform.position, _target.position) <= range)
                {
                    // Mana Check
                    float currentMp = _character.attributeSystem.GetAttribute(AttributeType.MP).CurrentValue;
                    if (currentMp >= skill.SkillData.BaseManaCost)
                    {
                        // 1.1 Individual AI Delay Check
                        if (_aiSkillIndividualTimers.TryGetValue(skill.SkillData.Id, out float delay) && delay > 0)
                            continue;

                        readyPool.Add(skill);
                    }
                }
            }

            if (readyPool.Count == 0) return;

            // 2. AI Global Cooldown Check
            if (_aiSkillCooldownTimer > 0) return;

            // 3. Selection: Randomly pick one
            int index = Random.Range(0, readyPool.Count);
            var selected = readyPool[index];
            
            Vector3 targetPos = _target != null ? _target.position : transform.position;
            _character.skillCaster.CastSkill(selected, targetPos, _target != null);

            // Set Individual AI Delay
            var entry = SkillPool.Find(e => e.Skill != null && e.Skill.Id == selected.SkillData.Id);
            if (entry != null)
            {
                _aiSkillIndividualTimers[selected.SkillData.Id] = entry.AIDelay;
            }

            // Set Global AI Cooldown
            _aiSkillCooldownTimer = AISkillCooldown;
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

        // ═══════════════════════════════════════════════════════════
        //   Helpers
        // ═══════════════════════════════════════════════════════════

        private void SetState(AIState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
            _stateTimer = 0f;

            if (newState == AIState.Idle || newState == AIState.Attack)
                StopMovement();
            else if (newState == AIState.Roam)
                PickRoamDestination();
        }

        private void PickRoamDestination()
        {
            Vector2 rnd = Random.insideUnitCircle * RoamRadius;
            Vector3 candidate = _homePosition + new Vector3(rnd.x, rnd.y, 0);

            if (UseHomeZone)
            {
                Vector3 offset = candidate - _homePosition;
                if (offset.magnitude > HomeZoneRadius)
                    candidate = _homePosition + offset.normalized * HomeZoneRadius;
            }

            _roamDestination = candidate;
            // Reset path so it requests a fresh one immediately
            _currentPath  = null;
            _nextPathTime = 0f;
        }

        private bool IsHostile(FactionType a, FactionType b)
        {
            if (a == FactionType.Player && b == FactionType.Enemy) return true;
            if (a == FactionType.Enemy  && b == FactionType.Player) return true;
            return false;
        }

        private bool IsTargetAliveAndValid(Transform t)
        {
            if (t == null) return false;
            var c = t.GetComponent<Character>();
            if (c?.attributeSystem != null)
                return c.attributeSystem.GetAttribute(AttributeType.HP).CurrentValue > 0;
            return true;
        }

        // ═══════════════════════════════════════════════════════════
        //   Events (OnTakeDamage)
        // ═══════════════════════════════════════════════════════════

        private void HandleTakeDamage(IEntity attacker, float damage)
        {
            if (_currentState == AIState.Dead) return;

            // Spawn health bar if not already present
            if (_spawnedStatusBar == null)
            {
                GameObject prefab = Resources.Load<GameObject>("Prefab/HealthLocalStatus");
                if (prefab != null)
                {
                    Transform parent = null;
                    GameObject worldSpaceObj = GameObject.Find("WorldSpace");
                    if (worldSpaceObj != null && worldSpaceObj.layer == LayerMask.NameToLayer("UI"))
                    {
                        parent = worldSpaceObj.transform;
                    }

                    GameObject go = Instantiate(prefab, parent);
                    _spawnedStatusBar = go.GetComponent<FloatingStatusBar>();
                    if (_spawnedStatusBar != null)
                    {
                        _spawnedStatusBar.Initialize(_character);
                    }

                    var follow = go.GetComponent<UIFollowTarget>();
                    if (follow != null)
                    {
                        follow.Target = transform;
                    }
                }
            }

            var atk = attacker as Character;
            if (atk == null) return;

            if (Behavior == AIBehaviorType.Defensive)
            {
                _target = atk.transform;
                SetState(AIState.Chase);
                AgentGroup.AlertGroup(this, _target, AlertRadius);
                _nextDecisionTime = Time.time;
            }
            else if (Behavior == AIBehaviorType.Coward)
            {
                _target = atk.transform;
                SetState(AIState.Flee);
                AgentGroup.AlertGroup(this, _target, AlertRadius);
            }
            else if (Behavior == AIBehaviorType.Aggressive && _target == null)
            {
                _target = atk.transform;
                SetState(AIState.Chase);
                AgentGroup.AlertGroup(this, _target, AlertRadius);
            }
        }

        private bool CheckDeath()
        {
            if (_currentState == AIState.Dead) return true;
            if (_character?.attributeSystem == null) return false;

            float hp = _character.attributeSystem.GetAttribute(AttributeType.HP).CurrentValue;
            if (hp <= 0)
            {
                SetState(AIState.Dead);
                UpdateAnimation(false);
                if (Anim != null) Anim.SetTrigger(DeadTriggerParam);
                AgentGroup.Unregister(this);

                if (_spawnedStatusBar != null)
                {
                    Destroy(_spawnedStatusBar.gameObject);
                }

                return true;
            }
            return false;
        }

        // ═══════════════════════════════════════════════════════════
        //   Group Alert Entry Point (called by AgentGroup)
        // ═══════════════════════════════════════════════════════════

        public void ReceiveGroupAlert(Transform newTarget)
        {
            if (_currentState == AIState.Dead) return;
            if (Behavior == AIBehaviorType.Passive) return;

            if (_target == null || (newTarget != null && 
                Vector3.Distance(transform.position, newTarget.position) < Vector3.Distance(transform.position, _target.position)))
            {
                _target = newTarget;
                if (_currentState != AIState.Attack) SetState(AIState.Chase);
                _nextDecisionTime = Time.time;
            }
        }

        // ═══════════════════════════════════════════════════════════
        //   Editor Helpers
        // ═══════════════════════════════════════════════════════════

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Home Zone (green)
            if (UseHomeZone)
            {
                UnityEditor.Handles.color = new Color(0, 1, 0, 0.12f);
                UnityEditor.Handles.DrawSolidDisc(Application.isPlaying ? _homePosition : transform.position, Vector3.forward, HomeZoneRadius);
                Gizmos.color = new Color(0, 1, 0, 0.7f);
                Gizmos.DrawWireSphere(Application.isPlaying ? _homePosition : transform.position, HomeZoneRadius);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Detection (yellow)
            Gizmos.color = new Color(1, 1, 0, 0.5f);
            Gizmos.DrawWireSphere(transform.position, DetectionRadius);

            // Attack Range (red)
            Gizmos.color = new Color(1, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, AttackRange);

            // Lose Sight (gray)
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, LoseSightRadius);

            // Lose Sight (gray)
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, LoseSightRadius);

            // Roam destination (blue)
            if (_currentState == AIState.Roam)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, _roamDestination);
                Gizmos.DrawWireSphere(_roamDestination, 0.4f);
            }

            // Current path waypoints (cyan)
            if (_currentPath != null && _currentPath.Count > 0)
            {
                Gizmos.color = new Color(0f, 0.9f, 1f, 0.8f);
                for (int i = _pathIndex; i < _currentPath.Count - 1; i++)
                    Gizmos.DrawLine(_currentPath[i], _currentPath[i + 1]);
                for (int i = _pathIndex; i < _currentPath.Count; i++)
                    Gizmos.DrawWireSphere(_currentPath[i], 0.15f);
            }

            // Target line (magenta)
            if (_target != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, _target.position);
            }
        }

        public void Editor_ForceReset()
        {
            _target = null;
            SetState(AIState.Idle);
            StopMovement();
            _nextDecisionTime = Time.time;
        }

        public void Editor_ForceThink()
        {
            _nextDecisionTime = Time.time;
            Think();
        }
#endif
    }
}
