# Character Asset Walkthrough

How to take a character from a drawing (or an AI image) to a working prefab, and how to fix an existing character PSB. Written 2026-10-17 for Unity 6000.4.8f1.

Rules and numbers live in [Character Asset Standard](./Character_Asset_Standard.md); this page is the how-to. AI prompts: [Character AI Art Prompts](./Character_AI_Art_Prompts.md).

---

## 1. What you use

| Tool | Where to open it | Used for |
|---|---|---|
| Blocking template | `Docs/02-Technical/Character/Template/Character_Template.psd` | Starting file for artists: six named layers at the right positions, hidden `_Guide` group |
| Layer map | `Docs/02-Technical/Character/Template/Character_LayerMap.png` | Picture of the layout with joints; attach to AI prompts |
| Style reference | `Docs/02-Technical/Character/Template/Character_StyleReference.png` (built from `Template/StyleReferences/` plus the three enemy PSBs) | Nine characters showing the art style; attach to AI prompts as the style image |
| Sheet templates | `Docs/02-Technical/Character/Template/Character_SheetTemplate_1x1.png` (3 x 3) and `_4x3.png` (4 x 3) | The grid the AI copies: cell order, part positions, joints, ground line, magenta grid lines |
| Pose guide | `Docs/02-Technical/Character/Template/Character_PoseGuide.png` | Grey figure with the game proportions (huge head, small torso, short limbs); attach to Step 1 |
| **Character Sheet Importer** | Unity menu **Pale Vertica > Character > Character Sheet Importer** | One AI sheet image in, ready PSB (and prefab) out: shows the cut, lets you place each part on its rotation point, builds and standardizes |
| Character Asset Tool | Unity menu **Pale Vertica > Character > Character Asset Tool**, or right-click a `.psb` in the Project window > **Pale Vertica > Validate Character PSB** | Validate, Apply Standard, Rebuild Atlas, Export Parts, Create Prefab |
| Python tool | `Tools/CharacterTemplate/character_psb_tool.py` (Python 3 with `psd-tools`) | `assemble`: six part PNGs to a standard PSB. `port`: a PSB from another project or rig to a standard PSB, from a JSON spec (Flow E). `template`: rebuild the template, layer map and sheet templates |
| Settings (optional) | **Assets > Create > Pale Vertica > Character > Character Asset Settings** | Override the standard's numbers (canvas, padding, anchors, base prefab, export folder). Without an asset the built-in defaults are used |

### The Character Asset Tool window

| Field / button | What it does |
|---|---|
| **PSB file** | The character file. Selecting a `.psb` in the Project window fills it automatically |
| **Base prefab** | Prefab copied by Create Prefab. Default `Assets/Addressable/Prefab/Entity/Enemy/Skeletom.prefab` |
| **New prefab path** | Where Create Prefab writes. Default `<base prefab folder>/<PSB name>.prefab`; edit it to put the prefab elsewhere |
| **Validate** | Read-only check. Lists Pass, Info, Warning and Error lines (section 6) |
| **Apply Standard** | Writes the standard importer settings, turns on Automatic Reslice, and sets every part's pivot to its rig anchor. Reimports the file |
| **Rebuild Atlas** | Turns on Automatic Reslice, raises the mosaic padding to 8 px and reimports. Keeps anchor pivots if the file already has them. Sprite IDs do not change |
| **Export Parts (PNG)** | Splits the atlas: one PNG sprite per part in `Assets/Image/Character/Parts/<PSB name>/`, PPU 100, pivot on the anchor |
| **Create Prefab From Base** | Validates (stops on any Error), copies the base prefab to the new path, puts the six sprites in by name. Never overwrites an existing file |
| **Open Template Folder** | Opens `Docs/02-Technical/Character/Template/` in the file browser |
| Log | What the last button did |

Every button that changes assets asks for confirmation first.

### The Character Sheet Importer window

| Area | What it does |
|---|---|
| **Sheet image / Browse...** | Loads one PNG or JPG from anywhere on disk (the file is read directly, so Unity import settings never resize it) |
| **Grid layout** | Auto picks 4 x 3 for wide images and 3 x 3 for square ones; force it if the guess is wrong. **Re-slice** cuts again |
| Info line | Image size, layout, and whether magenta grid lines were found (if not, the image is split into equal cells) |
| **1. How it will be cut** | The sheet with every cell outlined and named. Click a cell to edit that part |
| **2. Place** | Tabs: **Preview** and the six parts. **Preview** tab: fit the AI's full character onto the rig (all joint crosses shown); it then serves as the faded ghost for the parts. Part tabs: the part in full color over the ghost (**Ghost: AI preview** or **built parts**). **Drag** to move, **mouse wheel** or the **Scale** slider to resize around the part's rotation point (magenta cross); blue dot = anchor, orange line = ground. Buttons: **Snap to box** (standard box as a start), **Scale to all parts**, **Mirror** (this part, around its joint), **Reset**, **Reset all**, **Mirror whole character** (flips all and swaps left/right limbs) |
| **3. Result on the rig** | All parts with your offsets, next to the AI's own Preview cell. They should match |
| **Checks** | Errors (empty cell, part not covering its rotation point) block the build; warnings (body not covering a joint) do not |
| **4. Build** | Character name, PSB path (`Assets/Image/Character/<Name>.psb`), Create prefab and Base prefab. **Build** writes the PSB, imports it, runs Apply Standard, creates the prefab and lists the validation result in the Log |

## 2. Flow A: new character drawn by an artist

**Input**: an idea, the template file. **Output**: `<Name>.psb` and `<Name>.prefab`.

1. Copy `Character_Template.psd` and open it (Photoshop, Krita, Clip Studio, Photopea...).
2. Turn on the hidden `_Guide` group. Magenta rings are joints, blue dots are anchors, the orange line is the ground (feet), the dashed line is the center.
3. Draw each part on its own layer over the colored blocking shape: `ArmL`, `Head`, `Body`, `ArmR`, `LegL`, `LegR`. `ArmL`/`LegL` are on the **right** side of the image.
4. Keep every magenta ring covered by its part, and let the body cover the shoulders and hips. Paint the hidden ends of the limbs completely.
5. Remove the blocking color you painted over (erase it or paint over it on the same layer). Do not add, rename or regroup the six layers.
6. Hide `_Guide`. Any extra helper layer must be hidden and named with a leading `_`.
7. Save as `<Name>.psb` (Photoshop: Save As > Large Document Format). Apps that cannot write PSB: save as PSD, then rename the file to `.psb`.
8. Copy the file into `Assets/Image/Character/`. Continue with Flow C.

## 3. Flow B: new character from an image AI (one sheet image)

**Input**: a Ref Character or Character Design Sheet image, the prompts doc. **Output**: `<Name>.psb` and `<Name>.prefab`.

1. No design yet: run Prompt A in [Character AI Art Prompts](./Character_AI_Art_Prompts.md) and keep the result you like as the reference.
2. Send **Prompt S** (prompts doc, section 4) with (1) the Ref Character / Design Sheet, (2) `Character_SheetTemplate_1x1.png` (square) or `_4x3.png` (4:3), (3) `Character_StyleReference.png`. Ask for the highest resolution the AI offers.
3. If it keeps adding fingers, long legs or extra clothes, use the two-step fallback (prompts doc, section 5): the character alone with `Character_PoseGuide.png`, then the parts as an edit of that image.
4. Check the sheet by eye: seven cells in the order Preview, Head, Body, ArmL, ArmR, LegL, LegR; one part per cell, each as small as in cell 1; transparent (or checkerboard, or flat) background; magenta grid lines; no text. Regenerate if parts are merged, missing or redesigned.
5. In Unity open **Pale Vertica > Character > Character Sheet Importer**, press **Browse...** and pick the image.
6. Look at **1. How it will be cut**: every outline should frame one cell. Wrong grid: change **Grid layout** and press **Re-slice**.
7. **Preview** tab first: move and scale the AI preview until the crosses sit on its neck, shoulders and hips and the feet touch the orange line.
8. Each part tab (or click its cell): scale (mouse wheel or slider) and drag it until it matches the faded preview and the magenta cross sits on its rotation point. Use **Mirror** or **Mirror whole character** if the AI flipped something. The **Checks** list must have no Error.
9. Compare **3. Result on the rig** with the AI preview. They should look the same.
10. Type the **Character name**, keep **Create prefab** on (base prefab `Skeletom` by default), press **Build**, confirm.
11. Read the Log: the PSB path, "Importer settings applied", "Anchor pivots set", the prefab path and the validation result. Continue with Flow C step 6 (stats, `Character.json`, Play mode).

### Flow B2: six separate PNGs (Prompt B)

1. Run Prompt B to get six 400 x 400 PNGs: `ArmL.png`, `Head.png`, `Body.png`, `ArmR.png`, `LegL.png`, `LegR.png`, each part drawn in place on a transparent canvas. Put them in one folder.
2. Build the PSB:
   ```bash
   python Tools/CharacterTemplate/character_psb_tool.py assemble --parts <folder with the six PNGs> --name <Name> --out Assets/Image/Character
   ```
3. Read the output:
   - `ERROR missing ...` or `... must be a 400x400 canvas`: fix that file and run again.
   - `ERROR ... does not cover its joint`: the limb is in the wrong place or too short. Move or regenerate it.
   - `WARN Body does not cover ...` / `WARN feet end at ...`: allowed, but it will show a gap or a floating shadow in game. Better fixed.
   - `written Assets/Image/Character/<Name>.psb`: done.
4. Continue with Flow C.

## 4. Flow C: import a PSB and make the prefab

**Input**: `Assets/Image/Character/<Name>.psb`. **Output**: a validated PSB and `<Name>.prefab`.

1. Open **Pale Vertica > Character > Character Asset Tool**. Select the PSB in the Project window.
2. Press **Validate**. Fix every **Error** in the art file (section 6), save, and let Unity reimport.
3. Press **Apply Standard**, confirm. Wait for the reimport.
4. Press **Validate** again. Expected: no Error and no Warning. Info lines are fine.
5. Check **Base prefab** and **New prefab path**, press **Create Prefab From Base**, confirm. The new prefab is pinged in the Project window.
6. Open the prefab: the six parts should sit together like the PSB preview. Set the `Character` component's stats and AI profile for the new character.
7. Open **Window > Pale Vertica > Pale Vertica GameDataWorkbench**, add the character to `Character.json` with `PrefabPath` = the new prefab path. Prefabs under `Assets/Addressable/Prefab/` are Addressable through the folder entry, so nothing else is needed.
8. Play mode: spawn it (dev tool **K** or a spawner) and watch idle, walk, attack and die. No gaps at the shoulders, hips or neck; feet on the shadow.

## 5. Flow D: fix an existing PSB (atlas looks off, sprites cropped)

**Input**: a PSB already used by prefabs. **Output**: the same sprites, correctly sliced.

1. Select the PSB, press **Validate**. "Automatic Reslice is off" or "Mosaic padding is 4 px" means the atlas can be stale.
2. Press **Rebuild Atlas**, confirm. Prefab references are kept (sprite IDs do not change).
3. Press **Validate**. The reslice and padding warnings are gone.
4. If it warns "Pivots are off their rig anchors", decide: **Apply Standard** moves the parts onto the anchors, so prefabs using this PSB show the parts exactly where they are drawn (up to 12 px change on Skeleton). Leave it if the current look must stay.

For costume art: run Flow C steps 1-4 on the costume PSB, then **Export Parts (PNG)** if the costume item needs single sprites (costume Sprite Layers take one sprite per part; Arms and Legs use index 0 = L, 1 = R).

## 5b. Flow E: import a PSB from another project (port)

**Input**: a PSB made for another project or rig: any canvas size, any layer names, parts inside groups or hidden, several characters in one file. **Output**: a new `Assets/Image/Character/<Name>.psb` that follows the standard; then Flow C (or the NPC builder) as usual. First used on 2026-11-07 for the four NPCs of AlchemiteReturn (`alchemist.psb`); their specs and the full example are in [AlchemiteReturn NPC Walkthrough](./AlchemiteReturn_NPC_Walkthrough.md).

Do not copy the foreign PSB or its `.meta` into `Assets/`. Unity would import every visible layer as a sprite with its foreign names, and the GUID could clash with a file already here. The original stays in its own project; `port` writes a new file, so Unity gives it a new GUID. Do not copy the other project's animator controller or clips either: they bind to that project's hierarchy. The Pale Vertica prefab rig and `Model.controller` animate any standard PSB.

1. **Look at the source.** List its layers and groups (Photoshop, Krita, or `psd-tools`), and render each group. Pick the six layers that make one character. Hidden layers are fine: `port` reads them. If the other project builds the character from pieces of several groups or files (skins on a rig), open its scene and note which sprite each joint shows. Write each layer as a path: `Group/Layer` (`Folder 7/Layer 12`).
2. **Find each part's joint in the source canvas**, in pixels with **y from the bottom**: the point the part turns around (neck for `Head`, torso root for `Body`, shoulders for the arms, hips for the legs).
   - Source rig in a scene: joint = sprite pivot in canvas px minus the skin's local position divided by (skin scale x 0.01) for PPU 100. Example: pivot at (267.1, 218.0), skin local (0.057, 0.098), skin scale 0.5: joint = (267.1 - 11.4, 218.0 - 19.6) = (255.7, 198.4).
   - No rig: read it off in an image editor (`y = canvas height - y from top`).
   - The joint must sit on an opaque pixel of the part. A rig that turns an arm around a point outside the sprite needs the joint moved onto the shoulder end of the arm.
3. **Pick the scale.** Start from Pale Vertica's neck-to-hip distance (85 px) divided by the source's, as the spec's `scale` for all parts. Then compare the step 6 preview with `Acrxer` (the player) and `Hymera`: when the source art has different proportions (a smaller head, thinner arms), give that part its own `"scale"` in its entry. Use the same per-part values for every character ported from one file, so they keep their relative size. Legs: keep the feet on y 22; the rig fixes hip-to-ground at 61 px, so a bigger leg scale only pushes the feet below the ground line. A part that would leave the canvas (a tall hat) is reported by the tool; add `"canvasHeight": 440` (up to 480) to the spec to give it headroom above, or lower that part's scale when it leaves the canvas sideways.
4. **Write the spec** as `Tools/CharacterTemplate/ports/<Project>/<Name>.json`:
   ```json
   {
     "name": "Alchemist",
     "source": "../../../../../AlchemiteReturn/Assets/Raw Source/Player/NPC/alchemist.psb",
     "scale": 0.735,
     "parts": {
       "Head": {"layer": "Folder 7/Layer 12", "joint": [255.74, 198.42]},
       "Body": {"layer": "Folder 7/Layer 11", "joint": [261.9, 137.0]},
       "ArmL": {"layer": "Folder 5/Folder 3 Copy", "joint": [298.8, 170.4]},
       "ArmR": {"layer": "Folder 5/Folder 3", "joint": [228.0, 169.2]},
       "LegL": {"layer": "Folder 5/Folder 4 Copy", "joint": [292.8, 86.2]},
       "LegR": {"layer": "Folder 5/Folder 4", "joint": [230.6, 86.6]}
     }
   }
   ```
   A part entry may add `"scale": 0.85` to override the spec's `scale` for that part only (the Blacksmith and OldMan specs use Head 0.85, Body 0.8, arms 0.82, legs 0.735). `source` is relative to the JSON file (here: the AlchemiteReturn project next to this one); `--source <path>` overrides it. `ArmL` and `LegL` are the parts on the **right** side of the image, as everywhere in the standard.
5. **Dry run into a scratch folder** (Python 3 with `psd-tools`, 1.24 tested):
   ```bash
   python Tools/CharacterTemplate/character_psb_tool.py port --spec Tools/CharacterTemplate/ports/AlchemiteReturn/Alchemist.json --out <scratch> --work <scratch>
   ```
6. **Check the result.** Open `<work>/<Name>/_Preview.png`: the six parts in game draw order, magenta rings on the rig joints, orange ground line. Feet on the line, no holes at the neck, shoulders and hips. Output lines:
   - `ERROR ... opaque px fall outside the 400x... canvas`: spans with a negative y-from-top need `canvasHeight` (headroom above); spans past x 0 or 400 need a lower `scale` for that part.
   - `ERROR ... source joint ... is not on an opaque pixel`: move that joint onto the part.
   - `ERROR ... layer '...' not found`: check the `Group/Layer` path (exact names, case counts).
   - `WARN Body does not cover the ArmR joint`: allowed; a small gap may show when that arm swings.
   - `written ...psb`: done. With any ERROR nothing is written.
7. **Write it for real** with `--out Assets/Image/Character`, then continue with Flow C from step 1 (Validate, Apply Standard, Create Prefab), or build an NPC with `NpcPrefabBuilder` ([AlchemiteReturn NPC Walkthrough](./AlchemiteReturn_NPC_Walkthrough.md), section 4).

Common mistakes in Flow E:
- **Copying the foreign PSB into `Assets/` "to have it here".** Keep it in its project, or outside `Assets/`.
- **Using the layer's bounding-box center as the joint.** Limbs turn at one end; a center joint makes them swing like propellers.
- **Per-part scales far apart.** Outline thickness then changes from part to part and the character looks stitched together. Keep them close (the AlchemiteReturn NPCs stay within 0.735-0.85) and only use them to match Pale Vertica proportions.
- **Making a ported character bigger with the prefab's Transform scale.** Every rig joint is fixed in canvas pixels; fix the size in the spec and port again.
- **Re-running `port --out Assets/...` over an imported PSB.** It overwrites the file. The layer IDs stay 1001-1006, so Unity keeps the sprite IDs and prefabs keep working, but any edit made to the ported PSB in an art app since is lost. Port again only when the source art changed, then press **Validate** and **Apply Standard** again (a part whose size changed needs its anchor pivot recomputed).
- **`pip` fails because drive C: is full.** Install with `TMP` set to another drive: `python -m pip install --target <folder> psd-tools`, then run the tool with `PYTHONPATH=<folder>`.

## 6. Validation messages and what to do

| Message starts with | Severity | Fix |
|---|---|---|
| `Canvas is ...` | Error | Resize the **canvas** (not the image) to 400 x 400 and keep the parts on the template positions |
| `Missing layer` / `must be named exactly` | Error | Rename the layer to the exact part name (case counts) |
| `... is used N times` | Error | Each part name once in the whole file; rename copies to `_Ref_<Part>` and hide them |
| `... is inside group` | Error | Drag the part to the root of the layer panel |
| `... is hidden or not imported` | Error | Make the part visible |
| `Extra layer ... is imported` | Error | Hide the layer and prefix it with `_` |
| `... has no opaque pixels on its joint` | Error | Extend the part over its magenta ring |
| `Include Hidden Layers is on` | Error | Apply Standard |
| `Pixels Per Unit` / `Sprite Mode` | Error | Apply Standard |
| `Layer order is ...` | Warning | Reorder to ArmL, Head, Body, ArmR, LegL, LegR (top to bottom) |
| `Mosaic padding` / `Automatic Reslice is off` | Warning | Rebuild Atlas (or Apply Standard) |
| `Pivots are off their rig anchors` | Warning | Apply Standard |
| `'Body' does not cover the ... joint` | Warning | Let the body overlap that joint |
| `Feet end at y = ...` | Warning | Move the legs so the feet touch the ground line |
| `Helper layer ... is not imported` | Info | Optional: prefix the name with `_` |

## 7. Common mistakes

- **AI adds fingers, long legs, extra clothes or longer sleeves.** Use the current short Prompt S with the grey-figure template, paste its Avoid line into the negative prompt if there is one, and fall back to the two-step prompts if it still drifts.
- **Sheet: parts drawn bigger than the preview, centered or zoomed.** Select the part, set **Scale** (slider or mouse wheel) until it matches the faded AI preview, then drag it into place. If every part is off by the same amount, set one and press **Scale to all parts**.
- **Sheet: a part or the whole character is mirrored.** **Mirror** flips the selected part around its rotation point. **Mirror whole character** flips everything around the center line and swaps ArmL/ArmR and LegL/LegR.
- **Sheet: fake transparency.** Many AIs paint a grey-and-white checkerboard instead of real alpha. That is fine: the importer removes real alpha, a painted checkerboard and any flat background connected to the cell edges (tested at 97% or better).
- **Sheet: a part touching the grid line.** Magenta-like pixels within about 2% of a cell edge are treated as grid and removed, so a pink or purple part touching the edge loses those pixels. Pink and purple inside the cell are safe. Ask for parts that stay clear of the lines.
- **Sheet: wrong cell order.** The importer always reads Preview, Head, Body, ArmL, ArmR, LegL, LegR in reading order. A swapped cell shows the wrong part on the tab; regenerate rather than rename.
- **Rebuilding over an existing character.** Build refuses to overwrite `<Name>.psb`: a new file would get new sprite IDs and break the old prefab. Use a new name, or redraw inside the existing PSB.
- **Renaming or deleting a part layer after import.** The sprite gets a new ID and every prefab that used it shows an empty part. Keep the names; redraw inside the layer.
- **Merging layers to "clean up".** A merged arm and body cannot rotate. Keep six layers.
- **Leaving `_Guide` visible.** Its layers get imported as sprites (Validate reports "Extra layer").
- **Drawing on a different canvas size or cropping the file.** Every position shifts; the rig joints no longer match.
- **Mirroring L/R by name.** `ArmL` is the arm on the right side of the image.
- **Soft glow or drop shadow on a part.** It grows the sprite rect and draws a halo in game. Remove effects before saving.
- **Moving the joint objects in the prefab to fit a new character.** That breaks every animation for that prefab. Fix the art instead.
- **Animator parameter names.** The shared `Model.controller` uses `IsMoving` (bool), `IsDead` (trigger) and `Attack` (trigger), matching `CharacterPresenter`. The attack trigger was renamed from `IsAttack` to `Attack` on 2026-10-17; a controller copied before that date needs the same rename or the attack animation never plays.

## 8. Rebuilding the template

After changing the anchors or the reference character:

```bash
python Tools/CharacterTemplate/character_psb_tool.py template --out Docs/02-Technical/Character/Template --reference Assets/Image/Character/Acrxer.psb
```

Keep the anchor numbers in the script, in `CharacterAssetSettings` and in the standard (section 4.1) identical.

## 9. Status

- Not yet run inside Unity (2026-10-17): the tool scripts compile against Unity 6000.4.8f1 outside the editor. Update this page with anything the first real run shows.
- Character Sheet Importer: the slicer and PSB writer were tested outside Unity on sheets built from zombie.psb (1:1 transparent with grid lines, 4:3 white background with grid lines, 1:1 without lines). Parts came back within 1 px of the originals, and psd-tools reads the written PSB with the right names, layer IDs and hidden preview layer. Unity reading that PSB and the window itself have not been tried yet.
- Flow E (`port`, added 2026-11-07): used on the four AlchemiteReturn NPCs; the written PSBs imported in Unity, passed Validate after Apply Standard, and their prefabs render and animate like `Ally` ([AlchemiteReturn NPC Walkthrough](./AlchemiteReturn_NPC_Walkthrough.md), section 5).
- Sorting (added 2026-10-17): `Ally`, `Dummy` and `Skeletom` have a `SortingGroup` on `Model` and a Sort At Root group on `Model/Shadow`; the 2D renderer sorts by Y. **Create Prefab From Base** adds both groups to new prefabs, so nothing to do by hand. The Player rig inside `Assets/Scenes/SandBox.unity` is not a prefab and still needs the same two groups added by hand. Details: standard, section 7.
