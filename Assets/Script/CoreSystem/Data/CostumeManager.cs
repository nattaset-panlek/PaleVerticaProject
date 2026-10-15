using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using System;
using System.Collections.Generic;



namespace CoreSystem.Data
{
    public class CostumeManager
    {
        private Entity.Character _character;
        public Action<CostumeInstance> OnCostumeChanged;

        public void Bind(Entity.Character character)
        {
            _character = character;
        }

        public CostumeInstance GetCostume(CostumeSlot slot)
        {
            if (_character == null) return null;
            int slotIdx = (int)slot;
            if (slotIdx < 0 || slotIdx >= _character.CostumeSlots.Length) return null;
            return _character.CostumeSlots[slotIdx] as CostumeInstance;
        }

        public void Equip(ItemInstance instance)
        {
            if (_character == null || instance == null || !(instance.Item is Costume costume)) return;
            
            // ยกเลิกสถานะสวมใส่ของชิ้นเก่าที่อยู่ใน Slot เดียวกัน
            int slotIndex = (int)costume.Slot;
            var oldInstance = _character.CostumeSlots[slotIndex] as CostumeInstance;
            if (oldInstance != null)
            {
                oldInstance.IsEquipped = false;
                // ถ้ามี Event แจ้งอัพเดตเฉพาะชิ้นสามารถเรียกตรงนี้ได้ ตอนนี้เปลี่ยน flag ก็พอ
            }

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
