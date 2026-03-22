using CoreSystem.Entity;
using CoreSystem;
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

        [Header("UI Reference")]
        public Image HealthBar;
        public Image ManaBar;

        public TMP_Text HealthText;
        public TMP_Text ManaText;

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
            if (CheckDeath()) return;

            HandleMovement();
            HandleSkillInput();
            HandleLookDirection();
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
                Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0f;

                Vector3 directionToMouse = (mousePos - transform.position).normalized;
                transform.up = directionToMouse;
            }
        }

        private void HandleSkillInput()
        {
            if (player.skillCaster != null)
            {
                if (Input.GetMouseButtonDown(0)) player.skillCaster.CastSkill(0);
                if (Input.GetMouseButtonDown(1)) player.skillCaster.CastSkill(1);

                if (Input.GetKeyDown(KeyCode.Alpha1)) player.skillCaster.CastSkill(0);
                if (Input.GetKeyDown(KeyCode.Alpha2)) player.skillCaster.CastSkill(1);
                if (Input.GetKeyDown(KeyCode.Alpha3)) player.skillCaster.CastSkill(2);
                if (Input.GetKeyDown(KeyCode.Alpha4)) player.skillCaster.CastSkill(3);
                if (Input.GetKeyDown(KeyCode.Alpha5)) player.skillCaster.CastSkill(4);
                if (Input.GetKeyDown(KeyCode.Alpha6)) player.skillCaster.CastSkill(5);
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
                    GUILayout.Label($"Slot {i+1}: {skill.SkillData.Name} - <color={color}>{cdText}</color>", new GUIStyle(GUI.skin.label) { richText = true });
                }
                else
                {
                    GUILayout.Label($"Slot {i+1}: Empty", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Italic });
                }
            }

            GUILayout.EndArea();
        }
    }
}
