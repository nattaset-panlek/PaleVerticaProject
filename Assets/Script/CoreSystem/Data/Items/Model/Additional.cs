using CoreSystem.Data.Items;

using CoreSystem;

namespace CoreSystem.Data.Items.Model
{
    public class Additional : Item
    {
        public override void OnUse()
        {
            // For other unspecified items
        }

        public override Item Clone()
        {
            return new Additional
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
