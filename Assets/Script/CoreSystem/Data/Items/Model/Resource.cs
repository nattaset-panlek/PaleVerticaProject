using CoreSystem.Data.Items;

using CoreSystem;

namespace CoreSystem.Data.Items.Model
{
    public class Resource : Item
    {
        public override void OnUse()
        {
            // Usually used in crafting, not directly usable
        }

        public override Item Clone()
        {
            return new Resource
            {
                Id = this.Id,
                Code = this.Code,
                Name = this.Name,
                Description = this.Description,
                IsStackable = this.IsStackable,
                Type = this.Type,
                Rarity = this.Rarity
            };
        }
    }
}
