using System.Collections.Generic;
using CoreSystem.Data.Items.Effects;
using UnityEngine;

namespace CoreSystem.Data.Items.Model
{
    public class Costume : Item
    {
        /// <summary>The costume slot this item occupies (Head/Body/Arms/Legs).</summary>
        public CostumeSlot Slot { get; set; }

        /// <summary>Addressable keys for sprite layers. Loaded async by CharacterVisualController.</summary>
        public List<string> VisualSpriteKeys { get; set; } = new List<string>();

        public int LayerPriority { get; set; }

        public List<EquipmentEffect> Effects { get; set; } = new List<EquipmentEffect>();

        public EquipmentSetData SetResource { get; set; }

        /// <summary>Addressable-key-driven prefab attachments. No GameObject stored here.</summary>
        public List<CostumeVisualPrefabData> VisualPrefabData { get; set; } = new List<CostumeVisualPrefabData>();

        public void OnEquip(Entity.Character player)
        {
            foreach (var effect in Effects)
                effect.Apply(player);
        }

        public void OnUnequip(Entity.Character player)
        {
            foreach (var effect in Effects)
                effect.Remove(player);
        }

        public override void OnUse() { base.OnUse(); }

        public override Item Clone()
        {
            return new Costume
            {
                Id             = this.Id,
                Code           = this.Code,
                Name           = this.Name,
                Description    = this.Description,
                Type           = this.Type,
                IsStackable    = this.IsStackable,
                Rarity         = this.Rarity,
                IconPath       = this.IconPath,
                Slot           = this.Slot,
                VisualSpriteKeys = new List<string>(this.VisualSpriteKeys),
                LayerPriority  = this.LayerPriority,
                Effects        = new List<EquipmentEffect>(this.Effects),
                SetResource    = this.SetResource,
                VisualPrefabData = new List<CostumeVisualPrefabData>(this.VisualPrefabData)
            };
        }
    }
}
