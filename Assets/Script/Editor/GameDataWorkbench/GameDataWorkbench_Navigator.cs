using UnityEditor;
using UnityEngine;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Variables/Fields]
    private static readonly (string category, ToolTab[] tabs)[] NavigatorTree =
    {
        ("OVERVIEW", new[] { ToolTab.Dashboard }),
        ("DATA", new[] { ToolTab.Attributes, ToolTab.Items, ToolTab.Sets, ToolTab.Characters, ToolTab.Quests, ToolTab.Dialogues, ToolTab.DropTables, ToolTab.Shops, ToolTab.Factions, ToolTab.Experience }),
        ("COMBAT", new[] { ToolTab.Skills, ToolTab.SupportSkills, ToolTab.UniquePassives }),
        ("SYSTEM", new[] { ToolTab.IdManager })
    };

    private string _navigatorFilter = "";
    private Vector2 _navigatorScroll;
    #endregion

    #region [Private Methods]
    private void DrawNavigator(Rect rect)
    {
        EditorGUI.DrawRect(rect, ThemePanel);
        EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y, 1, rect.height), ThemeLine);

        GUILayout.BeginArea(new Rect(rect.x + 8, rect.y + 10, rect.width - 16, rect.height - 20));
        _navigatorFilter = EditorGUILayout.TextField(_navigatorFilter, EditorStyles.toolbarSearchField, GUILayout.Height(20));
        string filter = string.IsNullOrEmpty(_navigatorFilter) ? null : _navigatorFilter.ToLowerInvariant();

        _navigatorScroll = EditorGUILayout.BeginScrollView(_navigatorScroll, GUIStyle.none, GUI.skin.verticalScrollbar);
        foreach (var (category, tabs) in NavigatorTree)
        {
            bool any = false;
            foreach (ToolTab tab in tabs)
                if (filter == null || GetTabTitle(tab).ToLowerInvariant().Contains(filter)) any = true;
            if (!any) continue;

            GUILayout.Space(10);
            GUILayout.Label(category, NavHeaderStyle);
            foreach (ToolTab tab in tabs)
            {
                string title = GetTabTitle(tab);
                if (filter != null && !title.ToLowerInvariant().Contains(filter)) continue;
                DrawNavigatorItem(tab, title);
            }
        }
        EditorGUILayout.EndScrollView();

        DrawValidationFooter();
        GUILayout.EndArea();
    }

    private void DrawNavigatorItem(ToolTab tab, string title)
    {
        bool selected = _currentTab == tab;
        Rect r = GUILayoutUtility.GetRect(10, 26, GUILayout.ExpandWidth(true));
        if (selected) EditorGUI.DrawRect(r, ThemeSelection);
        else if (r.Contains(Event.current.mousePosition)) EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.04f));
        GUI.Label(r, title, selected ? NavItemSelectedStyle : NavItemStyle);
        int count = GetTabCount(tab);
        if (count >= 0) GUI.Label(new Rect(r.x, r.y, r.width - 8, r.height), count.ToString(), MutedRightStyle);

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && r.Contains(Event.current.mousePosition))
        {
            SelectTab(tab);
            Event.current.Use();
        }
    }

    private void DrawValidationFooter()
    {
        string text;
        MessageType type;
        if (!_isValidated) { text = "Validation: not run"; type = MessageType.Info; }
        else if (_validationErrors.Count == 0) { text = "Validation: all checks passed"; type = MessageType.Info; }
        else { text = $"Validation: {_validationErrors.Count} issue{(_validationErrors.Count == 1 ? "" : "s")}"; type = MessageType.Warning; }

        GUILayout.Space(6);
        Rect r = EditorGUILayout.GetControlRect(false, 30);
        EditorGUI.DrawRect(r, ThemeField);
        string icon = type == MessageType.Warning ? "console.warnicon.sml" : "console.infoicon.sml";
        GUI.Label(new Rect(r.x + 6, r.y + 7, 16, 16), EditorGUIUtility.IconContent(icon));
        GUI.Label(new Rect(r.x + 26, r.y, r.width - 30, r.height), text);
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && r.Contains(Event.current.mousePosition))
        {
            SelectTab(ToolTab.Dashboard);
            Event.current.Use();
        }
    }

    private void SelectTab(ToolTab tab)
    {
        if (_currentTab == tab) return;
        _currentTab = tab;
        _searchQuery = "";
        _listScroll = Vector2.zero;
        _editorScroll = Vector2.zero;
        _contentScroll = Vector2.zero;
        GUI.FocusControl(null);
    }

    private string GetTabTitle(ToolTab tab)
    {
        switch (tab)
        {
            case ToolTab.Dashboard: return "Dashboard";
            case ToolTab.Attributes: return "Attributes";
            case ToolTab.Items: return "Items";
            case ToolTab.Sets: return "Sets";
            case ToolTab.Characters: return "Characters";
            case ToolTab.Quests: return "Quests";
            case ToolTab.Dialogues: return "Dialogues";
            case ToolTab.DropTables: return "Drop Tables";
            case ToolTab.Shops: return "Shops";
            case ToolTab.Factions: return "Factions";
            case ToolTab.Skills: return "Skills";
            case ToolTab.SupportSkills: return "Support Skills";
            case ToolTab.UniquePassives: return "Unique Passives";
            case ToolTab.IdManager: return "ID Manager";
            case ToolTab.Experience: return "Experience";
            default: return tab.ToString();
        }
    }

    private int GetTabCount(ToolTab tab)
    {
        switch (tab)
        {
            case ToolTab.Attributes: return _attributeData?.Attributes?.Count ?? 0;
            case ToolTab.Items: return _itemData?.Items?.Count ?? 0;
            case ToolTab.Sets: return _itemSetData?.Sets?.Count ?? 0;
            case ToolTab.Characters: return _characterData?.Characters?.Count ?? 0;
            case ToolTab.Quests: return _questData?.Quests?.Count ?? 0;
            case ToolTab.Dialogues: return _dialogueData?.Dialogues?.Count ?? 0;
            case ToolTab.DropTables: return _dropTableData?.Tables?.Count ?? 0;
            case ToolTab.Shops: return _shopData?.Shops?.Count ?? 0;
            case ToolTab.Factions: return _factionData?.Factions?.Count ?? 0;
            case ToolTab.Skills: return _skillData?.ActiveSkills?.Count ?? 0;
            case ToolTab.SupportSkills: return _supportSkillData?.SupportModifiers?.Count ?? 0;
            case ToolTab.UniquePassives: return _uniquePassiveData?.UniquePassives?.Count ?? 0;
            default: return -1;
        }
    }
    #endregion
}
