using CoreSystem.Agent;
using CoreSystem.Controllers;
using CoreSystem.Data;
using CoreSystem.Data.Save;
using CoreSystem.Entity;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Visuals;
using CoreSystem.Utility;
using UnityEngine;

namespace CoreSystem.Player
{
    [DefaultExecutionOrder(-90)]
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public class PlayerCore : MonoBehaviour
    {
        #region [Variables/Fields]
        private static PlayerCore _instance;

        [Header("Debug")]
        public bool ShowDebugUI = true;

        private Character _controlled;
        private Camera _mainCamera;
        private InputSystem_Actions _inputActions;
        private readonly PlayerDeathScreen _deathScreen = new PlayerDeathScreen();
        private readonly PlayerInteraction _interaction = new PlayerInteraction();
        private bool _isDead;
        private int _selectedSkillSlot = -1;
        private SkillInstance _selectedSkillInstance;
        private IEntity _autoAttackTarget;
        #endregion

        #region [Properties]
        public static PlayerCore Instance => _instance;
        public static bool IsReady => _instance != null;
        public static event System.Action<PlayerCore> OnReady;
        public static event System.Action<Character> OnPlayerChanged;
        public Character Controlled => _controlled;
        public bool HasControl => _controlled != null && _controlled.Control.CanCommand(this);
        public event System.Action<Character> OnPossessed;
        public event System.Action<Character> OnReleased;
        public event System.Action<float> OnTick;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _mainCamera = Camera.main;
            _inputActions = new InputSystem_Actions();
            OnReady?.Invoke(this);
        }

        private void OnEnable()
        {
            _inputActions?.Enable();
        }

        private void OnDisable()
        {
            _inputActions?.Disable();
        }

        private void OnDestroy()
        {
            if (_instance != this) return;
            Release();
            _inputActions?.Dispose();
            _instance = null;
        }

        private void Update()
        {
            if (_controlled == null) return;

            OnTick?.Invoke(Time.deltaTime);

            if (CheckDeath())
            {
                _autoAttackTarget = null;
                SkillIndicatorManager.Instance?.HideIndicator();
                return;
            }

            if (!HasControl)
            {
                _autoAttackTarget = null;
                _controlled.Presenter.SetMoving(false);
                SkillIndicatorManager.Instance?.HideIndicator();
                return;
            }

            HandleMovement();
            HandleLookDirection();
            HandleSkillInput();
            HandleAutoAttack();
            UpdateIndicator();
        }
        #endregion

        #region [Public Methods]
        public static PlayerCore Create()
        {
            return RuntimeRoot.CreateChild("PlayerCore").AddComponent<PlayerCore>();
        }

        public static PlayerCore Ensure()
        {
            return _instance != null ? _instance : Create();
        }

        public static Character GetPlayer()
        {
            return _instance != null && _instance._controlled != null ? _instance._controlled : null;
        }

        public static bool TryGetPlayer(out Character player)
        {
            player = GetPlayer();
            return player != null;
        }

        public static bool IsPlayer(Character character)
        {
            return character != null && character == GetPlayer();
        }

        public void Possess(Character character)
        {
            if (character == null || character == _controlled) return;

            Release();
            if (AgentCore.Instance != null) AgentCore.Instance.Unregister(character);
            if (character.Control.TransferOwnership(this) == null)
            {
                Debug.LogWarning($"[PlayerCore] Could not take ownership of '{character.name}'.");
                return;
            }

            _controlled = character;
            _isDead = false;

            GameData.Player.Stats.Bind(character.attributeSystem);
            GameData.Player.Skills.Bind(character.skillCaster);
            GameData.Player.Equipment.Bind(character);
            GameData.Player.Costumes.Bind(character);

            character.OnTakeDamage += HandleControlledTakeDamage;

            OnPossessed?.Invoke(character);
            OnPlayerChanged?.Invoke(character);
        }

        public void Release()
        {
            if (_controlled == null) return;

            var previous = _controlled;
            previous.OnTakeDamage -= HandleControlledTakeDamage;
            previous.Control.Release(this);
            _controlled = null;
            _selectedSkillInstance = null;
            _selectedSkillSlot = -1;
            _autoAttackTarget = null;
            _deathScreen.Close();

            OnReleased?.Invoke(previous);
            OnPlayerChanged?.Invoke(null);
        }

        [ContextMenu("Save Data")]
        public PlayerDataModel SaveData()
        {
            return PlayerSaveStorage.Capture(_controlled);
        }

        [ContextMenu("Load Data")]
        public void LoadData()
        {
            LoadData(GameData.Player.Data);
        }

        public void LoadData(PlayerDataModel data)
        {
            PlayerSaveStorage.Restore(_controlled, data);
        }

        public Vector3 GetMouseWorldPosition()
        {
            Vector3 fallback = _controlled != null ? _controlled.transform.position : Vector3.zero;
            if (_mainCamera == null) return fallback;

            Vector2 screenPos = UnityEngine.InputSystem.Mouse.current != null ? UnityEngine.InputSystem.Mouse.current.position.ReadValue() : Vector2.zero;
            Vector3 mousePos = _mainCamera.ScreenToWorldPoint(screenPos);
            mousePos.z = 0f;
            return mousePos;
        }

        public void TryEnterAimingMode(SkillInstance instance, Vector3 mousePos)
        {
            _autoAttackTarget = null;
            if (_controlled == null || _controlled.skillCaster == null || instance == null || instance.SkillData == null || instance.SkillData.BaseTargeting == null) return;
            if (_controlled.skillCaster.CurrentState != SkillCasterState.Idle) return;

            if (instance.SkillData.BaseTargeting.RequiresSelection)
            {
                _selectedSkillInstance = instance;
                _selectedSkillSlot = _controlled.skillCaster.EquippedSkills.IndexOf(instance);
                _controlled.skillCaster.AimedSkillInstance = instance;
                _controlled.skillCaster.AimedSkillSlot = _selectedSkillSlot;

                var payload = _controlled.skillCaster.BuildPayload(instance, mousePos, true);
                SkillIndicatorManager.Instance?.ShowIndicator(payload, instance.SkillData.BaseTargeting);
            }
            else
            {
                _controlled.Control.CommandCastSkill(instance, mousePos, true, this);
                ClearAiming();
                SkillIndicatorManager.Instance?.HideIndicator();
            }
        }

        public void SetAutoAttackTarget(IEntity target)
        {
            if (_controlled == null || target == null || ReferenceEquals(target, _controlled)) return;
            if (!FactionDatabase.IsHostile(_controlled.Faction, target.Faction)) return;
            ClearAiming();
            _autoAttackTarget = target;
        }

        public void TryEnterAimingMode(int slot, Vector3 mousePos)
        {
            if (_controlled == null || _controlled.skillCaster == null) return;
            if (slot < 0 || slot >= _controlled.skillCaster.EquippedSkills.Count) return;
            TryEnterAimingMode(_controlled.skillCaster.EquippedSkills[slot], mousePos);
        }
        #endregion

        #region [Private Methods]
        private void HandleControlledTakeDamage(IEntity attacker, float damage)
        {
            CameraController.Instance?.ScreenShake(0.15f, 0.4f);
        }

        private bool CheckDeath()
        {
            if (_controlled.attributeSystem == null) return _isDead;

            float hp = _controlled.attributeSystem.GetAttribute(AttributeType.HP).CurrentValue;
            if (hp > 0)
            {
                _isDead = false;
                return false;
            }

            if (!_isDead)
            {
                _isDead = true;
                _controlled.Presenter.SetMoving(false);
                _controlled.Presenter.PlayDeath();
                SkillIndicatorManager.Instance?.HideIndicator();
                _controlled.Control.CommandInterruptCast(this);
                _deathScreen.Show(Revive);
            }
            return true;
        }

        private void Revive()
        {
            _isDead = false;
            _deathScreen.Close();

            if (_controlled != null && _controlled.attributeSystem != null)
            {
                var hpAttr = _controlled.attributeSystem.GetAttribute(AttributeType.HP);
                var maxHpAttr = _controlled.attributeSystem.GetAttribute(AttributeType.MaxHP);
                if (hpAttr != null && maxHpAttr != null)
                {
                    hpAttr.BaseValue = maxHpAttr.CurrentValue;
                    hpAttr.Recalculate();
                }
            }

            _controlled?.Presenter.ResetAnimator();
        }

        private void HandleMovement()
        {
            if (_controlled.movementSystem == null || _inputActions == null) return;

            Vector2 moveInput = _inputActions.Player.Move.ReadValue<Vector2>();
            Vector3 direction = new Vector3(moveInput.x, moveInput.y, 0f).normalized;
            bool isMoving = direction.magnitude > 0.1f;

            if (isMoving)
            {
                _autoAttackTarget = null;
                _controlled.Control.CommandMove(direction, this);
                _controlled.Presenter.Face(new Vector3(moveInput.x, 0f, 0f));
            }

            _controlled.Presenter.SetMoving(isMoving);
        }

        private void HandleLookDirection()
        {
            Vector3 position = _controlled.transform.position;
            if (_autoAttackTarget != null)
            {
                _controlled.FaceDirection.UpdateDirection((_autoAttackTarget.transform.position - position).normalized);
                return;
            }

            if (_mainCamera != null) _controlled.FaceDirection.UpdateDirection((GetMouseWorldPosition() - position).normalized);
        }

        private void HandleSkillInput()
        {
            if (_controlled.skillCaster == null || _inputActions == null) return;

            Vector3 mousePos = GetMouseWorldPosition();

            if (_inputActions.Player.Attack.triggered)
            {
                SkillInstance instance = _selectedSkillInstance;
                if (instance == null && _selectedSkillSlot >= 0 && _selectedSkillSlot < _controlled.skillCaster.EquippedSkills.Count)
                {
                    instance = _controlled.skillCaster.EquippedSkills[_selectedSkillSlot];
                }

                if (instance != null) CastAimedSkill(instance, mousePos);
                else SelectAutoAttackTarget(mousePos);
            }

            if (_inputActions.UI.RightClick.triggered)
            {
                bool wasAiming = _selectedSkillInstance != null || _selectedSkillSlot >= 0;
                ClearAiming();
                SkillIndicatorManager.Instance?.HideCurrent();
                if (!wasAiming) _interaction.TryOpenMenu(this, mousePos);
            }
        }

        private void CastAimedSkill(SkillInstance instance, Vector3 mousePos)
        {
            var payload = _controlled.skillCaster.BuildPayload(instance, mousePos, true);

            if (instance.SkillData.BaseTargeting is SingleUnitTargeting sut)
            {
                Collider2D col = Physics2D.OverlapPoint(mousePos);
                IEntity target = col != null ? col.GetComponentInParent<IEntity>() : null;

                if (target == null || !sut.IsValidTarget(_controlled, target))
                {
                    Debug.LogWarning("Invalid Target for Lock-On Skill!");
                    return;
                }

                float dist = Vector2.Distance(_controlled.transform.position, target.transform.position);
                float maxRange = instance.CalculateAOERadiusModified(_controlled.attributeSystem, sut.CastRange);
                if (dist > maxRange)
                {
                    Debug.LogWarning("Target is out of range!");
                    return;
                }

                payload.Targets = new System.Collections.Generic.List<IEntity> { target };
                payload.IsPositionLocked = true;
            }

            _controlled.Control.CommandCastSkill(instance, payload, this);
            SkillIndicatorManager.Instance?.StartExecution(payload);

            bool keepIndicator = instance.SkillData.BaseTargeting is ConeTargeting ||
                                 instance.SkillData.BaseTargeting is BoxTargeting ||
                                 instance.SkillData.BaseTargeting is LinePierceTargeting;

            ClearAiming();
            if (!keepIndicator) SkillIndicatorManager.Instance?.HideCurrent();
        }

        private void SelectAutoAttackTarget(Vector3 mousePos)
        {
            Collider2D col = Physics2D.OverlapPoint(mousePos);
            if (col == null)
            {
                _autoAttackTarget = null;
                return;
            }

            IEntity target = col.GetComponentInParent<IEntity>();
            if (target != null && !ReferenceEquals(target, _controlled) && FactionDatabase.IsHostile(_controlled.Faction, target.Faction))
            {
                _autoAttackTarget = target;
            }
        }

        private void ClearAiming()
        {
            _selectedSkillInstance = null;
            _selectedSkillSlot = -1;
            if (_controlled == null || _controlled.skillCaster == null) return;
            _controlled.skillCaster.AimedSkillInstance = null;
            _controlled.skillCaster.AimedSkillSlot = -1;
        }

        private void HandleAutoAttack()
        {
            if (_autoAttackTarget == null) return;

            if (_autoAttackTarget.attributeSystem != null)
            {
                var hpAttr = _autoAttackTarget.attributeSystem.GetAttribute(AttributeType.HP);
                if (hpAttr == null || hpAttr.CurrentValue <= 0)
                {
                    _autoAttackTarget = null;
                    _controlled.Presenter.SetMoving(false);
                    return;
                }
            }

            Vector3 position = _controlled.transform.position;
            Vector3 direction = (_autoAttackTarget.transform.position - position).normalized;
            float dist = Vector3.Distance(position, _autoAttackTarget.transform.position);

            if (dist > _controlled.AutoAttackRange)
            {
                _controlled.Control.CommandMove(direction, this);
                _controlled.Presenter.Face(direction);
                _controlled.Presenter.SetMoving(true);
                return;
            }

            _controlled.Presenter.SetMoving(false);
            _controlled.transform.up = direction;

            if (_controlled.BasicAttackCooldownTimer <= 0 && _controlled.skillCaster.CurrentState == SkillCasterState.Idle)
            {
                _controlled.Control.CommandBasicAttack(_autoAttackTarget, this);
                _controlled.Presenter.PlayAttack();
            }
        }

        private void UpdateIndicator()
        {
            var caster = _controlled.skillCaster;
            bool aiming = _selectedSkillInstance != null || _selectedSkillSlot != -1;

            if (!aiming && caster.CurrentState != SkillCasterState.Casting)
            {
                SkillIndicatorManager.Instance?.HideIndicator();
                return;
            }

            Vector3 mousePos = GetMouseWorldPosition();
            Vector3 origin = _controlled.transform.position;
            Vector3 direction = (mousePos - origin).normalized;

            SkillInstance skill;
            if (_selectedSkillInstance != null) skill = _selectedSkillInstance;
            else if (_selectedSkillSlot != -1 && _selectedSkillSlot < caster.EquippedSkills.Count) skill = caster.EquippedSkills[_selectedSkillSlot];
            else skill = caster.ActiveSkillInstance;

            if (skill == null || skill.SkillData == null) return;

            Vector3 indicatorPos = mousePos;
            if (skill.SkillData.BaseTargeting is TargetLocationTargeting targetLoc)
            {
                float scaledRange = skill.CalculateAOERadiusModified(_controlled.attributeSystem, targetLoc.CastRange);
                indicatorPos = targetLoc.ClampTargetPosition(origin, mousePos, scaledRange);
            }

            SkillIndicatorManager.Instance?.UpdateIndicator(origin, indicatorPos, direction);

            if (caster.CurrentState == SkillCasterState.Casting && skill == caster.ActiveSkillInstance)
            {
                SkillIndicatorManager.Instance?.SetProgress(caster.CurrentCastTimer / caster.CurrentCastTimeTotal);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            OnReady = null;
            OnPlayerChanged = null;
        }
        #endregion
    }
}
