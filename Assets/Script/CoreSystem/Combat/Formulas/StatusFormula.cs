using UnityEngine;

namespace CoreSystem.Combat
{
    public static class StatusFormula
    {
        #region [Public Methods]
        public static float ExtendedDuration(float remaining, float added)
        {
            return Mathf.Max(0f, remaining) + Mathf.Max(0f, added);
        }
        #endregion
    }
}
