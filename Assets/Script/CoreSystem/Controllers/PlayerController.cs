using CoreSystem.Entity;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Visuals;
using CoreSystem.Utile;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.Controllers
{
    [RequireComponent(typeof(PlayerCharacter))]
    public class PlayerController : MonoBehaviour
    {
        private PlayerCharacter player;
        private Camera mainCamera;
        private bool _isDead;
        private int _selectedSkillSlot = -1;
        private SkillInstance _selectedSkillInstance = null;
        private IEntity _autoAttackTarget;
        private InputSystem_Actions _inputActions;

        public static PlayerController Instance { get; private set; }

        [Header("UI Reference")]
        public TMP_Text PassivePointText;

        [Header("Animation")]
        public Animator Anim;
        public string MovingBoolParam = "IsMoving";
        public string DeadTriggerParam = "IsDead";

        [Header("Visual Controls")]
        public Transform ModelTransform;
        public bool LockModelRotation = true;

        private void Awake()
        {
            Instance = this;
            player = GetComponent<PlayerCharacter>();
            mainCamera = Camera.main;
            _inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _inputActions?.Enable();
        }

        private void OnDisable()
        {
            _inputActions?.Disable();
        }

        private void Update()
        {
            if (CheckDeath())
            {
                _autoAttackTarget = null;
                return;
            }

            HandleMovement();
            HandleLookDirection(); // Renamed from HandleMouseRotation in the instruction, keeping original name
            HandleSkillInput();
            HandleAutoAttack();

            // Update Skill Indicator if aiming or casting
            if (_selectedSkillInstance != null || _selectedSkillSlot != -1 || player.skillCaster.CurrentState == SkillCasterState.Casting)
            {
                Vector3 mousePos = GetMouseWorldPosition();
                Vector3 origin = transform.position;
                Vector3 direction = (mousePos - origin).normalized;
                
                SkillInstance skill = null;
                if (_selectedSkillInstance != null)
                {
                    skill = _selectedSkillInstance;
                }
                else if (_selectedSkillSlot != -1 && _selectedSkillSlot < player.skillCaster.EquippedSkills.Count)
                {
                    skill = player.skillCaster.EquippedSkills[_selectedSkillSlot];
                }
                else
                {
                    skill = player.skillCaster.ActiveSkillInstance;
                }

                if (skill != null && skill.SkillData != null)
                {
                    Vector3 indicatorPos = mousePos;
                    if (skill.SkillData.BaseTargeting is TargetLocationTargeting targetLoc)
                    {
                        float scaledRange = skill.CalculateAOERadiusModified(player.attributeSystem, targetLoc.CastRange);
                        indicatorPos = targetLoc.ClampTargetPosition(origin, mousePos, scaledRange);
                    }
                    
                    SkillIndicatorManager.Instance?.UpdateIndicator(origin, indicatorPos, direction);
                    
                    if (player.skillCaster.CurrentState == SkillCasterState.Casting && skill == player.skillCaster.ActiveSkillInstance)
                    {
                        float progress = player.skillCaster.CurrentCastTimer / player.skillCaster.CurrentCastTimeTotal;
                        SkillIndicatorManager.Instance?.SetProgress(progress);
                    }
                }
            }
            else if (_selectedSkillInstance == null && _selectedSkillSlot == -1 && player.skillCaster.CurrentState != SkillCasterState.Casting)
            {
                // Ensure hidden if not aiming or casting
                SkillIndicatorManager.Instance?.HideIndicator();
            }
            UpdateUI();
        }

        private bool CheckDeath()
        {
            if (player != null && player.attributeSystem != null)
            {
                float hp = player.attributeSystem.GetAttribute(AttributeType.HP).CurrentValue;
                if (hp <= 0)
                {
                    if (!_isDead)
                    {
                        _isDead = true;
                        if (Anim != null) Anim.SetBool(MovingBoolParam, false);
                        PlayDeathAnimation();
                        SkillIndicatorManager.Instance?.HideIndicator();
                        player.commandDirector?.CommandInterruptCast(this);
                    }
                    return true;
                }
                else
                {
                    _isDead = false;
                }
            }
            return _isDead;
        }

        private void RevivePlayer()
        {
            _isDead = false;

            if (player != null && player.attributeSystem != null)
            {
                var hpAttr = player.attributeSystem.GetAttribute(AttributeType.HP);
                var maxHpAttr = player.attributeSystem.GetAttribute(AttributeType.MaxHP);
                if (hpAttr != null && maxHpAttr != null)
                {
                    hpAttr.BaseValue = maxHpAttr.CurrentValue;
                    hpAttr.Recalculate();
                }
            }

            if (Anim != null)
            {
                Anim.Rebind();
                Anim.Update(0f);
            }
        }

        private void LateUpdate()
        {
            if (LockModelRotation && ModelTransform != null)
            {
                ModelTransform.rotation = Quaternion.identity;
            }
        }

        private void UpdateUI()
        {
            if (player != null && player.attributeSystem != null)
            {
                if (PassivePointText != null)
                {
                    int selected = (PassiveSkillLoader.Instance != null && PassiveSkillLoader.Instance.gameObject.activeInHierarchy) ? PassiveSkillLoader.Instance.SelectedCount : 0;
                    int displayPoints = player.PassivePoints - selected;
                    PassivePointText.text = $"{NumberFormatter.FormatWithCommas(displayPoints)}";
                }
            }
        }

        private void HandleMovement()
        {
            if (player.movementSystem != null)
            {
                float horizontal = 0f;
                float vertical = 0f;

                if (_inputActions != null)
                {
                    Vector2 moveInput = _inputActions.Player.Move.ReadValue<Vector2>();
                    horizontal = moveInput.x;
                    vertical = moveInput.y;
                }

                Vector3 direction = new Vector3(horizontal, vertical, 0f).normalized;
                bool isMoving = direction.magnitude > 0.1f;

                if (isMoving)
                {
                    _autoAttackTarget = null;
                    player.commandDirector.CommandMove(direction, this);

                    if (ModelTransform != null)
                    {
                        if (horizontal > 0.1f)
                        {
                            ModelTransform.localScale = new Vector3(-1, 1, 1);
                        }
                        else if (horizontal < -0.1f)
                        {
                            ModelTransform.localScale = new Vector3(1, 1, 1);
                        }
                    }
                }

                if (Anim != null)
                {
                    Anim.SetBool(MovingBoolParam, isMoving);
                }
            }
        }

        public void PlayDeathAnimation()
        {
            if (Anim != null) Anim.SetTrigger(DeadTriggerParam);
        }

        private void HandleLookDirection()
        {
            if (_autoAttackTarget != null)
            {
                Vector3 directionToTarget = (_autoAttackTarget.transform.position - transform.position).normalized;
                transform.up = directionToTarget;
                return;
            }

            if (mainCamera != null)
            {
                Vector3 mousePos = GetMouseWorldPosition();
                Vector3 directionToMouse = (mousePos - transform.position).normalized;
                transform.up = directionToMouse;
            }
        }

        public Vector3 GetMouseWorldPosition()
        {
            if (mainCamera == null) return transform.position;
            Vector2 screenPos = UnityEngine.InputSystem.Mouse.current != null ? UnityEngine.InputSystem.Mouse.current.position.ReadValue() : Vector2.zero;
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(screenPos);
            mousePos.z = 0f;
            return mousePos;
        }

        private void HandleSkillInput()
        {
            if (player.skillCaster != null)
            {
                Vector3 mousePos = GetMouseWorldPosition();

                if (_inputActions != null)
                {
                    if (_inputActions.Player.Attack.triggered)
                    {
                        SkillInstance instance = _selectedSkillInstance;
                        if (instance == null && _selectedSkillSlot >= 0 && _selectedSkillSlot < player.skillCaster.EquippedSkills.Count)
                        {
                            instance = player.skillCaster.EquippedSkills[_selectedSkillSlot];
                        }

                        if (instance != null)
                        {
                            var payload = player.skillCaster.BuildPayload(instance, mousePos, true);
                            
                            player.commandDirector.CommandCastSkill(instance, mousePos, true, this);
                            SkillIndicatorManager.Instance?.StartExecution(payload);
                            
                            bool shouldKeepIndicator = instance.SkillData.BaseTargeting is ConeTargeting || 
                                                     instance.SkillData.BaseTargeting is BoxTargeting || 
                                                     instance.SkillData.BaseTargeting is LinePierceTargeting;

                            _selectedSkillInstance = null;
                            _selectedSkillSlot = -1;
                            player.skillCaster.AimedSkillInstance = null;
                            player.skillCaster.AimedSkillSlot = -1;

                            if (!shouldKeepIndicator)
                            {
                                SkillIndicatorManager.Instance?.HideCurrent();
                            }
                        }
                        else
                        {
                            Collider2D col = Physics2D.OverlapPoint(mousePos);
                            if (col != null)
                            {
                                IEntity target = col.GetComponentInParent<IEntity>();
                                if (target != null && target != player && player.Faction != target.Faction)
                                {
                                    _autoAttackTarget = target;
                                }
                            }
                            else
                            {
                                _autoAttackTarget = null;
                            }
                        }
                    }

                    if (_inputActions.UI.RightClick.triggered)
                    {
                        _selectedSkillInstance = null;
                        _selectedSkillSlot = -1;
                        player.skillCaster.AimedSkillInstance = null;
                        player.skillCaster.AimedSkillSlot = -1;
                        SkillIndicatorManager.Instance?.HideCurrent();
                    }
                }
            }
        }

        public void TryEnterAimingMode(SkillInstance instance, Vector3 mousePos)
        {
            _autoAttackTarget = null;
            if (player.skillCaster == null || instance == null || instance.SkillData == null || instance.SkillData.BaseTargeting == null) return;
            if (player.skillCaster.CurrentState != SkillCasterState.Idle) return;

            if (instance.SkillData.BaseTargeting.RequiresSelection)
            {
                _selectedSkillInstance = instance;
                _selectedSkillSlot = player.skillCaster.EquippedSkills.IndexOf(instance);
                player.skillCaster.AimedSkillInstance = instance;
                player.skillCaster.AimedSkillSlot = _selectedSkillSlot;

                var payload = player.skillCaster.BuildPayload(instance, mousePos, true);
                SkillIndicatorManager.Instance?.ShowIndicator(payload, instance.SkillData.BaseTargeting);
            }
            else
            {
                player.commandDirector.CommandCastSkill(instance, mousePos, true, this);
                _selectedSkillInstance = null;
                _selectedSkillSlot = -1;
                player.skillCaster.AimedSkillInstance = null;
                player.skillCaster.AimedSkillSlot = -1;
                SkillIndicatorManager.Instance?.HideIndicator();
            }
        }

        public void TryEnterAimingMode(int slot, Vector3 mousePos)
        {
            if (player == null || player.skillCaster == null) return;
            if (slot < 0 || slot >= player.skillCaster.EquippedSkills.Count) return;
            TryEnterAimingMode(player.skillCaster.EquippedSkills[slot], mousePos);
        }

        private void OnGUI()
        {
            if (player == null || player.skillCaster == null) return;

            if (_isDead)
            {
                float width = 300f;
                float height = 150f;
                float x = (Screen.width - width) / 2f;
                float y = (Screen.height - height) / 2f;

                GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
                GUILayout.Space(20);
                GUILayout.Label("<color=red><b>YOU ARE DEAD</b></color>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter, fontSize = 20 });
                GUILayout.Space(20);

                if (GUILayout.Button("Respawn", GUILayout.Height(40)))
                {
                    RevivePlayer();
                }

                GUILayout.EndArea();
                return;
            }

            var caster = player.skillCaster;

            GUILayout.BeginArea(new Rect(10, 10, 250, 300), GUI.skin.box);
            GUILayout.Label($"<b>Skill System Status</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });

            GUILayout.Label($"State: {caster.CurrentState}");

            if (caster.CurrentState == SkillCasterState.Casting && caster.ActiveSkillInstance != null && caster.ActiveSkillInstance.SkillData != null)
            {
                GUILayout.Label($"Casting: {caster.ActiveSkillInstance.SkillData.Name}");
                GUILayout.Label($"Progress: {caster.CurrentCastTimer:F2} / {caster.CurrentCastTimeTotal:F2} s");
            }

            if (caster.CurrentState == SkillCasterState.CastDelay)
            {
                GUILayout.Label($"Cast Delay: {caster.GlobalCastDelayTimer:F2} s");
            }

            GUILayout.Space(10);
            GUILayout.Label($"<b>Equipped Skills</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });

            for (int i = 0; i < caster.EquippedSkills.Count; i++)
            {
                var skill = caster.EquippedSkills[i];
                if (skill != null && skill.SkillData != null)
                {
                    string color = skill.IsReady() ? "green" : "red";
                    string cdText = skill.IsReady() ? "Ready" : $"{skill.CurrentCooldown:F2}s";
                    GUILayout.Label($"Slot {i + 1}: {skill.SkillData.Name} - <color={color}>{cdText}</color>", new GUIStyle(GUI.skin.label) { richText = true });
                }
                else
                {
                    GUILayout.Label($"Slot {i + 1}: Empty", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Italic });
                }
            }

            GUILayout.EndArea();
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
                    if (Anim != null) Anim.SetBool(MovingBoolParam, false);
                    return;
                }
            }

            float dist = Vector3.Distance(transform.position, _autoAttackTarget.transform.position);
            float attackRange = player.AutoAttackRange;

            if (dist > attackRange)
            {
                Vector3 direction = (_autoAttackTarget.transform.position - transform.position).normalized;
                if (player.commandDirector != null)
                {
                    player.commandDirector.CommandMove(direction, this);
                }

                if (ModelTransform != null)
                {
                    if (direction.x > 0.1f) ModelTransform.localScale = new Vector3(-1, 1, 1);
                    else if (direction.x < -0.1f) ModelTransform.localScale = new Vector3(1, 1, 1);
                }

                if (Anim != null) Anim.SetBool(MovingBoolParam, true);
            }
            else
            {
                if (Anim != null) Anim.SetBool(MovingBoolParam, false);

                Vector3 direction = (_autoAttackTarget.transform.position - transform.position).normalized;
                transform.up = direction;

                if (player.BasicAttackCooldownTimer <= 0 && player.skillCaster.CurrentState == SkillCasterState.Idle)
                {
                    player.BasicAttack(_autoAttackTarget);
                    if (Anim != null) Anim.SetTrigger("Attack");
                }
            }
        }
    }
}
