using System;
using UnityEngine;

namespace CoreSystem.UI
{
    [Serializable]
    public class UIElementEntry
    {
        #region [Variables/Fields]
        public string StyleName;
        public UIElementType Type;
        public GameObject Prefab;

        [Tooltip("The path to the child object holding the actual component, used by UIBuilder to inject data/events.")]
        public string TargetComponentPath;

        [Tooltip("Minimum size of this style in builder layouts. 0 = no minimum on that axis. Code (MinWidth/Height) can raise it, never lower it.")]
        public Vector2 MinSize;

        [Tooltip("Locks the size of this style in builder layouts: it never grows or shrinks. 0 = not locked on that axis. Code (Width/Height/Fill) still wins.")]
        public Vector2 FixedSize;

        [Tooltip("Corner radius in pixels for builder elements of this style, applied through the Image's Pixels Per Unit Multiplier. 0 = keep the prefab's multiplier. Code (CornerRadius) still wins.")]
        [Min(0f)]
        public float CornerRadius;
        #endregion

        #region [Properties]
        public bool HasSizing => MinSize.x > 0f || MinSize.y > 0f || FixedSize.x > 0f || FixedSize.y > 0f;
        public bool HasRules => HasSizing || CornerRadius > 0f;
        #endregion
    }
}
