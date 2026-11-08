using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Variables/Fields]
    private const float NavigatorWidth = 220f;
    private const float ListPaneWidth = 300f;
    private const float BannerHeight = 40f;
    private const float ListRowHeight = 40f;
    private const float TwoColumnMinWidth = 720f;

    private static readonly Dictionary<Color, Texture2D> _solidTextures = new Dictionary<Color, Texture2D>();
    private static GUIStyle _titleStyle;
    private static GUIStyle _bigTitleStyle;
    private static GUIStyle _mutedStyle;
    private static GUIStyle _mutedRightStyle;
    private static GUIStyle _navHeaderStyle;
    private static GUIStyle _navItemStyle;
    private static GUIStyle _navItemSelectedStyle;
    private static GUIStyle _chipStyle;
    private static GUIStyle _groupBodyStyle;
    private static GUIStyle _groupHeaderStyle;
    private static GUIStyle _letterStyle;
    private static GUIStyle _statValueStyle;
    private static GUIStyle _dangerButtonStyle;
    #endregion

    #region [Properties]
    private static bool IsDark => EditorGUIUtility.isProSkin;
    private static Color ThemeWindow => IsDark ? Hex(0x383838) : Hex(0xC8C8C8);
    private static Color ThemePanel => IsDark ? Hex(0x3C3C3C) : Hex(0xCBCBCB);
    private static Color ThemeBanner => IsDark ? Hex(0x3C3C3C) : Hex(0xCBCBCB);
    private static Color ThemeGroup => IsDark ? Hex(0x3C3C3C) : Hex(0xD6D6D6);
    private static Color ThemeGroupHeader => IsDark ? Hex(0x464646) : Hex(0xBDBDBD);
    private static Color ThemeField => IsDark ? Hex(0x2A2A2A) : Hex(0xF0F0F0);
    private static Color ThemeLine => IsDark ? Hex(0x232323) : Hex(0x999999);
    private static Color ThemeSelection => IsDark ? Hex(0x2C5D87) : Hex(0x3A72B0);
    private static Color ThemeAccent => IsDark ? Hex(0x4D9BE6) : Hex(0x2F6CB0);
    private static Color ThemePrimary => Hex(0x3A79BB);
    private static Color ThemeText => IsDark ? Hex(0xD2D2D2) : Hex(0x111111);
    private static Color ThemeMuted => IsDark ? Hex(0x999999) : Hex(0x555555);
    private static Color ThemeWarn => IsDark ? Hex(0xF4BC02) : Hex(0xA87800);
    private static Color ThemeError => IsDark ? Hex(0xFF534A) : Hex(0xC62828);
    private static Color ThemeOk => IsDark ? Hex(0x5BB85C) : Hex(0x2E7D32);

    private static GUIStyle TitleStyle => _titleStyle ??= new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };
    private static GUIStyle BigTitleStyle => _bigTitleStyle ??= new GUIStyle(EditorStyles.boldLabel) { fontSize = 20, fixedHeight = 26 };
    private static GUIStyle MutedStyle => _mutedStyle ??= new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = ThemeMuted } };
    private static GUIStyle MutedRightStyle => _mutedRightStyle ??= new GUIStyle(MutedStyle) { alignment = TextAnchor.MiddleRight };
    private static GUIStyle NavHeaderStyle => _navHeaderStyle ??= new GUIStyle(EditorStyles.miniBoldLabel) { normal = { textColor = ThemeMuted } };
    private static GUIStyle NavItemStyle => _navItemStyle ??= new GUIStyle(EditorStyles.label) { padding = new RectOffset(22, 8, 0, 0), alignment = TextAnchor.MiddleLeft };
    private static GUIStyle NavItemSelectedStyle => _navItemSelectedStyle ??= new GUIStyle(NavItemStyle) { fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
    private static GUIStyle ChipStyle => _chipStyle ??= new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, padding = new RectOffset(7, 7, 0, 0), fontStyle = FontStyle.Bold };
    private static GUIStyle GroupHeaderStyle => _groupHeaderStyle ??= new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft };
    private static GUIStyle LetterStyle => _letterStyle ??= new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
    private static GUIStyle DangerButtonStyle => _dangerButtonStyle ??= new GUIStyle(GUI.skin.button)
    {
        fontStyle = FontStyle.Bold,
        normal = { textColor = Color.white },
        hover = { textColor = Color.white },
        active = { textColor = Color.white },
        focused = { textColor = Color.white }
    };
    private static GUIStyle StatValueStyle => _statValueStyle ??= new GUIStyle(EditorStyles.boldLabel) { fontSize = 20, fixedHeight = 28 };

    private static GUIStyle GroupBodyStyle
    {
        get
        {
            if (_groupBodyStyle == null || _groupBodyStyle.normal.background == null)
            {
                _groupBodyStyle = new GUIStyle
                {
                    padding = new RectOffset(0, 0, 0, 10),
                    margin = new RectOffset(0, 0, 0, 12),
                    normal = { background = SolidTexture(ThemeGroup) }
                };
            }
            return _groupBodyStyle;
        }
    }
    #endregion

    #region [Private Methods]
    private static Color Hex(int rgb)
    {
        return new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
    }

    private static Texture2D SolidTexture(Color color)
    {
        if (_solidTextures.TryGetValue(color, out Texture2D tex) && tex != null) return tex;
        tex = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
        tex.SetPixel(0, 0, color);
        tex.Apply();
        _solidTextures[color] = tex;
        return tex;
    }

    private static void DrawBorder(Rect rect, Color color)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y, 1, rect.height), color);
    }

    private static float ChipWidth(string text)
    {
        return ChipStyle.CalcSize(new GUIContent(text)).x;
    }

    private static Rect DrawChip(Rect rect, string text, Color color, bool solid = false)
    {
        EditorGUI.DrawRect(rect, new Color(color.r, color.g, color.b, solid ? 1f : 0.18f));
        Color prev = ChipStyle.normal.textColor;
        ChipStyle.normal.textColor = solid ? Hex(0x1E1E1E) : color;
        GUI.Label(rect, text, ChipStyle);
        ChipStyle.normal.textColor = prev;
        return rect;
    }

    private static void ChipLayout(string text, Color color, bool solid = false)
    {
        Rect r = GUILayoutUtility.GetRect(ChipWidth(text), 18, GUILayout.Width(ChipWidth(text)), GUILayout.Height(18));
        DrawChip(r, text, color, solid);
    }

    private static void DrawIconSquare(Rect rect, Color color, string letter, Sprite sprite)
    {
        EditorGUI.DrawRect(rect, new Color(color.r, color.g, color.b, 0.22f));
        DrawBorder(rect, new Color(color.r, color.g, color.b, 0.6f));
        Texture2D tex = sprite != null ? (AssetPreview.GetAssetPreview(sprite) ?? sprite.texture) : null;
        if (tex != null && sprite != null)
        {
            Rect inner = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);
            Rect uv = sprite.textureRect;
            if (tex == sprite.texture)
                GUI.DrawTextureWithTexCoords(inner, tex, new Rect(uv.x / tex.width, uv.y / tex.height, uv.width / tex.width, uv.height / tex.height));
            else
                GUI.DrawTexture(inner, tex, ScaleMode.ScaleToFit);
            return;
        }
        Color prev = LetterStyle.normal.textColor;
        int prevSize = LetterStyle.fontSize;
        LetterStyle.normal.textColor = color;
        LetterStyle.fontSize = Mathf.RoundToInt(rect.height * 0.42f);
        GUI.Label(rect, string.IsNullOrEmpty(letter) ? "" : letter.Substring(0, Mathf.Min(2, letter.Length)), LetterStyle);
        LetterStyle.normal.textColor = prev;
        LetterStyle.fontSize = prevSize;
    }

    private static bool PrimaryButton(string label, params GUILayoutOption[] options)
    {
        Color prev = GUI.backgroundColor;
        GUI.backgroundColor = ThemePrimary * 1.6f;
        bool clicked = GUILayout.Button(label, options);
        GUI.backgroundColor = prev;
        return clicked;
    }

    private static bool DangerButton(string label, params GUILayoutOption[] options)
    {
        Color prev = GUI.backgroundColor;
        GUI.backgroundColor = Hex(0xD9342B) * 2.2f;
        bool clicked = GUILayout.Button(label, DangerButtonStyle, options);
        GUI.backgroundColor = prev;
        return clicked;
    }

    private static void DrawStatusLine(string message, MessageType type)
    {
        string icon = type == MessageType.Error ? "console.erroricon.sml" : type == MessageType.Warning ? "console.warnicon.sml" : "console.infoicon.sml";
        Color tint = type == MessageType.Error ? ThemeError : type == MessageType.Warning ? ThemeWarn : ThemeOk;
        Rect r = EditorGUILayout.GetControlRect(false, 22);
        EditorGUI.DrawRect(r, new Color(tint.r, tint.g, tint.b, 0.12f));
        GUI.Label(new Rect(r.x + 4, r.y + 3, 16, 16), EditorGUIUtility.IconContent(icon));
        GUI.Label(new Rect(r.x + 24, r.y, r.width - 28, r.height), message);
    }
    #endregion
}
