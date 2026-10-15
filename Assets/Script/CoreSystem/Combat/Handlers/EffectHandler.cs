using System;
using UnityEngine.Scripting;

namespace CoreSystem.Combat
{
    public interface IEffectHandler
    {
        TargetRule TargetRule { get; }
        Outcome Resolve(EffectSpec spec, ResolveContext context);
        void Apply(Outcome outcome, ActionResult result);
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class EffectHandlerAttribute : PreserveAttribute
    {
        #region [Properties]
        public EffectKind Kind { get; }
        #endregion

        #region [Public Methods]
        public EffectHandlerAttribute(EffectKind kind)
        {
            Kind = kind;
        }
        #endregion
    }

    public abstract class EffectHandler<TSpec, TOutgoing> : IEffectHandler where TSpec : EffectSpec
    {
        #region [Properties]
        public abstract TargetRule TargetRule { get; }
        #endregion

        #region [Public Methods]
        public Outcome Resolve(EffectSpec spec, ResolveContext context)
        {
            var typedSpec = (TSpec)spec;
            TOutgoing outgoing = Offense(typedSpec, context);
            return Defense(typedSpec, outgoing, context);
        }

        public void Apply(Outcome outcome, ActionResult result)
        {
            Commit((TSpec)outcome.Spec, outcome, result);
        }
        #endregion

        #region [Private Methods]
        protected abstract TOutgoing Offense(TSpec spec, ResolveContext context);
        protected abstract Outcome Defense(TSpec spec, TOutgoing outgoing, ResolveContext context);
        protected abstract void Commit(TSpec spec, Outcome outcome, ActionResult result);
        #endregion
    }
}
