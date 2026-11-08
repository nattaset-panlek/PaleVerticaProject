using System;
using CoreSystem.Enums;
using UnityEngine;

namespace CoreSystem.Visuals.WeaponVfx
{
    #region [Public Enums]
    public enum WeaponVfxAnchor
    {
        Target,
        Attacker
    }

    public enum WeaponVfxSelectMode
    {
        Cycle,
        Random
    }

    public enum WeaponVfxOverrideMode
    {
        Replace,
        Add
    }

    public enum WeaponVfxColorMode
    {
        Element,
        Fixed,
        Neutral
    }
    #endregion

    #region [Public Structs]
    [Serializable]
    public struct VfxColorRamp
    {
        public Color Shadow;
        public Color Mid;
        public Color Highlight;

        public static VfxColorRamp Neutral => new VfxColorRamp
        {
            Shadow = new Color(0.35f, 0.38f, 0.45f, 1f),
            Mid = Color.white,
            Highlight = Color.white
        };

        public static VfxColorRamp FromColor(Color color)
        {
            return new VfxColorRamp
            {
                Shadow = color * 0.35f,
                Mid = color,
                Highlight = Color.Lerp(color, Color.white, 0.75f)
            };
        }
    }

    [Serializable]
    public struct ElementVfxRamp
    {
        public ElementType Element;
        public VfxColorRamp Ramp;
    }
    #endregion
}
