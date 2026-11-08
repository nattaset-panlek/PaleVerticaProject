# Global Visualizer Walkthrough

Verified: **2026-10-18**, package version **1.0.0** (P1 to P7). Plan and roadmap (Thai): [Global_Visualizer_Plan_TH.md](./Global_Visualizer_Plan_TH.md). Package readme: `Packages/com.globalvisualizer.core/README.md` (Thai: `README_TH.md`), changes: `CHANGELOG.md`.

Global Visualizer is a framework for showing a list of anything in uGUI with any prefab.

- **Virtualized:** only the items inside the viewport (plus a buffer) get a GameObject.
- **Pooled:** GameObjects are reused as you scroll, so 10,000 items cost about as much as 20.
- **Layouts:** Vertical, Horizontal, Grid and Flow (tags that wrap). Items can have different sizes.
- **Interaction:** selection, click / double click / right click / submit events, keyboard and gamepad navigation, drag-or-scroll, snapping.
- **Separate from the game:** it lives in its own package and knows nothing about Pale Vertica. The game uses it through a few small classes listed in "Using it in Pale Vertica".

## Where it is

| What | Path |
|---|---|
| Package (embedded) | `Packages/com.globalvisualizer.core/` (shows in Package Manager as **Global Visualizer**) |
| Runtime code | `Runtime/` (asmdef `GlobalVisualizer.Runtime`, namespace `GlobalVisualizer`): `GlobalVisualizerView`, `Layout/`, `Binding/`, `Interaction/`, `Core/` |
| Editor tools | `Editor/` (asmdef `GlobalVisualizer.Editor`, namespace `GlobalVisualizer.EditorTools`) |
| Hierarchy menu | right-click in Hierarchy (or menu **GameObject**) > **Global Visualizer** > Vertical List / Horizontal List / Grid / Flow (Tags) / Create Item Template... |
| Project menu | right-click a folder > **Create > Global Visualizer > Item Template...** |
| EditMode tests | `Tests/Editor/` (asmdef `GlobalVisualizer.Tests.Editor`), test components in `Tests/Support/` |
| Samples | Package Manager > Global Visualizer > Samples > **Showcase** (scene with lessons, playground, benchmark and self test: [Showcase walkthrough](./Global_Visualizer_Showcase_Walkthrough.md)), **Basic Demo** or **Interaction Demo** > Import |
| Game glue | `Assets/Script/UI/Visualizer/VisualizerViewFactory.cs`, `Assets/Script/CoreSystem/UI/Manager/UIVisualizerFactory.cs` |

## Inputs

- A **ScrollRect** with a Viewport and a Content. The default Unity **UI > Scroll View** is fine.
- An **item prefab**: any GameObject with a RectTransform.
- **Data**:
  - a list of anything (`IReadOnlyList<T>`); or
  - just a count, when the bind code reads the data itself.

## Quick start (Editor menus)

1. **Create the list.**
   - Right-click a UI object in the Hierarchy (or empty space), then **Global Visualizer** and one of:
     - **Vertical List**
     - **Horizontal List**
     - **Grid**
     - **Flow (Tags)**
   - You get:
     - a ScrollRect (Viewport with RectMask2D, Content, one scrollbar);
     - a `GlobalVisualizerView` with that layout;
     - an inactive **Item Template** child, already set as its Item Prefab;
     - an inactive **Empty Template** child (label "Nothing to show"), already set as its Empty Prefab.
   - **Flow** gives a tag template (Image + Horizontal Layout Group + label) and sets **Size Mode = Measure**, so every tag is as wide as its text.
   - With no Canvas in the scene, a Canvas (and an EventSystem with the Input System module if available) is created. Undo removes everything.
2. **Make a real item template.**
   - With the new object selected, use right-click > **Global Visualizer > Create Item Template...**, or the **Create Item Template...** button in its Inspector.
   - Fill in:
     - **Template Name**: the prefab name. The script is `<Name>View`.
     - **Data Type**: `int`, `string`, or any public type such as `ItemInstance`.
     - **Namespace**, **Folder**.
     - **Shape**: Row = icon left, label right. Cell = icon top, label bottom.
     - **Size**.
     - **Assign To**: the visualizer that should use it.
   - Press **Create**. You get `<Folder>/<Name>.prefab` and `<Folder>/<Name>View.cs` (`IVisualItem<T>`, five regions). After compiling, the script is added to the prefab and its fields are linked. The console prints "item template '<Name>' is ready".
3. **Preview without Play mode.**
   - Inspector > **Edit Mode Preview**: set **Preview Items**, then **Show Preview**.
   - Preview objects are never saved. They are removed by **Clear Preview**, before Play mode, before script reloads and when the scene closes.
4. **Check the layout in the Scene view.** The selected visualizer draws:
   - the viewport (green);
   - the buffer (orange);
   - every alive item (blue);
   - a label with the visible range and the view counts.
5. **Bind your data from code** (step 5 below).

## Steps (manual setup and code)

1. **Add the component.** Select the Scroll View root (the object with ScrollRect), then Add Component > **UI > Global Visualizer**.
   - The component takes over the Content: top-left anchors, and any LayoutGroup or ContentSizeFitter there is disabled with a warning.
   - From code, `VisualizerViewFactory.AttachTo(content, prefab)` does the same and copies the old LayoutGroup's settings (see "Using it in Pale Vertica").
2. **Set the item prefab** (Inspector **Item Prefab**). Its root size is used when Item Size is 0.
3. **Choose the layout** (Inspector **Layout**).
   - **Kind**:
     - Vertical;
     - Horizontal;
     - Grid;
     - Flow (left to right, wraps to the next row, scrolls vertically).
   - **Item Size**: pixels per item. 0 means "use the prefab size". On the cross axis of Vertical/Horizontal, 0 means "stretch to the viewport". In Flow it is the fallback size.
   - **Spacing**, **Padding**.
   - **Grid Constraint**: Fit, Fixed Columns, or Fixed Rows (scrolls horizontally).
   - **Cross Alignment** (Vertical/Horizontal): Start, Center or End for items narrower than the viewport.
4. **Choose the size mode** (Inspector **Size Mode**). Grid ignores it.

   | Mode | Sizes come from | Use for |
   |---|---|---|
   | Fixed | Item Size / prefab size | Most lists. Fastest. |
   | Provider | `view.SizeProvider = index => new Vector2(0f, height)` in code | Headers and rows, chat bubbles with known heights |
   | Measure | The bound item's Layout Group / Layout Element / text, read with `LayoutUtility` | Tags, wrapped text |

   - Unmeasured items use the average of measured ones.
   - When a measured size differs, the layout is rebuilt and the first visible item stays still.
   - Call `InvalidateSizes()` after something changes all sizes (for example the font).
5. **Give it data**, with whichever binding style you like:
   - Callback, no script on the prefab:
     ```csharp
     visualizer.SetItems(items, (view, item, index) => view.GetComponentInChildren<Text>().text = item.Name);
     ```
   - `IVisualItem<T>` on any component inside the prefab. It is called automatically and lookups are cached, so binding does not allocate:
     ```csharp
     public sealed class InventorySlotView : MonoBehaviour, IVisualItem<ItemInstance>
     {
         public void Bind(ItemInstance data, VisualItemInfo info) { ... }
         public void Unbind() { }
     }
     visualizer.SetItems(items);
     ```
   - Count only: `visualizer.SetCount(10000, (view, index) => ...)`.
   - `VisualItemInfo` has `Index`, `Owner`, `IsSelected` and `IsFocused`. A component implementing `IVisualItemState` gets `OnStateChanged(info)` when selection or focus changes, without a full rebind.
6. **Tell it when the data changes.** The visualizer never polls.

   | Call | When |
   |---|---|
   | `NotifyChanged(index)` / `NotifyRangeChanged(index, count)` | Content changed. Only visible views are rebound, and those items are measured again. |
   | `NotifyInserted(index, count)` / `NotifyRemoved(index, count)` | The list grew or shrank. Selection and focus indices shift with it. |
   | `NotifyReset()` | Many changes at once. Keeps the scroll position. |
   | `SetItems(newList)` | You have a different list object. |
   | `Clear()` | Show nothing. |

7. **Scroll from code.**
   - `ScrollTo(index, alignment)`: 0 start, 0.5 centre, 1 end. With variable sizes it corrects itself after measuring.
   - `EnsureVisible(index)` scrolls only as much as needed.
   - `ScrollPosition` reads and writes pixels.
   - `OnVisibleRangeChanged` reports the visible range.
   - **Snap To Items** (Inspector) lines an item up with the viewport edge after a drag or wheel scroll.
8. **Selection and pointer events.**
   - **Selection Mode**:
     - None: the default; clicks only raise events.
     - Single.
     - Multi: Shift-click selects a range, Ctrl-click adds or removes one.
   - API: `Select(i)`, `ClearSelection()`, `IsSelected(i)`, `SelectedIndex`, `GetSelected(list)`, `OnSelectionChanged`.
   - Events: `OnItemClicked`, `OnItemDoubleClicked`, `OnItemRightClicked` (open a context menu here), `OnItemSubmitted`. Each passes the index and the `PointerEventData`.
   - Needs **Handle Input** on (default). Each item view then gets a small `VisualItemInput` component.
   - **Focus On Click** (default on) also selects the clicked item in the EventSystem, so the keyboard continues from it. The game factories turn it off, because WASD moves the player and would otherwise move the list focus.
9. **Keyboard and gamepad.**
   - `Focus(index)` selects that item in the EventSystem. The arrow keys, d-pad or stick then move through the layout:
     - grid rows and columns;
     - flow rows (nearest item above or below);
     - list order.
   - Focus can move to items that do not exist yet: the list scrolls and creates them.
   - Submit (Enter or the south button) raises `OnItemSubmitted` and selects the item.
   - At an edge, `OnNavigateOut(direction)` fires so you can move focus to other UI.
   - A Selectable on the item root gets its own navigation turned off, so the visualizer's navigation is the only one.
10. **Drag or scroll.**
    - Set `view.DragHandler` to an `IVisualDragHandler`.
    - A drag across the scroll axis (angle above **Item Drag Angle**, default 45 degrees) goes to your handler. A drag along the axis scrolls.
    - When the list cannot scroll, every drag goes to the handler.
    - Items that already have their own drag handlers (like the game's inventory slots and skill rows) are left alone.
11. **Empty placeholder.** **Empty Prefab** is shown inside the viewport when data is set but empty, and hidden as soon as there are items. It is not shown before any data is set, or after `Clear()`.

## Using it in Pale Vertica

| Where | How |
|---|---|
| **Inventory window** (`UI/Inventory/InventoryWindow.cs`) | The prefab is unchanged. On first open, `VisualizerViewFactory.AttachTo(_inventorySlotParent, ItemSlotSquarePrefab)` puts a visualizer on the existing Scroll View and reads the Content's GridLayoutGroup (cell 80x80, spacing 25x15, padding 20/10). Slots are bound with `ItemSlotGlobal.UpdateUI`. Inventory events are coalesced to one refresh per frame. When the window is closed it only marks itself dirty, and it refreshes on open. |
| **Skill list** (`UI/Skill Manage Page/SkillManageWindow.cs`) | Same factory on `_itemContainer`. The VerticalLayoutGroup (UpperCenter, no width control) becomes a Vertical layout with the row width and **Cross Alignment = Center**. `RefreshUI` fills `_listedSkills` and calls `NotifyReset`. The selection bar is applied in bind and in `SelectSkill`. |
| **Dev Tool** (`CoreSystem/UI/DevTools/GameDevToolUI.cs`) | **Inventory page:** `ui.Visualizer(items, VisualLayoutSettings.Grid(new Vector2(118f, 122f), 8f), (cell, item, i) => BuildInventorySlot(cell, item), 0f)`. **Database page:** one Vertical list in Measure mode. `DevToolDatabaseRow.Fill` flattens the items into group headers and rows; open groups keep the old `db.<Type>.open` state keys. Clicking a header inserts or removes that group's rows in place (`NotifyInserted` / `NotifyRemoved`). Clicking a row refreshes only the detail card (region `db.card`) and rebinds the visible rows, so the list does not jump. "Add All" calls `PlayerInventory.AddItems(...)`, and "Discard All" wraps its loop in `BeginBatch` / `EndBatch`. |
| **Any UIBuilder screen** | `ui.Visualizer(items, layout, (cell, item, index) => { ...build the cell with the builder... }, height, sizeMode)`. `height` 0 fills the remaining height, like `Scroll`. Pass `VisualSizeMode.Measure` when cells differ in height. Each recycled cell is cleared and rebuilt with your builder code. Only visible cells are built. `UIVisualizerFactory.Find(element)` returns the view. |
| **Many inventory changes at once** | `inventory.AddItems(templates)`, or `inventory.BeginBatch(); ...; inventory.EndBatch();`. `OnInventoryChanged` fires once at the end, and only if something changed. |

## Expected result

- **GameObjects:** only `ActiveViewCount` are alive. `CreatedViewCount` stops growing after the first scroll pass.
- **Scrolling:** no managed allocation. The test `Scrolling_DoesNotAllocate` scrolls 20,000 steps and requires managed heap growth (`GC.GetTotalMemory`) under 1 byte per step. `GC.GetAllocatedBytesForCurrentThread` always returns 0 in Unity's Mono, so do not use it to check allocations.
- **Dev Tool "Add All"** (199 items, inventory window open):
  - one `OnInventoryChanged`;
  - at most about 35 slot objects;
  - the refresh takes far less than one 50 ms frame (test `AddAll_WithRealSlotsStaysInsideOneFrame`, with the real slot prefab).
  - Before this, every item raised an event that destroyed and recreated every slot.
- **Interaction Demo:**
  - multi-selection and highlight, focus with the arrow keys;
  - "Jump to row 1000";
  - rows of two heights (provider);
  - a tag flow sized from its text;
  - the empty placeholder ("Empty / Fill").

## Common mistakes

- **Adding a LayoutGroup or ContentSizeFitter on Content** to "help". The visualizer disables them.
- **Rebuilding the list on every change.** Mutate your list, then call one `Notify*`. For inventory, use `AddItems` or `BeginBatch` / `EndBatch`.
- **Measure mode without a layout component on the item root.** Then nothing can be measured and the estimate is used. The Inspector warns. Put a Layout Group, Layout Element or text on the root. A ContentSizeFitter alone is not enough.
- **Expecting Grid to use Size Mode.** Grid has one cell size. Use Flow for mixed sizes.
- **State kept on views** (toggles, highlight). Views are reused for other indices. Keep state in data or use the selection API, and apply it in `Bind` or `IVisualItemState`.
- **Prefab root anchors.** The visualizer sets item anchors and pivot to top-left and sizes the root. Lay out children relative to the root.
- **Refreshing from inside a layout pass.** When a LayoutGroup resizes the visualizer, its refresh waits until the canvas rebuild has finished (uGUI does not allow disabling UI during a rebuild). Your own code should also avoid calling `Notify*` from layout callbacks.
- **Calling `SetItems` with the same list after each change.** It works, but it rebinds everything. `NotifyReset()` keeps the views and the scroll position.
- **Template wizard: "type not found".** The type must be public and compiled. Use the full name when a simple name is ambiguous.
- **Hand-editing preview items.** They are temporary. Edit the item prefab.

## Tests

Package tests (`Packages/com.globalvisualizer.core/Tests/Editor`, 66 tests):

- `VisualLayoutTests`, `VisualVariableLayoutTests`:
  - rects, ranges and clamping for every layout;
  - variable heights and widths, estimates, Flow wrapping and neighbours, Grid and list neighbours;
  - cross alignment, the size table and selection math.
- `GlobalVisualizerViewTests`: virtualization, reuse, zero-allocation scrolling, binding paths, `Notify*`, `ScrollTo`, prefab swap.
- `GlobalVisualizerSizeTests`:
  - Provider and Measure sizes;
  - `ScrollTo` with measuring;
  - re-measure on `NotifyChanged`;
  - Flow placement, Grid ignoring size modes;
  - the empty placeholder.
- `GlobalVisualizerInteractionTests`:
  - single and multi selection, double and right click;
  - input relay, grid navigation and scroll-into-view, navigate-out, submit;
  - selection and focus shifting on insert and remove, `IVisualItemState`;
  - drag-or-scroll decisions and the drag relay;
  - snapping, `EnsureVisible`.
- `GlobalVisualizerEditorTests`:
  - factories for all kinds, including Flow and the Empty Template;
  - preview cleanup, script and prefab templates, type lookup.

Game tests (`Assets/Script/Editor/Tests`):

- `InventoryVisualizerTests`:
  - batch events;
  - LayoutGroup conversion, including centred fixed-width rows;
  - Add All with the real slot prefab inside one frame.
- `UIVisualizerFactoryTests`: `ui.Visualizer` builds only visible cells and rebuilds recycled cells.
- `DevToolDatabaseRowTests`:
  - closed groups show only headers;
  - an open group lists its items right after its header;
  - search hides groups without matches;
  - the state keys match the old foldouts.
- `UILayoutRegressionTests` (existing) builds every Dev Tool page, including the virtualized Database and Inventory pages.

Run them with Unity closed:

```bash
"D:/Unity/6000.4.8f1/Editor/Unity.exe" -batchmode -nographics -projectPath "F:/GameProject/Pale Vertica" -runTests -testPlatform EditMode -testResults results.xml -logFile unity.log
```

**Standalone check (P7).** A blank Unity 6 project was verified with only these packages: uGUI, the Test Framework, and this package as `file:` plus `testables` (no Input System). Results:

- the package's EditMode tests pass;
- both samples compile;
- a PlayMode smoke test runs both demos.

## Not done yet

- Stat tables (Character Inspect / Attributes) still use plain UIBuilder rows. They are short lists, so there is no gain yet.
- No game screen uses `IVisualDragHandler` yet, because inventory slots and skill rows keep their own drag code (`ItemDragHandler`, `SkillItemListUI`).
