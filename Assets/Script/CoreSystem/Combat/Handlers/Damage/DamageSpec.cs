namespace CoreSystem.Combat
{
    public sealed class DamageSpec : EffectSpec
    {
        #region [Variables/Fields]
        public AttributeType TargetAttribute = AttributeType.HP;
        #endregion

        #region [Properties]
        public override EffectKind Kind => EffectKind.Damage;
        #endregion
    }
}
