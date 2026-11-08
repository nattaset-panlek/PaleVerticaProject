using UnityEngine;

namespace CoreSystem.Combat
{
    public static class ActiveSkillFormula
    {
        #region [Public Methods]
        public static float ChainJumpMultiplier(float decayPerJump, int jumpIndex)
        {
            if (jumpIndex <= 0) return 1f;
            return Mathf.Pow(Mathf.Clamp01(decayPerJump), jumpIndex);
        }
        #endregion
    }
}
