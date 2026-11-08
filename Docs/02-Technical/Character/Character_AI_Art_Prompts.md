# Character AI Art Prompts

Ready-to-paste prompts for an image-generation AI to make Pale Vertica characters that follow the [Character Asset Standard](./Character_Asset_Standard.md). Written 2026-10-17 from the three reference characters (Acrxer, Skeleton, zombie); revised the same day for the one-image grid sheet and the Character Sheet Importer.

**Main path**: Prompt S (one prompt, one image) -> **Pale Vertica > Character > Character Sheet Importer** -> prefab. Fallback: the two-step prompts in section 5. Step by step: [Character Asset Walkthrough](./Character_Asset_Walkthrough.md), Flow B.

Image models do not place pixels exactly. The Sheet Importer shows each part on the rig and lets you drag it so its rotation point sits on the joint, so small placement errors are fixed in Unity, not by regenerating.

---

## 1. Attachments

Attach the images in this order and keep the names in the prompt. The prompts refer to them as Attachment 1-3.

| # | File | What it tells the AI |
|---|---|---|
| 1 | **Your character reference**: a Ref Character image or a Character Design Sheet (front view, colors, costume details) | **What** to draw. The character in the output must be this character |
| 2 | Prompt S and Step 2: `Template/Character_SheetTemplate_1x1.png` (or `_4x3.png` for a 4:3 image). Step 1: `Template/Character_PoseGuide.png` | **Proportions** (the grey figure) and **where** to draw: the grid, the cell order, the position and size of each part, the joints and the ground line |
| 3 | `Template/Character_StyleReference.png` | **How** it should look: the game's art style (six townsfolk references plus Acrxer, Skeleton and zombie) |

No design yet? Run Prompt A first with Attachment 3 only, approve one result, and use it as Attachment 1.

## 2. Style notes

Source: the six townsfolk references in `Template/StyleReferences/` (`Style_01_OldManCoat` to `Style_06_ScholarMask`, added 2026-10-17) and the three enemy PSBs. `Template/Character_StyleReference.png` shows all nine.

| Aspect | Rule |
|---|---|
| Proportions | Chibi. Head (with hair) about 55-65% of the total height and wider than the shoulders; small torso; short stubby arms; short stubby legs |
| Detail level | **Low.** Read at a glance at game size (the whole character is small on screen). Big flat shapes, few lines, nothing fiddly |
| Hands | Small round nubs in skin color at the end of the sleeve. **No fingers, no thumb, no nails, no knuckles**, no gloves detail |
| Feet | Short rounded dark stubs (trousers and shoes as one shape, at most a slight tone change for the shoe). **No toes, no laces, no soles, no shoe detail** |
| View | Body nearly front-facing; head and face turned slightly toward the **left of the image** (about 15 degrees): eyes and mouth a little left of the head's center, one ear visible on the right side of the head. The game draws the art as is when walking left and mirrors it when walking right |
| Head angle | Upright. **No head tilt and no neck lean**, even if the reference shows one; the neck joint must stay straight above the body |
| Outline | Thick, smooth, near-black ink outline (about 2% of the canvas width) around every part, slightly wobbly, rounded joins |
| Face (townsfolk) | **Minimal**: two small black dot eyes or short curved closed-eye lines, a tiny mouth, pink blush ovals on the cheeks, a warm red tint inside the ears and under the fringe. No nose (or a tiny tick), no eyebrows unless they are part of the design, no eyelashes beyond one stroke, no teeth or lip detail. Skin is warm cream |
| Face (enemies) | Big simple shapes: black sockets, glowing slits or round whites, readable at thumbnail size |
| Hair | One big chunky mass made of a few large clumps, often with an ahoge (single upright strand); glossy white highlight spots; vivid colors and soft two-color gradients are welcome (pink to purple, teal to green) |
| Headwear | Big hats (witch hat, wide brim with ornaments) are part of the Head and may be wider than the head, but stay inside the canvas |
| Clothing | Simple readable shapes (long coat, apron, robe, T-shirt); at most one or two small details (an emblem, a pocket, a tool) and one or two accent colors. No folds or wrinkles beyond one shadow shape, no buttons rows, no stitching, no patterns or textures |
| Shading | Flat base color plus one hard-edged darker tone; soft gradients only on hair and large shapes; small highlights. No glow, no rim light, no cast shadow, no painterly texture |
| Palette | Townsfolk: clean saturated accents on neutral or muted clothes. Enemies: muted and desaturated with one saturated accent. Pure magenta (#FF00FF) is reserved for the grid lines; pink and purple inside the character are fine |
| Mood | Cozy dark-fantasy world: villagers, mages, scholars, plus undead and armored enemies |
| Background | Transparent. On AI sheets a painted checkerboard or a flat color is also fine: the importer removes it |

## 3. Part layout (one character canvas = one grid cell)

Each cell of the sheet is a full square character canvas (the game uses 400 x 400 px). Positions below are in that canvas, from the top-left. The sheet template shows the same thing as pictures.

| Part | What it contains | Box it should roughly fill (x, y) | Rotation point (joint) it must cover |
|---|---|---|---|
| Head | Whole head with hair, hat, helmet, horns; no neck | x 62-352, y 7-256 | neck (207, 232), at least 8 px above the chin |
| Body | Torso with collar, belt, cloth, plus a short neck stub | x 149-264, y 205-341 | shoulders (172, 251) and (245, 251), hips (175, 317) and (228, 317) |
| ArmL | Arm on the **right side of the image** (near the camera), shoulder to hand, hanging down-right | x 232-317, y 236-308 | shoulder (245, 251) |
| ArmR | Arm on the **left side of the image**, shoulder to hand, hanging down-left | x 97-182, y 236-308 | shoulder (172, 251) |
| LegL | Leg on the right side of the image, hip to foot | x 213-258, y 308-378 | hip (228, 317) |
| LegR | Leg on the left side of the image, hip to foot | x 153-198, y 308-378 | hip (175, 317) |

- Feet touch the ground line y = 378. Body center line x = 207.
- Draw order, front to back: ArmL, Head, Body, ArmR, LegL, LegR.
- Each arm and leg extends **8-20 px past its joint** into the part it attaches to, and that hidden end is fully painted and outlined.

## 4. Prompt S: one prompt, one image (main)

Attachments, in this order:
1. The Ref Character or Character Design Sheet (what to draw).
2. `Template/Character_SheetTemplate_1x1.png` for a square image, or `Template/Character_SheetTemplate_4x3.png` for a 4:3 image. Transparent background, magenta grid; cell 1 holds the grey figure with the game proportions, cells 2-7 the same figure cut into parts at the same size.
3. `Template/Character_StyleReference.png` (how it should look).

Pick the `{...}` option that matches the template you attached. Ask for the highest resolution the AI offers. If the tool has a separate negative prompt field, paste the Avoid line there too.

```text
Make ONE image: a body-parts sheet for a 2D game character. Copy the layout of Attachment 2 exactly.

1. CHARACTER: the character from Attachment 1. Keep its face, hair, colors, clothes, sleeve length and accessories. Do not add or remove any clothing.
2. PROPORTIONS: exactly the grey figure in cell 1 of Attachment 2: huge round head about two thirds of the total height, small torso, short arms with the hands at the bottom of the torso, very short stubby legs.
3. STYLE: like Attachment 3: chibi, thick dark outline, flat colors with one shadow tone, minimal face (two dot eyes, tiny mouth, pink blush), very low detail. Hands are round mitten blobs with no fingers. Feet are short rounded dark stubs with no toes.
4. FACING: the face looks a little toward the LEFT edge of the image (eyes and mouth slightly left of the head's center, one ear visible on the right side of the head). Head upright, no tilt. Do not mirror the character.
5. BACKGROUND: transparent (PNG with alpha) everywhere outside the character and the grid lines. No floor, no shadow, no scenery.
6. GRID: {square image with a 3 x 3 grid | 4:3 image with a 4 x 3 grid} of equal square cells separated by thin pure magenta (#FF00FF) lines. No text.
7. CELLS (left to right, top to bottom):
   - Cell 1: the full character, standing, feet on the bottom guide line. This is the master drawing.
   - Cells 2 to 7 are made by COPYING cell 1 and ERASING everything except one part (erased areas become transparent): 2 the head, 3 the torso, 4 the arm on the RIGHT side of the image, 5 the arm on the LEFT side of the image, 6 the leg on the RIGHT side of the image, 7 the leg on the LEFT side of the image.
   - Same scale and same position as cell 1: never enlarge, zoom, center or move a part. Each part looks exactly as small as in cell 1, with lots of empty space around it.
   - Keep left and right exactly as in cell 1: a part on the right side of cell 1 stays on the right side of its cell.
   - Where a part was hidden in cell 1, draw the missing piece: the torso continues behind the arms and above the legs; each arm and leg continues a little into the torso, fully outlined.
   - All other cells stay empty.

Avoid: fingers, thumbs, toes, long legs, long arms, adult proportions, extra or changed clothes, different sleeve length, enlarged or zoomed parts, centered parts, mirrored or swapped left and right, head tilt, background scenery, detailed shoes, folds, patterns, text, labels, shadows.
```

Why it is written this way:
- **Short and ordered by priority.** Image models stop following long prompts.
- **Copy and erase.** "Copy cell 1 and erase everything except one part" keeps every part at the master's size and place; "draw the arm in cell 4" makes models zoom the part to fill the cell.
- **Sides by image position.** Models confuse "left arm" and "right arm"; "the arm on the right side of the image" is unambiguous. The importer maps it to `ArmL`.
- **Transparent, but nothing breaks if it is not.** Many image models cannot write real alpha and paint a grey-and-white checkerboard instead, or use a flat color. The importer removes all three (real alpha, a painted checkerboard, any flat background connected to the cell edges). If your tool has a "transparent background" switch, turn it on.

The importer can still fix what the AI gets wrong: per-part **Scale** (slider or mouse wheel), **Mirror** for one part, and **Mirror whole character** (also swaps the left and right limbs).

## 5. Two-step fallback

If the one-prompt sheet keeps adding fingers, long legs or extra clothes, split the job: first the character alone, then the parts as an edit of that image.

### Step 1: the character in the game proportions

Attachments: (1) the Ref Character or Character Design Sheet, (2) `Template/Character_PoseGuide.png`, (3) `Template/Character_StyleReference.png`.

```text
Redraw the character from Attachment 1 as ONE full-body game sprite.

KEEP from Attachment 1: face, hair, colors, clothes, sleeve length and accessories. Do not add or remove any clothing.
CHANGE to the game style of Attachment 3: chibi, very low detail.
MATCH the silhouette and proportions of Attachment 2 (grey figure) exactly: huge round head about two thirds of the total height, small torso, short arms whose hands end at the bottom of the torso, very short stubby legs.

Rules:
- Body facing the viewer, head turned slightly to the LEFT of the image, head upright (no tilt), arms hanging down slightly away from the body, feet side by side on the ground.
- Hands are small round mitten blobs with no fingers. Feet are short rounded dark stubs with no toes.
- Thick dark outline, flat colors with one shadow tone, minimal face (two dot eyes, tiny mouth, pink blush).
- Square image, transparent background, one character centered, feet near the bottom edge, nothing else.

Avoid: fingers, thumbs, toes, long legs, long arms, adult proportions, extra clothes, longer or shorter sleeves than Attachment 1, detailed shoes, folds, patterns, text, background, ground shadow.
```

Check before Step 2: head about two thirds of the height, hands are blobs at the bottom of the torso, legs short, same clothes and sleeves as the reference, head straight.

### Step 2: the parts sheet

Run it as an edit of the Step 1 image (same chat, or an image-edit model). Attachments: (1) the approved Step 1 image, (2) `Template/Character_SheetTemplate_1x1.png` (or `_4x3.png`).

```text
Cut the character in Attachment 1 into body parts on a character sheet. This is the SAME drawing split into pieces, not a new drawing: keep every color, line, size and position exactly as in Attachment 1.

Layout: copy Attachment 2 exactly. {1:1 image, 3 x 3 grid | 4:3 image, 4 x 3 grid} of equal square cells separated by thin pure magenta (#FF00FF) lines, transparent background.
Cells in reading order: 1 Preview = Attachment 1 unchanged, 2 Head, 3 Body, 4 ArmL = the arm on the RIGHT side of the image, 5 ArmR = the arm on the LEFT side, 6 LegL = the leg on the RIGHT side, 7 LegR = the leg on the LEFT side. Other cells stay empty.
Make cells 2-7 by copying cell 1 and erasing everything except that part (erased areas become transparent): never enlarge, zoom or center a part, and keep left and right as in cell 1. Stacking cells 2-7 rebuilds the Preview.
Complete the hidden areas: the torso continues behind the arms and above the legs; each arm and leg continues a little past its joint into the torso, fully drawn and outlined. Parts do not touch the cell edges.

Avoid: redrawing or redesigning, enlarged or zoomed parts, mirrored or swapped left and right, changing proportions, making limbs longer, adding fingers or toes, adding clothes, text, labels, shadows, background scenery.
```

Small position errors are fine: the Character Sheet Importer lets you drag each part onto its joint. If the parts keep changing, keep the Step 1 image and split it by hand over `Character_Template.psd` (walkthrough Flow A).

## 6. Prompt A: quick mockup (design approval)

```text
Create a single full-body game character mockup for a dark-cute gothic 2D action RPG.

WHO TO DRAW: {If you have a Ref Character / Design Sheet: "The character shown in Attachment 1; copy its design exactly." Otherwise: a short description, e.g. "a plague doctor healer with a beaked mask, long dark coat and a small lantern"}.
Main colors: {hair, clothes and one accent color}.
HOW IT LOOKS: match the art style of the attached style reference.

Canvas and framing: square, transparent background, no floor, shadow, text or border. One character, standing neutral pose, body nearly front-facing, head and face turned slightly toward the LEFT of the image (about 15 degrees), so the eyes and mouth sit a little left of the head's center and one ear shows on the right side of the head, head upright with no tilt, arms hanging down and slightly outward, legs straight, feet flat near the bottom (about 5% margin). Head with hair about 55-65% of the total height and wider than the shoulders.

Style: chibi, small torso, short stubby arms and legs; thick smooth near-black ink outline; minimal face with two small dot or closed-line eyes, a tiny mouth and pink blush, no nose; chunky hair clumps with glossy white highlights; flat cel shading with one hard-edged shadow tone, soft gradients only on hair and large shapes; no glow, no rim light, no painterly texture.
Low detail: hands are small round nubs with no fingers; feet are short rounded dark stubs with no toes or shoe details; clothes are big flat shapes with at most one or two small details, no wrinkles, buttons, stitching or patterns; hair is a few big clumps.

Keep the head, torso, both arms and both legs visually distinct (clear outline between them) so the design can later be split into parts.
```

## 7. Prompt B: six separate PNGs (alternative)

For AIs that return several files. Same rules as the sheet prompts, but the output is six 400 x 400 transparent PNGs (`ArmL.png`, `Head.png`, `Body.png`, `ArmR.png`, `LegL.png`, `LegR.png`), each holding one part drawn in place on its own canvas (positions in section 3). Use `Template/Character_LayerMap.png` as Attachment 2 instead of the sheet template. Paste Prompt S and replace its IMAGE FORMAT and EACH CELL blocks with:

```text
OUTPUT FORMAT (strict): six PNG files with alpha, each exactly 400 x 400 px, named ArmL.png, Head.png, Body.png, ArmR.png, LegL.png, LegR.png. Each file contains ONE part only, drawn at its final position on the 400 x 400 canvas (do not center or crop it); everything else is fully transparent. Stacking the files front to back in the order ArmL, Head, Body, ArmR, LegL, LegR must reproduce the complete character. No text, background, shadow or glow. Positions follow Attachment 2 (the layer map): head x 62-352, y 7-256; torso x 149-264, y 205-341; ArmL x 232-317, y 236-308; ArmR x 97-182, y 236-308; LegL x 213-258, y 308-378; LegR x 153-198, y 308-378; rotation points neck (207, 232), shoulders (172, 251) and (245, 251), hips (175, 317) and (228, 317).
```

Build the PSB with `python Tools/CharacterTemplate/character_psb_tool.py assemble --parts <folder> --name <Name> --out Assets/Image/Character`, then follow the walkthrough Flow C.

## 8. From the sheet to the game (summary)

1. Save the AI image anywhere (it does not need to be inside `Assets`).
2. Unity: **Pale Vertica > Character > Character Sheet Importer** > **Browse...** > pick the image. Check the cut overlay.
3. For each part: drag it until the magenta cross sits on its rotation point. Fix every Error in the checks list.
4. Enter the character name, keep **Create prefab** on, press **Build**. You get `Assets/Image/Character/<Name>.psb` (standard layers, anchor pivots, validated) and `<Name>.prefab` next to the base prefab.

Details and common mistakes: [Character Asset Walkthrough](./Character_Asset_Walkthrough.md).
