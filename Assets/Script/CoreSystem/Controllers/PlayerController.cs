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

        [Header("UI Reference")]
        public Image HealthBar;
        public Image ManaBar;

        public TMP_Text HealthText;
        public TMP_Text ManaText;

        public TMP_Text BaseLevel;
        public TMP_Text JobLevel;

        public TMP_Text ExpText;
        public Image ExpBar;

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
            player = GetComponent<PlayerCharacter>();
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (_isDead) return;

            HandleMovement();
            HandleLookDirection(); // Renamed from HandleMouseRotation in the instruction, keeping original name
            HandleSkillInput();

            // Update Skill Indicator if aiming or casting
            if (_selectedSkillSlot != -1 || player.skillCaster.CurrentState == SkillCasterState.Casting)
            {
                Vector3 mousePos = GetMouseWorldPosition();
                Vector3 origin = transform.position;
                Vector3 direction = (mousePos - origin).normalized;
                
                SkillInstance skill = null;
                if (_selectedSkillSlot != -1)
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
            else if (_selectedSkillSlot == -1 && player.skillCaster.CurrentState != SkillCasterState.Casting)
            {
                // Ensure hidden if not aiming or casting
                SkillIndicatorManager.Instance?.HideIndicator();
            }
            UpdateUI();
        }

        private bool CheckDeath()
        {
            if (_isDead) return true;

            if (player != null && player.attributeSystem != null)
            {
                float hp = player.attributeSystem.GetAttribute(AttributeType.HP).CurrentValue;
                if (hp <= 0)
                {
                    _isDead = true;
                    if (Anim != null) Anim.SetBool(MovingBoolParam, false);
                    PlayDeathAnimation();
                    SkillIndicatorManager.Instance?.HideIndicator();
                    return true;
                }
            }
            return false;
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
                if (HealthBar != null)
                {
                    float hp = player.attributeSystem.GetAttribute(AttributeType.HP).CurrentValue;
                    float maxHp = player.attributeSystem.GetAttribute(AttributeType.MaxHP).CurrentValue;
                    HealthBar.fillAmount = maxHp > 0 ? (hp / maxHp) : 0f;
                }

                if (ManaBar != null)
                {
                    float mp = player.attributeSystem.GetAttribute(AttributeType.MP).CurrentValue;
                    float maxMp = player.attributeSystem.GetAttribute(AttributeType.MaxMP).CurrentValue;
                    ManaBar.fillAmount = maxMp > 0 ? (mp / maxMp) : 0f;
                }

                if (HealthText != null)
                {
                    float hp = player.attributeSystem.GetAttribute(AttributeType.HP).CurrentValue;
                    float maxHp = player.attributeSystem.GetAttribute(AttributeType.MaxHP).CurrentValue;
                    HealthText.text = $"{hp:F0}/{maxHp:F0}";
                }

                if (ManaText != null)
                {
                    float mp = player.attributeSystem.GetAttribute(AttributeType.MP).CurrentValue;
                    float maxMp = player.attributeSystem.GetAttribute(AttributeType.MaxMP).CurrentValue;
                    ManaText.text = $"{mp:F0}/{maxMp:F0}";
                }

                if (BaseLevel != null)
                {
                    int baseLevel = (int)(player.attributeSystem.GetAttribute(AttributeType.BaseLevel)?.CurrentValue ?? 1);
                    BaseLevel.text = $"{baseLevel.ToString("000")}";
                }

                if (JobLevel != null)
                {
                    int jobLevel = (int)(player.attributeSystem.GetAttribute(AttributeType.JobLevel)?.CurrentValue ?? 1);
                    JobLevel.text = $"{jobLevel.ToString("000")}";
                }

                if (PassivePointText != null)
                {
                    int selected = (PassiveSkillLoader.Instance != null && PassiveSkillLoader.Instance.gameObject.activeInHierarchy) ? PassiveSkillLoader.Instance.SelectedCount : 0;
                    int displayPoints = player.PassivePoints - selected;
                    PassivePointText.text = $"{NumberFormatter.FormatWithCommas(displayPoints)}";
                }

                if (ExpText != null)
                {
                    float exp = player.expSystem.CurrentBaseExp;
                    float nextLevelExp = player.expSystem.GetNextBaseExp((int)player.attributeSystem.GetAttribute(AttributeType.BaseLevel));
                    ExpText.text = $"{NumberFormatter.FormatWithCommas((long)exp)} / {NumberFormatter.FormatWithCommas((long)nextLevelExp)}";
                }

                if (ExpBar != null)
                {
                    float exp = player.expSystem.CurrentBaseExp;
                    float nextLevelExp = player.expSystem.GetNextBaseExp((int)player.attributeSystem.GetAttribute(AttributeType.BaseLevel));
                    ExpBar.fillAmount = nextLevelExp > 0 ? (exp / nextLevelExp) : 0f;
                }
            }
        }

        private void HandleMovement()
        {
            if (player.movementSystem != null)
            {
                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Input.GetAxisRaw("Vertical");

                Vector3 direction = new Vector3(horizontal, vertical, 0f).normalized;
                bool isMoving = direction.magnitude > 0.1f;

                if (isMoving)
                {
                    player.movementSystem.Move(direction);

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
            if (mainCamera != null)
            {
                Vector3 mousePos = GetMouseWorldPosition();
                Vector3 directionToMouse = (mousePos - transform.position).normalized;
                transform.up = directionToMouse;
            }
        }

        private Vector3 GetMouseWorldPosition()
        {
            if (mainCamera == null) return transform.position;
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            return mousePos;
        }

        private void HandleSkillInput()
        {
            if (player.skillCaster != null)
            {
                Vector3 mousePos = GetMouseWorldPosition();

                // Selection Logic (Alpha Keys)
                if (Input.GetKeyDown(KeyCode.Alpha1)) TryEnterAimingMode(0, mousePos);
                if (Input.GetKeyDown(KeyCode.Alpha2)) TryEnterAimingMode(1, mousePos);
                if (Input.GetKeyDown(KeyCode.Alpha3)) TryEnterAimingMode(2, mousePos);
                if (Input.GetKeyDown(KeyCode.Alpha4)) TryEnterAimingMode(3, mousePos);
                if (Input.GetKeyDown(KeyCode.Alpha5)) TryEnterAimingMode(4, mousePos);
                if (Input.GetKeyDown(KeyCode.Alpha6)) TryEnterAimingMode(5, mousePos);

                // Execution Logic (Left Click)
                if (Input.GetMouseButtonDown(0))
                {
                    if (_selectedSkillSlot != -1)
                    {
                        var instance = player.skillCaster.EquippedSkills[_selectedSkillSlot];
                        var payload = player.skillCaster.BuildPayload(instance, mousePos, true);
                        
                        player.skillCaster.CastSkill(_selectedSkillSlot, mousePos);
                        SkillIndicatorManager.Instance?.StartExecution(payload);
                        
                        bool shouldKeepIndicator = instance.SkillData.BaseTargeting is ConeTargeting || 
                                                 instance.SkillData.BaseTargeting is BoxTargeting || 
                                                 instance.SkillData.BaseTargeting is LinePierceTargeting;

                        _selectedSkillSlot = -1;
                        player.skillCaster.AimedSkillSlot = -1;

                        if (!shouldKeepIndicator)
                        {
                            SkillIndicatorManager.Instance?.HideCurrent();
                        }
                    }
                }

                // Cancellation Logic (Right Click)
                if (Input.GetMouseButtonDown(1))
                {
                    _selectedSkillSlot = -1;
                    player.skillCaster.AimedSkillSlot = -1;
                    SkillIndicatorManager.Instance?.HideCurrent();
                }
            }
        }

        private void TryEnterAimingMode(int slot, Vector3 mousePos)
        {
            if (player.skillCaster == null) return;
            
            // Block aiming if currently casting or in cast delay
            if (player.skillCaster.CurrentState != SkillCasterState.Idle) return;

            if (slot < 0 || slot >= player.skillCaster.EquippedSkills.Count) return;

            var instance = player.skillCaster.EquippedSkills[slot];
            if (instance == null || instance.SkillData == null || instance.SkillData.BaseTargeting == null) return;

            if (instance.SkillData.BaseTargeting.RequiresSelection)
            {
                _selectedSkillSlot = slot;
                player.skillCaster.AimedSkillSlot = slot;

                // Show Visual Indicator
                var payload = player.skillCaster.BuildPayload(instance, mousePos, true);
                SkillIndicatorManager.Instance?.ShowIndicator(payload, instance.SkillData.BaseTargeting);
            }
            else
            {
                // Instant Skill - Fire immediately
                player.skillCaster.CastSkill(slot, mousePos);
                _selectedSkillSlot = -1;
                player.skillCaster.AimedSkillSlot = -1;
                SkillIndicatorManager.Instance?.HideIndicator(); // Hide indicator for instant skills
            }
        }

        private void OnGUI()
        {
            if (player == null || player.skillCaster == null) return;

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
    }
}
