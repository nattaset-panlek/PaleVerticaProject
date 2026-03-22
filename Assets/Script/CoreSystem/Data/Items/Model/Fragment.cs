using System.Collections.Generic;
using CoreSystem.Data.Items.Effects;
using CoreSystem.Data.Items;
using CoreSystem.Data;

using CoreSystem;

namespace CoreSystem.Data.Items.Model
{
    public class Fragment : Item
    {
        public FragmentType FragmentKind { get; set; }
        
        // Effects to apply when socketed into an Equipment
        public List<EquipmentEffect> Effects { get; set; } = new List<EquipmentEffect>();

        public override void OnUse()
        {
            // Fragment usage logic (e.g., open a socketing UI)
        }

        public override Item Clone()
        {
            return new Fragment
            {
                Id = this.Id,
                Code = this.Code,
                Name = this.Name,
                Description = this.Description,
                IsStackable = this.IsStackable,
                Type = this.Type,
                Rarity = this.Rarity,
                FragmentKind = this.FragmentKind,
                Effects = new List<EquipmentEffect>(this.Effects)
            };
        }
    }

    public enum FragmentType
    {
        BasicFragment, // Attribute bonuses
        Rune,          // Skills or Auto-casts
        Essence        // Complex conditional bonuses
    }
}
