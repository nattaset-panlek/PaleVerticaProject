using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Variables/Fields]
    private static GUIStyle _cellStyle;
    private static GUIStyle _headerCellStyle;
    #endregion

    #region [Properties]
    private static GUIStyle CellStyle => _cellStyle ??= new GUIStyle(EditorStyles.label) { clipping = TextClipping.Clip, padding = new RectOffset(6, 6, 0, 0), alignment = TextAnchor.MiddleLeft };
    private static GUIStyle HeaderCellStyle => _headerCellStyle ??= new GUIStyle(EditorStyles.miniBoldLabel) { clipping = TextClipping.Clip, padding = new RectOffset(6, 6, 0, 0), alignment = TextAnchor.MiddleLeft, normal = { textColor = ThemeMuted } };
    #endregion

    #region [Private Methods]
    private void DrawTableView<T>(List<T> list, int selectedIndex, System.Action<int> onSetSelectedIndex, System.Action<T> drawFields, System.Func<T, EntryView> describe, System.Func<T> createNew, System.Func<T, bool> filter, System.Action drawHeader, System.Func<T, bool> canModify, (string name, System.Func<T, System.IComparable> key)[] sorts, (string header, float width, System.Func<T, string> text, System.Func<T, Color?> chip)[] columns)
    {
        float areaWidth = position.width - NavigatorWidth;
        var shown = columns.Where(c => !IsColumnHidden(c.header)).ToArray();

        EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        GUILayout.Label(GetTabTitle(_currentTab), TitleStyle, GUILayout.ExpandWidth(false));
        GUILayout.Label(list.Count.ToString(), MutedStyle, GUILayout.ExpandWidth(false), GUILayout.Height(20));
        GUILayout.Space(8);
        DrawViewToggle();
        GUILayout.Space(8);
        _searchQuery = EditorGUILayout.TextField(_searchQuery, EditorStyles.toolbarSearchField, GUILayout.Width(220), GUILayout.Height(20));
        GUILayout.FlexibleSpace();
        int sortIndex = DrawSortControls(sorts, 130);
        GUILayout.Space(6);
        if (GUILayout.Button("Columns", EditorStyles.miniPullDown, GUILayout.Width(74))) ShowColumnsMenu(columns.Select(c => c.header).ToArray());
        GUILayout.Space(6);
        if (PrimaryButton("+ Row", GUILayout.Width(64), GUILayout.Height(20)))
        {
            list.Add(createNew());
            onSetSelectedIndex(list.Count - 1);
            MarkDirty();
            GUI.FocusControl(null);
        }
        GUILayout.Space(16);
        EditorGUILayout.EndHorizontal();

        if (drawHeader != null)
        {
            GUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(16);
            EditorGUILayout.BeginVertical();
            _filterChipWidth = areaWidth - 40;
            drawHeader();
            EditorGUILayout.EndVertical();
            GUILayout.Space(16);
            EditorGUILayout.EndHorizontal();
        }

        var visible = ComputeVisible(list, filter, describe, sorts, sortIndex);
        GUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        GUILayout.Label(visible.Count == list.Count ? $"{list.Count} entries" : $"Showing {visible.Count} of {list.Count}", MutedStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label("Click a row to edit it below. Double-click to open it in the inspector.", MutedStyle);
        GUILayout.Space(16);
        EditorGUILayout.EndHorizontal();

        float totalWidth = shown.Sum(c => c.width) + 24;
        DrawTableHeader(shown, totalWidth, sorts);

        _tableScroll = EditorGUILayout.BeginScrollView(_tableScroll, GUILayout.ExpandHeight(true));
        Rect content = GUILayoutUtility.GetRect(totalWidth, visible.Count * TableRowHeight, GUILayout.ExpandWidth(true));
        float viewTop = _tableScroll.y - TableRowHeight;
        float viewBottom = _tableScroll.y + position.height;
        for (int r = 0; r < visible.Count; r++)
        {
            float y = content.y + r * TableRowHeight;
            if (y < viewTop || y > viewBottom) continue;
            int index = visible[r].Key;
            T entry = list[index];
            Rect row = new Rect(content.x, y, Mathf.Max(content.width, totalWidth), TableRowHeight);
            bool selected = index == selectedIndex;
            if (selected) EditorGUI.DrawRect(row, ThemeSelection);
            else if (r % 2 == 1) EditorGUI.DrawRect(row, new Color(1, 1, 1, 0.025f));
            EditorGUI.DrawRect(new Rect(row.x, row.yMax - 1, row.width, 1), new Color(0, 0, 0, 0.15f));

            float x = row.x + 12;
            foreach (var col in shown)
            {
                Rect cell = new Rect(x, row.y, col.width, row.height);
                string text = col.text(entry) ?? "";
                Color? chip = col.chip != null ? col.chip(entry) : null;
                if (chip.HasValue && !string.IsNullOrEmpty(text))
                {
                    float w = Mathf.Min(ChipWidth(text), col.width - 12);
                    DrawChip(new Rect(cell.x + 6, cell.y + 3, w, 18), text, chip.Value);
                }
                else
                {
                    GUI.Label(cell, string.IsNullOrEmpty(text) ? "-" : text, selected ? EditorStyles.whiteLabel : CellStyle);
                }
                x += col.width;
            }

            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && row.Contains(e.mousePosition))
            {
                onSetSelectedIndex(index);
                if (e.clickCount == 2) _tableMode[_currentTab] = false;
                GUI.FocusControl(null);
                e.Use();
            }
        }
        EditorGUILayout.EndScrollView();

        if (selectedIndex >= 0 && selectedIndex < list.Count)
            DrawTableDrawer(list, selectedIndex, onSetSelectedIndex, drawFields, describe, canModify, areaWidth);

        EditorGUILayout.EndVertical();
    }

    private void DrawTableHeader<T>((string header, float width, System.Func<T, string> text, System.Func<T, Color?> chip)[] shown, float totalWidth, (string name, System.Func<T, System.IComparable> key)[] sorts)
    {
        Rect bar = GUILayoutUtility.GetRect(10, 26, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(bar, ThemeGroupHeader);
        EditorGUI.DrawRect(new Rect(bar.x, bar.yMax - 1, bar.width, 1), ThemeLine);
        GUI.BeginClip(bar);
        float x = 12 - _tableScroll.x;
        _sortIndex.TryGetValue(_currentTab, out int activeSort);
        foreach (var col in shown)
        {
            Rect cell = new Rect(x, 0, col.width, bar.height);
            int sortMatch = sorts == null ? -1 : System.Array.FindIndex(sorts, s => s.name == col.header);
            string label = col.header;
            if (sortMatch >= 0 && activeSort == sortMatch + 1) label += IsSortDescending() ? "  (desc)" : "  (asc)";
            GUI.Label(cell, label, HeaderCellStyle);
            if (sortMatch >= 0 && Event.current.type == EventType.MouseDown && Event.current.button == 0 && cell.Contains(Event.current.mousePosition))
            {
                if (activeSort == sortMatch + 1) _sortDescending[_currentTab] = !IsSortDescending();
                else
                {
                    _sortIndex[_currentTab] = sortMatch + 1;
                    _sortDescending[_currentTab] = false;
                }
                Event.current.Use();
            }
            x += col.width;
        }
        GUI.EndClip();
    }

    private void DrawTableDrawer<T>(List<T> list, int selectedIndex, System.Action<int> onSetSelectedIndex, System.Action<T> drawFields, System.Func<T, EntryView> describe, System.Func<T, bool> canModify, float areaWidth)
    {
        T entry = list[selectedIndex];
        bool isLocked = canModify != null && !canModify(entry);
        float height = Mathf.Clamp(position.height * 0.45f, 240f, 460f);

        EditorGUILayout.BeginVertical(GUILayout.Height(height));
        Rect accent = GUILayoutUtility.GetRect(10, 2, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(accent, ThemeAccent);
        GUILayout.Space(8);
        if (DrawEntryHeader(list, selectedIndex, onSetSelectedIndex, describe(entry), isLocked, 28, true))
        {
            GUILayout.Space(4);
            DrawEntryFields(entry, isLocked, drawFields, areaWidth);
        }
        EditorGUILayout.EndVertical();
    }

    private bool IsColumnHidden(string header)
    {
        return _hiddenColumns.TryGetValue(_currentTab, out var hidden) && hidden.Contains(header);
    }

    private void ShowColumnsMenu(string[] headers)
    {
        ToolTab tab = _currentTab;
        if (!_hiddenColumns.TryGetValue(tab, out var hidden))
        {
            hidden = new HashSet<string>();
            _hiddenColumns[tab] = hidden;
        }
        GenericMenu menu = new GenericMenu();
        for (int i = 0; i < headers.Length; i++)
        {
            string h = headers[i];
            if (i == 0)
            {
                menu.AddDisabledItem(new GUIContent(h), true);
                continue;
            }
            menu.AddItem(new GUIContent(h), !hidden.Contains(h), () =>
            {
                if (!hidden.Remove(h)) hidden.Add(h);
                Repaint();
            });
        }
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Show all columns"), false, () => { hidden.Clear(); Repaint(); });
        menu.ShowAsContext();
    }
    #endregion
}
