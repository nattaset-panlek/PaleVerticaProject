using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Variables/Fields]
    private struct EntryChip
    {
        public string Text;
        public Color Color;
        public EntryChip(string text, Color color) { Text = text; Color = color; }
    }

    private struct EntryView
    {
        public string Id;
        public string Name;
        public string Meta;
        public Color MetaColor;
        public Color Accent;
        public Sprite Icon;
        public string Subtitle;
        public EntryChip[] Chips;
    }

    private readonly Dictionary<string, bool> _groupOpen = new Dictionary<string, bool>();
    private readonly Stack<bool> _groupOpenStack = new Stack<bool>();
    private bool _twoColumns;
    private float _columnWidth;
    private int _lastSelectedIndex = -1;
    private ToolTab _lastSelectedTab;
    private readonly Dictionary<ToolTab, int> _sortIndex = new Dictionary<ToolTab, int>();
    private readonly Dictionary<ToolTab, bool> _sortDescending = new Dictionary<ToolTab, bool>();
    private readonly Dictionary<ToolTab, bool> _tableMode = new Dictionary<ToolTab, bool>();
    private readonly Dictionary<ToolTab, HashSet<string>> _hiddenColumns = new Dictionary<ToolTab, HashSet<string>>();
    private Vector2 _tableScroll;
    private float _filterChipWidth = ListPaneWidth - 20;
    private const float TableRowHeight = 24f;
    #endregion

    #region [Private Methods]
    private void DrawEditor<T>(List<T> list, int selectedIndex, System.Action<int> onSetSelectedIndex, System.Action<T> drawFields, System.Func<T, EntryView> describe, System.Func<T> createNew, System.Func<T, bool> filter = null, System.Action drawHeader = null, System.Func<T, bool> canModify = null, (string name, System.Func<T, System.IComparable> key)[] sorts = null, (string header, float width, System.Func<T, string> text, System.Func<T, Color?> chip)[] columns = null)
    {
        if (selectedIndex != _lastSelectedIndex || _currentTab != _lastSelectedTab)
        {
            _lastSelectedIndex = selectedIndex;
            _lastSelectedTab = _currentTab;
            _editorScroll = Vector2.zero;
        }

        if (columns != null && IsTableMode())
        {
            DrawTableView(list, selectedIndex, onSetSelectedIndex, drawFields, describe, createNew, filter, drawHeader, canModify, sorts, columns);
            return;
        }

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
        DrawListPane(list, selectedIndex, onSetSelectedIndex, describe, createNew, filter, drawHeader, sorts, columns != null);

        Rect sep = GUILayoutUtility.GetRect(1, 1, GUILayout.Width(1), GUILayout.ExpandHeight(true));
        EditorGUI.DrawRect(sep, ThemeLine);

        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        if (selectedIndex >= 0 && selectedIndex < list.Count)
            DrawInspector(list, selectedIndex, onSetSelectedIndex, drawFields, describe, canModify);
        else
            DrawEmptyInspector();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    private bool IsTableMode()
    {
        return !_tableMode.TryGetValue(_currentTab, out bool table) || table;
    }

    private void DrawViewToggle()
    {
        bool table = IsTableMode();
        Color prev = GUI.backgroundColor;
        GUI.backgroundColor = table ? Color.white : ThemePrimary * 1.6f;
        if (GUILayout.Button("List", EditorStyles.miniButtonLeft, GUILayout.Width(40), GUILayout.Height(20)) && table)
        {
            _tableMode[_currentTab] = false;
            GUI.FocusControl(null);
        }
        GUI.backgroundColor = table ? ThemePrimary * 1.6f : Color.white;
        if (GUILayout.Button("Table", EditorStyles.miniButtonRight, GUILayout.Width(46), GUILayout.Height(20)) && !table)
        {
            _tableMode[_currentTab] = true;
            GUI.FocusControl(null);
        }
        GUI.backgroundColor = prev;
    }

    private List<KeyValuePair<int, EntryView>> ComputeVisible<T>(List<T> list, System.Func<T, bool> filter, System.Func<T, EntryView> describe, (string name, System.Func<T, System.IComparable> key)[] sorts, int sortIndex)
    {
        string query = string.IsNullOrEmpty(_searchQuery) ? null : _searchQuery.ToLowerInvariant();
        var visible = new List<KeyValuePair<int, EntryView>>();
        for (int i = 0; i < list.Count; i++)
        {
            T entry = list[i];
            if (filter != null && !filter(entry)) continue;
            EntryView view = describe(entry);
            if (query != null && !(view.Name ?? "").ToLowerInvariant().Contains(query) && !(view.Id ?? "").ToLowerInvariant().Contains(query)) continue;
            visible.Add(new KeyValuePair<int, EntryView>(i, view));
        }
        if (sortIndex > 0 && sorts != null && sortIndex <= sorts.Length)
        {
            var key = sorts[sortIndex - 1].key;
            var comparer = Comparer<System.IComparable>.Default;
            visible = (IsSortDescending() ? visible.OrderByDescending(v => key(list[v.Key]), comparer) : visible.OrderBy(v => key(list[v.Key]), comparer)).ToList();
        }
        return visible;
    }

    private void DrawListPane<T>(List<T> list, int selectedIndex, System.Action<int> onSetSelectedIndex, System.Func<T, EntryView> describe, System.Func<T> createNew, System.Func<T, bool> filter, System.Action drawHeader, (string name, System.Func<T, System.IComparable> key)[] sorts, bool hasTableView)
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(ListPaneWidth), GUILayout.ExpandHeight(true));
        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Label(GetTabTitle(_currentTab), TitleStyle, GUILayout.ExpandWidth(false));
        GUILayout.Label(list.Count.ToString(), MutedStyle, GUILayout.ExpandWidth(false), GUILayout.Height(20));
        GUILayout.FlexibleSpace();
        if (hasTableView) DrawViewToggle();
        if (PrimaryButton("+ Add New", GUILayout.Width(84), GUILayout.Height(22)))
        {
            list.Add(createNew());
            onSetSelectedIndex(list.Count - 1);
            MarkDirty();
            GUI.FocusControl(null);
        }
        GUILayout.Space(10);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);
        _searchQuery = EditorGUILayout.TextField(_searchQuery, EditorStyles.toolbarSearchField, GUILayout.Height(20));
        GUILayout.Space(10);
        EditorGUILayout.EndHorizontal();

        if (drawHeader != null)
        {
            GUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            _filterChipWidth = ListPaneWidth - 20;
            drawHeader();
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
        }

        int sortIndex = DrawSortRow(sorts);
        bool fileOrder = sortIndex == 0;

        var visible = ComputeVisible(list, filter, describe, sorts, sortIndex);

        GUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Label(visible.Count == list.Count ? $"{list.Count} entries" : $"Showing {visible.Count} of {list.Count}", MutedStyle);
        GUILayout.FlexibleSpace();
        if (!fileOrder) GUILayout.Label("Reorder: File order only", MutedStyle);
        GUILayout.Space(10);
        EditorGUILayout.EndHorizontal();

        _listScroll = EditorGUILayout.BeginScrollView(_listScroll, GUILayout.ExpandHeight(true));
        foreach (var item in visible)
        {
            int i = item.Key;
            int moved = DrawListRow(item.Value, i == selectedIndex, fileOrder, out bool clicked);
            if (clicked)
            {
                onSetSelectedIndex(i);
                GUI.FocusControl(null);
            }
            if (moved != 0)
            {
                SwapItems(list, i, i + moved, selectedIndex, onSetSelectedIndex);
                MarkDirty();
                break;
            }
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private int DrawListRow(EntryView view, bool selected, bool allowReorder, out bool clicked)
    {
        clicked = false;
        Rect row = GUILayoutUtility.GetRect(10, ListRowHeight, GUILayout.ExpandWidth(true));
        row = new Rect(row.x + 8, row.y + 1, row.width - 16, row.height - 2);
        if (selected) EditorGUI.DrawRect(row, ThemeSelection);
        else if (row.Contains(Event.current.mousePosition)) EditorGUI.DrawRect(row, new Color(1, 1, 1, 0.04f));

        DrawIconSquare(new Rect(row.x + 8, row.y + 8, 24, 24), view.Accent, view.Name, view.Icon);

        float right = row.xMax - 8;
        int moved = 0;
        if (selected && allowReorder)
        {
            if (GUI.Button(new Rect(right - 20, row.y + 10, 20, 20), "▼", EditorStyles.miniButtonRight)) moved = 1;
            if (GUI.Button(new Rect(right - 40, row.y + 10, 20, 20), "▲", EditorStyles.miniButtonLeft)) moved = -1;
            right -= 46;
        }
        if (!string.IsNullOrEmpty(view.Meta))
        {
            float w = ChipWidth(view.Meta);
            DrawChip(new Rect(right - w, row.y + 11, w, 18), view.Meta, view.MetaColor);
            right -= w + 6;
        }

        Rect nameRect = new Rect(row.x + 40, row.y + 4, right - row.x - 40, 18);
        GUI.Label(nameRect, view.Name ?? "", selected ? EditorStyles.whiteBoldLabel : EditorStyles.label);
        GUI.Label(new Rect(nameRect.x, row.y + 20, nameRect.width, 16), view.Id ?? "", MutedStyle);

        if (moved == 0 && Event.current.type == EventType.MouseDown && Event.current.button == 0 && row.Contains(Event.current.mousePosition))
        {
            clicked = true;
            Event.current.Use();
        }
        return moved;
    }

    private void DrawInspector<T>(List<T> list, int selectedIndex, System.Action<int> onSetSelectedIndex, System.Action<T> drawFields, System.Func<T, EntryView> describe, System.Func<T, bool> canModify)
    {
        T entry = list[selectedIndex];
        bool isLocked = canModify != null && !canModify(entry);
        GUILayout.Space(14);
        if (!DrawEntryHeader(list, selectedIndex, onSetSelectedIndex, describe(entry), isLocked, 52, false)) return;

        GUILayout.Space(10);
        Rect divider = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(new Rect(divider.x + 20, divider.y, divider.width - 40, 1), ThemeLine);

        DrawEntryFields(entry, isLocked, drawFields, position.width - NavigatorWidth - ListPaneWidth);
    }

    private bool DrawEntryHeader<T>(List<T> list, int selectedIndex, System.Action<int> onSetSelectedIndex, EntryView view, bool isLocked, float iconSize, bool compact)
    {
        bool stillValid = true;
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);
        Rect iconRect = GUILayoutUtility.GetRect(iconSize, iconSize, GUILayout.Width(iconSize), GUILayout.Height(iconSize));
        DrawIconSquare(iconRect, view.Accent, view.Name, view.Icon);
        GUILayout.Space(compact ? 8 : 12);

        if (compact)
        {
            GUILayout.Label(string.IsNullOrEmpty(view.Name) ? "(unnamed)" : view.Name, TitleStyle, GUILayout.ExpandWidth(false), GUILayout.Height(iconSize));
            GUILayout.Label("#" + view.Id + (string.IsNullOrEmpty(view.Subtitle) ? "" : "  " + view.Subtitle), MutedStyle, GUILayout.ExpandWidth(false), GUILayout.Height(iconSize));
            if (isLocked) ChipLayout("Built-in (locked)", ThemeMuted);
            GUILayout.FlexibleSpace();
        }
        else
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label(string.IsNullOrEmpty(view.Name) ? "(unnamed)" : view.Name, BigTitleStyle);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("#" + view.Id + (string.IsNullOrEmpty(view.Subtitle) ? "" : "  " + view.Subtitle), MutedStyle, GUILayout.ExpandWidth(false), GUILayout.Height(18));
            if (view.Chips != null)
                foreach (EntryChip chip in view.Chips)
                {
                    GUILayout.Space(4);
                    ChipLayout(chip.Text, chip.Color);
                }
            if (isLocked)
            {
                GUILayout.Space(4);
                ChipLayout("Built-in (locked)", ThemeMuted);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginVertical(GUILayout.Width(compact ? 300 : 170));
        if (!compact) GUILayout.Space(14);
        EditorGUILayout.BeginHorizontal();
        if (compact)
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open in inspector", GUILayout.Height(24)))
            {
                _tableMode[_currentTab] = false;
                GUI.FocusControl(null);
            }
        }
        EditorGUI.BeginDisabledGroup(isLocked);
        if (GUILayout.Button("Duplicate", GUILayout.Height(24)))
        {
            T clone = JsonUtility.FromJson<T>(JsonUtility.ToJson(list[selectedIndex]));
            list.Insert(selectedIndex + 1, clone);
            onSetSelectedIndex(selectedIndex + 1);
            MarkDirty();
        }
        if (DangerButton("Delete", GUILayout.Height(24)) && EditorUtility.DisplayDialog("Delete Entry", $"Delete '{view.Name}' (#{view.Id})?", "Delete", "Cancel"))
        {
            list.RemoveAt(selectedIndex);
            onSetSelectedIndex(-1);
            MarkDirty();
            stillValid = false;
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        GUILayout.Space(20);
        EditorGUILayout.EndHorizontal();
        return stillValid;
    }

    private void DrawEntryFields<T>(T entry, bool isLocked, System.Action<T> drawFields, float availableWidth)
    {
        _twoColumns = availableWidth - 40 >= TwoColumnMinWidth;
        _columnWidth = _twoColumns ? (availableWidth - 72) * 0.5f : 0f;

        _editorScroll = EditorGUILayout.BeginScrollView(_editorScroll, GUILayout.ExpandHeight(true));
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.BeginVertical();
        GUILayout.Space(12);
        float prevLabel = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 140;
        EditorGUI.BeginDisabledGroup(isLocked);
        EditorGUI.BeginChangeCheck();
        drawFields(entry);
        if (EditorGUI.EndChangeCheck()) MarkDirty();
        EditorGUI.EndDisabledGroup();
        EditorGUIUtility.labelWidth = prevLabel;
        EditorGUILayout.EndVertical();
        GUILayout.Space(20);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    private int DrawSortRow<T>((string name, System.Func<T, System.IComparable> key)[] sorts)
    {
        if (sorts == null || sorts.Length == 0) return 0;
        GUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);
        int index = DrawSortControls(sorts, -1);
        GUILayout.Space(10);
        EditorGUILayout.EndHorizontal();
        return index;
    }

    private int DrawSortControls<T>((string name, System.Func<T, System.IComparable> key)[] sorts, float popupWidth)
    {
        if (sorts == null || sorts.Length == 0) return 0;
        if (!_sortIndex.TryGetValue(_currentTab, out int index)) index = 0;
        string[] names = new string[sorts.Length + 1];
        names[0] = "File order";
        for (int i = 0; i < sorts.Length; i++) names[i + 1] = sorts[i].name;

        GUILayout.Label("Sort", MutedStyle, GUILayout.Width(30), GUILayout.Height(20));
        index = Mathf.Clamp(popupWidth > 0 ? EditorGUILayout.Popup(index, names, GUILayout.Width(popupWidth)) : EditorGUILayout.Popup(index, names), 0, names.Length - 1);
        _sortIndex[_currentTab] = index;
        EditorGUI.BeginDisabledGroup(index == 0);
        bool desc = IsSortDescending();
        if (GUILayout.Button(desc ? "Desc" : "Asc", EditorStyles.miniButton, GUILayout.Width(44))) _sortDescending[_currentTab] = !desc;
        EditorGUI.EndDisabledGroup();
        return index;
    }

    private bool IsSortDescending()
    {
        return _sortDescending.TryGetValue(_currentTab, out bool d) && d;
    }

    private int DrawFilterPopup(string label, string[] options, int index, float labelWidth = 70)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(label, MutedStyle, GUILayout.Width(labelWidth));
        index = EditorGUILayout.Popup(Mathf.Clamp(index, 0, options.Length - 1), options);
        EditorGUILayout.EndHorizontal();
        return index;
    }

    private void DrawEmptyInspector()
    {
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Select an entry in the list to edit it", EditorStyles.centeredGreyMiniLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
    }

    private void BeginInspectorColumns()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(_twoColumns ? GUILayout.Width(_columnWidth) : GUILayout.ExpandWidth(true));
    }

    private void NextInspectorColumn()
    {
        if (!_twoColumns) return;
        EditorGUILayout.EndVertical();
        GUILayout.Space(12);
        EditorGUILayout.BeginVertical(GUILayout.Width(_columnWidth));
    }

    private void EndInspectorColumns()
    {
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    private bool BeginGroup(string title, Color accent, string rightLabel = null)
    {
        string key = _currentTab + "/" + title;
        if (!_groupOpen.TryGetValue(key, out bool open)) open = true;

        EditorGUILayout.BeginVertical(GroupBodyStyle);
        Rect header = GUILayoutUtility.GetRect(10, 26, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(header, ThemeGroupHeader);
        EditorGUI.DrawRect(new Rect(header.x + 8, header.y + 6, 3, 14), accent);
        GUI.Label(new Rect(header.x + 18, header.y, 14, header.height), open ? "▾" : "▸", GroupHeaderStyle);
        GUI.Label(new Rect(header.x + 30, header.y, header.width - 40, header.height), title, GroupHeaderStyle);
        if (!string.IsNullOrEmpty(rightLabel)) GUI.Label(new Rect(header.x, header.y, header.width - 10, header.height), rightLabel, MutedRightStyle);

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && header.Contains(Event.current.mousePosition))
        {
            _groupOpen[key] = !open;
            Event.current.Use();
            GUI.FocusControl(null);
        }

        if (open)
        {
            GUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(12);
            EditorGUILayout.BeginVertical();
        }
        _groupOpenStack.Push(open);
        return open;
    }

    private void EndGroup()
    {
        bool open = _groupOpenStack.Count > 0 && _groupOpenStack.Pop();
        if (open)
        {
            EditorGUILayout.EndVertical();
            GUILayout.Space(12);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawFilterChips(string[] names, ref int index, System.Func<string, Color> colorOf = null)
    {
        float maxWidth = _filterChipWidth;
        float x = 0;
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < names.Length; i++)
        {
            float w = ChipWidth(names[i]) + 2;
            if (x + w > maxWidth)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(3);
                EditorGUILayout.BeginHorizontal();
                x = 0;
            }
            Color c = colorOf != null && i > 0 ? colorOf(names[i]) : ThemeAccent;
            Rect r = GUILayoutUtility.GetRect(w, 18, GUILayout.Width(w), GUILayout.Height(18));
            DrawChip(r, names[i], i == index ? c : ThemeMuted, i == index);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && r.Contains(Event.current.mousePosition))
            {
                index = i;
                Event.current.Use();
            }
            GUILayout.Space(3);
            x += w + 3;
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawStatTiles(params (string label, string value)[] tiles)
    {
        EditorGUILayout.BeginHorizontal();
        foreach (var tile in tiles)
        {
            Rect r = GUILayoutUtility.GetRect(60, 46, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(r, ThemeField);
            GUI.Label(new Rect(r.x + 8, r.y + 4, r.width - 12, 14), tile.label, MutedStyle);
            GUI.Label(new Rect(r.x + 8, r.y + 17, r.width - 12, 26), tile.value, StatValueStyle);
            GUILayout.Space(6);
        }
        EditorGUILayout.EndHorizontal();
    }
    #endregion
}
