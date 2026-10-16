# UIBuilder Guide: runtime pages from code

`UIBuilder` builds native Unity UI (RectTransform, LayoutGroups, TextMeshPro) from C# lambdas, in the spirit of HTML: a **window** hosts a **document**, the document is a tree of **elements**, elements get **CSS-like styling**, and parts of the tree can be refreshed on their own. Any system can open a fully working page, menu or dialog at runtime without a prefab of its own.

Refactored 2026-10-16. The previous version of this guide and of the scripts are in `Backup/2026-10-16_ui-core-refactor/`.

---

## 1. Pieces

| Type | File (`CoreSystem/UI/Manager/`) | Role (HTML analogy) |
|---|---|---|
| `UIBuilder` | `UIBuilder.cs` | Entry point: `BuildGlobalModal(title, build, size)` opens (or reuses) a named window and builds it |
| `UIGlobalModal` | `UIGlobalModal.cs` | The **window** (browser chrome): title, close, drag, resize, sizing (`FitContent`, `Center`), dismiss rules. Hosts one `UIDocument` |
| `UIDocument` | `UIDocument.cs` | The **DOM**: root builder, `Refresh()`, `Refresh(regionId)`, ids (`Find`), per-window state (`GetState`/`SetState`), page navigation (`ShowPage`, `RefreshPage`). Keeps scroll positions of id'd scroll views across refreshes |
| `UIBuilderContext` | `UIBuilderContext.cs` | The **builder** passed to every lambda: layout, text, controls, regions, foldouts, pages |
| `UIElementRef` | `UIElementRef.cs` | Handle to one element; fluent **CSS-like** modifiers (`Width`, `Fill`, `Padding`, `Gap`, `Variant`, `Id` ...) |
| `UIElementFactory` | `UIElementFactory.cs` | Creates elements: registry prefab first, themed fallback otherwise; enforces layout rules |
| `UITheme` | `UITheme.cs` | **CSS variables**: colours, font sizes, spacing. Lives on the `UIElementRegistry` asset (Inspector: Theme) |
| `UIPageSet` | `UIPageSet.cs` | Page list for `Pages(...)` (title + builder, optional header) |
| `UIBuilderTypes` | `UIBuilderTypes.cs` | `UIVariant`, `UILayoutKind`, `UIPageNav` |
| `UIElementRegistrySO` | `UIElementRegistrySO.cs` | Style name -> prefab per element type, default font, theme |
| `UICustomElement` | `UICustomElement.cs` | Complex prefabs that expose children by key |
| `UIModalResizeHandle` | `UIModalResizeHandle.cs` | Resize grip: owns the resize math and the default grip sprite |

---

## 2. Opening a window

```csharp
_modal = UIBuilder.BuildGlobalModal("Inventory Debug", BuildUI, new Vector2(900, 600));
_modal.SetMinSize(new Vector2(600, 400));
_modal.OnClosed += () => _modal = null;
```

- Windows are keyed by name. Calling `BuildGlobalModal` again with the same name reuses and reopens the same window; its document state (current page, foldouts) survives.
- Window options (all chainable): `SetTitle`, `SetWindowSize`, `Center()`, `SetWindowPosition`, `SetWindowAnchors`, `FitContent(true, maxSize)`, `SetResizable`, `SetDraggable`, `SetDismissable(false)` (no close button, Esc and overlay click do nothing), `OverrideCloseAction`, `SetMinSize`/`SetMaxSize`.
- Events: `OnOpened`, `OnClosed`, `OnResized`.
- `FitContent` sizes the window to its content (header and margins are measured, not guessed), clamped to `MinSize`, the given max and the canvas. It re-fits after every build/refresh until the user resizes the window by hand.

---

## 3. Building

```csharp
private void BuildUI(UIBuilderContext ui)
{
    ui.Title("Character");
    ui.Row(row =>
    {
        row.Label("Name").Width(80);
        row.Input(_name, value => _name = value).Fill();
        row.Button("Save", Save, UIVariant.Primary);
    });
    ui.Panel(panel =>
    {
        panel.KeyValue("Level", _level.ToString());
        panel.Progress(_exp / _next);
    });
}
```

### Layout
| Method | Result |
|---|---|
| `Column(b)` / `Vertical(style, b)` / `Div(style, b)` | Vertical flow |
| `Row(b)` / `Horizontal(style, b)` | Horizontal flow, children vertically centred |
| `Grid(b)` / `Grid(style, b)` | Wrapping grid; set cells with `.Cell(w, h)`, fixed columns with `.Columns(n)` (default: as many as fit) |
| `Panel(b)` | Column with theme panel background, padding and gap (a "card") |
| `Scroll(b, height)` / `ScrollView(style, b, horizontal, vertical)` | Scroll area |
| `Spacer()` / `Spacer(px)` | Flexible gap (pushes siblings apart) or fixed gap |
| `Separator()` | Thin themed line |

### Text
`Title`, `Heading`, `Label` (body), `Caption` (small, muted), `KeyValue(key, value)`, `Badge(text, color)`, or `Text(style, content)` for a registry style. Rich text tags work (`<color=#33FF66>`).

### Controls
`Button(label, onClick, variant)`, `Button(style, label, onClick)`, `Input(text, onChanged)` (+ `.OnSubmit(...)`), `Toggle(label, isOn, onChanged)`, `Image(sprite)`, `RawImage(style, texture)`, `Progress(value, variant)` / `ProgressBar(style, value, color)`, `Custom(style)`.

### Layout rules (the "CSS")
1. Inside a **column**, containers, text, scroll views and progress bars stretch to the column width (like block elements). Inside a **row**, everything is sized by its content unless you call `.Fill()`.
2. Only the window root sizes itself to content. Any element placed under a layout group has its `ContentSizeFitter` removed, so nested layouts never fight.
3. A container always gets the layout group you asked for. `Row(...)` with a registry style whose prefab is vertical is converted to horizontal (padding and alignment are kept). Styles are visual; the method decides the layout.
4. Default containers have no padding; use `.Padding(...)` or `Panel` for spacing.

### Modifiers on `UIElementRef`
| Modifier | Effect |
|---|---|
| `Id(id)` | Register the element; get it back with `document.Find(id)` / `modal.Find(id)` |
| `Width(px)`, `Height(px)`, `Size(w, h)` | Fixed size (also cancels block stretch) |
| `Fill(weight)`, `FillHeight(weight)` | Take remaining space |
| `MinWidth`, `MinHeight` | Minimum size |
| `Padding(all)`, `Padding(h, v)`, `Padding(l, r, t, b)`, `Gap(px)`, `Align(anchor)` | Container spacing and alignment |
| `Cell(w, h)`, `Columns(n)` | Grid cells |
| `Background(color)`, `Variant(UIVariant)` | Colours; variants come from the theme |
| `TextColor`, `FontSize`, `Bold()`, `TextAlign`, `Text(value)` | Text styling and content |
| `OnClick(action)` | Makes any element clickable (cards, slots) |
| `OnSubmit(action)`, `Interactable(bool)`, `Visible(bool)` | Behaviour |

The older `Set*` methods (`SetPreferredSize`, `SetFlexibleSize`, `SetFitParent`, `SetBackgroundColor`, `SetTextColor`, `SetPadding`, `SetSpacing`, `SetChildAlignment`, `SetGrid*` ...) still work.

### Variants (`UIVariant`)
`Default` keeps the registry prefab look. `Primary`, `Secondary`, `Danger`, `Success`, `Warning`, `Selected`, `Ghost` recolour the element and its text from `UITheme`. Use them instead of hard-coded colours.

---

## 4. Updating without rebuilding everything

Rebuilding the whole window loses input focus and resets inner scroll views. Split the window into regions instead.

```csharp
ui.Row(bar => bar.Input(_query, q => { _query = q; _modal.Refresh("results"); }).Fill());
ui.Region("results", list =>
{
    foreach (var item in Filter(_query)) list.Label(item.Name);
});
```

- `ui.Region(id, builder)` builds now and remembers the builder; `modal.Refresh(id)` rebuilds only that region. The search field above keeps focus.
- `modal.Refresh()` rebuilds the whole document. Ids are re-registered, state is kept.
- Scroll views with an `Id` keep their scroll position across refreshes.
- `ui.Foldout(id, title, builder, expanded)` is a region with a header button; its open state lives in the document state (`"<id>.open"`).
- To change one value without any rebuild, give the element an id and update it: `modal.Find("hp").Text($"{hp}/{max}")`.

---

## 5. Pages and menus

```csharp
ui.Pages("settings", pages =>
{
    pages.WithHeader(BuildSearchBar)
         .Add("Video", BuildVideo)
         .Add("Audio", BuildAudio)
         .Add("Controls", BuildControls);
}, UIPageNav.SideMenu);
```

- `UIPageNav.Tabs` puts a scrolling tab bar on top; `UIPageNav.SideMenu` puts a vertical menu on the left (`pages.NavWidth`).
- `WithHeader` adds a fixed area above the page content (search bars, filters); it is not rebuilt when pages change.
- Switching pages refreshes only the menu and the content regions. The current page is stored in the document (`document.GetPage(id)`) and survives full refreshes and closing/reopening the window.
- From code: `modal.ShowPage("settings", "Audio")`, `modal.RefreshPage("settings")` (rebuild the current page after an action).

---

## 6. Theme and styles

- **Theme** (`UIElementRegistry` asset > Theme): surface, text and accent colours, title/heading/body/caption sizes, gap, padding, control height. Defaults match the light Global Modal prefab.
- **Styles**: `UIElementRegistrySO` maps (`UIElementType`, style name) to a prefab, with an optional `TargetComponentPath` to the child holding the real component. Registered today: `Text`, `Button`, `Primary Button`, `Button Fitter`, `InputField`, `Image`, `Toggle`, `Slider`, `Dropdown`, `ScrollRect`, `TabsScrollView`, `Container`, `Custom`, `Scrollbar`, `RawImage`, `ProgressBar`, `Divider`, `Mask`.
- Unregistered names (for example the default container style `Div` and grid style `Grid`) fall back to themed code-generated elements. Register a style only when you want a designed prefab.

### Custom prefabs (`UICustomElement`)
1. Build any prefab, add `UICustomElement` to its root and map children to keys (`"Icon"`, `"Title"`, `"Action"`).
2. Register it under `UIElementType.Custom` with a style name.
3. `var card = ui.Custom("ItemCard"); card.Get<TMP_Text>("Title").text = "Excalibur";`

---

## 7. Real examples

- `CoreSystem/UI/DevTools/GameDevToolUI.cs`: side-menu page app (9 pages), search header, foldouts, item grids with clickable cards, detail panels, variants, `RequestRebuild()` = `RefreshPage`.
- `CoreSystem/Player/PlayerDeathScreen.cs`: small non-dismissable dialog, `FitContent`, `Center`, centred action row with spacers.
- `Assets/Script/Editor/Tests/UIDocumentTests.cs`: layout rules, regions, ids, pages and foldouts (EditMode).

---

## 8. Migration from the old API

| Before | Now |
|---|---|
| `modal.Build(BuildUI)` after every click | `modal.Refresh("region")` or `modal.RefreshPage(id)`; `Build` still works |
| Manual tab bar + `_selectedTab` + full rebuild | `ui.Pages(id, ...)` |
| `Dictionary<string,bool>` for foldouts | `ui.Foldout(id, title, builder)` |
| `SetPreferredSize(w, -1)` | `Width(w)` |
| `SetFlexibleSize(1, -1)` | `Fill()` |
| `SetBackgroundColor(new Color(...))` on buttons | `Variant(UIVariant.Danger)` etc. |
| `AutoFitWindow(false, true, 0, 80)` | `FitContent(true, maxSize)` (header measured automatically) |
| `SetCloseButtonVisible(false)` for blocking dialogs | `SetDismissable(false)` (also blocks Esc and overlay click) |
| `SetPadding(0,0,10,0)` on text for spacing | `Spacer(10)` |
