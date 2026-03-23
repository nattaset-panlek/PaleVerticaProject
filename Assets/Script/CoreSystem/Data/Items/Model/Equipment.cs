using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Data.Items.Effects;
using CoreSystem.Data.Items;
using CoreSystem.Attributes;
using UnityEngine;

namespace CoreSystem.Data.Items.Model
{
    public class Equipment : Item
    {
        public EquipmentSlot Slot { get; set; }
        public EquipmentSetData SetResource { get; set; }
        
        // Sockets
        public int MaxSockets { get; set; } = 0;

        public List<EquipmentEffect> Effects { get; set; } = new List<EquipmentEffect>();
        
        public float BasePhysical { get; set; } = 1f;
        public float BaseMagic { get; set; } = 1f;

        // Track system-level baseline modifiers (e.g. mandatory +1)
        private List<(AttributeType type, AttributeModifier mod)> _systemModifiers = new List<(AttributeType, AttributeModifier)>();

        public void OnEquip(Entity.Character player, EquipmentInstance instance = null)
        {
            ApplySystemBaselines(player);

            foreach(var effect in Effects)
            {
                effect.Apply(player);
            }
            
            if (instance != null)
            {
                foreach(var fragment in instance.SocketedFragments)
                {
                    if (fragment != null)
                    {
                        foreach(var effect in fragment.Effects) 
                        {
                            effect.Apply(player);
                        }
                    }
                }
            }
        }

        private void ApplySystemBaselines(Entity.Character player)
        {
            if (player == null || player.attributeSystem == null) return;

            bool isWeapon = Slot == EquipmentSlot.WeaponOneHandLeft || 
                            Slot == EquipmentSlot.WeaponOneHandRight || 
                            Slot == EquipmentSlot.WeaponTwoHand;

            // Adaptive mapping: 
            // Weapon -> WeaponATK/MATK
            // Armor/Gear -> Defense/StatusMDEF
            
            float phyValue = Mathf.Max(1f, BasePhysical);
            float magValue = Mathf.Max(1f, BaseMagic);

            if (isWeapon)
            {
                AddSystemModifier(player, AttributeType.WeaponATK, phyValue);
                AddSystemModifier(player, AttributeType.WeaponMATK, magValue);
            }
            else
            {
                AddSystemModifier(player, AttributeType.Defense, phyValue);
                AddSystemModifier(player, AttributeType.StatusDEF, magValue);
            }
        }

        private void AddSystemModifier(Entity.Character player, AttributeType type, float value)
        {
            var mod = new AttributeModifier(value, ModifierType.Flat);
            player.attributeSystem.AddModifier(type, mod);
            _systemModifiers.Add((type, mod));
        }

        public void OnUnequip(Entity.Character player, EquipmentInstance instance = null)
        {
            foreach(var effect in Effects)
            {
                effect.Remove(player);
            }

            if (instance != null)
            {
                foreach(var fragment in instance.SocketedFragments)
                {
                    if (fragment != null)
                    {
                        foreach(var effect in fragment.Effects) effect.Remove(player);
                    }
                }
            }

            // Remove system baselines
            foreach(var record in _systemModifiers)
            {
                player.attributeSystem.RemoveModifier(record.type, record.mod);
            }
            _systemModifiers.Clear();
        }

        public override void OnUse()
        {
            // Equip logic from inventory
        }

        public override Item Clone()
        {
            return new Equipment
            {
                Id = this.Id,
                Code = this.Code,
                Name = this.Name,
                Description = this.Description,
                IsStackable = this.IsStackable,
                Type = this.Type,
                Rarity = this.Rarity,
                Slot = this.Slot,
                SetResource = this.SetResource,
                MaxSockets = this.MaxSockets,
                Effects = new List<EquipmentEffect>(this.Effects),
                BasePhysical = this.BasePhysical,
                BaseMagic = this.BaseMagic
            };
        }
    }

    public enum EquipmentSlot
    {
        Helm,
        Armor,
        WeaponOneHandLeft,
        WeaponOneHandRight,
        WeaponTwoHand,
        Glove,
        Accessory,          // ไม่แยก Left/Right ใน data — Manager จะ assign slot อัตโนมัติ
        AccessoryLeft,      // Internal slot index helper (ไม่ใช้ใน JSON)
        AccessoryRight,     // Internal slot index helper (ไม่ใช้ใน JSON)
        Talisman
    }
}
