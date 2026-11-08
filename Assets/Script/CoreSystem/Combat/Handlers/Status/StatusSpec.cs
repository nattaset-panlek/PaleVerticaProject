using CoreSystem.Entity;

namespace CoreSystem.Combat
{
    public enum StatusKind
    {
        Stun,
        Freeze,
        Slow,
        Burn,
        Poison
    }

    public sealed class StatusSpec : EffectSpec
    {
        #region [Variables/Fields]
        public StatusKind Status;
        public float Duration;
        public float TickDamage;
        public float TickInterval = 0.5f;
        public float SlowPercent;
        public IEntity CreditTo;
        #endregion

        #region [Properties]
        public override EffectKind Kind => EffectKind.Status;
        #endregion
    }
}
