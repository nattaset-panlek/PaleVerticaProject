using UnityEngine;

namespace CoreSystem.UI.Loading
{
    public static class LoadingFormula
    {
        #region [Variables/Fields]
        private const float SceneLoadCeiling = 0.9f;
        #endregion

        #region [Public Methods]
        public static float NormalizeSceneProgress(float asyncProgress)
        {
            return Mathf.Clamp01(asyncProgress / SceneLoadCeiling);
        }

        public static float StepToward(float current, float target, float speed, float deltaTime)
        {
            return Mathf.MoveTowards(current, Mathf.Clamp01(target), Mathf.Max(0f, speed) * Mathf.Max(0f, deltaTime));
        }
        #endregion
    }
}
