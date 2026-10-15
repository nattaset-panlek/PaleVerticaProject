using System.Collections.Generic;
using UnityEngine;
using CoreSystem;
using CoreSystem.Data;
using CoreSystem.Data.Items.Model;
using CoreSystem.Data.Items.Effects;
using CoreSystem.Attributes;
using CoreSystem.Data.Skills;

namespace CoreSystem.Data.Items
{
    [System.Serializable]
    public class ItemEntry
    {
        public int ItemId;
        public string ItemType; // "Currency", "Consumable", "Equipment", etc.
        public string Name;
        public LocalizedString Description;
        public int MaxStack;

        /// <summary>Resources-relative path for the item icon, e.g. "Sprite/Item/UI_Helmet_Icon".
        /// Leave empty to use the UI_Null_Icon fallback.</summary>
        public string IconPath;

        // Specific fields
        public string EquipmentSlot; // e.g., "Weapon", "Helm"
        public string Rarity;
        public int SkillID;
        public string FragmentKind;
        public float BasePhysical;
        public float BaseMagic;
        public int MaxSockets;
        public List<EffectEntry> Effects;

        // Costume specific
        public string CostumeSlot;
        public int LayerPriority;
        /// <summary>Addressable keys for costume sprite layers (e.g. "Costume/Head_A_Sprite").</summary>
        public List<string> VisualSpritePaths;
        /// <summary>Addressable prefab entries for visual attachments.</summary>
        public List<CostumePrefabEntry> VisualPrefabEntries;

        public string SetID;
    }
}
