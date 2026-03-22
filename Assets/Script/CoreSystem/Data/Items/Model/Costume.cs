using System.Collections.Generic;
using CoreSystem.Data.Items.Effects;
using UnityEngine;

namespace CoreSystem.Data.Items.Model
{
    public class Costume : Item
    {
        public CostumeSlot Slot { get; set; }
        

        public Sprite[] VisualSprites { get; set; }
        

        public int LayerPriority { get; set; }


        public List<EquipmentEffect> Effects { get; set; } = new List<EquipmentEffect>();


        public EquipmentSetData SetResource { get; set; }

        public void OnEquip(Entity.Character player)
        {
            foreach(var effect in Effects)
            {
                effect.Apply(player);
            }
        }

        public void OnUnequip(Entity.Character player)
        {
            foreach(var effect in Effects)
            {
                effect.Remove(player);
            }
        }

        public override void OnUse()
        {
            base.OnUse();
        }

        public override Item Clone()
        {
            return new Costume
            {
                Id = this.Id,
                Code = this.Code,
                Name = this.Name,
                Description = this.Description,
                Type = this.Type,
                IsStackable = this.IsStackable,
                Rarity = this.Rarity,
                IconPath = this.IconPath,
                
                Slot = this.Slot,
                VisualSprites = this.VisualSprites != null ? (Sprite[])this.VisualSprites.Clone() : null,
                LayerPriority = this.LayerPriority,
                Effects = new List<EquipmentEffect>(this.Effects),
                SetResource = this.SetResource
            };
        }
    }
}
