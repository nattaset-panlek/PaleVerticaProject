using System.Collections.Generic;
using CoreSystem.Skills;

namespace CoreSystem.Combat
{
    public abstract class EffectSpec
    {
        #region [Properties]
        public abstract EffectKind Kind { get; }
        #endregion
    }
}
