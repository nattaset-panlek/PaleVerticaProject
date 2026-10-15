using System;
using UnityEngine.Scripting;

namespace CoreSystem.Skills
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class SupportModifierIdAttribute : PreserveAttribute
    {
        #region [Properties]
        public int Id { get; }
        #endregion

        #region [Public Methods]
        public SupportModifierIdAttribute(int id)
        {
            Id = id;
        }
        #endregion
    }
}
