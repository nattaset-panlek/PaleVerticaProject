# Light UI Kit Walkthrough (Global Modal skin + registry prefab styles)

Verified: **2026-10-18** against `LightUIKitGenerator.cs`, `UIGlobalModal.cs`, `UIBuilderContext.cs`, `GameDevToolUI.cs` and `CharacterInspectWindow.cs`.

The light screens from Figma (Dev Tool, Character Inspect) are built from the existing **Global Modal** plus a set of **registry prefab styles** made from the UI art in `Assets/Art/UI/New/`. The prefabs are produced by one editor command. This page explains how to run it, what it changes and how to use the styles from code.

- Design source and mapping: [Figma_UI_Handoff.md](./Figma_UI_Handoff.md).
- Builder API: [UIBuilder_Guide.md](./UIBuilder_Guide.md).
- Mood and rules (white and black windows, colour only for meaning): [UI_Visual_Language.md](../../01-Design/UI_Visual_Language.md).

## 1. Where to open it

Unity menu **Pale Vertica > UI > Build Light UI Kit**.

From code: `CoreSystem.Editor.UI.LightUIKitGenerator.Build()` (returns a log string, no dialog).

With Unity closed (batch, used by AI agents), build the kit and then render the snapshots:

```
"D:/Unity/6000.4.8f1/Editor/Unity.exe" -batchmode -projectPath "F:/GameProject/Pale Vertica" -executeMethod CoreSystem.Editor.UI.LightUIKitGenerator.BuildBatch -logFile kit.log
"D:/Unity/6000.4.8f1/Editor/Unity.exe" -batchmode -projectPath "F:/GameProject/Pale Vertica" -executeMethod CoreSystem.Editor.UI.UISnapshotRenderer.RenderAllBatch -logFile render.log
```

## 2. Inputs

| Input | Path |
|---|---|
| Light window art (9-slice L/B/R 26, T 112, Full Rect mesh) | `Assets/Art/UI/New/Modal/UI_Modal_Skill_Window.png` |
| Dark window art (9-slice L/B/R 28, T 116, Full Rect mesh) | `Assets/Art/UI/New/Modal/UI_Modal_Window.png` |
| Item card art (9-slice L/R 18, B 25, T 75, Full Rect mesh) | `Assets/Art/UI/New/Modal/UI_Modal_ItemInfomation.png` |
| Button art (used for both buttons) | `Button/UI_Primary_Button.png` |
| Shapes and ornaments | `Rect/rounded.png`, `Page/Skill Manager Page/UI_Deco_Line.png`, `Frame/UI_Num_Frame.png` |
| Icons | `Icon/UI_StarGylp_Icon.png` (title tag), `Icon/item.png` (item fallback) |
| Fonts | Kanit Regular / Medium / SemiBold / Bold SDF, `Novecentosanswide-DemiBold SDF` (window title, level numbers) |
| Targets | `Assets/Prefab/UIElementRegistry.asset`, `Assets/Prefab/Game Page/Global/Global Modal.prefab` |

The borders above are already set in the `.meta` files. If you replace the art, keep the same borders (Sprite Editor) or the window corners and header band will stretch.

## 3. Steps

1. Open the project and wait for scripts to compile (no red errors in the Console).
2. Run **Pale Vertica > UI > Build Light UI Kit**. If the kit already exists, confirm the dialog: it overwrites the kit prefabs.
3. Read the Console log: it lists every prefab it saved, then "Registry entries and light theme updated." and "Global Modal: light skin, header band, title tag and content insets applied."
4. Run **Pale Vertica > UI > Render UI Snapshots** and open `Logs/UIDebug/`:
   - compare `devtool_01c_Database_selected.png` (an item selected) and `inspect_character.png` with the Figma frames (nodes `1543-13236` and `1544-13118`);
   - check every `.txt` file: the screens should report 0 errors (the death screen has one warning, see section 6).
5. In Play mode press **K** for the Dev Tool, or right-click a character > Inspect.

## 4. What it creates

Prefabs in `Assets/Prefab/Game Page/Global/UIElement/Light/`, registered in `UIElementRegistry`:

| Registry style (type) | Prefab | Look | Used by |
|---|---|---|---|
| `Primary Button` (Button) | `Art Primary Button` | white art button, label #676363, min 181 x 50 | `Button(label, onClick, UIVariant.Primary)` and `Selected` |
| `Button` (Button) | `Art Secondary Button` | same art tinted #1E1E1E, label #C9C9C9, min 181 x 50 | `Button(label, onClick)`, `Secondary`, `Danger/Success/Warning` (label colour only) |
| `Nav Button` (Button) | `Nav Chip` | light chip #D9D9D9, caps 12; selected #353535 + white | `pages.NavButtonStyle = "Nav Button"`, list selections |
| `Chip` (Button) | `Nav Chip` | same chip | small actions in dense rows (+, -, Eq 1, Discard) |
| `Foldout Header` (Button) | `Section Header` | ornament line with an outlined pill | every `ui.Foldout(...)` header |
| `Section Header` (Custom) | `Section Header` | same, not clickable | `ui.SectionHeader("DETAILS")` |
| `Key Value` (Custom) | `Key Value` | light box, key left (muted), value right (bold) | every `ui.KeyValue(key, value)` |
| `Item Row` (Custom) | `Item Row` | dark icon box, caps name, subtitle, dark value box | Dev Tool database list |
| `Attr Cell` (Custom) | `Attr Cell` | dark 72 x 72 cell, label + big value | Inspect attributes |
| `Level Frame` (Custom) | `Level Frame` | `UI_Num_Frame` with BASE / JOB numbers | not used since 2026-11-07 (Inspect VITALS uses `Status Vitals`, see [Status Vitals Element Walkthrough](./Status_Vitals_Element_Walkthrough.md)) |
| `HP Bar`, `MP Bar`, `EXP Bar` (ProgressBar) | same names | outlined bar, fill #00FF99 / #7B00FF / #141313 | `ui.ProgressBar("HP Bar", value)` |
| `Item Card` (Container) | `Item Card` | `UI_Modal_ItemInfomation` card (tiled header), padding 18 / 70 top | `ui.Div("Item Card", ...)` |
| `InputField` (InputField) | `Search Field` | 2 px #353535 outline, radius 8 | every `ui.Input(...)` |

Custom element keys (`UICustomElement`, set with `SetText` / `SetSprite` / `SetColor`):

| Style | Keys |
|---|---|
| `Section Header` | `Title` |
| `Key Value` | `Key`, `Value` |
| `Item Row` | `Outline` (border colour), `Body` (row fill, darker when selected), `Icon`, `Name`, `Sub`, `ValueLabel`, `Value` |
| `Attr Cell` | `Label`, `Value` |
| `Level Frame` | `Base`, `Job` |

It also:
- sets the light **Theme** on the registry to the Figma values (Panel #F2F2F2, PanelAlt #D9D9D9, Text #141313, TextMuted #616161, Primary #141313, Secondary #BDBDBD, Selected #353535, Danger #D93636, Success #12A06A, Warning #FFBB00). The dark theme is not changed;
- updates the **Global Modal** prefab (no code change in `UIGlobalModal`):
  - Background uses the light window art, Image type **Tiled**, so the contour header is cropped, not squashed, in narrow windows;
  - Light skin = light art, Dark skin = dark art; the title is white in both, the close icon is #141313 (light) or #E6E6E6 (dark);
  - `Top Navigator` is 94 px high; the title sits in a black **Title Tag** (star icon, Novecento caps 24) at the top left; the close button is 56 px at the top right;
  - `Content Container` insets: left 16, right 16, top 100, bottom 16 (inside the art's inner panel).

## 5. Using the styles in code

```csharp
ui.SectionHeader("DETAILS");
ui.KeyValue("Rarity", item.Rarity.ToString());
ui.Button("Add To Inventory", Add, UIVariant.Primary);
ui.Button("Add All", AddAll, UIVariant.Danger);
ui.Button("Chip", "+10", Increase);
ui.ProgressBar("HP Bar", hp / maxHp);
ui.Custom("Attr Cell").SetText("Label", "STR").SetText("Value", "24");
```

- **Keep Prefab Look** (registry entry, Builder Layout section) marks an art style: the builder keeps the prefab's colours, label font size and layout padding. With it, `Button(label, onClick, variant)` recolours only the label for Danger / Success / Warning, uses the Primary art for Selected, and makes Ghost transparent. Explicit `.Variant(...)` calls still recolour the background (that is how nav chips show the selected state).
- A custom nav style keeps its prefab colour for pages that are not selected (only the selected page gets `UIVariant.Selected`).
- `ui.Badge(text, color)` picks dark text on light colours (Unique yellow) and white text on dark ones; add `.CornerRadius(10f)` for a pill.

## 5b. Size rules for art styles (2026-10-20)

Every registry entry with **Keep Prefab Look** on and a Min Size or Fixed Size (art buttons, all `MH *` styles) gets a `UIStyleSizeLock` component. It is an `ILayoutElement` with **layout priority 100**, higher than `LayoutElement` (1), TMP text and images (0). Unity's layout always reads the highest priority first, so:

- **Min Size is a floor that always wins.** `Primary Button` / `Button` / `MH Button *` are at least 181 x 55. Nothing can make them smaller: not `.Size()`, `.Height()`, `.SetMinSize()` or `.SetPreferredSize()`, not a short row, not the theme `ControlHeight` (34). A larger size is still allowed, for example `.Width(300)`, or `.Fill()` in a row.
- **Fixed Size is exact** on that axis (min = preferred, flexible 0), for example `MH Menu Row` 30 px and `MH Tab Button` 34 px high.
- **Grids** ignore layout elements, so `.Cell(w, h)` and every button added to a grid grow the cell to the largest minimum (`UIStyleSizeLock.FitGridCell`).
- A parent that does not control child size (no layout group, or `childControlHeight` off) uses the RectTransform; `UIStyleSizeLock` raises `sizeDelta` to the minimum when it is attached and after each size modifier.
- A parent with `childForceExpand` can still make the element **bigger** (never smaller).

Priority order, highest first:
1. `UIStyleSizeLock` (registry Min / Fixed Size).
2. `LayoutElement` (prefab values, `FitControl`, `.Size()` and other modifiers).
3. Content (TMP preferred size, Image native size).

To make an art button smaller on purpose, lower the entry's Min Size or use a style without Keep Prefab Look.

Fixed 2026-10-20:
- `Art Primary Button.prefab` had height 5 instead of 55. `CoreSystem.Editor.UI.LightUIKitGenerator.ResizeArtButtons` resets both art prefabs to 181 x 55.
- The `MH Slider` knob is drawn with preserve aspect, so a taller slider no longer stretches it into an oval.

Tests: `UIStyleSizeLockTests`.

## 6. Common mistakes

| Symptom | Cause | Fix |
|---|---|---|
| Primary button is black or blue | the `Primary Button` entry lost **Keep Prefab Look**, so the variant tints the art | tick Keep Prefab Look on the entry, or run the kit again |
| Nav chips that are not selected are transparent | `Nav Button` style missing, so the default Ghost nav is used | run the kit; check `pages.NavButtonStyle` matches the registry name |
| Window header contour looks squashed | Background Image type is Sliced, not Tiled, or the sprite lost its border | run the kit again; check the `.meta` borders in section 2 |
| `[UICustomElement] Cannot find element with key` warnings | the prefab was edited and a key mapping removed | restore the key on the prefab's UICustomElement (table above) |
| Rows in other dev pages run off the right edge | a small action uses `Button(...)` (min 181 px art) | use the `Chip` style for small inline actions |
| Re-running the kit lost a manual prefab tweak | the kit overwrites the prefabs in the `Light` folder | put permanent tweaks in `LightUIKitGenerator.cs`, or stop re-running it after hand edits |
| The death screen shows an empty black tag | it sets an empty title; the tag keeps its star icon | give the window a title, or accept it as an emblem |
| Dark mode: black title tag on the dark window, light Key Value boxes, blue accents | the kit only themes the light windows; the dark palette was not changed | follow-up: greyscale `Dark Theme` and a white tag for the dark skin |
| Old look wanted back | registry points at the kit prefabs | point the entries back to the prefabs in `UIElement/` (`Button`, `Primary Button`, `InputField`) and revert `Global Modal.prefab` from git |
