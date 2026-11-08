namespace CoreSystem.Combat
{
    public sealed class HealSpec : EffectSpec
    {
        #region [Variables/Fields]
        public AttributeType TargetAttribute = AttributeType.HP;
        public float BaseAmount;
        public float MatkScale;
        #endregion

        #region [Properties]
        public override EffectKind Kind => EffectKind.Heal;
        #endregion
    }
}
