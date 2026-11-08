using System;
using UnityEngine;

namespace CoreSystem.UI.ActionProgress
{
    public class ActionProgressRequest
    {
        #region [Variables/Fields]
        public string ActionName = string.Empty;
        public string ActionType = string.Empty;
        public Sprite Icon;
        public float Duration;
        public ActionProgressColor Color = ActionProgressColor.Cyan;
        public bool UseUnscaledTime;
        public object Owner;
        public Action OnCompleted;
        public Action OnCancelled;
        #endregion

        #region [Properties]
        public bool IsTimed => Duration > 0f;
        public bool HasIcon => Icon != null;
        #endregion
    }
}
