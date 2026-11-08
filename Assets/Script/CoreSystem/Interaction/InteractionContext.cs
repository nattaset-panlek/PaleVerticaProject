using CoreSystem.Entity;

namespace CoreSystem.Interaction
{
    public readonly struct InteractionContext
    {
        #region [Variables/Fields]
        public readonly Character Actor;
        public readonly Character Target;
        public readonly object Holder;
        #endregion

        #region [Properties]
        public bool IsSelf => Actor != null && Actor == Target;
        #endregion

        #region [Public Methods]
        public InteractionContext(Character actor, Character target, object holder)
        {
            Actor = actor;
            Target = target;
            Holder = holder;
        }
        #endregion
    }
}
