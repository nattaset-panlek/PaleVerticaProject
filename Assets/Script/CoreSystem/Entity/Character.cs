using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Data.Items;
using CoreSystem.Movement;
using CoreSystem.Skills;
using CoreSystem.Data.Items.Model;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace CoreSystem.Entity
{
    public enum AutoAttackType
    {
        Melee,
        Range
    }

    public abstract class Character : MonoBehaviour, IEntity
    {
        [Header("Character Identity")]
        [SerializeField] private int characterId;
        public int CharacterId => characterId;

        public CoreSystem.Data.CharacterModel GetCharacterData()
        {
            return CoreSystem.Data.Database.GetCharacter(characterId);
        }

        [SerializeField] private AttributeSystem _attributeSystem = new AttributeSystem();
        public AttributeSystem attributeSystem => _attributeSystem;

        public MovementSystem movementSystem = new MovementSystem();
        public SkillCaster skillCaster = new SkillCaster();
        public CommandDirector commandDirector = new CommandDirector();
        public ExperienceSystem expSystem = new ExperienceSystem();
        private CoreSystem.Entity.UniquePassives.UniquePassiveManager _uniquePassives = new CoreSystem.Entity.UniquePassives.UniquePassiveManager();
        public CoreSystem.Entity.UniquePassives.UniquePassiveManager UniquePassives => _uniquePassives;

        [Header("Default Skills")]
        public List<int> DefaultSkillIds = new List<int> { 1001 };

        public float BasicAttackCooldownTimer { get; protected set; }

        [Header("Auto Attack Settings")]
        [SerializeField] private AutoAttackType _autoAttackType = AutoAttackType.Melee;
        [SerializeField] private DamageType _autoAttackDamageType = DamageType.Physical;
        [SerializeField] private float _autoAttackRange = 1.8f;
        [SerializeField] private string _customProjectileAddress = "";

        public System.Func<AutoAttackType, AutoAttackType> OnResolveAutoAttackType;
        public System.Func<DamageType, DamageType> OnResolveAutoAttackDamageType;
        public System.Func<float, float> OnResolveAutoAttackRange;
        public System.Func<string, string> OnResolveProjectileAddress;

        public AutoAttackType AutoAttackType
        {
            get
            {
                AutoAttackType val = _autoAttackType;
                if (OnResolveAutoAttackType != null)
                {
                    val = OnResolveAutoAttackType(val);
                }
                return val;
            }
        }

        public float AutoAttackRange
        {
            get
            {
                float val = _autoAttackRange;
                if (OnResolveAutoAttackRange != null)
                {
                    val = OnResolveAutoAttackRange(val);
                }
                return val;
            }
        }

        public DamageType AutoAttackDamageType
        {
            get
            {
                DamageType val = _autoAttackDamageType;
                if (OnResolveAutoAttackDamageType != null)
                {
                    val = OnResolveAutoAttackDamageType(val);
                }
                return val;
            }
            set => _autoAttackDamageType = value;
        }

        public string CustomProjectileAddress
        {
            get => _customProjectileAddress;
            set => _customProjectileAddress = value;
        }

        public string GetProjectileAddress()
        {
            string address = "";
            if (OnResolveProjectileAddress != null)
            {
                address = OnResolveProjectileAddress(address);
            }
            if (!string.IsNullOrEmpty(address))
            {
                return address;
            }
            if (!string.IsNullOrEmpty(_customProjectileAddress))
            {
                return _customProjectileAddress;
            }
            if (AutoAttackDamageType == DamageType.Magic)
            {
                return "Assets/Addressable/Prefab/Effect/MagicProjectile.prefab";
            }
            return "Assets/Addressable/Prefab/Effect/PhysicalProjectile.prefab";
        }
        
        [Header("Equipment Slots")]

        public ItemInstance[] EquipmentSlots = new ItemInstance[8];
        public ItemInstance[] CostumeSlots = new ItemInstance[4];


        [ContextMenu("RecalculateStats")]
        public virtual void RecalculateStats()
        {
            // If this is a PlayerCharacter, apply Skill Tree bonuses
            // We use global SkillTreeManager for now
            var passiveStats = SkillTree.SkillTreeManager.Instance?.GetCurrentStats();
            if (passiveStats != null)
            {
                _attributeSystem.ApplyPassiveStats(passiveStats);
            }

            _attributeSystem.UpdateDerivedAttributes();
        }

        public void Equip(ItemInstance instance, int slotIndex)
        {
            if (instance == null || !(instance.Item is Equipment equip)) return;
            if (slotIndex < 0 || slotIndex >= EquipmentSlots.Length) return;

            if (EquipmentSlots[slotIndex] != null)
            {
                UnEquip(slotIndex);
            }

            EquipmentSlots[slotIndex] = instance;
            equip.OnEquip(this);
            RecalculateStats();
        }

        public void UnEquip(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= EquipmentSlots.Length) return;
            
            var instance = EquipmentSlots[slotIndex];
            if (instance != null && instance.Item is Equipment equip)
            {
                equip.OnUnequip(this);
                EquipmentSlots[slotIndex] = null;
                RecalculateStats();
            }
        }

        public void EquipCostume(ItemInstance instance)
        {
            if (instance == null || !(instance.Item is Costume costume)) return;
            int slotIndex = (int)costume.Slot;
            
            if (slotIndex < 0 || slotIndex >= CostumeSlots.Length) return;

            if (CostumeSlots[slotIndex] != null)
            {
                UnEquipCostume(slotIndex);
            }

            CostumeSlots[slotIndex] = instance;
            costume.OnEquip(this);
            OnCostumeChanged?.Invoke();
            RecalculateStats();
        }

        public void UnEquipCostume(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= CostumeSlots.Length) return;

            var instance = CostumeSlots[slotIndex];
            if (instance != null && instance.Item is Costume costume)
            {
                costume.OnUnequip(this);
                CostumeSlots[slotIndex] = null;
                OnCostumeChanged?.Invoke();
                RecalculateStats();
            }
        }

        public ItemInstance GetEquipmentInSlot(EquipmentSlot slot, bool secondSlot = false)
        {
            int index = GetSlotIndex(slot, secondSlot);
            if (index >= 0 && index < EquipmentSlots.Length)
                return EquipmentSlots[index];
            return null;
        }

        private int GetSlotIndex(EquipmentSlot slot, bool secondSlot)
        {
            switch (slot)
            {
                case EquipmentSlot.WeaponOneHandLeft:  return 0;
                case EquipmentSlot.WeaponTwoHand:      return 0;
                case EquipmentSlot.WeaponOneHandRight: return 7;
                case EquipmentSlot.Helm:               return 1;
                case EquipmentSlot.Armor:              return 2;
                case EquipmentSlot.Glove:              return 3;
                case EquipmentSlot.Talisman:           return 4;
                case EquipmentSlot.Accessory:          return secondSlot ? 6 : 5;
                case EquipmentSlot.AccessoryLeft:      return 5;
                case EquipmentSlot.AccessoryRight:     return 6;
                default: return -1;
            }
        }

        [Header("Character Demographics")]
        [SerializeField] private ElementType element = ElementType.Neutral;
        [SerializeField] private SizeType size = SizeType.Medium;
        [SerializeField] private RaceType race = RaceType.Formless;
        public FactionType faction = FactionType.Neutral;

        public ElementType Element => element;
        public SizeType Size => size;
        public RaceType Race => race;
        public FactionType Faction => faction;

        public float StunDuration { get; private set; }
        public bool IsStunned => StunDuration > 0;

        public bool IsDead
        {
            get
            {
                if (_attributeSystem == null) return false;
                var hpAttr = _attributeSystem.GetAttribute(AttributeType.HP);
                return hpAttr != null && hpAttr.CurrentValue <= 0;
            }
        }

        public void ApplyStun(float duration)
        {
            if (duration > StunDuration)
            {
                StunDuration = duration;
                // Interrupt casting if stunning
                skillCaster?.InterruptCast();
            }
        }

        public event System.Action<IEntity, float> OnHitTarget;
        public event System.Action<IEntity, float> OnTakeDamage;
        public event System.Action OnCostumeChanged;

        public void TriggerOnHit(IEntity target, float damage) => OnHitTarget?.Invoke(target, damage);
        public void TriggerOnTakeDamage(IEntity attacker, float damage) => OnTakeDamage?.Invoke(attacker, damage);

        public virtual void BasicAttack(IEntity target)
        {
            if (IsDead || target == null) return;
            if (BasicAttackCooldownTimer > 0) return;

            var damageEffect = new CoreSystem.Skills.Logic.DamageEffect
            {
                TargetAttribute = AttributeType.HP
            };

            float bonusDamage = 0f;
            if (attributeSystem != null)
            {
                if (_autoAttackDamageType == DamageType.Magic)
                {
                    bonusDamage = attributeSystem.GetAttribute(AttributeType.INT).CurrentValue;
                }
                else
                {
                    if (_autoAttackType == AutoAttackType.Melee)
                    {
                        bonusDamage = attributeSystem.GetAttribute(AttributeType.STR).CurrentValue;
                    }
                    else if (_autoAttackType == AutoAttackType.Range)
                    {
                        bonusDamage = attributeSystem.GetAttribute(AttributeType.DEX).CurrentValue;
                    }
                }
            }

            var payload = new CoreSystem.Skills.SkillPayload
            {
                Caster = this,
                SkillID = 0,
                SkillLevel = 1,
                SkillElement = ElementType.Neutral,
                DamageType = _autoAttackDamageType,
                OriginPosition = transform.position,
                TargetPosition = target.transform.position,
                IsTargetPositionSet = true,
                BaseDamage = bonusDamage
            };

            GameObject prefab = null;
            if (_autoAttackType == AutoAttackType.Range)
            {
                string path = GetProjectileAddress();
                try
                {
                    var handle = Addressables.LoadAssetAsync<GameObject>(path);
                    prefab = handle.WaitForCompletion();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[Character] Failed to load projectile from Addressables at path: {path}. Error: {ex.Message}");
                }
            }

            if (_autoAttackType == AutoAttackType.Range && prefab != null)
            {
                Vector3 dir = (target.transform.position - transform.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                GameObject projGo = Instantiate(prefab, transform.position, rotation);

                var proj = projGo.GetComponent<CoreSystem.Skills.Logic.ProjectileBehaviour>();
                if (proj == null) proj = projGo.AddComponent<CoreSystem.Skills.Logic.ProjectileBehaviour>();
                proj.Initialize(payload, damageEffect);
            }
            else
            {
                damageEffect.Apply(payload, target);
            }

            _uniquePassives.TriggerOnBasicAttack(target);

            int AttactSpeedMinPreHit = 1;
            int AttactSpeedMaxPreHit = 100;
            float aspd = attributeSystem.GetAttribute(AttributeType.ASPD).CurrentValue;
            float attackDelay = (200f - aspd) / 50f;
            float minDelay = 1f / AttactSpeedMaxPreHit;
            float maxDelay = 1f / AttactSpeedMinPreHit;
            attackDelay = Mathf.Clamp(attackDelay, minDelay, maxDelay);
            BasicAttackCooldownTimer = attackDelay;
        }

        public virtual void Awake()
        {
            Initialize();
        }

        public virtual void Initialize()
        {
            _uniquePassives.Initialize(this);
            _attributeSystem.Initialize(this);
            movementSystem.Initialize(this);
            skillCaster.Initialize(this);
            commandDirector.Initialize(this);
            expSystem.Initialize(this);
        }

        private float _regenTimer = 0f;
        private const float RegenInterval = 1f;

        protected virtual void Update()
        {
            if (StunDuration > 0)
            {
                StunDuration -= Time.deltaTime;
                // While stunned, we might still allow recovery but no more thinking/movement (handled in subclasses/MovementSystem)
            }

            if (BasicAttackCooldownTimer > 0)
            {
                BasicAttackCooldownTimer -= Time.deltaTime;
            }

            if (skillCaster != null)
            {
                // SkillCaster state is checked internally (e.g. can't cast while stunned if we implement that check)
                skillCaster.Tick(Time.deltaTime);
            }

            _uniquePassives.Tick(Time.deltaTime);
            HandleRecovery(Time.deltaTime);
        }

        private void HandleRecovery(float dt)
        {
            _regenTimer += dt;
            if (_regenTimer >= RegenInterval)
            {
                _regenTimer -= RegenInterval;
                if (_attributeSystem != null)
                {
                    var hpAttr = _attributeSystem.GetAttribute(AttributeType.HP);
                    var maxHpAttr = _attributeSystem.GetAttribute(AttributeType.MaxHP);
                    var hpRegen = _attributeSystem.GetAttribute(AttributeType.HPRecovery);

                    if (hpAttr != null && maxHpAttr != null && hpRegen != null)
                    {
                        if (hpAttr.CurrentValue > 0 && hpAttr.CurrentValue < maxHpAttr.CurrentValue && hpRegen.CurrentValue > 0)
                        {
                            hpAttr.BaseValue = Mathf.Min(maxHpAttr.CurrentValue, hpAttr.CurrentValue + hpRegen.CurrentValue);
                            hpAttr.Recalculate();
                        }
                    }

                    var mpAttr = _attributeSystem.GetAttribute(AttributeType.MP);
                    var maxMpAttr = _attributeSystem.GetAttribute(AttributeType.MaxMP);
                    var mpRegen = _attributeSystem.GetAttribute(AttributeType.MPRecovery);

                    if (mpAttr != null && maxMpAttr != null && mpRegen != null && hpAttr != null)
                    {
                        if (hpAttr.CurrentValue > 0 && mpAttr.CurrentValue < maxMpAttr.CurrentValue && mpRegen.CurrentValue > 0)
                        {
                            mpAttr.BaseValue = Mathf.Min(maxMpAttr.CurrentValue, mpAttr.CurrentValue + mpRegen.CurrentValue);
                            mpAttr.Recalculate();
                        }
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (skillCaster != null)
            {
                skillCaster.DrawGizmosSelected();
            }
        }
#endif
        
        public void ApplyImpulse(Vector3 direction, float distance, float duration)
        {
            if (_uniquePassives.HasPassive("Rock")) return;
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(PerformImpulse(direction, distance, duration));
            }
        }

        private System.Collections.IEnumerator PerformImpulse(Vector3 direction, float distance, float duration)
        {
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos + direction.normalized * distance;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Use smoothstep for better feeling knockback/dash
                float smoothT = t * t * (3f - 2f * t);
                transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
                yield return null;
            }

            transform.position = targetPos;
        }
    }
}
