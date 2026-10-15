using System;
using UnityEngine.Scripting;

namespace CoreSystem.Entity.UniquePassives
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class UniquePassiveIdAttribute : PreserveAttribute
    {
        #region [Properties]
        public string Id { get; }
        #endregion

        #region [Public Methods]
        public UniquePassiveIdAttribute(string id)
        {
            Id = id;
        }
        #endregion
    }
}
