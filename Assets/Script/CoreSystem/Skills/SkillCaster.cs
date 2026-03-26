using System.Collections.Generic;
using CoreSystem.Entity;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Visuals;
using UnityEngine;
using CoreSystem;

namespace CoreSystem.Skills
{
    [System.Serializable]
    public class SkillCaster
    {
        public List<SkillInstance> EquippedSkills = new List<SkillInstance>();
        
        public void EquipSkill(ActiveSkill skillData, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= EquippedSkills.Count) return;

            if (skillData == null)
            {
                EquippedSkills[slotIndex] = null;
                return;
            }

            EquippedSkills[slotIndex] = new SkillInstance(skillData);
        }

        public List<SkillInstance> GrantedSkills = new List<SkillInstance>();

        [Header("Debug")]
        public List<GameObject> LastHitTargets = new List<GameObject>();
        
        public SkillCasterState CurrentState { get; private set; } = SkillCasterState.Idle;
        
        public SkillInstance ActiveSkillInstance => activeSkillInstance;
        private SkillInstance activeSkillInstance;

        public float CurrentCastTimeTotal => currentCastTimeTotal;
        private float currentCastTimeTotal;
        private Vector3 activeTargetPosition;
        private bool activeIsTargetSet;
        private bool activeIsPositionLocked;
        
        public float CurrentCastTimer => currentCastTimer;
        private float currentCastTimer;
        
        public float GlobalCastDelayTimer => globalCastDelayTimer;
        private float globalCastDelayTimer;
        
        public System.Action<string, float> OnCastInitiated;
        public System.Action<float, float> OnCastProgress;
        public System.Action<string> OnCastExecuted;
        public System.Action OnCastInterrupted;
        
        public int AimedSkillSlot = -1;


        private Character character;

        public void Initialize(Character owner)
        {
            this.character = owner;
        }

        public void Tick(float dt)
        {
            for (int i = 0; i < EquippedSkills.Count; i++)
            {
                if (EquippedSkills[i] != null)
                    EquippedSkills[i].UpdateCooldown(dt);
            }

            for (int i = 0; i < GrantedSkills.Count; i++)
            {
                if (GrantedSkills[i] != null)
                    GrantedSkills[i].UpdateCooldown(dt);
            }

            UpdateToggleSkills();

            if (globalCastDelayTimer > 0)
            {
                globalCastDelayTimer -= dt;
                if (globalCastDelayTimer <= 0 && CurrentState == SkillCasterState.CastDelay)
                    CurrentState = SkillCasterState.Idle;
            }

            switch (CurrentState)
            {
                case SkillCasterState.Casting:
                    ProcessCastingState(dt);
                    break;
            }
        }

        private void UpdateToggleSkills()
        {
            for (int i = 0; i < EquippedSkills.Count; i++)
            {
                SkillInstance instance = EquippedSkills[i];
                if (instance != null && instance.SkillData != null && instance.SkillData.IsToggle && instance.IsToggledOn)
                {
                    if (instance.IsReady())
                    {
                        // Use BuildPayload to get accurate modified mana cost
                        SkillPayload payload = BuildPayload(instance);

                        if (character != null && character.attributeSystem != null)
                        {
                            float currentMp = character.attributeSystem.GetAttribute(AttributeType.MP).CurrentValue;
                            if (currentMp < payload.ManaCost)
                            {
                                instance.IsToggledOn = false;
                                OnCastInterrupted?.Invoke();
                                continue;
                            }
                        }

                        ExecutePipeline(instance, payload);
                        instance.StartCooldown(payload.Cooldown);
                        OnCastExecuted?.Invoke(instance.SkillData.Name + " (Toggle)");
                    }
                }
            }
        }

        private void ProcessCastingState(float dt)
        {
            currentCastTimer += dt;
            OnCastProgress?.Invoke(currentCastTimer, currentCastTimeTotal);

            if (currentCastTimer >= currentCastTimeTotal)
            {
                ExecuteSkill(activeSkillInstance);
            }
        }

        public void CastSkill(int slotIndex, Vector3 targetPos = default)
        {
            if (CurrentState != SkillCasterState.Idle) return;
            if (slotIndex < 0 || slotIndex >= EquippedSkills.Count) return;

            SkillInstance instance = EquippedSkills[slotIndex];
            if (instance == null || instance.SkillData == null) return;

            if (instance.SkillData.IsToggle)
            {
                if (!instance.IsToggledOn) // Turning ON
                {
                    SkillPayload payload = BuildPayload(instance, targetPos, targetPos != default);
                    if (character != null && character.attributeSystem != null)
                    {
                        float currentMp = character.attributeSystem.GetAttribute(AttributeType.MP).CurrentValue;
                        if (currentMp < payload.ManaCost) return;
                    }

                    instance.IsToggledOn = true;
                    if (instance.IsReady())
                    {
                        ExecutePipeline(instance, payload);
                        instance.StartCooldown(payload.Cooldown);
                        OnCastExecuted?.Invoke(instance.SkillData.Name + " (Toggle ON)");
                    }
                }
                else // Turning OFF
                {
                    instance.IsToggledOn = false;
                }
                return;
            }
            
            InitiateCastSequence(instance, targetPos, targetPos != default);
        }

        public void CastSkill(SkillInstance instance, Vector3 targetPos = default, bool isTargetSet = false)
        {
            if (CurrentState != SkillCasterState.Idle) return;
            if (instance == null || instance.SkillData == null) return;
            InitiateCastSequence(instance, targetPos, isTargetSet);
        }

        public void CastSkillDirectly(ActiveSkill skillData, Vector3 targetPos = default, bool isTargetSet = false)
        {
            if (skillData == null) return;
            SkillInstance instance = GrantedSkills.Find(s => s.SkillData == skillData) ?? new SkillInstance(skillData);
            if (CurrentState != SkillCasterState.Idle) return;
            InitiateCastSequence(instance, targetPos, isTargetSet);
        }

        public void GrantSkill(ActiveSkill skillData)
        {
            if (skillData == null) return;
            if (GrantedSkills.Exists(s => s.SkillData == skillData)) return;
            GrantedSkills.Add(new SkillInstance(skillData));
        }

        public void RevokeSkill(ActiveSkill skillData)
        {
            if (skillData == null) return;
            GrantedSkills.RemoveAll(s => s.SkillData == skillData);
        }

        public SkillPayload BuildPayload(SkillInstance instance, Vector3 targetPos = default, bool isTargetSet = false, bool isLocked = false)
        {
            SkillPayload payload = new SkillPayload
            {
                Caster = this.character,
                SkillID = instance.SkillData != null ? instance.SkillData.Id : 0,
                SkillLevel = instance.CurrentLevel,
                SkillElement = instance.SkillData != null ? instance.SkillData.SkillElement : ElementType.Neutral,
                DamageType = instance.SkillData != null ? instance.SkillData.SkillDamageType : DamageType.Physical,
                OriginPosition = character.transform.position,
                TargetPosition = targetPos,
                IsTargetPositionSet = isTargetSet,
                IsPositionLocked = isLocked,
                
                BaseDamage = instance.SkillData != null ? instance.SkillData.BaseDamage : 0,
                AOERadius = instance.SkillData != null ? instance.SkillData.BaseAOERadius : 0,
                ProjectileCount = instance.SkillData != null ? instance.SkillData.BaseProjectileCount : 0,
                
                VCT = instance.SkillData != null ? instance.SkillData.BaseVCT : 0,
                FCT = instance.SkillData != null ? instance.SkillData.BaseFCT : 0,
                CastDelay = instance.SkillData != null ? instance.SkillData.BaseCastDelay : 0,
                Cooldown = instance.CalculateCooldownModified(this.character?.attributeSystem, instance.SkillData != null ? instance.SkillData.BaseCooldown : 0),
                ManaCost = instance.SkillData != null ? instance.SkillData.BaseManaCost : 0
            };

            if (instance.SkillData != null && instance.SkillData.Scalings != null && this.character != null && this.character.attributeSystem != null)
            {
                var stats = this.character.attributeSystem;
                for (int i = 0; i < instance.SkillData.Scalings.Count; i++)
                {
                    instance.SkillData.Scalings[i].ApplyScaling(ref payload, stats, instance.CurrentLevel);
                }
            }

            for (int i = 0; i < instance.LinkedModifiers.Count; i++)
            {
                if (instance.LinkedModifiers[i] != null)
                {
                    instance.LinkedModifiers[i].ModifyPayload(ref payload);
                }
            }

            return payload;
        }

        private void InitiateCastSequence(SkillInstance instance, Vector3 targetPos = default, bool isTargetSet = false)
        {
            if (instance == null || instance.SkillData == null || !instance.IsReady()) return;

            activeTargetPosition = targetPos;
            activeIsTargetSet = isTargetSet;
            activeIsPositionLocked = false;
            
            // Lock position for TargetLocationTargeting to prevent the target area from "following" the caster
            if (isTargetSet && instance.SkillData.BaseTargeting is TargetLocationTargeting tlt)
            {
                activeTargetPosition = tlt.ClampTargetPosition(character.transform.position, targetPos);
                activeIsPositionLocked = true;
            }

            SkillPayload payload = BuildPayload(instance, activeTargetPosition, activeIsTargetSet, activeIsPositionLocked);

            if (character != null && character.attributeSystem != null)
            {
                float currentMp = character.attributeSystem.GetAttribute(AttributeType.MP).CurrentValue;
                if (currentMp < payload.ManaCost) return;
            }

            activeSkillInstance = instance;
            currentCastTimer = 0f;
            currentCastTimeTotal = instance.CalculateCastTimeModified(character?.attributeSystem, payload.VCT, payload.FCT);

            OnCastInitiated?.Invoke(instance.SkillData.Name, currentCastTimeTotal);

            // Spawn Cast Visuals immediately if provided
            if (instance.SkillData.CastVisualPrefab != null)
            {
                Vector3 spawnPos = payload.TargetPosition;
                Transform parent = null;

                if (instance.SkillData.CastSpawnPoint == VisualAppearance.CasterPosition)
                {
                    spawnPos = character.transform.position;
                }
                else if (instance.SkillData.CastSpawnPoint == VisualAppearance.FollowCaster)
                {
                    spawnPos = character.transform.position;
                    parent = character.transform;
                }
                else if (!payload.IsTargetPositionSet)
                {
                    spawnPos = payload.OriginPosition;
                }

                GameObject castVfx = GameObject.Instantiate(instance.SkillData.CastVisualPrefab, spawnPos, Quaternion.identity, parent);
                SkillVisualEffect sve = castVfx.GetComponent<SkillVisualEffect>();
                if (sve != null)
                {
                    sve.Initialize(payload);
                }
            }

            if (currentCastTimeTotal <= 0f)
            {
                ExecuteSkill(instance);
            }
            else
            {
                CurrentState = SkillCasterState.Casting;
            }
        }

        public void InterruptCast()
        {
            if (CurrentState == SkillCasterState.Casting)
            {
                CurrentState = SkillCasterState.Idle;
                activeSkillInstance = null;
                OnCastInterrupted?.Invoke();
            }
        }

        private void ExecuteSkill(SkillInstance instance)
        {
            SkillPayload payload = BuildPayload(instance, activeTargetPosition, activeIsTargetSet, activeIsPositionLocked);
            instance.StartCooldown(payload.Cooldown);

            if (payload.CastDelay > 0f)
            {
                globalCastDelayTimer = payload.CastDelay;
                CurrentState = SkillCasterState.CastDelay;
            }
            else
            {
                CurrentState = SkillCasterState.Idle;
            }

            OnCastExecuted?.Invoke(instance.SkillData.Name);
            ExecutePipeline(instance, payload);
            activeSkillInstance = null;
        }

        private void ExecutePipeline(SkillInstance instance, SkillPayload payload = null)
        {
            if (payload == null) payload = BuildPayload(instance);

            if (this.character != null && this.character.attributeSystem != null && payload.ManaCost > 0f)
            {
                var mpAttr = this.character.attributeSystem.GetAttribute(AttributeType.MP);
                float newMp = Mathf.Max(0, mpAttr.CurrentValue - payload.ManaCost);
                mpAttr.BaseValue = newMp;
                mpAttr.Recalculate();
            }

            if (instance.SkillData.IsContinuousArea && instance.SkillData.BaseTargeting != null)
            {
                GameObject areaGo = new GameObject("ContinuousArea_" + instance.SkillData.Name);
                areaGo.transform.position = payload.IsTargetPositionSet ? payload.TargetPosition : payload.OriginPosition;
                
                var areaBehaviour = areaGo.AddComponent<ContinuousAreaBehaviour>();
                areaBehaviour.Initialize(payload, instance.SkillData.BaseTargeting, instance.SkillData.Effects, instance.SkillData.VisualData, instance.SkillData.AreaDuration, instance.SkillData.AreaTickRate);
                return;
            }

            if (instance.SkillData.BaseTargeting != null)
            {
                payload.Targets = instance.SkillData.BaseTargeting.GetTargets(payload);
                LastHitTargets.Clear();
                for (int i = 0; i < payload.Targets.Count; i++)
                {
                    if (payload.Targets[i] is MonoBehaviour mb)
                    {
                        LastHitTargets.Add(mb.gameObject);
                    }
                }
            }

            for (int i = 0; i < instance.SkillData.Effects.Count; i++)
            {
                EffectLogic effect = instance.SkillData.Effects[i];
                if (effect != null)
                {
                    if (effect.ApplyOncePerCast)
                    {
                        effect.Apply(payload, payload.Targets.Count > 0 ? payload.Targets[0] : null);
                    }
                    else
                    {
                        for (int j = 0; j < payload.Targets.Count; j++)
                        {
                            effect.Apply(payload, payload.Targets[j]);
                        }
                    }
                }
            }

            if (instance.SkillData.VisualData != null)
            {
                VisualManager.Instance?.PlayVisual(instance.SkillData.VisualData, payload);
            }

            if (instance.SkillData.VisualPrefab != null)
            {
                Vector3 spawnPos = payload.IsTargetPositionSet ? payload.TargetPosition : payload.OriginPosition;
                GameObject vfxInstance = GameObject.Instantiate(instance.SkillData.VisualPrefab, spawnPos, Quaternion.identity);
                
                SkillVisualEffect sve = vfxInstance.GetComponent<SkillVisualEffect>();
                if (sve != null)
                {
                    sve.Initialize(payload);
                }
            }
        }

#if UNITY_EDITOR
        public void DrawGizmosSelected()
        {
            if (character == null) return;

            for (int i = 0; i < EquippedSkills.Count; i++)
            {
                if (i != AimedSkillSlot) continue;
                SkillInstance instance = EquippedSkills[i];
                if (instance != null && instance.SkillData != null && instance.SkillData.BaseTargeting != null)
                {
                    // Calculate mouse position
                    Vector3 mousePos = Vector3.zero;
                    if (Camera.main != null)
                    {
                        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        mousePos.z = 0f;
                    }

                    // Use BuildPayload to ensure scalings (e.g. AOE Increase) are reflected in the gizmo
                    SkillPayload payload = BuildPayload(instance, mousePos, true);
                    
                    Vector2 direction = character.transform.up;
                    instance.SkillData.BaseTargeting.DrawGizmos(character.transform.position, direction, payload);
                }
            }
        }
#endif
    }
}
