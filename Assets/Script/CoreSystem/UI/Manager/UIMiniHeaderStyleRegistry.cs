using UnityEngine;

namespace CoreSystem.UI
{
    public static class UIMiniHeaderStyleRegistry
    {
        #region [Variables/Fields]
        public const string ButtonPrimary = "MH Button Primary";
        public const string ButtonSecondary = "MH Button Secondary";
        public const string ButtonDanger = "MH Button Danger";
        public const string MenuRow = "MH Menu Row";
        public const string TabButton = "MH Tab Button";
        public const string Switch = "MH Switch Toggle";
        public const string Checkbox = "MH Checkbox";
        public const string Slider = "MH Slider";
        public const string Dropdown = "MH Dropdown";
        public const string Input = "MH Input Field";
        public const string Separator = "MH Separator";
        public const string FoldoutHeader = "MH Foldout Header";
        public const string KeyButton = "MH Key Button";

        public static Color Body = Hex(0x141313);
        public static Color Text = Hex(0xD9D9D9);
        public static Color Sub = Hex(0x8A8A8A);
        public static Color Disabled = Hex(0x555555);
        public static Color Accent = Hex(0x67E8F9);
        public static Color Field = Hex(0x0D0D0D);
        public static Color FieldLine = Hex(0x2E2E2E);
        public static Color ListBackground = Hex(0x0F0F0F);
        public static Color Placeholder = Hex(0x5A5A5A);
        public static Color Raised = Hex(0x181818);
        public static Color Inner = Hex(0x0E0E0E);
        public static Color InnerLine = Hex(0x222222);
        public static Color Track = Hex(0x2A2A2A);
        public static Color SwitchOn = Hex(0x2F6E78);
        public static Color BoxLine = Hex(0x4A4A4A);
        public static Color ScrollTrack = Hex(0x1E1E1E);
        public static Color ScrollThumb = Hex(0x4A4A4A);
        public static Color KeyCap = Hex(0x1E1E1E);
        public static Color KeyCapLine = Hex(0x4A4A4A);
        public static Color TooltipBackground = Hex(0x0B0B0B);
        public static Color SeparatorColor = new Color(1f, 1f, 1f, 0.08f);

        public static Color ButtonPrimaryTint = Color.white;
        public static Color ButtonPrimaryInk = Hex(0x676363);
        public static Color ButtonSecondaryTint = Hex(0x1E1E1E);
        public static Color ButtonSecondaryInk = Hex(0xC9C9C9);
        public static Color ButtonDangerTint = Hex(0x1E1E1E);
        public static Color ButtonDangerInk = Hex(0xC9C9C9);

        public static Vector2 ButtonSize = new Vector2(181f, 55f);
        public static float HeaderHeight = 33f;
        public static float MenuRowHeight = 30f;
        public static float TabHeight = 34f;
        public static Vector4 ContentInsets = new Vector4(14f, 14f, 45f, 14f);
        public static float ScrollbarWidth = 6f;
        public static float ScrollbarInset = 3f;
        public static float ScrollContentGap = 12f;
        public static int ScrollContentRightPadding => Mathf.CeilToInt(ScrollbarWidth + ScrollbarInset + ScrollContentGap);

        public static float SystemMenuWidth = 300f;
        public static Vector2 SettingsWindowSize = new Vector2(720f, 500f);
        public static Vector2 ConfirmWindowSize = new Vector2(440f, 200f);
        public static float SettingsRowHeight = 36f;
        public static float SettingsControlWidth = 240f;
        public static float FooterGap = 12f;
        public static float ConfirmButtonGap = 16f;
        public static Vector2 KeyButtonSize = new Vector2(110f, 28f);
        public static float KeybindSearchWidth = 260f;
        public static Color KeyCapturing = Hex(0x67E8F9);
        public static float DeathWindowWidth = 440f;
        public static Color DeathTitle = Hex(0xFF5A4E);
        public static float DeathTitleSpacing = 6f;
        #endregion

        #region [Public Methods]
        public static UIGlobalModal Prepare(UIGlobalModal modal, string title)
        {
            if (modal == null) return null;
            return modal.SetStyle(UIModalStyle.MiniHeader)
                        .SetFollowGlobalTheme(false)
                        .SetThemeMode(UIThemeMode.Dark)
                        .SetTitle(title)
                        .SetResizable(false);
        }

        public static Color Hex(int rgb)
        {
            return new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
        }
        #endregion
    }
}
