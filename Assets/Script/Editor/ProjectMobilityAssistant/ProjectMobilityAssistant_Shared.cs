using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public partial class ProjectMobilityAssistant : EditorWindow
{
    private static readonly Color SectionBg = new Color(0.25f, 0.25f, 0.25f, 0.4f);
    private static GUIStyle _sectionStyle;
    private static GUIStyle SectionStyle 
    {
        get {
            if (_sectionStyle == null) {
                _sectionStyle = new GUIStyle(EditorStyles.helpBox);
                _sectionStyle.padding = new RectOffset(10, 10, 10, 10);
                _sectionStyle.margin = new RectOffset(0, 0, 5, 5);
                // Use a subtle background
                Texture2D tex = new Texture2D(1, 1);
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
                _paddingStyle.padding = new RectOffset(15, 15, 15, 15);
            }
            return _paddingStyle;
        }
    }

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

    // ── Generic List Editor Engine ──────────────────────────────────────────
    private void DrawEditor<T>(List<T> list, int selectedIndex, System.Action<int> onSetSelectedIndex, System.Action<T> drawFields, System.Func<T, string> getLabel, System.Func<T> createNew, System.Action<Rect, T> drawIcon = null, System.Func<T, bool> filter = null, System.Action drawHeader = null)
    {
        EditorGUILayout.BeginHorizontal();

        // ── Sidebar (List View) ──
        EditorGUILayout.BeginVertical(GUILayout.Width(280));
        
        if (drawHeader != null) drawHeader();

        // Search
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        _searchQuery = EditorGUILayout.TextField(_searchQuery, EditorStyles.toolbarSearchField);
        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(20))) _searchQuery = "";
        EditorGUILayout.EndHorizontal();

        _listScroll = EditorGUILayout.BeginScrollView(_listScroll, GUILayout.ExpandHeight(true));
        for (int i = 0; i < list.Count; i++)
        {
            T entry = list[i];
            string label = getLabel(entry);
            
            // Search filter
            if (!string.IsNullOrEmpty(_searchQuery) && !label.ToLower().Contains(_searchQuery.ToLower())) continue;
            // Custom filter
            if (filter != null && !filter(entry)) continue;

            Rect rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(32));
            if (selectedIndex == i) GUI.backgroundColor = new Color(0.1f, 0.6f, 1f, 0.6f);
            
            if (GUILayout.Button("", "Label", GUILayout.ExpandWidth(true), GUILayout.Height(32)))
            {
                onSetSelectedIndex(i);
                GUI.FocusControl(null);
            }
            
            // Draw Icon Overlay
            if (drawIcon != null) drawIcon(rect, entry);

            // Re-draw text on top of the button
            GUI.Label(new Rect(rect.x + 10, rect.y, rect.width - 40, rect.height), label, selectedIndex == i ? EditorStyles.whiteLabel : EditorStyles.label);
            
            // Reorder buttons (Note: these should also updates selection if we want consistency, but swap handles it internally usually)
            if (GUILayout.Button("▲", EditorStyles.miniButtonLeft, GUILayout.Width(20), GUILayout.Height(32))) { 
                SwapItems(list, i, i - 1, selectedIndex, onSetSelectedIndex); 
                break; 
            }
            if (GUILayout.Button("▼", EditorStyles.miniButtonRight, GUILayout.Width(20), GUILayout.Height(32))) { 
                SwapItems(list, i, i + 1, selectedIndex, onSetSelectedIndex); 
                break; 
            }

            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("＋ Add New", GUILayout.Height(30)))
        {
            T newItem = createNew();
            list.Add(newItem);
            onSetSelectedIndex(list.Count - 1);
        }
        EditorGUILayout.EndVertical();

        // ── Main View (Field Editor) ──
        EditorGUILayout.BeginVertical(PaddingStyle, GUILayout.ExpandWidth(true));
        if (selectedIndex >= 0 && selectedIndex < list.Count)
        {
            T entry = list[selectedIndex];
            _editorScroll = EditorGUILayout.BeginScrollView(_editorScroll);
            drawFields(entry);
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space(20);
            GUI.backgroundColor = new Color(1f, 0.3f, 0.3f, 0.8f);
            if (GUILayout.Button("🗑️  Delete Entry", GUILayout.Height(28)))
            {
                if (EditorUtility.DisplayDialog("Delete Confirmation", "Are you sure you want to delete this entry?", "Yes", "No"))
                {
                    list.RemoveAt(selectedIndex);
                    onSetSelectedIndex(-1);
                }
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Select an item to edit", EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
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
}
