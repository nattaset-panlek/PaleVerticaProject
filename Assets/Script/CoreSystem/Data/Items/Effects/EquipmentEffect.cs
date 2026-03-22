using CoreSystem;
using CoreSystem.Entity;

namespace CoreSystem.Data.Items.Effects
{
    [System.Serializable]
    public abstract class EquipmentEffect
    {
        public abstract void Apply(Character player);
        public abstract void Remove(Character player);
        public virtual string GetDescription() => "";
        public override string ToString() => GetDescription();
    }
}
