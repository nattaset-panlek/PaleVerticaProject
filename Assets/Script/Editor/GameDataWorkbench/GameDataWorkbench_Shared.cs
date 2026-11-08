using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Variables/Fields]
    private static Color SectionBg => EditorGUIUtility.isProSkin ? new Color(0.165f, 0.165f, 0.165f, 0.55f) : new Color(0.86f, 0.86f, 0.86f, 1f);
    private static GUIStyle _sectionStyle;
    private static GUIStyle SectionStyle 
    {
        get {
            if (_sectionStyle == null || _sectionStyle.normal.background == null) {
                _sectionStyle = new GUIStyle(EditorStyles.helpBox);
                _sectionStyle.padding = new RectOffset(10, 10, 10, 10);
                _sectionStyle.margin = new RectOffset(0, 0, 5, 5);
                Texture2D tex = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
                tex.SetPixel(0, 0, SectionBg);
                tex.Apply();
                _sectionStyle.normal.background = tex;
            }
            return _sectionStyle;
        }
    }

    private static GUIStyle _paddingStyle;
    private static GUIStyle PaddingStyle 
    {
        get {
            if (_paddingStyle == null) {
                _paddingStyle = new GUIStyle();
                _paddingStyle.padding = new RectOffset(15, 15, 10, 15);
            }
            return _paddingStyle;
        }
    }

    private static GUIStyle _foldoutHeaderStyle;
    private static GUIStyle FoldoutHeaderStyle
    {
        get {
            if (_foldoutHeaderStyle == null) {
                _foldoutHeaderStyle = new GUIStyle(EditorStyles.foldoutHeader);
                _foldoutHeaderStyle.fontStyle = FontStyle.Bold;
            }
            return _foldoutHeaderStyle;
        }
    }
    #endregion

    #region [Private Methods]
    private void DrawSectionHeader(string title)
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        EditorGUILayout.Space(2);
    }

    private void DrawSummaryCard(string title, string value, Color color)
    {
        EditorGUILayout.BeginVertical(SectionStyle, GUILayout.Width(180));
        GUI.color = color;
        GUILayout.Label(title, EditorStyles.miniBoldLabel);
        GUI.color = Color.white;
        GUILayout.Label(value, new GUIStyle(EditorStyles.largeLabel) { fontSize = 24, fontStyle = FontStyle.Bold });
        EditorGUILayout.EndVertical();
    }

    private void SwapItems<T>(List<T> list, int a, int b, int currentSelectedIndex, System.Action<int> onSetSelectedIndex)
    {
        if (a < 0 || a >= list.Count || b < 0 || b >= list.Count) return;
        T temp = list[a];
        list[a] = list[b];
        list[b] = temp;
        
        if (currentSelectedIndex == a) onSetSelectedIndex(b);
        else if (currentSelectedIndex == b) onSetSelectedIndex(a);
    }
    #endregion
}
