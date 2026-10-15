using CoreSystem.Entity;

namespace CoreSystem.Combat
{
    public static class CombatCore
    {
        #region [Variables/Fields]
        public const int MaxDepth = 2;
        #endregion

        #region [Public Methods]
        public static ActionResult Resolve(CombatAction action, IEntity user, IEntity target, ICombatRandom random = null)
        {
            return Resolve(action, user, target, random ?? UnityCombatRandom.Instance, 0);
        }

        public static ActionResult Execute(CombatAction action, IEntity user, IEntity target, ICombatRandom random = null)
        {
            ActionResult result = Resolve(action, user, target, random);
            OutcomeApplier.Apply(result);
            return result;
        }
        #endregion

        #region [Private Methods]
        internal static ActionResult Resolve(CombatAction action, IEntity user, IEntity target, ICombatRandom random, int depth)
        {
            var result = new ActionResult(action, user, target, depth);
            if (action == null || action.Payload == null)
            {
                result.BlockReason = BlockReason.InvalidAction;
                return result;
            }

            result.BlockReason = Gate.CheckAction(target);
            if (result.IsBlocked) return result;

            var context = new ResolveContext(action, user, target, random, depth, result);
            var effects = action.Effects;
            for (int i = 0; i < effects.Count; i++)
            {
                result.AddOutcome(ResolveEffect(effects[i], context));
            }

            RiderStage.Run(context);
            return result;
        }

        private static Outcome ResolveEffect(EffectSpec spec, ResolveContext context)
        {
            if (spec == null) return null;

            IEffectHandler handler = EffectHandlers.For(spec.Kind);
            if (handler == null) return new Outcome(spec, context.User, context.Target, BlockReason.NoHandler);

            BlockReason reason = Gate.CheckEffect(handler.TargetRule, context.Action, context.User, context.Target);
            if (reason != BlockReason.None) return new Outcome(spec, context.User, context.Target, reason);

            return handler.Resolve(spec, context);
        }
        #endregion
    }
}
