using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Data.Items.Effects;
using CoreSystem.Data.Items;
using CoreSystem.Attributes;
using UnityEngine;
using CoreSystem.Combat;

namespace CoreSystem.Data.Items.Model
{
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
