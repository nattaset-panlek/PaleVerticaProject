# UI Visual Language (Mood Analysis)

Verified: **2026-10-18**. Based on every UI frame on the Figma page **Preview** of [Whiper-of-pale](https://www.figma.com/design/zZ5QnVfPMSFuiMFjpIGbOC/Whiper-of-pale?node-id=569-1557):

- screens: Base, Equipment, Skill Manage (+ Effect, Basic), Refinement, Socket, Class Change Mastery, Passive Skill, WorldSpace, UI_Transition_Page;
- asset sheets: Assets-Built-in, Assets Extract.

Use this page when designing new screens. Implementation details are in [Figma_UI_Handoff.md](../02-Technical/UI/Figma_UI_Handoff.md).

## 1. Mood in one line

**Pale ink on old maps.** The UI is a quiet, mature, slightly occult monochrome: black and paper-white surfaces, cartographic contour lines, celestial and gothic ornaments. Colour appears only when it means something. The flat, high-contrast UI sits on top of the HD-2D world instead of competing with it.

## 2. Motifs

| Motif | Where | Meaning |
|---|---|---|
| Topographic contour lines | window header bands, list rows, item boards, fragment medallions, transition screen | exploration, the world as a map |
| Compass star and sacred geometry | Forge Mastery and Socket Mastery emblems, transition emblem, star glyph icon | crafting, fate, power focus |
| Gothic lancet arches | Class Mastery cards | classes as sacred callings |
| Ornament rule with diamond ends and a centre notch | title frames (`UI_TitleFrame_Line`), section dividers (`UI_Deco_Line`), "SUPPORT SKILL" headers | ceremony; separates sections |
| Arrow-tipped inner bevel line | buttons (`UI_Primary_Button`), filter pills | crafted, engraved feel |
| Hexagon, diamond and circle nodes | skill tree (attribute, class, skill nodes) | node type at a glance |

## 3. Two-tone polarity

The game does **not** have one theme. Every window is either dark or light, and the opposite tone is used inside it for emphasis. That is the "white and black alternating" look.

| | Dark window | Light window |
|---|---|---|
| Surface | #141313 frame, #09090A inner panel | #E6E6E6 frame, #F2F2F2 / white inner panel |
| Header | dark band with faint white contour lines | light band with white contour lines |
| Title tag | white tag, black caps text, icon | black tag, white caps text, icon |
| Emphasis items | white tags, white selected pills, white item-slot outlines | black section bars, dark rows (`UI_Stat_Block`, `UI_Item_block_Mock`), dark level boxes, dark icon squares |
| Examples | Inventory, Equipment, Forge, Socket, Class Mastery, Passive Tree, skill info card | Skill Manage, Status, item info card |

Rules that hold everywhere:
- **Primary action = white button** with a black 4 px outline (Confirm). **Secondary = #1E1E1E button** (Cancel). This does not change with the window tone.
- **Selected state = inverted chip.** On a light window it is dark #353535 with white text (MODIFIER tab). On a dark window it is a white pill with black text (BASIC STATS).
- **One tone per window.** Use the opposite tone only for small, meaningful blocks (rows, tags, level boxes), never for a second large surface.

## 4. Colour carries meaning only

About 95% of the UI is greyscale. Colour is reserved for:

| Meaning | Colour |
|---|---|
| Item rarity: slot borders, item names | Common #D9D9D9, Uncommon #0048FF, Rare #00FFAE, Epic #8800FF, Unique #FFBB00, Crafting #FF006E, Legendary #FF0606 (code: `ItemRarityExtensions`) |
| HP / MP | #00FF99 / #7B00FF |
| Success, progress, positive effect text | teal / cyan (#25E3B7, 65% success, effect lines) |
| Element tags | e.g. Fire orange chip |
| Allocated passive nodes | blue |
| Danger and hostile | red |

There is no decorative gradient or tinted panel. The only gradients are the soft contour-texture fades.

## 5. Typography

- **Novecento Sans Wide**, uppercase and tracked: window titles (CLASS MASTERY, PASSIVE SKILL TREE), labels (LEVEL, DPS), big numbers (01, 99).
- **Kanit**: body text, descriptions, Thai text, values, button labels.
- **Large numerals** for levels and core values (BASE 12 / JOB 05, LEVEL 01).

## 6. Shape and layout

- Rounded rectangles with radius 8 to 12, a 2 px outline and an inner bevel line. Tags and buttons are pills.
- Window anatomy:
  - header band of about 84 to 94 px with the contour texture;
  - title tag at the top left, close X at the top right;
  - content inside an inset panel;
  - list and detail split by a thin vertical rule.
- Generous spacing. Thin bar scrollbars (4 px).
- Icons are bold flat glyphs, one colour (white on dark, black on light). The item placeholder is a cube.

## 7. Do and don't

| Do | Don't |
|---|---|
| Pick the window tone first, then use the opposite tone for rows, tags and level boxes | Mix two large surfaces of different tone in one window |
| Use colour only for rarity, HP/MP, success, danger, element | Colour headers, panels or buttons for decoration |
| Use caps and tracking (Novecento) for titles and labels | Use Kanit Bold caps where the art uses Novecento, except as a temporary stand-in |
| Reuse the contour texture on headers and list rows | Put the contour texture behind body text |
| Keep buttons as the white / #1E1E1E pair | Recolour buttons per theme |

## 8. Reference screens built from this analysis

In the Figma section **`Screens Light`** (page `WockSpace` > `10 · Screens`):
- `DevTools — Light / Database`: the Dev Tool (`GameDevToolUI`) as a light window.
- `Character Inspect — Light`: the Inspect window (`CharacterInspectWindow`) as a light window.

Both use only Assets-Built-in components and pieces of the example screens. Mapping: [Figma_UI_Handoff.md](../02-Technical/UI/Figma_UI_Handoff.md).
