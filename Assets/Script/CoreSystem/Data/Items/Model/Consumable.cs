using CoreSystem.Data.Items;

using CoreSystem;
using CoreSystem.Combat;
using CoreSystem.Entity;
using CoreSystem.Player;
using CoreSystem.Skills;

namespace CoreSystem.Data.Items.Model
{
    public class Consumable : Item
    {
        public float HealHP;
        public float HealMP;
        public bool LastUseConsumed { get; private set; } = true;

        public override void OnUse()
        {
            var player = PlayerCore.GetPlayer();
            if (player == null) return;
            if (!player.UniquePassives.CanUseConsumable())
            {
                LastUseConsumed = false;
                return;
            }

            Restore(player, AttributeType.HP, HealHP);
            Restore(player, AttributeType.MP, HealMP);
            player.UniquePassives.TriggerOnConsumableUsed(this);
            LastUseConsumed = !player.UniquePassives.ShouldKeepConsumable();
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
                Rarity = this.Rarity,
                IconPath = this.IconPath,
                HealHP = this.HealHP,
                HealMP = this.HealMP
            };
        }

        private static void Restore(Character player, AttributeType attribute, float amount)
        {
            if (amount <= 0f) return;
            var spec = new HealSpec { TargetAttribute = attribute, BaseAmount = amount };
            CombatCore.Execute(new CombatAction(new SkillPayload { Caster = player }).WithEffect(spec), player, player);
        }
    }
}
