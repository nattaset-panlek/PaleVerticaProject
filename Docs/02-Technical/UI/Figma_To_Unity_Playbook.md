# Figma to Unity Playbook (how to make a screen look exactly like the design)

Verified: **2026-10-19**. Written from the real work on the Dev Tool, Character Inspect, the Light UI Kit and the Skill Manage window (2026-10-18). Every method below was used and worked; every pitfall below happened.

This page is for an AI agent (or a new developer) who must turn a Figma frame into a Unity UI that matches it **to the pixel**. Read it top to bottom once, then use sections 3 to 9 as a recipe and section 11 as a checklist.

Ready-made scripts: `Tools/FigmaToUnity/` (`figma_tree.py`, `measure_art.py`, `prefab_tree.py`, `compare.py`, `compile_check.py`, `unity_batch.sh`). The short, step-by-step version for agents is the skill `.claude/skills/figma-to-unity/SKILL.md`; the scripts' usage is at the top of each file.

Related pages:
- How to connect Talk to Figma and where things are in the file: [Figma_Design_Workflow_Walkthrough.md](./Figma_Design_Workflow_Walkthrough.md).
- The look and its rules: [UI_Visual_Language.md](../../01-Design/UI_Visual_Language.md).
- Code-built windows: [UIBuilder_Guide.md](./UIBuilder_Guide.md), [Light_UI_Kit_Walkthrough.md](./Light_UI_Kit_Walkthrough.md).
- Prefab-built windows: [Skill_Manage_Window_Walkthrough.md](./Skill_Manage_Window_Walkthrough.md).

---

## 1. The ten rules

1. **Measure, never guess.** Every position, size, colour, font size and letter spacing comes from Figma node data or from pixels of the art. If you did not read a number from somewhere, it is wrong.
2. **Look at the result.** A UI change is not done until you have rendered it and compared it with the Figma export, image against image.
3. **Reuse the project's art.** `Assets/Art/UI/New/` already holds the sprites the designer used. Export from Figma only a shape that does not exist yet.
4. **Clone, don't rebuild.** If the window already has a header, a scroll view or a button that looks right, copy that object for the new part. It keeps the exact look for free.
5. **Data in prefabs, logic in small Views.** Colours, sprites and fonts live on prefabs and registry entries. A View component only fills text and listens to events.
6. **Build with a tool, not by hand-editing YAML.** Write an editor script that loads the prefab, changes it and saves it. It is repeatable and reviewable.
7. **Never change game formulas for the UI.** Read values through the existing APIs (for example `SkillCaster.BuildPayload`).
8. **Ask the user before each asset or code change**, and before closing their Unity.
9. **Zero layout errors.** The layout inspector and `UILayoutRegressionTests` must pass.
10. **Write the walkthrough.** A new tool is not done until `Docs/02-Technical/<System>/<Name>_Walkthrough.md` exists and is linked from `Docs/README.md`.

---

## 2. Tools you will use

| Tool | What for | How |
|---|---|---|
| **Talk to Figma MCP** | read node data, export images | socket `bunx cursor-talk-to-figma-socket` (port 3055), plugin in Figma desktop shows a channel (for example `flowux`), then `join_channel`. Tools: `get_document_info`, `get_node_info`, `export_node_as_image`, `scan_text_nodes` |
| **Python + Pillow** | measure art pixels, diff images, parse node JSON and prefab YAML | `python -I` (isolated mode) |
| **Unity batch mode** | run editor tools, render snapshots, run tests | needs Unity **closed** (see 9.1) |
| **dotnet build** | check that C# compiles while Unity is open or closed | section 9.2 |
| **UISnapshotRenderer** | render screens to PNG + layout report | `Pale Vertica > UI > Render UI Snapshots` or `RenderAllBatch` |
| **Unity MCP (AI Game Developer)** | run editor code live | optional; it was **not available** for this user, so everything below works without it |

Talk to Figma notes:
- If a call says "Must join a channel", call `join_channel` again. Its channel changes when the plugin reconnects; ask the user for it.
- `get_node_info` on a whole 1920 x 1080 frame is large (50k+ characters). The tool saves it to a file; parse it with Python (section 3.2), do not read it raw.
- `get_node_info` can time out on single vector nodes. `export_node_as_image` of the same node usually still works.
- Vector children (shapes) are often missing from node data. Export them as images to see them.

---

## 3. Read the design

### 3.1 Export the reference images

Export every frame you will build at **scale 1** (a 1920 x 1080 frame gives a 1920 x 1080 PNG):

```
export_node_as_image(nodeId="1786:23247", format="PNG", scale=1)  # Screen / Skill Manage
```

Copy each PNG from the tool-results folder to your scratchpad (for example `scratchpad/figma/skill_main.png`). These are your ground truth for the whole job. Look at them carefully and write down what each part is.

If the art folder has a mock PNG of the screen (for example `Assets/Art/UI/New/Page/Skill Manager Page/Mock/Skill Manage 16_9.png`), it is the same image; use it too.

### 3.2 Turn node data into a readable tree

Save the `get_node_info` output to a file and print a compact tree with positions **relative to the frame**:

```python
import json, sys
sys.stdout.reconfigure(encoding="utf-8")          # Thai layer names crash cp1252 otherwise
root = json.load(open(sys.argv[1], encoding="utf-8"))
box = root.get("absoluteBoundingBox", {}); ox, oy = box.get("x", 0), box.get("y", 0)
maxdepth = int(sys.argv[2]) if len(sys.argv) > 2 else 99

def paints(p):
    out = []
    for f in p or []:
        if f.get("type") == "SOLID": out.append(f.get("color", "") + (f"@{f['opacity']}" if f.get("opacity") not in (None, 1) else ""))
        else: out.append(f.get("type"))
    return ",".join(out)

def walk(n, d):
    if d > maxdepth: return
    b = n.get("absoluteBoundingBox") or {}
    s = f"{'  '*d}{n.get('name')} [{n.get('type')}] {int(b.get('x',0)-ox)},{int(b.get('y',0)-oy)} {int(b.get('width',0))}x{int(b.get('height',0))}"
    if n.get("fills"): s += f" fill={paints(n['fills'])}"
    if n.get("strokes"): s += f" stroke={paints(n['strokes'])}"
    if n.get("cornerRadius"): s += f" r={n['cornerRadius']}"
    if n.get("characters"):
        st = n.get("style", {})
        s += f" text='{n['characters'][:40]}' {st.get('fontFamily')} {st.get('fontWeight')} {st.get('fontSize')} ls={st.get('letterSpacing')} lh={st.get('lineHeightPx')} align={st.get('textAlignHorizontal')}"
    print(s)
    for c in n.get("children") or []: walk(c, d + 1)

walk(root, 0)
```

Run it with `python -I figma_tree.py node.json 14 > tree.txt`. You now have, for every element: x, y, width, height, fill, stroke, radius, and for text: family, weight, size, letter spacing, line height, alignment.

### 3.3 Find the window origin in each frame

Frames often place the window at different spots. Example from Skill Manage:

| Frame | Window top-left in the frame |
|---|---|
| Main | 150, 187 |
| Effect | 415, 187 |
| Basic | 418, 171 |

Always convert element positions to **window-relative** (element x minus window x). Store the window centre per frame; you need it for the renders (section 8.1).

---

## 4. Map Figma to Unity assets

### 4.1 Fonts

| Figma family | Unity TMP asset |
|---|---|
| Kanit Regular / Medium / SemiBold / Bold (400/500/600/700) | `Assets/Art/Front/Kanit/Kanit-<Weight> SDF.asset` (use the weight asset, not `Bold()`) |
| Novecento DemiBold, Novecento sans wide 650, Novecento Bold 700 | `Assets/Art/Front/Novecento Sans/Novecentosanswide-DemiBold SDF.asset` |

Conversions:
- **Letter spacing**: TMP `characterSpacing` is in 1/100 em. `characterSpacing = letterSpacingPx / fontSizePx * 100`. Example: 4.8 px on 32 px gives 15; 1 px on 20 px gives 5. Check the result visually: our Novecento needed 8 instead of 15 for the level numbers.
- **Line height**: Kanit's own line height is about 1.495 em (18 px gives 26.9, as in Figma), so keep `lineSpacing = 0`.
- **Caps**: if Figma shows caps but the characters are mixed case, set `fontStyle = FontStyles.UpperCase`. Do not type caps into the data.
- **Novecento lowercase is small caps.** "dmg 13" in Novecento renders as small caps and looks too small. Always UpperCase Novecento text.
- Novecento digits in this project are shorter than Figma's. That is the font, not the layout; note it as a known difference.
- Turn **off** `enableAutoSizing` on text that has a fixed size in Figma; auto-size shrinks it silently.

### 4.2 Colours

Take hex values straight from the tree (`fill=#363636`). Convert with `ColorUtility.TryParseHtmlString` or `new Color(r/255f, ...)`. Colours that carry meaning (rarity, element, HP/MP) must come from data or a serialized field, never be typed in game logic.

When Figma text has highlighted words (red "ignores 25%", orange "30% chance"), sample the exact colours from the export:

```python
from PIL import Image; from collections import Counter
im = Image.open("skill_effect.png").convert("RGB"); c = Counter()
for y in range(535, 610):
    for x in range(950, 1420):
        p = im.getpixel((x, y))
        if max(p) - min(p) > 60: c[p] += 1          # keep saturated pixels only
print(c.most_common(6))                              # gave #E00000 and #FF6A00
```

### 4.3 Shapes and sprites

1. Search `Assets/Art/UI/New/` for the art first. List sizes and borders:
   ```bash
   grep -E "spriteBorder|spriteMode|spritePixelsToUnits" "<file>.png.meta"
   ```
   `spriteMode: 2` (Multiple) keeps the border inside `spriteSheet > sprites > border`.
2. **Measure the art with pixels** to find the outer stroke, the inner panel and the header band:
   ```python
   im = Image.open("UI_Modal_Skill_Window.png").convert("RGBA"); w, h = im.size
   print([(y, im.getpixel((500, y))) for y in range(85, 105)])      # where the inner panel starts
   print([(x, im.getpixel((x, 400))) for x in range(0, 20)])        # left stroke and gap
   ```
   Example result for the light window: 3 px outer stroke, inner panel border at x 10..13 and y 94..97, so the 9-slice border is L/B/R 26, T 112.
3. **Image type**:
   - **Sliced** for frames whose middle is a flat colour.
   - **Tiled** for art with a texture in the border (the contour header): Tiled crops it at native scale instead of squashing it when the window is narrower than the art. Set `spriteMeshType: 0` (Full Rect) in the meta for tiled or bordered sprites.
4. **Rounded rectangles** without special art: `Rect/rounded.png` (42 x 42, border 21) with `Image.type = Sliced` and `pixelsPerUnitMultiplier = 21 / radiusPx`. An outline = a dark rounded Image with a lighter rounded child inset by the stroke width (or a layout group with padding = stroke width).
5. **Shape not in the project** (example: the attribute row with pointed ends): export only the shape node at scale 1, save it next to the related art, then set it to Sprite, 9-slice and Full Rect from the editor tool (section 6.4). Check the pixels once (stroke colour and width) so you know it is clean.
6. Never scale button art. Buttons use their art size (`Button/UI_Primary_Button.png` 181 x 50); long labels grow sideways through the 9-slice.

### 4.4 Art versus Figma frame size

The Figma window frame can be **smaller than the art** because Figma draws the outer stroke outside the frame. Skill Manage: the art is 1089 x 744 and the Figma frame is 1083 x 738, so **art coordinate = Figma coordinate + 3**. Check this every time: put the art's top-left 3 px inside and confirm one known element (the title tag) lands on the same pixel.

---

## 5. Choose how to build

| Situation | Build with |
|---|---|
| A tool or debug window, many rows, built from data | the code builder (`UIBuilder.BuildGlobalModal` + registry styles). See the Light UI Kit: prefab styles registered in `UIElementRegistry`, with Keep Prefab Look for art buttons |
| A game window with its own art (Skill Manage, Inventory) | a hand-made prefab, completed and tuned by an **editor builder script** |
| One small fix on an existing prefab | the same builder script approach, or ask the user to do it in the Inspector |

Do not create a new window system when one exists. The user said it plainly: "use the global modal; just make prefab styles".

---

## 6. Build a prefab page with an editor script

### 6.1 Script skeleton

`Assets/Script/Editor/UI/<Name>PrefabBuilder.cs`, static class, `#region` layout, no comments:

```csharp
[MenuItem("Pale Vertica/UI/Build <Name>")]
public static void BuildFromMenu() { if (EditorUtility.DisplayDialog(...)) Debug.Log(Build()); }

public static void BuildBatch()                       // for -executeMethod
{
    int code = 0;
    try { Debug.Log(Build()); } catch (System.Exception e) { Debug.LogException(e); code = 1; }
    EditorApplication.Exit(code);
}

public static string Build()
{
    var root = PrefabUtility.LoadPrefabContents(PrefabPath);
    try
    {
        // find parts by path, check they exist, then build
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
    }
    finally { PrefabUtility.UnloadPrefabContents(root); }
    AssetDatabase.SaveAssets();
    return log;
}
```

Rules:
- **Idempotent**: before creating "Effect Page", delete any child with that name (`Remove(parent, name)`). Running it twice gives the same prefab.
- **Touch only what you own.** Leave the rest of the hand-made prefab alone; log what you changed.
- **Fail loudly**: if a required path is missing, return a message naming it, do not half-build.
- Wire serialized fields with `SerializedObject` + `FindProperty("_field")` + `ApplyModifiedPropertiesWithoutUndo()`.
- Instantiate a nested prefab with `PrefabUtility.InstantiatePrefab(prefab, parent)` so it stays linked.

### 6.2 Place elements at Figma coordinates

```csharp
private static void Place(RectTransform rect, RectTransform window, float x, float y, float width, float height)
{
    Rect area = window.rect;
    Vector3 world = window.TransformPoint(new Vector3(area.xMin + x + ArtInset, area.yMax - (y + ArtInset), 0f));
    var parent = (RectTransform)rect.parent;
    Vector3 local = parent.InverseTransformPoint(world);
    Rect parentArea = parent.rect;
    rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
    rect.pivot = new Vector2(0f, 1f);
    rect.anchoredPosition = new Vector2(local.x - parentArea.xMin, local.y - parentArea.yMax);
    rect.sizeDelta = new Vector2(width, height);
}
```

`x, y` are the **window-relative Figma** values from your tree; `ArtInset` is the difference from section 4.4. This works whatever anchors the parents use.

### 6.3 Clone existing parts

For a new section header, copy the window's existing header (`Object.Instantiate(template, parent)`), then change only the pill width and the text. For a new scroll area, copy an existing scroll view, delete its content children, and reset:
- viewport stretched, right offset = space for the scrollbar;
- `verticalScrollbarVisibility = Permanent` if Figma always shows the track;
- scrollbar anchored right, width from Figma (4 px), track and handle colours from Figma;
- content: `VerticalLayoutGroup` (control height on) + `ContentSizeFitter` vertical preferred.

### 6.4 Prepare a sprite from code

```csharp
AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
var importer = (TextureImporter)AssetImporter.GetAtPath(path);
importer.textureType = TextureImporterType.Sprite;
importer.spriteImportMode = SpriteImportMode.Single;
importer.spriteBorder = new Vector4(24, 0, 24, 0);        // L, B, R, T
importer.mipmapEnabled = false;
var s = new TextureImporterSettings(); importer.ReadTextureSettings(s);
s.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(s);
importer.SaveAndReimport();
```

For a sprite whose meta already exists you can also edit the `.meta` text (`spriteBorder`, `spriteMeshType`) while Unity is closed; Unity re-imports it.

### 6.5 Tune sub-prefabs

Open each item prefab (list item, tag, row) with `LoadPrefabContents` and set exactly the Figma values: font, size, colour, spacing, box height, overflow mode. Find children by path (`root.Find("Skill Summry/Damage")`) and skip if missing.

---

## 7. Layout rules that bite

These all happened. Learn them before you build.

| Symptom | Cause | Fix |
|---|---|---|
| A rounded badge or chip is 64 px tall | an `Image` with a sliced sprite reports its sprite border sum as preferred size, and it wins over the layout group at equal priority | give the element a `LayoutElement` with the Figma size (priority 1 wins), or give the sprite a high pixels-per-unit |
| A pill stretches across the whole row | its own layout group has `childForceExpand` on, which gives it flexible size 1 | turn force-expand off, or set `LayoutElement.flexibleWidth = 0` |
| Tags are 40 tall instead of 30 | same flexible-size leak in the height | `LayoutElement` with `flexibleHeight = 0` and preferred 30 |
| `NestedFitter` error | a `ContentSizeFitter` inside a layout group | remove the fitter; let the parent layout group control the size (`childControlWidth/Height = true`) |
| A text line vanished | TMP **Ellipsis/Truncate** hides a line taller than its box | make the box at least the font's line height (Novecento 16 needs about 24) |
| Text overflows its box (`TextOverflow`) | fixed box shorter than the line height | raise the box height; align to the side Figma uses (top or bottom) |
| `SiblingOverlap` errors | negative spacing in a layout group | spacing 0; shrink font or padding instead |
| A text looks smaller than in Figma | `enableAutoSizing` is on, or Novecento lowercase | turn auto-size off; use UpperCase |
| Label padding ignored on art buttons | the builder's `FitControl` overwrote prefab padding and font size | mark the registry entry **Keep Prefab Look** |
| Art button turned black or blue | a variant tinted the art | Keep Prefab Look; variants only recolour the label |
| A tag row shows a scrollbar line | the horizontal scrollbar of the tag scroll view | disable it and unassign it from the ScrollRect |
| List jumps to the bottom when a section opens | scroll position restored from a list that used to fit | do not restore scroll for content that fitted (fixed in `UIDocument`) |
| Window header texture squashed | Sliced image on a narrow window | Tiled image + Full Rect mesh |
| Lowercase letters render as full capitals (Novecento Normal `99.99s`) | the project's `Novecento Normal.ttf` has no small-cap lowercase, unlike the font Figma uses | measure the glyph height in the Figma export and use rich text `<size=66%>s</size>` |
| A Noto Sans Thai label vanishes in the render | TMP Ellipsis hides a line taller than its 24 px Figma box | `overflowMode = Overflow` (or a taller box) |
| Novecento Normal text is about 15 % bigger than Figma at the same font size (Socket CONSUMPTION, credits) | the project's Novecento Normal has taller caps than the font Figma uses | measure the cap height and width in the export and scale the font size (18 -> 15, 24 -> 21 gave a pixel match) |
| Art from an earlier agent has placeholder content baked in (a cube icon, a hidden "modified" badge) | the slot was exported with its children | rebuild the frame from `Rect/rounded.png` (frame + inset fill) and put the live icon on top; never draw a real icon over a baked one |
| A builder that worked before now produces white or missing sprites | art was moved (for example `Art/UI/Refinement/` to `Art/UI/New/Refinement/`) but the builder kept the old path | resolve every sprite GUID in the prefab to its file before rebuilding, and fix the builder constants |
| Text in Unity is Title Case but Figma shows CAPS (ItemCard name, labels, column headers) | the Figma text has `textCase: UPPER`; the characters are stored mixed case | read `textCase` with `run_script` and set `FontStyles.UpperCase` on the TMP text (`TITLE` = type it in Title Case) |
| A batch run crashes at start with `pure virtual function` / thread assertions in the log | `timeout` (or any kill) stopped `unity_batch.sh` while Unity was starting | run `unity_batch.sh` in the background without a timeout; never kill it mid-run |
| A batch run keeps compiling and never reaches the builder | scripts were added or edited while the batch Unity was running | do not touch `Assets/` scripts until the batch finishes; edit docs meanwhile |

Layout group cheat sheet:
- `childControlWidth/Height = true`: the group sets the child's size from min / preferred / flexible.
- `childForceExpand = true`: children get flexible 1; usually **off** for designs with fixed sizes.
- Cross axis: size = clamp(available, min, flexible > 0 ? available : preferred).
- `LayoutElement` has priority 1 and overrides Images and groups (priority 0).
- Fixed Figma size: `LayoutElement` min = preferred = that size, flexible = 0.

---

## 8. Verify like a machine

### 8.1 Render at the Figma position

`UISnapshotRenderer` (`Assets/Script/Editor/UI/UISnapshotRenderer.cs`) renders 1920 x 1080 PNGs into `Logs/UIDebug/` plus `.issues.png` and `.txt`. For a prefab page, `ProcessPrefab`:
- instantiates the prefab on a canvas with reference 1920 x 1080 and match height;
- sets `anchoredPosition` = Figma window centre minus (960, 540), y flipped (Skill Manage main: (-268.5, -16));
- uses the Figma backdrop colour (#434343);
- calls the window's setup with a preview player that has granted skills, selects a tab, and calls the page Views' `Render` (in edit mode `OnEnable` does not run).

Add each new screen or tab as a snapshot entry. Then the PNG and the Figma export have the same pixel grid.

### 8.2 Compare images

Blend and zoom (Pillow):

```python
from PIL import Image, ImageChops
a = Image.open("figma/skill_main.png").convert("RGB").resize((1920, 1080))
b = Image.open("Logs/UIDebug/skillmanage_1_Modifier.png").convert("RGB")
box = (140, 175, 1245, 935)                                      # the window area
Image.blend(a, b, 0.5).crop(box).save("blend.png")               # aligned parts look sharp, misaligned parts look doubled
d = ImageChops.difference(a, b).convert("L").crop(box)
print(sum(d.getdata()) / (d.size[0] * d.size[1]))                 # mean difference, should drop each iteration

for i, area in enumerate([(175, 375, 610, 480), (780, 300, 1200, 390)]):
    w, h = area[2] - area[0], area[3] - area[1]
    c = Image.new("RGB", (w * 2, h * 4))
    c.paste(a.crop(area).resize((w * 2, h * 2)), (0, 0))         # Figma on top
    c.paste(b.crop(area).resize((w * 2, h * 2)), (0, h * 2))     # Unity below
    c.save(f"zoom_{i}.png")
```

Read the blend first (layout), then the zooms (fonts, sizes, spacing, colours). Write down every difference with its pixel size ("DPS text 4 px too low, about 3 px smaller"), find the cause in the prefab, fix it in the builder, rebuild, re-render. Repeat until the only differences are data (names, icons) or known font limits.

Data differs between Figma and the preview (Figma says "Dragon Tooth Pendant", the preview player has "Tackle"). Compare positions, sizes and styles, not the words.

### 8.3 Read the layout report

`Logs/UIDebug/<name>.txt` lists numbered issues with fix hints, then the full layout tree with real sizes and min / preferred / flexible values. Use it to explain every surprise in section 7. The goal is **0 errors** per screen. Warnings that are by design (a 100 x 100 close button hit area) can stay.

### 8.4 Run the tests

```bash
"D:/Unity/6000.4.8f1/Editor/Unity.exe" -batchmode -nographics -projectPath "F:/GameProject/Pale Vertica" -runTests -testPlatform EditMode -testResults results.xml -logFile tests.log
```

`UILayoutRegressionTests` builds every snapshot screen and fails on any layout error. If you change a core UI behaviour, a test that pinned the old numbers may fail: update it to the new rule on purpose (example: corner-radius tests after the rounded sprite changed its pixels per unit), and add tests for the new behaviour.

---

## 9. Running Unity from an agent

### 9.1 Batch mode needs Unity closed

Batch mode cannot open a project that the editor has open.

```bash
tasklist //FI "IMAGENAME eq Unity.exe" | grep -c Unity.exe       # 0 = closed
ls "F:/GameProject/Pale Vertica/Temp/UnityLockfile"               # must not exist
```

Ask the user to save and close Unity, then wait with a monitor loop instead of asking again:

```bash
until ! tasklist //FI "IMAGENAME eq Unity.exe" | grep -q Unity.exe && [ ! -f "<project>/Temp/UnityLockfile" ]; do sleep 3; done
```

Then run, in order, each as its own Unity process:

```bash
Unity.exe -batchmode -projectPath "<project>" -executeMethod CoreSystem.Editor.UI.SkillManagePrefabBuilder.BuildBatch -logFile build.log
Unity.exe -batchmode -projectPath "<project>" -executeMethod CoreSystem.Editor.UI.UISnapshotRenderer.RenderAllBatch -logFile render.log
```

Rendering needs a GPU: do **not** pass `-nographics` to the renderer (tests can use it). Grep the logs for `error CS`, `Exception` and your tool's own log lines.

Once Unity is closed you can iterate many times (build, render, compare, fix) without bothering the user. Tell them when they can open Unity again.

### 9.2 Compile-check without Unity

Unity's generated `Assembly-CSharp.csproj` cannot be built directly (project references loop). Generate a check project instead:
1. Read the `<Compile Include>` list, `<HintPath>` references (make relative paths absolute against the project root), `DefineConstants` and `LangVersion` from the Unity csproj.
2. Replace each `ProjectReference` with a reference to `Library/ScriptAssemblies/<Name>.dll`.
3. Add your **new** files (Unity only lists files it has already imported).
4. Write `<Project Sdk="Microsoft.NET.Sdk">` with `netstandard2.1`, `NoStdLib`, `DisableImplicitFrameworkReferences`, those items, into the scratchpad; build the runtime project first, then the editor project referencing the runtime DLL you just built.

`dotnet build -v:q` then prints `Build succeeded` or the real compiler errors. Do this after every code change; Unity compiles the same code.

### 9.3 Unity MCP

If the Unity MCP plugin is connected (`npx unity-mcp-cli run-tool ping` answers), you can run editor code live (`script-execute`). In this project it was not available, so the batch route above is the default.

---

## 10. Code side of a designed screen

- **Views listen, windows own data.** Example: `SkillManageWindow` raises `OnSelectedSkillChanged`; `SkillEffectPageView`, `SkillBasicPageView`, `SkillAdvancePageView` subscribe in `OnEnable`, render, and unsubscribe in `OnDisable`. Page switching is the existing `TabGroup` (`Tab.Page`).
- **Fill from the existing game APIs.** Basic and Advance read `SkillCaster.BuildPayload(instance)` and compare it with the base values; no formula was copied or changed.
- **Missing data**: if the design shows text the data does not have (skill description), ask the user. Options are to generate it from data (effect lines from the `Effects` list) or add a field through the whole pipeline (JSON class, ScriptableObject, Database sync, converter, Workbench editor).
- **Colours that mean something** (element tags) live in serialized lists on the window, with sensible defaults, so a designer can change them in the Inspector.
- **Pools**: reuse spawned rows (`SetActive`), keep order with `SetSiblingIndex`.
- **Empty states**: every list needs a placeholder (the Modifier page shows "NO SUPPORT SKILL" when no support is linked).
- **Menus**: use the existing `ContextMenuManager.Instance.Show(items, screenPosition)`; keep filter state in its own View (`SkillListFilterView`) that raises `OnFilterChanged`.
- Follow `AGENTS.md`: no comments, `#region` order, no runtime Find, suffix rules, one public type per file (small grouped types go in `<Area>Types.cs`).

---

## 11. Checklist

Before building:
- [ ] Read the screen's docs, the project skill and `UI_Visual_Language.md`.
- [ ] Exported every frame at scale 1 and saved them.
- [ ] Dumped the node tree and wrote window-relative positions.
- [ ] Found the art for every part; measured borders; listed what must be exported.
- [ ] Asked the user about missing data, scope (which frames, which states) and approval for asset or code changes.

While building:
- [ ] Editor builder script, idempotent, logs its changes, has `BuildBatch`.
- [ ] Positions from Figma through `Place`, sizes through `LayoutElement`.
- [ ] Existing parts cloned for headers, scroll views, buttons.
- [ ] Fonts, sizes, spacing, colours, caps, auto-size off, box heights at least the line height.
- [ ] Views fill data from events; no game math in the UI.
- [ ] `dotnet build` passes for runtime and editor code.

Verifying:
- [ ] Snapshot entry for every screen and tab, at the Figma position.
- [ ] Blend and zoom compared; every difference explained or fixed.
- [ ] Layout report: 0 errors.
- [ ] EditMode tests pass.

Finishing:
- [ ] Walkthrough written and linked from `Docs/README.md`; project skill updated.
- [ ] Known differences listed honestly (fonts, missing data, screens without a design).
- [ ] Nothing committed unless the user asked; other sessions' uncommitted files untouched.
- [ ] User told they can open Unity again.

---

## 12. Worked example: Skill Manage window, in order

1. Exported frames 569-1558, 569-1827, 569-2084 and dumped their trees.
2. Rendered the existing prefab at the Figma position: it was close, but the title tag, the Effect and Basic pages and the tab wiring were missing, the tags were all dark, "PHYSICAL" appeared twice, and two list texts overflowed.
3. Asked the user: where the Effect text and the description come from, which Basic stats to show, and whether to build the dark support card (answer: later).
4. Added the `Description` data field, `SkillEffectPageView`, `SkillBasicPageView`, `SkillAttributeRowView`, and a window event.
5. Wrote `SkillManagePrefabBuilder` (title tag, pages cloned from the Modifier header and scroll view, row prefab from an exported shape, tab wiring).
6. Asked the user to close Unity; build, render, blend, zoom.
7. Fixed what the zoom showed: tag colours, size and dedupe; damage line auto-size and caps; level digit spacing; tag height leak; hidden damage line (ellipsis box too short); nested fitters.
8. Final: 0 layout errors on all tabs, 120/120 tests, walkthrough written.
