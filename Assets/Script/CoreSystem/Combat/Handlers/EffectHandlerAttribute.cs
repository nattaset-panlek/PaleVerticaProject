using System;
using UnityEngine.Scripting;

namespace CoreSystem.Combat
{
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
}
