using System.Collections.Generic;
using CoreSystem.Entity;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Visuals;
using UnityEngine;

namespace CoreSystem.Skills
{

    [System.Serializable]
    public class SkillCaster
    {
        public List<SkillInstance> EquippedSkills = new List<SkillInstance>(new SkillInstance[5]);
        
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

        
        // Skills granted from items/equipment that don't take up standard hotbar slots
        public List<SkillInstance> GrantedSkills = new List<SkillInstance>();

        [Header("Debug")]
        public List<GameObject> LastHitTargets = new List<GameObject>();
        
        // --- State Machine ---
        public SkillCasterState CurrentState { get; private set; } = SkillCasterState.Idle;
        
        // Context for current cast
        public SkillInstance ActiveSkillInstance => activeSkillInstance;
        private SkillInstance activeSkillInstance;

        public float CurrentCastTimeTotal => currentCastTimeTotal;
        private float currentCastTimeTotal;

        public float CurrentCastTimer => currentCastTimer;
        private float currentCastTimer;
        
        // Global delay blocking all skills
        public float GlobalCastDelayTimer => globalCastDelayTimer;
        private float globalCastDelayTimer;
        
        // --- Events / Callbacks ---
        public System.Action<string, float> OnCastInitiated; // SkillName, TotalCastTime
        public System.Action<float, float> OnCastProgress; // CurrentTime, TotalTime
        public System.Action<string> OnCastExecuted; // SkillName
        public System.Action OnCastInterrupted;
        
        private Character character;

        public void Initialize(Character owner)
        {
            this.character = owner;
        }

        public void Tick(float dt)
        {
            // Update individual skill cooldowns regardless of state
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

            // Handle Toggle Skills Execution
            UpdateToggleSkills();

            // Handle State Machine
            switch (CurrentState)
            {
                case SkillCasterState.Casting:
                    ProcessCastingState(dt);
                    break;
                case SkillCasterState.CastDelay:
                    ProcessCastDelayState(dt);
                    break;
                case SkillCasterState.Idle:
                default:
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
                        // Check Mana
                        if (character != null && character.attributeSystem != null)
                        {
                            float currentMp = character.attributeSystem.GetAttribute(AttributeType.MP).CurrentValue;
                            if (currentMp < instance.SkillData.BaseManaCost)
                            {
                                // Not enough mana, toggle off
                                instance.IsToggledOn = false;
                                OnCastInterrupted?.Invoke();
                                continue;
                            }
                        }

                        // Check if we are currently globally delayed or casting something else that blocks?
                        // Usually toggle skills run on their own aura loop.
                        ExecutePipeline(instance);
                        instance.StartCooldown();
                        OnCastExecuted?.Invoke(instance.SkillData.Name + " (Toggle)");
                    }
                }
            }
        }

        private void ProcessCastingState(float dt)
        {
            currentCastTimer += dt;
            
            // Notify UI of cast bar progress
            OnCastProgress?.Invoke(currentCastTimer, currentCastTimeTotal);

            if (currentCastTimer >= currentCastTimeTotal)
            {
                // Execution Phase
                ExecuteSkill(activeSkillInstance);
            }
        }

        private void ProcessCastDelayState(float dt)
        {
            globalCastDelayTimer -= dt;
            if (globalCastDelayTimer <= 0f)
            {
                CurrentState = SkillCasterState.Idle;
            }
        }

        public void CastSkill(int slotIndex)
        {
            if (CurrentState != SkillCasterState.Idle) 
                return; // Blocked by Cast Delay or already casting

            if (slotIndex < 0 || slotIndex >= EquippedSkills.Count)
                return;

            SkillInstance instance = EquippedSkills[slotIndex];
            if (instance == null || instance.SkillData == null) return;

            if (instance.SkillData.IsToggle)
            {
                instance.IsToggledOn = !instance.IsToggledOn;
                
                // Optional: Execute immediately when turned on if ready
                if (instance.IsToggledOn && instance.IsReady())
                {
                    ExecutePipeline(instance);
                    instance.StartCooldown();
                    OnCastExecuted?.Invoke(instance.SkillData.Name + " (Toggle ON)");
                }
                return;
            }

            InitiateCastSequence(instance);
        }

        public void CastSkillDirectly(ActiveSkill skillData)
        {
            if (skillData == null) return;
            
            // Look for existing instance in GrantedSkills to respect cooldowns
            SkillInstance instance = GrantedSkills.Find(s => s.SkillData == skillData);
            
            if (instance == null)
            {
                // If not found, create a temporary one (or we can just skip if auto-cast shouldn't use ungranted skills)
                instance = new SkillInstance(skillData);
            }

            if (CurrentState != SkillCasterState.Idle) 
                return;

            InitiateCastSequence(instance);
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

        public SkillPayload BuildPayload(SkillInstance instance)
        {
            SkillPayload payload = new SkillPayload
            {
                Caster = this.character,
                SkillID = instance.SkillData != null ? instance.SkillData.Id : 0,
                SkillLevel = instance.CurrentLevel,
                SkillElement = instance.SkillData != null ? instance.SkillData.SkillElement : ElementType.Neutral,
                DamageType = instance.SkillData != null ? instance.SkillData.SkillDamageType : DamageType.Physical,
                OriginPosition = character.transform.position,
                
                BaseDamage = instance.SkillData != null ? instance.SkillData.BaseDamage : 0,
                AOERadius = instance.SkillData != null ? instance.SkillData.BaseAOERadius : 0,
                ProjectileCount = instance.SkillData != null ? instance.SkillData.BaseProjectileCount : 0,
                
                VCT = instance.SkillData != null ? instance.SkillData.BaseVCT : 0,
                FCT = instance.SkillData != null ? instance.SkillData.BaseFCT : 0,
                CastDelay = instance.SkillData != null ? instance.SkillData.BaseCastDelay : 0,
                Cooldown = instance.SkillData != null ? instance.SkillData.BaseCooldown : 0,
                ManaCost = instance.SkillData != null ? instance.SkillData.BaseManaCost : 0
            };

            // 1. Apply Dynamic Output Scalings
            if (instance.SkillData != null && instance.SkillData.Scalings != null && this.character != null && this.character.attributeSystem != null)
            {
                var stats = this.character.attributeSystem;
                for (int i = 0; i < instance.SkillData.Scalings.Count; i++)
                {
                    instance.SkillData.Scalings[i].ApplyScaling(ref payload, stats, instance.CurrentLevel);
                }
            }

            // 2. Apply Support Modifiers
            for (int i = 0; i < instance.LinkedModifiers.Count; i++)
            {
                if (instance.LinkedModifiers[i] != null)
                {
                    instance.LinkedModifiers[i].ModifyPayload(ref payload);
                }
            }

            return payload;
        }

        private void InitiateCastSequence(SkillInstance instance)
        {
            if (instance == null || instance.SkillData == null || !instance.IsReady())
                return; // Cooldown not ready

            // Build payload early to get final ManaCost and CastTime
            SkillPayload payload = BuildPayload(instance);

            // Check Mana
            if (character != null && character.attributeSystem != null)
            {
                float currentMp = character.attributeSystem.GetAttribute(AttributeType.MP).CurrentValue;
                if (currentMp < payload.ManaCost)
                {
                    // Not enough mana
                    return; 
                }
            }

            // Initiation Phase
            activeSkillInstance = instance;
            currentCastTimer = 0f;
            
            // Calculate final cast time using the modified VCT/FCT from payload
            currentCastTimeTotal = instance.CalculateCastTimeModified(character?.attributeSystem, payload.VCT, payload.FCT);

            // Trigger UI Initiation
            OnCastInitiated?.Invoke(instance.SkillData.Name, currentCastTimeTotal);

            // Cast Time Phase
            if (currentCastTimeTotal <= 0f)
            {
                // Instant cast
                ExecuteSkill(instance);
            }
            else
            {
                // Begin casting bar
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
            // Rebuild payload to get latest targets/positions if needed, 
            // but for simple timing/mana we already have it.
            // However, ExecutePipeline rebuilds it for the full execution.
            SkillPayload payload = BuildPayload(instance);

            // 1. Trigger cooldowns & delays
            // Use modified values from payload
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

            // 2. Visual / UI cue for execution
            OnCastExecuted?.Invoke(instance.SkillData.Name);

            // 3. Process the actual effects
            ExecutePipeline(instance, payload);

            activeSkillInstance = null;
        }

        private void ExecutePipeline(SkillInstance instance, SkillPayload payload = null)
        {
            if (payload == null) payload = BuildPayload(instance);

            // 0. Consume Mana
            if (this.character != null && this.character.attributeSystem != null && payload.ManaCost > 0f)
            {
                var mpAttr = this.character.attributeSystem.GetAttribute(AttributeType.MP);
                float newMp = Mathf.Max(0, mpAttr.CurrentValue - payload.ManaCost);
                mpAttr.BaseValue = newMp;
                mpAttr.Recalculate();
            }

            // If it's a persistent continuous area, spawn a handler and exit early
            if (instance.SkillData.IsContinuousArea && instance.SkillData.BaseTargeting != null)
            {
                GameObject areaGo = new GameObject("ContinuousArea_" + instance.SkillData.Name);
                areaGo.transform.position = payload.OriginPosition;
                
                var areaBehaviour = areaGo.AddComponent<ContinuousAreaBehaviour>();
                areaBehaviour.Initialize(payload, instance.SkillData.BaseTargeting, instance.SkillData.Effects, instance.SkillData.VisualData, instance.SkillData.AreaDuration, instance.SkillData.AreaTickRate);
                
                return; // Stop normal execution; the area handles effects
            }

            // Normal single-hit execution
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
                    if (effect is CoreSystem.Skills.Logic.Effects.MovementEffectLogic mov && mov.ApplyOncePerCast)
                    {
                        // Apply only once (using the first target if available, else null)
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
        }

#if UNITY_EDITOR
        public void DrawGizmosSelected()
        {
            if (character == null) return;

            for (int i = 0; i < EquippedSkills.Count; i++)
            {
                SkillInstance instance = EquippedSkills[i];
                if (instance != null && instance.SkillData != null && instance.SkillData.BaseTargeting != null)
                {
                    SkillPayload mockPayload = new SkillPayload
                    {
                        Caster = this.character,
                        OriginPosition = character.transform.position,
                        AOERadius = instance.SkillData.BaseAOERadius
                    };
                    
                    Vector2 direction = character.transform.up; // Assuming front is UP in 2D
                    instance.SkillData.BaseTargeting.DrawGizmos(character.transform.position, direction, mockPayload);
                }
            }
        }
#endif
    }
}
