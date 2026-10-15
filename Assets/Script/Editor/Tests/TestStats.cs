using CoreSystem;
using CoreSystem.Attributes;

namespace CoreSystem.Tests
{
    public static class TestStats
    {
        #region [Public Methods]
        public static AttributeSystem Create(float str = 1f, float agi = 1f, float vit = 1f, float @int = 1f, float dex = 1f, float luk = 1f)
        {
            var stats = new AttributeSystem();
            stats.Initialize(null);
            SetBase(stats, AttributeType.STR, str);
            SetBase(stats, AttributeType.AGI, agi);
            SetBase(stats, AttributeType.VIT, vit);
            SetBase(stats, AttributeType.INT, @int);
            SetBase(stats, AttributeType.DEX, dex);
            SetBase(stats, AttributeType.LUK, luk);
            stats.UpdateDerivedAttributes();
            return stats;
        }

        public static void SetBase(AttributeSystem stats, AttributeType type, float value)
        {
            var attribute = stats.GetAttribute(type);
            attribute.BaseValue = value;
            attribute.Recalculate();
        }
        #endregion
    }
}
