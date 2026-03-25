using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using System;
using System.Collections.Generic;



namespace CoreSystem.Data
{
    public class PlayerData
    {
        public PlayerInventory Inventory { get; } = new PlayerInventory();
        public CharacterManager CharacterManager { get; } = new CharacterManager();
        public EquipmentManager EquipmentManager { get; } = new EquipmentManager();
        public CostumeManager CostumeManager { get; } = new CostumeManager();
        public SkillManager SkillManager { get; } = new SkillManager();
    }

    public class SkillManager 
    {
        private SkillCaster _skillCaster;

        public void Bind(SkillCaster skillCaster)
        {
            _skillCaster = skillCaster;
        }

        // Add skill-related manager methods here
        // e.g., Equipping skills to slots, removing skills, verifying cooldowns
    }

    public class EquipmentManager 
    {
        private Entity.Character _character;
        
        // Tracking active sets and applied bonuses
        private Dictionary<EquipmentSetData, int> _activeSets = new Dictionary<EquipmentSetData, int>();
        private Dictionary<EquipmentSetData, List<int>> _appliedSetBonusLevels = new Dictionary<EquipmentSetData, List<int>>();

        public Action<EquipmentInstance> OnEquipmentChanged;

        public void Bind(Entity.Character character)
        {
            _character = character;
        }

        /// <summary>
        /// Smart equip entry point — resolves the correct slot index given item rules,
        /// then calls Equip(slotIndex, instance).
        /// Call this from UI, inventory buttons, drag-drop etc.
        /// </summary>
        public void SmartEquip(ItemInstance instance)
        {
            if (instance == null || !(instance.Item is Equipment equipment)) return;

            int targetSlot = ResolveSmartSlot(equipment, -1);
            if (targetSlot >= 0)
                Equip(targetSlot, instance);
        }

        /// <summary>
        /// Resolve the best slot index for an equipment, given the slot the user dragged to
        /// (pass -1 if unknown / auto). Enforces all equipping rules.
        /// </summary>
        public int ResolveSmartSlot(Equipment equipment, int requestedSlot)
        {
            if (_character == null) return -1;

            switch (equipment.Slot)
            {
                // ─── Weapon: TwoHand ──────────────────────────────────────
                // ใส่ได้ทั้ง Slot 0 และ 7 แต่ต้องการแค่ 1 ชิ้น
                // ถ้า drag ไป Right (7) → ok, ถ้าไม่ระบุ → 0
                case EquipmentSlot.WeaponTwoHand:
                    return (requestedSlot == 7) ? 7 : 0;

                // ─── Weapon: OneHandLeft ──────────────────────────────────
                // ใส่ได้แค่ Left (0) เท่านั้น ถ้า drag ไป Right → redirect Left
                case EquipmentSlot.WeaponOneHandLeft:
                    return 0;

                // ─── Weapon: OneHandRight ─────────────────────────────────
                // ใส่ได้แค่ Right (7) เท่านั้น ถ้า drag ไป Left → redirect Right
                case EquipmentSlot.WeaponOneHandRight:
                    return 7;

                // ─── Accessory (ใส่ได้ทั้ง Left และ Right) ───────────────
                // หา slot ว่างก่อน ถ้าเต็มทั้งคู่ → แทนที่ slot ที่ requested หรือ slot 5
                case EquipmentSlot.Accessory:
                {
                    bool slot5Empty = _character.EquipmentSlots[5] == null;
                    bool slot6Empty = _character.EquipmentSlots[6] == null;

                    if (requestedSlot == 6)
                        return slot6Empty ? 6 : (slot5Empty ? 5 : 6);
                    else
                        return slot5Empty ? 5 : (slot6Empty ? 6 : 5);
                }

                // ─── AccessoryLeft → เฉพาะ slot 5 เท่านั้น ──────────────
                case EquipmentSlot.AccessoryLeft:
                    return 5;

                // ─── AccessoryRight → เฉพาะ slot 6 เท่านั้น ─────────────
                case EquipmentSlot.AccessoryRight:
                    return 6;

                // ─── ทุกประเภทอื่น → ใช้ default slot ─────────────────────
                default:
                    return requestedSlot >= 0 ? requestedSlot : Database.GetDefaultSlotForType(equipment.Slot);
            }
        }

        public void Equip(int slotIndex, ItemInstance instance)
        {
            if (_character == null || slotIndex < 0 || slotIndex >= _character.EquipmentSlots.Length) return;

            // Check if this specific instance is already equipped in another slot (uniqueness check)
            // This prevents duplication when dragging an item from one slot to another (e.g. Accessory L -> R)
            if (instance != null)
            {
                for (int i = 0; i < _character.EquipmentSlots.Length; i++)
                {
                    if (i != slotIndex && _character.EquipmentSlots[i] == instance)
                    {
                        UnequipInternal(i);
                    }
                }
            }

            // ── TwoHand: clear อีก weapon slot ────────────────────────────
            if (instance != null && instance.Item is Equipment newEquip)
            {
                if (newEquip.Slot == EquipmentSlot.WeaponTwoHand)
                {
                    // clear slot ตรงข้าม (0 ↔ 7)
                    int otherWeaponSlot = (slotIndex == 0) ? 7 : 0;
                    if (_character.EquipmentSlots[otherWeaponSlot] != null)
                        UnequipInternal(otherWeaponSlot);
                }
                // ── OneHand เข้าช่อง TwoHand ที่มีอยู่: clear TwoHand เดิมออกทั้งคู่
                else if (newEquip.Slot == EquipmentSlot.WeaponOneHandLeft ||
                         newEquip.Slot == EquipmentSlot.WeaponOneHandRight)
                {
                    // ถ้าใน slot ปลายทางมี TwoHand อยู่ → ต้อง clear อีก slot ด้วย
                    var existingInTarget = _character.EquipmentSlots[slotIndex] as EquipmentInstance;
                    if (existingInTarget?.Item is Equipment existingEquip &&
                        existingEquip.Slot == EquipmentSlot.WeaponTwoHand)
                    {
                        int otherSlot = (slotIndex == 0) ? 7 : 0;
                        if (_character.EquipmentSlots[otherSlot] != null)
                            UnequipInternal(otherSlot);
                    }
                }
            }

            EquipmentInstance equipmentInstance = null;

            // Unequip old item in target slot
            var oldInstance = _character.EquipmentSlots[slotIndex];
            if (oldInstance != null && oldInstance.Item is Equipment oldEquip)
            {
                if (oldInstance is EquipmentInstance oldEq)
                {
                    oldEq.IsEquipped = false;
                    equipmentInstance = oldEq;
                }
                oldEquip.OnUnequip(_character, oldInstance as EquipmentInstance);
                UpdateSetBonus(oldEquip.SetResource, -1);
            }

            // Equip new item
            _character.EquipmentSlots[slotIndex] = instance;
            if (instance != null && instance.Item is Equipment equipment)
            {
                if (instance is EquipmentInstance newEq)
                {
                    newEq.IsEquipped = true;
                    equipmentInstance = newEq;
                }
                equipment.OnEquip(_character, equipmentInstance);
                UpdateSetBonus(equipment.SetResource, 1);
            }

            OnEquipmentChanged?.Invoke(equipmentInstance);
        }

        /// <summary>Internal unequip without raising event (used for TwoHand cleanup).</summary>
        private void UnequipInternal(int slotIndex)
        {
            var old = _character.EquipmentSlots[slotIndex];
            if (old == null) return;
            if (old.Item is Equipment oldEquip)
            {
                if (old is EquipmentInstance oldEq) oldEq.IsEquipped = false;
                oldEquip.OnUnequip(_character, old as EquipmentInstance);
                UpdateSetBonus(oldEquip.SetResource, -1);
            }
            _character.EquipmentSlots[slotIndex] = null;
            OnEquipmentChanged?.Invoke(null);
        }

        public EquipmentInstance GetEquipment(EquipmentSlot slot)
        {
            if (_character == null) return null;
            int slotIndex = Database.GetDefaultSlotForType(slot);
            if (slotIndex < 0 || slotIndex >= _character.EquipmentSlots.Length) return null;
            return _character.EquipmentSlots[slotIndex] as EquipmentInstance;
        }

        public void SocketFragment(EquipmentInstance target, ItemInstance fragmentInstance)
        {
            if (target == null || fragmentInstance == null) return;
            if (!(fragmentInstance.Item is Fragment fragment)) return;
            if (!(target.Item is Equipment equipment)) return;

            if (target.SocketedFragments.Count >= equipment.MaxSockets)
            {
                UnityEngine.Debug.LogWarning("Equipent has no more free sockets.");
                return;
            }

            // Remove from inventory
            GameData.Player.Inventory.RemoveItemByInstance(fragmentInstance.Id, 1);

            // Add to equipment
            target.SocketedFragments.Add(fragment);

            // If equipped, apply effects immediately
            if (target.IsEquipped && _character != null)
            {
                foreach (var effect in fragment.Effects)
                {
                    effect.Apply(_character);
                }
            }

            OnEquipmentChanged?.Invoke(target);
        }

        private void UpdateSetBonus(EquipmentSetData setData, int direction)
        {
            if (setData == null || _character == null) return;

            if (!_activeSets.ContainsKey(setData)) _activeSets[setData] = 0;
            if (!_appliedSetBonusLevels.ContainsKey(setData)) _appliedSetBonusLevels[setData] = new List<int>();

            _activeSets[setData] += direction;
            int currentCount = _activeSets[setData];

            // Evaluate all bonus levels
            foreach (var level in setData.BonusLevels)
            {
                bool isApplied = _appliedSetBonusLevels[setData].Contains(level.RequiredPieces);

                if (currentCount >= level.RequiredPieces && !isApplied)
                {
                    // Apply newly met threshold
                    foreach (var effect in level.BonusEffects) effect.Apply(_character);
                    _appliedSetBonusLevels[setData].Add(level.RequiredPieces);
                }
                else if (currentCount < level.RequiredPieces && isApplied)
                {
                    // Remove threshold that is no longer met
                    foreach (var effect in level.BonusEffects) effect.Remove(_character);
                    _appliedSetBonusLevels[setData].Remove(level.RequiredPieces);
                }
            }
        }
    }

    public class CostumeManager
    {
        private Entity.Character _character;
        public Action<CostumeInstance> OnCostumeChanged;

        public void Bind(Entity.Character character)
        {
            _character = character;
        }

        public void Equip(ItemInstance instance)
        {
            if (_character == null || instance == null || !(instance.Item is Costume costume)) return;
            
            if (instance is CostumeInstance costumeInstance)
            {
                costumeInstance.IsEquipped = true;
                _character.EquipCostume(instance);
                OnCostumeChanged?.Invoke(costumeInstance);
            }
        }

        public void Unequip(int slotIndex)
        {
            if (_character == null) return;
            
            var oldInstance = _character.CostumeSlots[slotIndex];
            if (oldInstance is CostumeInstance costumeInstance)
            {
                costumeInstance.IsEquipped = false;
                _character.UnEquipCostume(slotIndex);
                OnCostumeChanged?.Invoke(costumeInstance);
            }
        }
    }
}
