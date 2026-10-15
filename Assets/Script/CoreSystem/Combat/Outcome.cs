using System.Collections.Generic;
using CoreSystem.Entity;

namespace CoreSystem.Combat
{
    public sealed class Outcome
    {
        #region [Properties]
        public EffectSpec Spec { get; }
        public EffectKind Kind => Spec.Kind;
        public IEntity Source { get; }
        public IEntity Target { get; }
        public AttributeType Attribute { get; set; }
        public float Amount { get; set; }
        public bool IsCritical { get; set; }
        public BlockReason BlockReason { get; }
        public bool IsBlocked => BlockReason != BlockReason.None;
        #endregion

        #region [Public Methods]
        public Outcome(EffectSpec spec, IEntity source, IEntity target, BlockReason blockReason = BlockReason.None)
        {
            Spec = spec;
            Source = source;
            Target = target;
            BlockReason = blockReason;
        }
        #endregion
    }
}
