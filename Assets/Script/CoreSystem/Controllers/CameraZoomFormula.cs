using UnityEngine;

namespace CoreSystem.Controllers
{
    public static class CameraZoomFormula
    {
        #region [Variables/Fields]
        private const float ScrollDeadZone = 0.01f;
        private const float StepSize = 0.1f;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static bool HasScroll(float rawScroll)
        {
            return Mathf.Abs(rawScroll) > ScrollDeadZone;
        }

        public static float ScrollStep(float rawScroll, bool pointerOverUI)
        {
            if (pointerOverUI || !HasScroll(rawScroll)) return 0f;
            return Mathf.Sign(rawScroll) * StepSize;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
