using System.Collections.Generic;
using CoreSystem.Skills;
using CoreSystem.Data.Items;

using CoreSystem;

namespace CoreSystem.Data.Items.Model
{
    public class SkillRune : Item
    {
        public int LinkedSkillID { get; set; }
        
        // Active or Support rune type depending on implementation
        public bool IsSupportRune { get; set; }

        public override void OnUse()
        {
            // Equip or activate rune logic
        }

        public override Item Clone()
        {
            return new SkillRune
            {
                Id = this.Id,
                Code = this.Code,
                Name = this.Name,
                Description = this.Description,
                IsStackable = this.IsStackable,
                Type = this.Type,
                Rarity = this.Rarity,
                LinkedSkillID = this.LinkedSkillID,
                IsSupportRune = this.IsSupportRune
            };
        }
    }
}
