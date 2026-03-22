using CoreSystem.Data.Items;

using CoreSystem;

namespace CoreSystem.Data.Items.Model
{
    public class Consumable : Item
    {
        // Add specific consumable data here (e.g., Heal amount, buff effect ID)

        public override void OnUse()
        {
            // Apply consumable logic
            // TODO: Move decrement logic to ItemInstance / Inventory
        }

        public override Item Clone()
        {
            return new Consumable
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
