using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Data.Items;
using CoreSystem.Movement;
using CoreSystem.Skills;
using CoreSystem.Data.Items.Model;
using UnityEngine;

namespace CoreSystem.Entity
{
    public abstract class Character : MonoBehaviour, IEntity
    {
        [SerializeField] private AttributeSystem _attributeSystem = new AttributeSystem();
        public AttributeSystem attributeSystem => _attributeSystem;

        public MovementSystem movementSystem = new MovementSystem();
        public SkillCaster skillCaster = new SkillCaster();
        public ExperienceSystem expSystem = new ExperienceSystem();
        
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

        public virtual void Awake()
        {
            Initialize();
        }

        public virtual void Initialize()
        {
            _attributeSystem.Initialize(this);
            movementSystem.Initialize(this);
            skillCaster.Initialize(this);
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

            if (skillCaster != null)
            {
                // SkillCaster state is checked internally (e.g. can't cast while stunned if we implement that check)
                skillCaster.Tick(Time.deltaTime);
            }

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
                    // HP Recovery
                    var hpAttr = _attributeSystem.GetAttribute(AttributeType.HP);
                    var maxHpAttr = _attributeSystem.GetAttribute(AttributeType.MaxHP);
                    var hpRegen = _attributeSystem.GetAttribute(AttributeType.HPRecovery);

                    if (hpAttr != null && maxHpAttr != null && hpRegen != null)
                    {
                        if (hpAttr.CurrentValue < maxHpAttr.CurrentValue && hpRegen.CurrentValue > 0)
                        {
                            hpAttr.BaseValue = Mathf.Min(maxHpAttr.CurrentValue, hpAttr.CurrentValue + hpRegen.CurrentValue);
                            hpAttr.Recalculate();
                        }
                    }

                    // MP Recovery
                    var mpAttr = _attributeSystem.GetAttribute(AttributeType.MP);
                    var maxMpAttr = _attributeSystem.GetAttribute(AttributeType.MaxMP);
                    var mpRegen = _attributeSystem.GetAttribute(AttributeType.MPRecovery);

                    if (mpAttr != null && maxMpAttr != null && mpRegen != null)
                    {
                        if (mpAttr.CurrentValue < maxMpAttr.CurrentValue && mpRegen.CurrentValue > 0)
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
