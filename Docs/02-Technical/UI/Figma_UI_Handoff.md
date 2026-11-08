# Figma UI Handoff: Light Screens (Dev Tool, Character Inspect)

Verified: **2026-10-18** (Unity implementation added the same day) against `GameDevToolUI.cs`, `CharacterInspectWindow.cs`, `UITheme.cs`, `UIElementRegistry.asset`, `ItemRarityExtensions.cs` and the sprites in `Assets/Art/UI/New/`.

Two light-window designs built from the project's own UI art and a small set of linked components, plus the token values behind them.

- The mood and rules: [UI_Visual_Language.md](../../01-Design/UI_Visual_Language.md).
- How to work in the file and connect Talk to Figma: [Figma_Design_Workflow_Walkthrough.md](./Figma_Design_Workflow_Walkthrough.md).
- Builder API: [UIBuilder_Guide.md](./UIBuilder_Guide.md).

> History, 2026-10-18:
> - A dark-theme kit was built and then removed by the user. The game alternates white and black windows, so it does not need a separate dark theme.
> - A first version of these two screens used absolute positioning and scaled art buttons. It was replaced by the auto-layout version below.

## 1. Where it is

File: [Whiper-of-pale](https://www.figma.com/design/zZ5QnVfPMSFuiMFjpIGbOC/Whiper-of-pale), page **WockSpace** (all UI work moved there on 2026-11-08; layout in [Figma_Design_Workflow_Walkthrough.md](./Figma_Design_Workflow_Walkthrough.md#file-layout-since-2026-11-08)).

| What | Node |
|---|---|
| Section `Screens Light` (in `10 · Screens`) | [1786-22478](https://www.figma.com/design/zZ5QnVfPMSFuiMFjpIGbOC/Whiper-of-pale?node-id=1786-22478) |
| `DevTools — Light / Database` | [1786-22483](https://www.figma.com/design/zZ5QnVfPMSFuiMFjpIGbOC/Whiper-of-pale?node-id=1786-22483) |
| `Character Inspect — Light` | [1786-22629](https://www.figma.com/design/zZ5QnVfPMSFuiMFjpIGbOC/Whiper-of-pale?node-id=1786-22629) |
| `Light Screen Components` (linked components) | [1786-22479](https://www.figma.com/design/zZ5QnVfPMSFuiMFjpIGbOC/Whiper-of-pale?node-id=1786-22479) |
| Section `Design Tokens` (in `00 · Foundations`, token values only) | [1786-22133](https://www.figma.com/design/zZ5QnVfPMSFuiMFjpIGbOC/Whiper-of-pale?node-id=1786-22133) |

Each screen has a "mapping" note on its left side.

## 2. Linked components (`Light Screen Components`)

Every repeated part of both screens is an instance of one of these. They are built with auto layout and bound to `UITheme` (Light) and `Game Colors` variables. Text is changed through component properties.

| Component | Built from | Properties | Unity |
|---|---|---|---|
| `Btn / Primary` | Passive Skill Confirm (instance of `UI_Primary_Button`) | Label | registry `Primary Button` (`Button/UI_Primary_Button`); `UIVariant.Primary` |
| `Btn / Secondary` | same, base #1E1E1E, label #C9C9C9 | Label | registry `Button`; Default / Secondary. Danger action = this with the `Danger` label colour |
| `Nav Chip` | Skill Manage tab chips | Selected (true/false), Label | new registry style `Nav Button` → `pages.NavButtonStyle` |
| `Item Row` | Skill list item layout: dark icon box, caps name, subtitle, dark value box ("LEVEL" → "ID") | Selected (rarity ring), Name, Sub, Value Label, Value, Icon (instance swap) | `UICustomElement` row (background like `Page/Skill Manager Page/UI_Skill_ListIten`) |
| `Section Header` | "SUPPORT SKILL" ornament header | Title | registry `Foldout Header` / section heading |
| `Key Value` | effect-list boxes | Key, Value | `ui.KeyValue(key, value)` |
| `Tag Chip` | ACTIVE / FIRE / MELEE chips | Style (Unique, Neutral, Dark, Danger), Label | `ui.Badge(text, color)` |
| `Attr Cell` | LEVEL boxes (`UI_Num_Frame`) | Label, Value | grid cell, `Cell(72, 72)` |
| `Icon / Cube` | item icon from `UI_Item_block_Mock` | none | item icon fallback (`Icon/item.png`) |

Buttons are used at their art size (181 x 50) and are never scaled. Long labels may use a 13 px override.

## 3. Dev Tool: `DevTools — Light / Database` (`GameDevToolUI`)

Layout, all auto layout: **Body** = Side Menu | Divider | List Column | Detail, 16 px gaps, inside the window's inner panel with 16 px padding.

| Part | Design | Code |
|---|---|---|
| Window 1000 x 680 | light window skin (as in Skill Manage), header band about 84 px | Global Modal light skin = `Modal/UI_Modal_Skill_Window` (the light art; `UI_Modal_Window` is the dark one), band 94 px, from the Light UI Kit. `BuildGlobalModal(WindowTitle, ...)`, `SetMinSize(760, 440)`, `SetResizable(true)`, `SetScrollable(false)` |
| Header band | black title tag "DEV TOOL"; Dark mode toggle (`SwitchToggle`); Snapshot = `Btn / Secondary`; close | title from the skin; Light/Dark and Snapshot buttons move from the search header to the window header |
| Side Menu (width 150) | 9 `Nav Chip`, gap 8; selected = Database | `UIPageNav.SideMenu`, `NavWidth 160` (or 150), `pages.NavButtonStyle = "Nav Button"` |
| List head | "ITEM DATABASE" (caps 20) + caption "199 items · 9 types"; "Add All" = `Btn / Secondary` with a Danger label | `header.Heading(...)`, `Button(..., Danger)` |
| Search row | input (2 px #353535 border, radius 8), dark search button with the `UI_Scearch_Icon` art, Filter chip | `BuildSearchBar`: `Input().Fill()`; Clear becomes an action inside the input |
| Scroll (db.list) | `Section Header` per item type ("EQUIPMENT · 42"), `Item Row` per item (selected = Unique ring + name colour), 4 px scrollbar | `Scroll(..., 0f).Id("db.list")`, `Foldout` per type; rows replace `Grid(...).Cell(168, 44)` buttons |
| Detail (width 300) | `UI_Modal_ItemInfomation` card as background; icon box with rarity ring, name, `Tag Chip` Unique + type; `Section Header` Details; 4 `Key Value` (ID, Type, Rarity, Sockets); `Section Header` Description; text; at the bottom a full-width `Btn / Secondary` "Add One", then a row with an amount input (width 80, 1-9999, invalid text keeps the last value) and `Btn / Primary` "Add Amount" (`GameDevToolUI.ParseAddAmount`, `PlayerInventory.AddItem(item, amount)` stacks stackables and adds non-stackables one by one) | `Panel(BuildDatabaseDetail).Width(DetailWidth)`, `KeyValue`, `Label`, `Button(Secondary)`, `Input`, `Button(Primary)` |

## 4. Character Inspect: `Character Inspect — Light` (`CharacterInspectWindow`)

Layout, all auto layout: Header / Vitals / Attributes / Combat / Tags. Each section opens with a `Section Header`. Gap 14, padding 20.

| Part | Design | Code |
|---|---|---|
| Window 560 x 857 | light window skin; black title tag "INSPECT" + `#runtimeId` | `BuildGlobalModal($"Inspect #{id}")`, title "Inspect". Window 560 x 760, `FitContent` up to **560 x 860** (`MaxWindowSize`) |
| Header | portrait 104 x 104, dark frame with a rarity ring (**proposed**, not in code); name in caps (28); `Tag Chip` relation (Hostile = Danger, Ally = Success, You = Primary) + controller (AI / Player); caption "Faction · Base Lv · Job Lv" | `Title(name)`, `Badge(relation)`, `Badge(ControllerLabel())`, `Caption(...)` |
| Vitals | HP / MP / EXP bars and the BASE / JOB level frame from the Status window art (HP #00FF99, MP #7B00FF) | `Bar("HP")`, `Bar("MP")`; EXP and the level frame are additions |
| Attributes | 6 `Attr Cell`, evenly spaced | `Grid(...).Cell(72, 72)` |
| Combat | 9 `Key Value` in a 3-column wrap grid | `Grid(...).Cell(166, 36)` |
| Tags | `Tag Chip` (Boss = Unique style, Undead = Dark, Elite = Neutral) | `Badge(tag)` |

## 5. Token values (Figma variables)

The new components and text bind to collection **`UITheme`** (Light). Copied art keeps its original colours. The values follow the art and are **not applied in Unity yet**: set them on `UIElementRegistry` > Theme in the Inspector.

| Variable | UITheme field | Code now | Figma (art) |
|---|---|---|---|
| Surfaces/Panel | `Panel` | #F0F2F7 | #F2F2F2 |
| Surfaces/PanelAlt | `PanelAlt` | #DEE3EB | #D9D9D9 |
| Surfaces/Divider | `Divider` | #000000 12% | #141313 14% |
| Text/Text | `Text` | #333338 | #141313 |
| Text/TextMuted | `TextMuted` | #737885 | #616161 |
| Text/TextOnAccent | `TextOnAccent` | #FFFFFF | #FFFFFF |
| Accents/Primary | `Primary` | #3D7DEB | #141313 |
| Accents/Secondary | `Secondary` | #DBDEE6 | #BDBDBD |
| Accents/Selected | `Selected` | #3875D9 | #353535 |
| Accents/Danger, Success, Warning | same fields | #D14242, #339E61, #F2B333 | #D93636, #12A06A, #FFBB00 |

Other collections:
- `UITheme Dark`: kept for the dark windows.
- `UITheme Size`: sizes.
- `Game Colors`: rarity, damage, element, HP/MP.

## 6. Known limits

- **Novecento**: the Figma MCP cannot load it, so new caps text uses Kanit as a stand-in. In Unity use `Novecentosanswide-DemiBold SDF` for titles, labels and big numbers.
- **Starter plan**: at most 3 pages, so the work lives in sections; and 1 variable mode per collection.
- **Unity** (2026-10-18): implemented with the Global Modal and registry prefab styles, see [Light_UI_Kit_Walkthrough.md](./Light_UI_Kit_Walkthrough.md). Differences from the Figma frames:
  - Dark mode toggle and Snapshot stay in the Dev Tool search row (the window header has no slot for actions; `UIGlobalModal` was not changed).
  - The search row spans the list and the detail column (it is the shared page header), so the detail card starts below it.
  - No search icon button or Filter chip yet; Clear is a chip.
  - Inspect shows `#runtimeId` in the caption line, not next to the title tag. No portrait.
  - Foldout headers show `+` after the title when closed.
  - Title tag and close icon keep the same colours in the dark skin (white title on a black tag).
