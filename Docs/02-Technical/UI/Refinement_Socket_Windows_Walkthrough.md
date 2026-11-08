# Refinement and Socket Windows Walkthrough

*Last verified: 2026-10-19*

This walkthrough describes the architecture, prefab configuration, asset reuse pipeline, and operation of the **Refinement Window** and **Socket Window** (16:9 HD modal layouts) matching Figma mockups.

---

## 1. Overview & Architecture

The Refinement and Socket systems provide item progression through stat upgrades (Refine level 1-20) and passive gem infusion (1-4 fragment sockets). Both windows extend `UIWindow` and are registered in `UIWindowRegistry.asset`.

### Component Architecture
- **Refinement Subsystem (`CoreSystem.Refinement`)**:
  - `RefinementRecipeDatabase`: Static registry managing dynamic material recipes (1 to 3 Resource items per recipe) mapped by equipment archetypes (Melee, Magic, Ranged, Armor, Accessory) and unique item overrides (e.g. Dragon Tooth Pendant).
  - `RefinementSystem`: Pure C# service executing validation (`CanRefine`), cost calculation (`(3000 + level * 1200) * mult`), success rate curves (+0..+4 safe 100%, +5..+7 80%, +8..+10 65%, +11..+15 45%, +16..+20 25%), inventory credit and material deductions, and `SmartEquip` auto-refresh.
- **Window Controllers**:
  - `CoreSystem.UI.Refinement.RefinementWindow`: Event-driven UI controller managing the view state, dynamic material slot binding (1-3 active slots), slider progress bar (`0.0` to `1.0`), and virtualized stat rows via `GlobalVisualizerView`.
  - `CoreSystem.UI.Refinement.RefinementCenterSlot`: Center drop target implementing `IDropHandler` and `IPointerClickHandler` accepting `EquipmentInstance` drag payloads from `GlobalDragManager`/`ItemDragHandler` with right-click unslotting.
  - `CoreSystem.UI.Refinement.RefinementStatRowView`: Reusable template row for `GlobalVisualizerView` displaying physical/magic power, attribute bonuses, and conditional refine perks (`[ACTIVE]` vs `[REQ. +N]`).
  - `CoreSystem.UI.Socket.SocketWindow`: Manages socket slot selection, fragment infusion, socket indicators, and credit consumption.
- **Economic Model**:
  - Currency: `Xeny Credit` (`ItemDatabase` ID `1000`, max stack `999,000,000`).
  - Materials: 1 to 3 `Resource` items from `Item.json` (e.g. Iron Ore, Coal, Hardwood, Magic Dust, Thread, Leather, Silver/Gold Ore).
  - Validation: Handled via `RefinementSystem.CanRefine(equipInst, inventory, out string failReason)`.
- **Decoupled Button & Visual Systems**:
  - `Rate Bar Background` utilizes native `UnityEngine.UI.Slider` (`m_Interactable: 0`, `LeftToRight`).
  - `Refine Button` drives the visual frame with baked schematic typography cleanly decoupled from ghost overlays.
  - `Stat Container` integrates `GlobalVisualizerView` (`Packages/com.globalvisualizer.core`) with clamped scrolling and scrollbar integration.

---

## 2. Prefab Structure & Assets

Both prefabs reside in `Assets/Prefab/Game Page/`:

| Prefab Name | GUID | Root Class |
|---|---|---|
| `Refinement Window.prefab` | `c18e1001a11144448888222233334444` | `RefinementWindow` |
| `Socket Window.prefab` | `c28e2002b22255559999333344445555` | `SocketWindow` |

### Shared & Reusable Assets
- **Primary Button Base**: `Assets/Art/UI/New/Button/UI_Primary_Button_Base.png` (GUID: `3e18a202d44f8101a18c89b251a37c99`)
- **Header & Pill Frames**: `Assets/Art/UI/New/Rect/rounded.png` (GUID: `18006f48e9dfbd34080ff908f68dc7a6`, sliced 9-slice)
- **Modal Backgrounds**:
  - Refinement: `Assets/Art/UI/New/Refinement/UI_Refinement_Background.png`
  - Socket: `Assets/Art/UI/New/Socket/UI_Socket_Background.png`
- **Stat Cards**: `Assets/Art/UI/New/Frame/UI_Stat_Card_Light.png` (GUID: `772bd3052af644547bd7dfa7dd569f8b`)
- **Dials / Mandalas**:
  - Refinement: `Assets/Art/UI/New/Refinement/UI_Refinement_Sunburst.png` (GUID: `4c3042e015830010111da33a43be481e`)
  - Socket: `Assets/Art/UI/New/Socket/UI_Socket_Mandala.png` (GUID: `e87674ca24bb978f5209f15c7da1b94d`)
- **Socket Slot Frames & Icons**:
  - Fragment Slot: `Assets/Art/UI/New/Page/Skill Manager Page/UI_Fragment_Slot.png` (Sprite 0: fileID `'-4362983358659938716'`)
  - Gem Icon: `Assets/Art/UI/New/Icon/UI_Fragment_Gem_Icon.png` (GUID: `5a10f011d88bbcc11223344556677881`)
  - Link Icon: `Assets/Art/UI/New/Icon/UI_Fragment_Link_Icon.png` (GUID: `5a20f022d88bbcc11223344556677882`)

---

## 3. How to Open the Windows

### Programmatic Opening (Runtime)
```csharp
// Open Refinement for equipped talisman or specific item
var talisman = GameData.Player.Equipment.GetEquipment(EquipmentSlot.Talisman);
UIManager.Instance.Open("Refinement", talisman);

// Open Socket window
UIManager.Instance.Open("Socket", talisman);
```

### Editor Snapshot & Build Tools
- **Menu Items**:
  - `Pale Vertica > UI > Build Refinement and Socket Windows`: Builds prefabs and registers them in `UIWindowRegistrySO`.
  - `Pale Vertica > UI > Take Snapshots of Refinement and Socket`: Renders 1920x1080 preview renders to `Logs/UIDebug/`.
- **Command Line (Batchmode)**:
```powershell
$p = Start-Process "D:\Unity\6000.4.8f1\Editor\Unity.exe" -ArgumentList "-batchmode", "-quit", "-projectPath", "`"F:\GameProject\Pale Vertica`"", "-executeMethod", "CoreSystem.Editor.UI.RefinementAndSocketPrefabBuilder.TakeSnapshotsBatch", "-logFile", "`"F:\GameProject\Pale Vertica\snapshot_batch.log`"" -PassThru; $p.WaitForExit()
```

---

## 4. Visual Verification Pipeline

A dedicated automated visual comparison pipeline is available in `Tools/UICompare/`:
1. `Tools/UICompare/compare_ui.py`: Loads Unity render snapshots and compares against Figma mockup previews. Computes crop similarity percentage and outputs side-by-side heatmaps:
   - `refinement_comparison.png`
   - `socket_comparison.png`
2. **Executing Comparison**:
```powershell
python Tools/UICompare/compare_ui.py
```

---

### Refine button flow (loading bar)
1. **Refine** in `RefinementWindow` first checks `RefinementSystem.CanRefine`. If it fails, the old behaviour runs (message, no loading).
2. Otherwise the button is disabled and `RefinementProgressModal` (`Assets/Script/UI/Refinement/RefinementProgressModal.cs`) opens as a global modal with the header **Refine**: "Refining...", the item name and level, and a progress bar that fills for `LoadingSeconds` = 0.5 s (fast to 90 %, then a slower last 10 %). It has no close button and cannot be dismissed.
3. When the bar is full, `RefinementSystem.ExecuteRefine` runs (cost and materials are taken and the roll is made at this moment, not on click).
4. The modal closes itself right away (no result screen). The Refinement window refreshes, so the new level (or the unchanged level after a fail) shows there.
5. Change the duration in `RefinementProgressModal.LoadingSeconds`; the curve is `EaseProgress` (covered by `RefinementProgressModalTests`).

## 4b. Socket Window matched to Figma "Screen / Socket" (2026-10-20)

Reference: Figma frame **Screen / Socket** (`1786:24776`). The window is 1083 x 705 and its top-left is at frame position 394, 189, so its centre is at 935.5, 541.5.

### Where and how

- **Builder:** `RefinementAndSocketPrefabBuilder.BuildSocketWindow()` (`Assets/Script/Editor/UI/RefinementAndSocketPrefabBuilder.cs`). It rebuilds the whole prefab from code.
  - Batch, socket only: `CoreSystem.Editor.UI.RefinementAndSocketPrefabBuilder.BuildSocketBatch`. It builds the window and registers it.
  - Menu: **Pale Vertica > UI > Build Refinement and Socket Windows**. This rebuilds both windows.
- **Snapshot:** `UISnapshotRenderer` renders `socket_window` at the Figma position (anchoredPosition -24.5, -1.5, backdrop #434343) to `Logs/UIDebug/socket_window.png`. The layout report is `socket_window.txt`.
- **Full loop with Unity closed:**
  1. `Tools/FigmaToUnity/unity_batch.sh CoreSystem.Editor.UI.RefinementAndSocketPrefabBuilder.BuildSocketBatch CoreSystem.Editor.UI.UISnapshotRenderer.RenderAllBatch`
  2. `python -I Tools/FigmaToUnity/compare.py figma_socket.png Logs/UIDebug/socket_window.png --box 388 183 1483 900`

### What the builder sets (all window-relative, from Figma node data or pixels of the export)

| Part | Value |
|---|---|
| Background | `UI_Socket_Background.png` is a 2x export with a 3 px black stroke outside the Figma frame, so the Image is inset -3 px on every side (1089 x 711) |
| Title tag | 17, 18, 266 x 40; text Kanit Bold 24, **UpperCase** (no faux Bold), box 56, 0, 204 x 40, middle-left |
| Mandala | `UI_Socket_Mandala.png` is the Figma group itself (758 x 758), so it is placed at -93, -26 at its full size. Its ring is 492 px wide, as in Figma |
| Center item slot | 247, 311, 79 x 79. Built from `Rect/rounded.png`: #0048FF frame with radius 16 (pixels per unit 21/16), and a #0A0A0B fill inset 5 px with radius 11. Item icon 37 x 37 centred (default sprite `UI_Null_Icon`, replaced by the item's icon). `UI_HasSocket_ItemSlot_Icon` 14 x 14 at 56, 57 |
| Rarity pill | 622, 33, 131 x 35, #0048FF; text Novecento DemiBold 20, character spacing -3, UpperCase |
| Item name | 622, 82, box 404 x 44; Novecento DemiBold 32, spacing 6.25 (2 px / 32 px), UpperCase, no wrap, **Ellipsis** ("DRAGON TOOTH PEN...") |
| Item type | 622, 128; Novecento Normal 16, spacing -3.75, UpperCase |
| Socket slots | 90 x 90 at 703/862, 208/353. Backing Circle 70 x 70 = Figma `Ellipse 21` (64 px #D9D9D9 + 3 px black stroke outside), exported as `Art/UI/New/Socket/UI_Fragment_Backing_Circle.png`. Fragment Icon 48 x 48, tinted black. Active Highlight 102 x 102, `UI_Fragment_Slot_Highlight.png` (white outline of the slot shape, 3 px outside it) tinted #FAFF00 |
| CONSUMPTION | 622, 545, box 200 x 22; Novecento Normal **15** (Figma says 18, but its caps are 10 px tall; the project font needs 15 for the same size) |
| Credits | right edge 1016, top 540, box 200 x 30; Novecento Normal **21** (Figma 24, caps 14 px) |
| Infuse button | 732, 605, 290 x 50, clean button art and live text Noto Sans SemiBold 20 at +26, +1 |

### Expected result

- `socket_window.txt`: 0 errors. Five warnings are by design:
  - the item name is truncated, as in Figma;
  - the four 90 px art socket buttons are larger than the theme control height.
- Mean difference against the Figma export: 2.82 (was 2.95 before the cost text fix).

### Known differences

- **Center slot item icon:** the project's default icon is an outlined cube; Figma shows the solid `FaCube` placeholder. Real items show their own icon.
- **Credits digits:** "5,000" digits render shorter than in Figma (Novecento font difference, see the playbook).
- **Infuse button decoration:** the line with two circles sits about 8 px further right than in Figma. It is baked into the shared `btn_primary_clean_290x50.png`. `UI_Primary_Button_Socket.png` has it in the right place but also bakes in the label, so it is not used. A text-free export of the Figma button would remove this difference.

## 4c. Refinement center drop slot states (Figma design, 2026-10-20)

Design only; Unity is not changed yet. Figma section **Refine Drop Slot** (`1786:27140`, page `WockSpace` > `20 · Features`). Each state is a 440 x 440 frame (`Drop Slot / <State>`) holding a copy of the dial from **Screen / Refinement** (`Dial / Sunburst` 1786:24567, `Ellipse 14` 1786:24563). The slot is 79 x 79, radius 19, at the dial centre, the same place as `ItemSlot` (1786:24637) in the screen.

Runtime owner: `RefinementCenterSlot` (`Assets/Script/UI/Refinement/RefinementCenterSlot.cs`) already handles drop (`OnDrop` -> `SetTarget`) and right-click clear. States 02 to 04 need new listeners (drag start/end from `GlobalDragManager`, pointer enter/exit on the slot).

| State | Frame | Slot | Icon / content | Pill (on slot bottom edge, 18 px, radius 5, Novecento DemiBold 12) | Dial |
|---|---|---|---|---|---|
| 01 Empty | `1786:27143` | fill #0A0A0A, 2 px #2E2E2E | plus 22 px #4A4A4A | "DROP ITEM", white / black text | unchanged |
| 02 Dragging equipment (anywhere on screen) | `1786:27156` | fill #0A0A0A, 2 px #22D3EE dashed 7/5 | plus #22D3EE | "DROP HERE", #22D3EE / black | ring stroke #22D3EE 80 % |
| 03 Hover, can refine | `1786:27169` | fill #06181B, 3 px #67E8F9, drop shadow #22D3EE 70 % blur 24 | down-to-tray arrow 26 px #67E8F9 | "RELEASE", #67E8F9 / black | radial disc #22D3EE 35 % -> 0 (200 px), ring #67E8F9, sunburst #A5F3FC |
| 04 Hover, not equipment | `1786:27185` | fill #1A0606, 3 px #FF0606, drop shadow #FF0606 70 % blur 24 | ban 26 px #FF4D4D | "EQUIPMENT ONLY", #FF0606 / white | ring #FF4D4D 60 % |
| 05 Filled | `1786:27199` | rarity colour frame 5 px (Uncommon #0048FF) + #0A0A0B fill, as in the Socket slot | item icon 37 x 37 centred, `UI_Equip_ItemSlot_Icon` top right | none | unchanged |

### Rules

- 02 shows on every open Refinement window while an equipment item is dragged; it ends on drop or cancel.
- 03 and 04 replace 02 while the pointer is over the slot. 04 refuses the drop (no `SetTarget`).
- Dropping on a filled slot replaces the target; right-click clears it (back to 01).
- Colours follow the existing accents: cyan #22D3EE / #67E8F9 (success text in the screen), red #FF0606.

### Known gaps

- `UI_Equip_ItemSlot_Icon` renders empty in this Figma file (also in **Screen / Refinement**), so frame 05 shows no marker.
- No art is exported yet. To build it in Unity: the slot can reuse the Socket approach (`Rect/rounded.png` frame + inset fill, recoloured per state), and the icons and pill need exports from these frames.

## 4d. Putting items in (2026-10-20)

Both windows open empty and take items by drag and drop or by the inventory right-click entry **Insert to Refine** / **Insert to Socket** / **Insert Fragment**. Socket fragments are dropped on the socket slots and staged until **Infuse Socket**. Full rules: [Item Insert Target Walkthrough](./Item_Insert_Target_Walkthrough.md).

## 5. Common Mistakes & Troubleshooting

1. **64-bit Integer FileID Truncation**:
   - `UI_Fragment_Slot` uses 64-bit internal sprite IDs (`-4362983358659938716`).
   - *Fix*: Always treat Unity fileIDs as string literals in JavaScript / Python generators to avoid floating point truncation.
2. **Missing Sprite Fallback to White Square**:
   - For single-sprite textures (`spriteMode: 1`), Unity's YAML sprite fileID is always `21300000`. Do not use internal texture IDs from `spriteSheet` meta fields.
3. **Button Text Wrapping**:
   - Ensure `m_enableWordWrapping: 0` is set on single-line button text components with `m_HorizontalAlignment: 2` and `m_VerticalAlignment: 512` (Center/Middle).
4. **Credit Deduction Failure**:
   - Refine and Socket actions require `Xeny Credit`. Ensure player inventory has sufficient credit instance balance or use `GameData.Player.Inventory.AddCredit(amount)` during testing.
5. **Refine button stays grey after closing the loading modal early**:
   - The modal is not dismissable, but if something closes it (scene change, `UIManager.Close("Refining")`) before the bar finishes, no refine happens and the button refreshes the next time the window opens.
