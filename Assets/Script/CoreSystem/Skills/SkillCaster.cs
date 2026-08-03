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
                SyncHotbarSlot(slotIndex, null);
                return;
            }

            var grantedInstance = GrantedSkills.Find(s => s.SkillData != null && s.SkillData.Id == skillData.Id);
            if (grantedInstance != null)
            {
                EquippedSkills[slotIndex] = grantedInstance;
            }
            else
            {
                EquippedSkills[slotIndex] = new SkillInstance(skillData);
            }

            SyncHotbarSlot(slotIndex, EquippedSkills[slotIndex]);
        }

        public void EquipSkill(SkillInstance skillInstance, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= EquippedSkills.Count) return;
            EquippedSkills[slotIndex] = skillInstance;
            SyncHotbarSlot(slotIndex, skillInstance);
        }

        private bool _isSyncingHotbar = false;

        private void SyncHotbarSlot(int slotIndex, SkillInstance instance)
        {
            if (_isSyncingHotbar) return;
            _isSyncingHotbar = true;

            try
            {
                if (CoreSystem.UI.Hotbar.HotbarManager.Instance != null && CoreSystem.UI.Hotbar.HotbarManager.Instance.ActiveRows.Count > 0)
                {
                    var row0 = CoreSystem.UI.Hotbar.HotbarManager.Instance.ActiveRows[0];
                    var slot = row0.GetSlot(slotIndex);
                    if (slot != null)
                    {
                        if (instance != null && instance.SkillData != null)
                        {
                            slot.AssignSkill(instance.SkillData.Id);
                        }
                        else
                        {
                            if (slot.SlotData != null && slot.SlotData.ActionType == CoreSystem.UI.Hotbar.HotbarActionType.Skill)
                            {
                                slot.ClearSlot();
                            }
                        }
                    }
                }
            }
            finally
            {
                _isSyncingHotbar = false;
            }
        }

        public List<SkillInstance> GrantedSkills = new List<SkillInstance>();

        [Header("Debug")]
        public List<GameObject> LastHitTargets = new List<GameObject>();
        
        public SkillCasterState CurrentState { get; private set; } = SkillCasterState.Idle;
        
        public SkillInstance ActiveSkillInstance => activeSkillInstance;
        private SkillInstance activeSkillInstance;
        private SkillPayload activePayload;

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
        public System.Action OnGrantedSkillsChanged;
        
        public int AimedSkillSlot = -1;
        public SkillInstance AimedSkillInstance = null;


        public Character CharacterRef => character;
        private Character character;

        public void Initialize(Character owner)
        {
            this.character = owner;

            if (EquippedSkills == null)
            {
                EquippedSkills = new List<SkillInstance>();
            }
            while (EquippedSkills.Count < 10)
            {
                EquippedSkills.Add(null);
            }

            if (character != null && character.DefaultSkillIds != null)
            {
                for (int i = 0; i < character.DefaultSkillIds.Count; i++)
                {
                    int skillId = character.DefaultSkillIds[i];
                    GrantSkill(skillId);

                    if (i < EquippedSkills.Count && EquippedSkills[i] == null)
                    {
                        var grantedInstance = GrantedSkills.Find(s => s.SkillData != null && s.SkillData.Id == skillId);
                        if (grantedInstance != null)
                        {
                            EquippedSkills[i] = grantedInstance;
                        }
                    }
                }
            }
        }

        public void Tick(float dt)
        {
            if (character != null && character.IsDead)
            {
                InterruptCast();
                for (int i = 0; i < EquippedSkills.Count; i++)
                {
                    var instance = EquippedSkills[i];
                    if (instance != null && instance.IsToggledOn)
                    {
                        instance.IsToggledOn = false;
                        if (instance.ActiveVisual != null)
                        {
                            GameObject.Destroy(instance.ActiveVisual);
                            instance.ActiveVisual = null;
                        }
                    }
                }
                for (int i = 0; i < GrantedSkills.Count; i++)
                {
                    var instance = GrantedSkills[i];
                    if (instance != null && instance.IsToggledOn)
                    {
                        instance.IsToggledOn = false;
                        if (instance.ActiveVisual != null)
                        {
                            GameObject.Destroy(instance.ActiveVisual);
                            instance.ActiveVisual = null;
                        }
                    }
                }
                return;
            }

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
                            float manaCost = payload.ManaCost;
                            float hpCost = 0f;
                            bool isHpCost = false;
                            if (character.UniquePassives != null)
                            {
                                character.UniquePassives.ModifyManaCost(ref manaCost, ref hpCost, ref isHpCost);
                            }

                            if (isHpCost)
                            {
                                float currentHp = character.attributeSystem.GetAttribute(AttributeType.HP).CurrentValue;
                                if (currentHp < hpCost)
                                {
                                    instance.IsToggledOn = false;
                                    OnCastInterrupted?.Invoke();
                                    continue;
                                }
                            }
                            else
                            {
                                float currentMp = character.attributeSystem.GetAttribute(AttributeType.MP).CurrentValue;
                                if (currentMp < manaCost)
                                {
                                    instance.IsToggledOn = false;
                                    OnCastInterrupted?.Invoke();
                                    continue;
                                }
                            }
                        }

                        ExecutePipeline(instance, payload);
                        instance.StartCooldown(payload.Cooldown);
                        OnCastExecuted?.Invoke(instance.SkillData.Name + " (Toggle)");
                    }
                }
                else if (instance != null && instance.ActiveVisual != null)
                {
                    // Clean up visual if toggle was turned off elsewhere
                    GameObject.Destroy(instance.ActiveVisual);
                    instance.ActiveVisual = null;
                }
            }
        }

        private void ProcessCastingState(float dt)
        {
            currentCastTimer += dt;
            OnCastProgress?.Invoke(currentCastTimer, currentCastTimeTotal);

            if (currentCastTimeTotal > 0f)
            {
                SkillIndicatorManager.Instance?.UpdateCastIndicatorProgress(currentCastTimer / currentCastTimeTotal);
            }

            if (currentCastTimer >= currentCastTimeTotal)
            {
                ExecuteSkill(activeSkillInstance);
            }
        }

        public void CastSkill(int slotIndex, Vector3 targetPos = default)
        {
            if (character != null && character.IsDead) return;
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
                        float manaCost = payload.ManaCost;
                        float hpCost = 0f;
                        bool isHpCost = false;
                        if (character.UniquePassives != null)
                        {
                            character.UniquePassives.ModifyManaCost(ref manaCost, ref hpCost, ref isHpCost);
                        }

                        if (isHpCost)
                        {
                            float currentHp = character.attributeSystem.GetAttribute(AttributeType.HP).CurrentValue;
                            if (currentHp < hpCost) return;
                        }
                        else
                        {
                            float currentMp = character.attributeSystem.GetAttribute(AttributeType.MP).CurrentValue;
                            if (currentMp < manaCost) return;
                        }
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
                    if (instance.ActiveVisual != null)
                    {
                        GameObject.Destroy(instance.ActiveVisual);
                        instance.ActiveVisual = null;
                    }
                }
                return;
            }
            
            InitiateCastSequence(instance, targetPos, targetPos != default);
        }

        public void CastSkill(SkillInstance instance, Vector3 targetPos = default, bool isTargetSet = false)
        {
            if (character != null && character.IsDead) return;
            if (CurrentState != SkillCasterState.Idle) return;
            if (instance == null || instance.SkillData == null) return;
            InitiateCastSequence(instance, targetPos, isTargetSet);
        }

        public void CastSkill(SkillInstance instance, SkillPayload payload)
        {
            if (character != null && character.IsDead) return;
            if (CurrentState != SkillCasterState.Idle) return;
            if (instance == null || payload == null) return;
            InitiateCastSequence(instance, payload);
        }

        public void CastSkillDirectly(ActiveSkill skillData, Vector3 targetPos = default, bool isTargetSet = false)
        {
            if (character != null && character.IsDead) return;
            if (skillData == null) return;
            SkillInstance instance = GrantedSkills.Find(s => s.SkillData == skillData) ?? new SkillInstance(skillData);
            if (CurrentState != SkillCasterState.Idle) return;
            InitiateCastSequence(instance, targetPos, isTargetSet);
        }

        public SkillInstance GrantSkill(ActiveSkill skillData)
        {
            if (skillData == null) return null;
            var existing = GrantedSkills.Find(s => s.SkillData != null && s.SkillData.Id == skillData.Id);
            if (existing != null) return existing;

            var newInstance = new SkillInstance(skillData);
            GrantedSkills.Add(newInstance);
            OnGrantedSkillsChanged?.Invoke();
            return newInstance;
        }

        public SkillInstance GrantSkill(int skillId)
        {
            var existing = GrantedSkills.Find(s => s.SkillData != null && s.SkillData.Id == skillId);
            if (existing != null) return existing;
            
            var skillData = CoreSystem.Data.Database.GetSynchronizedSkill(skillId);
            if (skillData != null)
            {
                var newInstance = new SkillInstance(skillData);
                GrantedSkills.Add(newInstance);
                OnGrantedSkillsChanged?.Invoke();
                return newInstance;
            }
            return null;
        }

        public void RevokeSkill(ActiveSkill skillData)
        {
            if (skillData == null) return;
            RevokeSkill(skillData.Id);
        }

        public void RevokeSkill(int skillId)
        {
            int removed = GrantedSkills.RemoveAll(s => s.SkillData != null && s.SkillData.Id == skillId);
            if (removed > 0)
            {
                OnGrantedSkillsChanged?.Invoke();
            }
        }

        public void RevokeAllSkills()
        {
            GrantedSkills.Clear();
            for (int i = 0; i < EquippedSkills.Count; i++)
            {
                EquippedSkills[i] = null;
            }
            OnGrantedSkillsChanged?.Invoke();
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
                SkillTargeting = instance.SkillData != null ? instance.SkillData.BaseTargeting : null,
                SkillEffects = instance.SkillData != null ? new List<Logic.EffectLogic>(instance.SkillData.Effects) : new List<Logic.EffectLogic>(),
                OriginPosition = character.transform.position,
                TargetPosition = targetPos,
                IsTargetPositionSet = isTargetSet,
                IsPositionLocked = isLocked,
                
                BaseDamage = instance.SkillData != null ? instance.SkillData.BaseDamage : 0,
                AOERadius = instance.CalculateAOERadiusModified(this.character?.attributeSystem, instance.SkillData != null ? instance.SkillData.BaseAOERadius : 0),
                CastRange = (instance.SkillData?.BaseTargeting is TargetLocationTargeting tlt) 
                    ? instance.CalculateAOERadiusModified(this.character?.attributeSystem, tlt.CastRange) 
                    : 0,
                ProjectileCount = instance.SkillData != null ? instance.SkillData.BaseProjectileCount : 0,
                SkillIndicatorPrefab = instance.SkillData != null ? instance.SkillData.SkillIndicatorPrefab : null,
                
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
            
            // Final calculation after all modifiers have potentially touched VCT/FCT
            payload.TotalCastTime = instance.CalculateCastTimeModified(this.character?.attributeSystem, payload.VCT, payload.FCT);

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
                float castRange = instance.CalculateAOERadiusModified(character?.attributeSystem, tlt.CastRange);
                activeTargetPosition = tlt.ClampTargetPosition(character.transform.position, targetPos, castRange);
                activeIsPositionLocked = true;
            }

            SkillPayload payload = BuildPayload(instance, activeTargetPosition, activeIsTargetSet, activeIsPositionLocked);
            InitiateCastSequence(instance, payload);
        }

        private void InitiateCastSequence(SkillInstance instance, SkillPayload payload)
        {
            if (character != null && character.attributeSystem != null)
            {
                float manaCost = payload.ManaCost;
                float hpCost = 0f;
                bool isHpCost = false;
                if (character.UniquePassives != null)
                {
                    character.UniquePassives.ModifyManaCost(ref manaCost, ref hpCost, ref isHpCost);
                }

                if (isHpCost)
                {
                    float currentHp = character.attributeSystem.GetAttribute(AttributeType.HP).CurrentValue;
                    if (currentHp < hpCost)
                    {
                        Debug.LogWarning($"Cast Failed: Not enough HP for {instance.SkillData.Name}! (Need {hpCost}, Have {currentHp})");
                        return;
                    }
                }
                else
                {
                    float currentMp = character.attributeSystem.GetAttribute(AttributeType.MP).CurrentValue;
                    if (currentMp < manaCost) 
                    {
                        Debug.LogWarning($"Cast Failed: Not enough MP for {instance.SkillData.Name}! (Need {manaCost}, Have {currentMp})");
                        return;
                    }
                }
            }

            activeSkillInstance = instance;
            activePayload = payload;
            activeTargetPosition = payload.TargetPosition;
            activeIsTargetSet = payload.IsTargetPositionSet;
            activeIsPositionLocked = payload.IsPositionLocked;
            
            currentCastTimer = 0f;
            currentCastTimeTotal = payload.TotalCastTime;

            Debug.Log($"<color=cyan>Cast Skill:</color> {instance.SkillData.Name} <color=yellow>Time: {currentCastTimeTotal:F2}s</color>");

            OnCastInitiated?.Invoke(instance.SkillData.Name, currentCastTimeTotal);

            SkillIndicatorManager.Instance?.StartCastIndicator(payload);

            if (instance.SkillData.CastVfxPrefab != null)
            {
                Vector3 spawnPos = payload.TargetPosition;
                Transform parent = null;

                if (instance.SkillData.CastVfxSpawnPoint == VisualAppearance.CasterPosition)
                {
                    spawnPos = character.transform.position;
                }
                else if (instance.SkillData.CastVfxSpawnPoint == VisualAppearance.FollowCaster)
                {
                    spawnPos = character.transform.position;
                    parent = character.transform;
                }
                else if (!payload.IsTargetPositionSet)
                {
                    spawnPos = payload.OriginPosition;
                }

                GameObject castVfx = GameObject.Instantiate(instance.SkillData.CastVfxPrefab, spawnPos, Quaternion.identity, parent);
                
                var visual = castVfx.GetComponent<ISkillVisual>();
                if (visual != null)
                {
                    visual.Initialize(payload);
                }
                else
                {
                    var visuals = castVfx.GetComponentsInChildren<ISkillVisual>();
                    foreach (var v in visuals)
                    {
                        v.Initialize(payload);
                    }
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
                SkillIndicatorManager.Instance?.StopCastIndicator();
                OnCastInterrupted?.Invoke();
            }
        }

        private void ExecuteSkill(SkillInstance instance)
        {
            SkillPayload payload = activePayload ?? BuildPayload(instance, activeTargetPosition, activeIsTargetSet, activeIsPositionLocked);
            
            if (payload != null && character != null)
            {
                payload.OriginPosition = character.transform.position;
            }

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
            SkillIndicatorManager.Instance?.StopCastIndicator();
            ExecutePipeline(instance, payload);
            activeSkillInstance = null;
            activePayload = null;
        }

        private void ExecutePipeline(SkillInstance instance, SkillPayload payload = null)
        {
            if (payload == null) payload = BuildPayload(instance);

            if (this.character != null && this.character.attributeSystem != null)
            {
                float manaCost = payload.ManaCost;
                float hpCost = 0f;
                bool isHpCost = false;
                if (this.character.UniquePassives != null)
                {
                    this.character.UniquePassives.ModifyManaCost(ref manaCost, ref hpCost, ref isHpCost);
                }

                if (isHpCost)
                {
                    if (hpCost > 0f)
                    {
                        var hpAttr = this.character.attributeSystem.GetAttribute(AttributeType.HP);
                        float newHp = Mathf.Max(1f, hpAttr.CurrentValue - hpCost);
                        hpAttr.BaseValue = newHp;
                        hpAttr.Recalculate();
                    }
                }
                else
                {
                    if (manaCost > 0f)
                    {
                        var mpAttr = this.character.attributeSystem.GetAttribute(AttributeType.MP);
                        float newMp = Mathf.Max(0f, mpAttr.CurrentValue - manaCost);
                        mpAttr.BaseValue = newMp;
                        mpAttr.Recalculate();
                    }
                }
            }

            // Delegate core logic to the skill asset (Polymorphism)
            if (instance.SkillData != null)
            {
                var cascadeMod = instance.LinkedModifiers.Find(m => m != null && m.ID == 1022);
                if (cascadeMod != null && instance.SkillData.BaseTargeting is TargetLocationTargeting)
                {
                    Vector3 casterPos = character.transform.position;
                    Vector3 targetPos = payload.TargetPosition;
                    
                    Vector3 direction = (targetPos - casterPos).normalized;
                    if (direction == Vector3.zero) direction = Vector3.up;
                    
                    Vector3 leftDir = new Vector3(-direction.y, direction.x, 0f);
                    Vector3 rightDir = new Vector3(direction.y, -direction.x, 0f);
                    
                    var mainPayload = payload.Clone();
                    mainPayload.BaseDamage *= 0.75f;
                    
                    var leftPayload = payload.Clone();
                    leftPayload.TargetPosition = targetPos + leftDir * 5f;
                    leftPayload.BaseDamage *= 0.50f;
                    
                    var rightPayload = payload.Clone();
                    rightPayload.TargetPosition = targetPos + rightDir * 5f;
                    rightPayload.BaseDamage *= 0.50f;
                    
                    instance.SkillData.Execute(leftPayload, instance, this);
                    instance.SkillData.Execute(mainPayload, instance, this);
                    instance.SkillData.Execute(rightPayload, instance, this);
                }
                else
                {
                    instance.SkillData.Execute(payload, instance, this);
                }

                var echoMod = instance.LinkedModifiers.Find(m => m != null && m.ID == 1021);
                if (echoMod != null)
                {
                    if (this.character != null && this.character.gameObject.activeInHierarchy)
                    {
                        this.character.StartCoroutine(ExecuteEchoCoroutine(instance, payload));
                    }
                }
            }
        }

        private System.Collections.IEnumerator ExecuteEchoCoroutine(SkillInstance instance, SkillPayload originalPayload)
        {
            yield return new WaitForSeconds(0.15f);
            if (instance != null && instance.SkillData != null && this.character != null && !this.character.IsDead)
            {
                var echoPayload = originalPayload.Clone();
                echoPayload.AOERadius *= 1.3f;
                echoPayload.BaseDamage *= 0.6f;

                var cascadeMod = instance.LinkedModifiers.Find(m => m != null && m.ID == 1022);
                if (cascadeMod != null && instance.SkillData.BaseTargeting is TargetLocationTargeting)
                {
                    Vector3 casterPos = character.transform.position;
                    Vector3 targetPos = echoPayload.TargetPosition;
                    
                    Vector3 direction = (targetPos - casterPos).normalized;
                    if (direction == Vector3.zero) direction = Vector3.up;
                    
                    Vector3 leftDir = new Vector3(-direction.y, direction.x, 0f);
                    Vector3 rightDir = new Vector3(direction.y, -direction.x, 0f);
                    
                    var mainPayload = echoPayload.Clone();
                    mainPayload.BaseDamage *= 0.75f;
                    
                    var leftPayload = echoPayload.Clone();
                    leftPayload.TargetPosition = targetPos + leftDir * 5f;
                    leftPayload.BaseDamage *= 0.50f;
                    
                    var rightPayload = echoPayload.Clone();
                    rightPayload.TargetPosition = targetPos + rightDir * 5f;
                    rightPayload.BaseDamage *= 0.50f;
                    
                    instance.SkillData.Execute(leftPayload, instance, this);
                    instance.SkillData.Execute(mainPayload, instance, this);
                    instance.SkillData.Execute(rightPayload, instance, this);
                }
                else
                {
                    instance.SkillData.Execute(echoPayload, instance, this);
                }
            }
        }

#if UNITY_EDITOR
        public void DrawGizmosSelected()
        {
            if (character == null) return;

            SkillInstance instance = AimedSkillInstance;
            if (instance == null && AimedSkillSlot >= 0 && AimedSkillSlot < EquippedSkills.Count)
            {
                instance = EquippedSkills[AimedSkillSlot];
            }

            if (instance != null && instance.SkillData != null && instance.SkillData.BaseTargeting != null)
            {
                Vector3 mousePos = Vector3.zero;
                if (Camera.main != null)
                {
                    Vector2 screenPos = UnityEngine.InputSystem.Mouse.current != null ? UnityEngine.InputSystem.Mouse.current.position.ReadValue() : Vector2.zero;
                    mousePos = Camera.main.ScreenToWorldPoint(screenPos);
                    mousePos.z = 0f;
                }

                SkillPayload payload = BuildPayload(instance, mousePos, true);
                Vector2 direction = character.transform.up;
                instance.SkillData.BaseTargeting.DrawGizmos(character.transform.position, direction, payload);
            }
        }
#endif
    }
}
