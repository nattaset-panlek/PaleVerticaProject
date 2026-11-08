# Figma Design Workflow Walkthrough

Verified: **2026-11-08** (file layout and node ids). How to connect an AI agent to Figma (Talk to Figma plugin and the Figma MCP), and how to design a new Pale Vertica screen from the project's own UI art so it can be built with the runtime UI core (`UIBuilder`, `UIElementRegistry`, `UIGlobalModal`).

- Rules for the look: [UI_Visual_Language.md](../../01-Design/UI_Visual_Language.md).
- What exists and how it maps to code: [Figma_UI_Handoff.md](./Figma_UI_Handoff.md).

## Where to open it

- File: [Whiper-of-pale](https://www.figma.com/design/zZ5QnVfPMSFuiMFjpIGbOC/Whiper-of-pale), page **WockSpace**. Since 2026-11-08 all UI work lives on this one page, grouped in five top-level sections (see [File layout](#file-layout-since-2026-11-08)). The page **Preview** is empty.
- **Source art**, which you read and never edit:
  - `Assets / Built-in` and `Assets / Extract` (in `90 · Archive & References` > `Assets & References`);
  - the example screens (Base, Equipment, Skill Manage, Refinement, Socket, Class Change Mastery, Passive Skill) in `10 · Screens` > `Game Screens 16:9`;
  - the game's main components in `00 · Foundations` > `Game Components`.
- **New work**: the section `Screens Light` in `10 · Screens`. It holds:
  - `Light Screen Components` (the linked components);
  - `DevTools — Light / Database`;
  - `Character Inspect — Light`.
- Tokens: section `Design Tokens` in `00 · Foundations`; variables in the right panel under Local variables (`UITheme`, `UITheme Dark`, `UITheme Size`, `Game Colors`).

## File layout (since 2026-11-08)

The content of the former `Preview` page was moved to `WockSpace` (cut and paste, so those nodes got **new ids**; main components such as `Btn / Primary` kept theirs). Every item sits inside one of these sections, left to right, top-aligned, 200 px apart. `WS /` and `PV /` prefixes were dropped; old WockSpace content is marked `(Legacy)`.

| Section | Id | Contents |
|---|---|---|
| `00 · Foundations` | `1786:34634` | `Design Tokens` `1786:22133`, `Game Components` `1614:18422`, `Dark UI Kit` `1786:27294`, `Shop Components` `1786:29914` |
| `10 · Screens` | `1786:34635` | `Game Screens 16:9` `1786:22777`, `HUD & World Space` `1786:25123`, `Screens Light` `1786:22478`, `Screens 16:9 (Legacy)` `1614:18420`, `Panels & Popups (Legacy)` `1614:18421` |
| `20 · Features` | `1786:34636` | `Merchant's Stall` `1786:29598`, `ItemCard (Classic Strip)` `1786:29856`, `ItemSlot` `1786:29942`, `System Menu` `1786:27208`, `Dark UI Mockups` `1786:27314`, `Death Screen` `1786:29078`, `Action Progress Modal` `1786:27045`, `Refine Drop Slot` `1786:27140` |
| `80 · Explorations` | `1786:34637` | `Global Progress Modal Options` `1786:26844`, `Death Screen Options` `1786:28779`, `Logo Studies` `1786:26614` |
| `90 · Archive & References` | `1786:34638` | `Assets & References` `1786:25732`, `Loose Shapes & Images (Legacy)` `1614:18423`, `Untitled 1 (Legacy)` `569:1475`, `Untitled 2 (Legacy)` `565:1891`, `Group 2367` `1786:27821`, `MTX / Focus Ring (unused copy)` `1784:84708` |

New work goes into the matching section: shared components in `00 · Foundations`, finished screens in `20 · Features`, option boards in `80 · Explorations`.

## A. Connect Talk to Figma (live link to your Figma desktop)

Talk to Figma lets an agent read and edit the file **that is open in your Figma desktop**, live. It has no monthly quota. The official Figma MCP is the other route: it edits the cloud file directly and can create components and variables, but on a View seat it has a read quota.

1. **Bun** (once): `npm install -g bun`, then check with `bun --version`.
2. **Socket server** (each work session, keep it running in a terminal):
   ```bash
   bunx cursor-talk-to-figma-socket
   ```
   Expected output: `WebSocket server running on port 3055`.
3. **Plugin** in Figma desktop:
   - Open the file, then Resources (Shift+I) > Plugins > **Cursor Talk To Figma MCP Plugin** > Run.
   - Keep port 3055 and press **Connect**. The plugin shows a channel name, for example `flowux`. The socket log also prints `Client joined channel "<name>"`.
4. **MCP server for Claude Code** (once, already done for this project): `~/.claude.json` > `projects` > `F:/GameProject/Pale Vertica` > `mcpServers` contains
   `"TalkToFigma": { "command": "bunx", "args": ["cursor-talk-to-figma-mcp@latest"] }`.
   New sessions get the `TalkToFigma` tools. Start by calling `join_channel` with the plugin's channel name.
5. **Check it works**: run `get_document_info`. It should return the current page (`WockSpace`) and its sections. Then run `export_node_as_image` on a frame and look at the PNG.

What you then see is your desktop's copy of the file. Always check final designs with an export from here: that is what the team sees.

**If exports or `get_node_info` fail with "Error executing command" or "Node not found"**, your desktop file is out of sync with the cloud file. It is missing nodes that the Figma MCP created. Close and reopen the file tab, run the plugin again and press Connect. Do not save while a stale tab is open: it can bring deleted layers back.

## B. Design a new screen

1. **Read the screen's code.** List every element and its builder call, plus the window size, min size and max size.
2. **Pick the window tone.** Light (paper, as in Skill Manage and Status) or dark (as in Inventory and the Mastery screens). Use the opposite tone only for small emphasis blocks.
3. **Frame.** 1920 x 1080, backdrop #434343, inside a section of `20 · Features` on `WockSpace`. The Starter plan allows no new pages.
4. **Window shell.**
   - Light: copy the `UI_Modal_Window` frame from `Screen / Skill Manage - Effect` > `Window / Skill Manage - Effect` (`1786:23549`). Keep `Rectangle 35` and the two `Mask group`s; delete the Skill content (`Line 7`, `UI_Close_Secon_Icon`, the `Block / ...` groups and `Union`), then resize. The inner panel starts at x 6, y 84.
   - Copy the title tag `Label / Skill` (`1786:23788`), set its text to auto width, then widen the black background to fit.
5. **Body = one auto-layout frame** placed at the inner panel + 16 px:
   - columns or sections with 12 to 16 px gaps;
   - columns set to Fill;
   - lists in a clipped `Scroll (<id>)` frame.
   Never position content by absolute x/y.
6. **Use the linked components** from `Light Screen Components`: `Btn / Primary`, `Btn / Secondary`, `Nav Chip`, `Item Row`, `Section Header`, `Key Value`, `Tag Chip`, `Attr Cell`, `Icon / Cube`. Change text only through their properties. If a new repeated part is needed, add a component there first and bind its colours to `UITheme` variables.
7. **Art pieces** go in as instances (main components from `Game Components`, such as `UI_Modal_ItemInfomation`) or copies (`SwitchToggle`, `UI_Scearch_Icon`, the Status bars in `Screen / Base HUD` > `Window / Status` `1786:23056`). Inside auto layout, put card backgrounds as **absolute-position** children with Stretch constraints.
8. **Buttons stay at 181 x 50.** Never scale or resize them. For a long label, reduce the label size on that instance only (13 px).
9. **Mapping note** left of the window: one line per part with its sprite and builder call. Copy it into the handoff.
10. **Check with Talk to Figma**: export the frame from the desktop (step A5) and fix what looks off.

## C. When UITheme changes in code

Update the matching variable in `UITheme` / `UITheme Dark`, then the token table in the handoff (section 5) and its Verified date. Components follow automatically.

## Expected result

- The screen is one auto-layout tree.
- Every repeated part is an instance of a linked component.
- Colours are bound to variables.
- Buttons are at art size.
- The desktop export matches what you built.
- The handoff lists the screen.

## For AI agents

- **Figma MCP (`use_figma`)**:
  - Load the Figma skills `figma-use` and `figma-generate-design` (and `figma-generate-library` for components) first.
  - Read before writing, and never run two calls at once.
  - A failed script is rolled back completely.
- **Limits:**
  - Starter plan: 3 pages, so `createPage` fails.
  - 1 variable mode, so `addMode` fails.
  - View seat: read quota of about 20 calls a month.
  - Novecento cannot be loaded. Set `fontName` to Kanit first to edit such text.
- **IDs:**
  - Page `WockSpace` `0:1` (all work); `Preview` `569:1557` is empty. Section ids: see [File layout](#file-layout-since-2026-11-08).
  - `Assets / Built-in` `1786:25733`.
  - Inside the sections:
    - `Game Screens 16:9` `1786:22777`: every in-game window frame, named `Screen / <Name>` (Base HUD `1786:22778`, Equipment `1786:23087`, Skill Manage x3 `1786:23247` / `1786:23541` / `1786:23800`, Passive Skill `1786:24077`, Class Change Mastery `1786:24163`, Refinement `1786:24436`, Socket `1786:24776`, Transition Page A/B `1786:25081` / `1786:25100`).
    - `HUD & World Space` `1786:25123`: `Screen / World Space HUD`, `Ref / Skill Indicator Shapes`.
    - `Assets & References` `1786:25732`: `Assets / Built-in`, `Assets / Extract`, `Assets / Mock Skill Slot`, `Assets / UI Cursor`, `Ref / Global Inventory Visualizer`.
    - `Logo Studies` `1786:26614`: `Logo Study 01..10`, `Logo Mark / White`.
    - `Global Progress Modal Options` `1786:26844`: small dark, header-less progress modals for any long action (cast, interact, craft): 01 Compact text, 02 Icon slot + text, 03 Centered ornate, 04 Toast pill, 05 Corner ornate + slot, 06 Vertical card. Bar = Refinement progression bar spec (track #262626 7 px, fill #22D3EE (no glow), value #67E8F9); panel #141416 with 1 px #2B2B2E border and Topographic texture at 14 %; text is Kanit (Novecento texts cannot be edited by the plugin).
    - The former `PV / Refine Loading Modal` section no longer exists in the file.
    - Layer names inside screens follow `Window / ...`, `Panel / ...`, `Label / <text>`, `Block / <first text>`, `List / ...`, `Icon / ...`, `Scrollbar`, `Shape / WxH`, `Hidden / Shape`.
  - Source pieces: light window `1786:23549`, title tag `Label / Skill` `1786:23788`, Confirm button `Label / Confirm` `1786:24149` (in `Screen / Passive Skill`), Status bars `Window / Status` `1786:23056`, Acrxer art `Acrxer 1` `1786:23138` (in `Screen / Equipment`).
  - Section `Screens Light` `1786:22478`. Screens: Dev Tool `1786:22483`, Inspect `1786:22629`. Components frame `1786:22479`.
  - Components: `Btn / Primary` `1541:13235`, `Btn / Secondary` `1541:13239`, `Icon / Cube` `1541:13318`, `Nav Chip` `1541:13323`, `Tag Chip` `1541:13332`, `Attr Cell` `1541:13333`, `Key Value` `1541:13336`, `Section Header` `1541:13339`, `Item Row` `1541:13368`.
  - Variables: `UITheme` `VariableCollectionId:1507:12897`, `Game Colors` `...:1507:12948`.
- **Gotchas:**
  - Scaling a group that contains an instance with masks (the art buttons) looked right in the MCP render but wrong on the desktop. Keep buttons at native size.
  - The art button's white base sits on two vectors inside the `UI_Primary_Button` instance. Recolour those layers to make the dark variant.
  - `findOne` by name can return a text layer that has the same name as a group. Match exact names or types.
  - If Talk to Figma cannot find nodes that the MCP created, the desktop tab is stale (section A).

## Common mistakes

- **Absolute positioning** instead of an auto-layout tree, which leads to misaligned and overlapping parts.
- **Detaching or editing the source art** instead of using instances and copies.
- **Two large surfaces of different tone in one window**, or colour used for decoration.
- **Scaling art buttons.**
- **Judging the design from one renderer only.** Check the desktop export.
