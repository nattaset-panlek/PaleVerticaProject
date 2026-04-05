using CoreSystem;
using CoreSystem.Entity;

namespace CoreSystem.Data.Items.Effects
{
    [System.Serializable]
    public abstract class EquipmentEffect
    {
        public abstract void Apply(Character player, ItemInstance instance = null);
        public abstract void Remove(Character player, ItemInstance instance = null);
        public virtual string GetDescription() => "";
        public override string ToString() => GetDescription();
    }
}
