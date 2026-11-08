using System.Collections.Generic;

namespace CoreSystem.Interaction
{
    public interface IInteractionProvider
    {
        #region [Public Methods]
        void Collect(InteractionContext context, List<InteractionOption> options);
        #endregion
    }
}
