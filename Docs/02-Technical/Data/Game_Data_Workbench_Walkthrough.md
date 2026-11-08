# Pale Vertica GameDataWorkbench Walkthrough

The Pale Vertica GameDataWorkbench is the editor window for every gameplay JSON table in `Assets/Resources/Data/`. Since 2026-11-07 it uses the Navigator + Table hybrid (Option 1+3 from the Figma file "Whiper of pale", page FreeSpace, section "Option 1+3 - Navigator + Table Hybrid"). Flat tables open as a spreadsheet; deep tables open as list + inspector.

## Where to open it

**Window > Pale Vertica > Pale Vertica GameDataWorkbench**. The minimum window size is 980 x 560. The inspector switches to two columns when it is wider than about 1240 px.

## Layout

| Area | What it holds |
|---|---|
| Header | Title, "N entries loaded", an **Unsaved changes** chip when something changed, **Reload** and **Save**. |
| Navigator (left, 220 px) | Every table grouped by OVERVIEW, DATA, COMBAT and SYSTEM, with entry counts. Type in the filter box to narrow the list. The footer shows the validation result; click it to open the Dashboard. |
| List pane (300 px) | Table name, count, **+ Add New**, search (name or ID), table filters, then the entries. Each row shows an icon (the real sprite when the entry has one), the name, the ID and a chip (element, rarity, faction and so on). |
| Inspector | Entry header (icon, name, `#id`, chips, **Duplicate**, **Delete**), then grouped fields in one or two columns. |
| Table view | Used instead of the list pane and inspector on Attributes, Items, Factions, Support Skills and Unique Passives. Toolbar: name, count, **List / Table** toggle, search, Sort, **Columns**, **+ Row**. Then the filters, the grid and a bottom drawer. The drawer edits the selected row with the same groups as the inspector. |

## Inputs

The eight JSON files: `Attribute`, `Item`, `ItemSet`, `Character`, `Faction`, `Skill`, `SupportSkill`, `UniquePassive`. Nothing else is read except sprites and prefabs that the entries reference.

## Steps

1. Open the window. All tables load and validation runs silently; the navigator footer shows the issue count.
2. Pick a table in the navigator.
3. Narrow and order the list:
   - **Search:** type a name or an ID.
   - **Filters:** every table has its own filters under the search box (see the table below). The line above the list shows "Showing N of M" while a filter is on.
   - **Sort:** pick a key from the Sort drop-down and toggle **Asc** / **Desc**. Sorting only changes what you see; the JSON keeps its file order. The up and down reorder arrows appear only while Sort is **File order**.
4. Click an entry. In list mode the inspector shows its groups. In table mode the drawer under the grid shows them; double-click a row or press **Open in inspector** to switch that table to list mode. Use the **List / Table** toggle to switch back. Each table remembers its mode until the window closes.
   - In table mode, click a column header (ID, Name, Rarity and so on) to sort by it; click again to flip Asc / Desc. **Columns** hides or shows columns; the first column always stays.
5. Click a group header to fold or unfold it, so only the groups you need stay open. Each table remembers this until the window closes.
6. Edit fields. The header shows **Unsaved changes** as soon as anything changes.
7. Reorder the selected entry with the up and down arrows on its row. Use **Duplicate** or **Delete** in the inspector header; Delete asks for confirmation.
8. Press **Save** or **Ctrl+S** to write all eight JSON files. The chip disappears and validation runs again.

## View per table

| Table | Opens as |
|---|---|
| Attributes, Items, Factions, Support Skills, Unique Passives | Table (can switch to List) |
| Skills, Sets, Characters | List + Inspector |
| Dashboard, ID Manager | Full width |

## Filters and sorting per table

| Table | Filters | Sort keys (plus File order) |
|---|---|---|
| Attributes | All / Flat / Percent | ID, Name, Full Name, Mapping |
| Items | Type chips, Rarity, Set (Any / In a set / No set), Only items with effects | ID, Name, Rarity, Item Type, Equipment Slot, Max Sockets, Max Stack, Effect Count |
| Sets | All / Has items / No items | Set ID, Name, Pieces, Bonus Levels |
| Characters | Faction | ID, Name, Faction, Base HP, Base MP |
| Factions | All / Built-in / Custom | ID, Name, Allies, Hostiles |
| Skills | Element chips, Damage Type chips, Target type, Only skills with chain overrides | ID, Name, Element, Base Damage, Mana Cost, Cooldown, Cast Time (VCT), AOE Radius |
| Support Skills | All / Has icon / No icon | ID, Name |
| Unique Passives | All / Has description / No description | ID, Name |

Filters and sort choices last until the window closes. The Delete button in the inspector header is red with white text.

## Groups per table

| Table | Left column | Right column |
|---|---|---|
| Attributes | Attribute | Description |
| Items | Basic Item Info, type group (Equipment, Skill Rune, Support Rune or Progression), Item Set | Effects, Visuals, Costume Visuals (Equipment and Costume only) |
| Sets | Basic Set Info, Bonus Progression, Items in this Set (each item has **Open**) | Bonus Configuration |
| Characters | Character Info | Prefab Settings |
| Factions | Faction Info | Positive (Allies), Negative (Hostiles) |
| Skills | Basic Info, Unique Skill Settings (only with a custom class), Stats, Cast Timing, Effects, Skill Chain & Combo Overrides, Skill Tags | Targeting, Scalings, Simulator, Skill Graphic Preview, Visuals |
| Support Skills | Basic Info | Description |
| Unique Passives | Unique Passive Details | Description |

Dashboard and ID Manager use the whole area to the right of the navigator. On the Dashboard, click a stat card to open that table.

## Expected result

- Saving writes the same JSON fields as before the redesign. Only the editor UI changed; no data format or game code changed.
- On a Scaling row, an attribute ID that is not in `Attribute.json` shows in warning colour as "Attr ID n (not in Attribute.json)".
- The Simulator shows Final Base Damage, Final Cast Time and Final AOE Radius as tiles. It uses the same formula as before: `(Stat x AttrMult + FlatBonus + LevelMult x Level) x GlobalMult`.
- Factions 1 to 3 are built in. Their inspector is read-only and shows a "Built-in (locked)" chip.

## Common mistakes

| Mistake | Fix |
|---|---|
| Closing Unity with the Unsaved changes chip showing | Press Save or Ctrl+S first. Reload also asks before it throws changes away. |
| Changing a value from a dropdown menu and not seeing the chip | Menu pickers (faction, item set, attribute, tags) mark the data dirty when you pick. If the chip is still missing, press Save anyway; it writes every table. |
| Looking for the old Overview / Data / Combat / System tab bars | They are gone; use the navigator on the left. |
| An icon in the list stays a letter after you assign a sprite | Sprites are cached per session. Press Reload to clear the cache. |
| Emoji in labels or buttons | The project does not allow emoji. Use plain text or Unity built-in icons (`EditorGUIUtility.IconContent("console.warnicon.sml")`). |

## Code map (`Assets/Script/Editor/GameDataWorkbench/`)

| File | Role |
|---|---|
| `GameDataWorkbench.cs` | Window, header, Ctrl+S, load/save, dirty state, sprite cache |
| `GameDataWorkbench_Theme.cs` | Unity editor colour tokens (dark and light skin), styles, chip, icon square and button helpers |
| `GameDataWorkbench_Navigator.cs` | Navigator tree, tab titles and counts, validation footer |
| `GameDataWorkbench_EntryPanes.cs` | `DrawEditor<T>`: list pane, inspector header, `BeginGroup`/`EndGroup`, columns, filter chips, stat tiles |
| `GameDataWorkbench_TableView.cs` | Table view: toolbar, sortable header, virtualised rows, bottom drawer, Columns menu |
| `GameDataWorkbench_TableColumns.cs` | Column definitions for the five table tabs (`Get<Table>Columns`) |
| `GameDataWorkbench_ListFilters.cs` | Filter state, filter predicates and sort keys for each table (`Draw<Table>Filters`, `<Table>Filter`, `Get<Table>Sorts`) |
| `GameDataWorkbench_<Table>.cs` | Fields for each table, grouped with `BeginInspectorColumns` / `BeginGroup` / `NextInspectorColumn` |

To add a group to a table, wrap its fields in `if (BeginGroup("Title", ThemeAccent)) { ... } EndGroup();`. Always call `EndGroup()`, even when the group is folded.
