using CoreSystem.Entity;

namespace CoreSystem.Combat
{
    public sealed class ResolveContext
    {
        #region [Properties]
        public CombatAction Action { get; }
        public IEntity User { get; }
        public IEntity Target { get; }
        public ICombatRandom Random { get; }
        public int Depth { get; }
        public ActionResult Result { get; }
        public bool CanSpawn => Depth < CombatCore.MaxDepth;
        #endregion

        #region [Public Methods]
        public ResolveContext(CombatAction action, IEntity user, IEntity target, ICombatRandom random, int depth, ActionResult result)
        {
            Action = action;
            User = user;
            Target = target;
            Random = random;
            Depth = depth;
            Result = result;
        }

        public Outcome CreateOutcome(EffectSpec spec)
        {
            return new Outcome(spec, User, Target);
        }

        public bool Spawn(CombatAction action, IEntity user, IEntity target)
        {
            if (action == null || !CanSpawn) return false;

            action.IsProc = true;
            Result.AddSubResult(CombatCore.Resolve(action, user, target, Random, Depth + 1));
            return true;
        }
        #endregion
    }
}
