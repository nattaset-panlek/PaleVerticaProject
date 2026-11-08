using UnityEngine;

namespace CoreSystem.UI.ActionProgress
{
    #region [Public Enums]
    public enum ActionProgressColor
    {
        Cyan,
        Magenta
    }
    #endregion

    #region [Public Structs]
    public static class ActionProgressPalette
    {
        public static readonly Color Cyan = new Color(0.133f, 0.827f, 0.933f, 1f);
        public static readonly Color Magenta = new Color(1f, 0f, 0.435f, 1f);

        public static Color Get(ActionProgressColor color) => color == ActionProgressColor.Magenta ? Magenta : Cyan;
    }
    #endregion
}
