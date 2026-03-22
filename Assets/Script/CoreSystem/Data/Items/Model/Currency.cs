using CoreSystem.Data.Items;

using CoreSystem;

namespace CoreSystem.Data.Items.Model
{
    public class Currency : Item
    {
        public override void OnUse()
        {
            // Currency is usually just a number, shouldn't be directly "used" from inventory in most cases
        }

        public override Item Clone()
        {
            return new Currency
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
