using System;
using UnityEngine;

namespace CoreSystem.UI
{
    [Serializable]
    public class UITheme
    {
        #region [Variables/Fields]
        [Header("Surfaces")]
        public Color Panel = new Color(0.94f, 0.95f, 0.97f, 1f);
        public Color PanelAlt = new Color(0.87f, 0.89f, 0.92f, 1f);
        public Color Divider = new Color(0f, 0f, 0f, 0.12f);

        [Header("Text")]
        public Color Text = new Color(0.2f, 0.2f, 0.22f, 1f);
        public Color TextMuted = new Color(0.45f, 0.47f, 0.52f, 1f);
        public Color TextOnAccent = Color.white;

        [Header("Accents")]
        public Color Primary = new Color(0.24f, 0.49f, 0.92f, 1f);
        public Color Secondary = new Color(0.86f, 0.87f, 0.90f, 1f);
        public Color Danger = new Color(0.82f, 0.26f, 0.26f, 1f);
        public Color Success = new Color(0.20f, 0.62f, 0.38f, 1f);
        public Color Warning = new Color(0.95f, 0.70f, 0.20f, 1f);
        public Color Selected = new Color(0.22f, 0.46f, 0.85f, 1f);

        [Header("Typography")]
        public float TitleSize = 28f;
        public float HeadingSize = 18f;
        public float BodySize = 14f;
        public float CaptionSize = 12f;

        [Header("Spacing")]
        public float Gap = 8f;
        public int Padding = 12;
        public float ControlHeight = 34f;
        #endregion

        #region [Properties]
        public static UITheme Default { get; } = new UITheme();
        #endregion

        #region [Public Methods]
        public Color GetVariantColor(UIVariant variant)
        {
            switch (variant)
            {
                case UIVariant.Primary: return Primary;
                case UIVariant.Danger: return Danger;
                case UIVariant.Success: return Success;
                case UIVariant.Warning: return Warning;
                case UIVariant.Selected: return Selected;
                case UIVariant.Ghost: return Color.clear;
                default: return Secondary;
            }
        }

        public Color GetVariantTextColor(UIVariant variant)
        {
            switch (variant)
            {
                case UIVariant.Default:
                case UIVariant.Secondary:
                case UIVariant.Ghost:
                    return Text;
                case UIVariant.Warning:
                    return new Color(0.15f, 0.12f, 0.05f, 1f);
                default:
                    return TextOnAccent;
            }
        }
        #endregion
    }
}
