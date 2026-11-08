# Slash Stylize V2 Walkthrough (EffectCraft)

Builds the improved stylized 2D slash in EffectCraft from a Python script, renders it, and exports a sprite sheet and preview GIFs. It is a clone of the original `VFX_Pure_Slash` (now `Tools/VfxPreview/projects/SlashStylize/VFX_Pure_Slash_V1.ecproj`); the original project is not touched. Folder layout, hand editing and rendering are shared with every other effect: see [VFX Preview Tool Walkthrough](./VFX_Preview_Tool_Walkthrough.md).

## Rule: grayscale only (blocking)

Every VFX sheet built with EffectCraft for this project is **grayscale** (black to white, `r == g == b` on every layer). Colour is applied in Unity with a gradient map (value -> colour ramp) or a tint, never baked into the sheet. The scripts enforce this: `enforce_grayscale_only()` runs before any project is built, and every render checks the pixels; both raise `GrayscaleOnlyRuleError`, so nothing coloured is exported. Do not remove the guard or add colour palettes. (Until 2026-10-18 the rule was pure white only; it was widened to grayscale for the painterly gradient-map style.)

## Where

- Script: `Tools/VfxPreview/scripts/build_slash_stylize_v2.py`
- EffectCraft: `C:\Program Files\EffectCraft` (`effectcraft.exe` = editor, `effectcraft-cli.exe` = headless CLI and MCP server)
- Editable project: `Tools/VfxPreview/projects/SlashStylize/VFX_Slash_Stylize_V2.ecproj`
- Output: `Tools/VfxPreview/output/SlashStylize/VFX_Slash_Stylize_V2/`

## Inputs

- Python 3 with Pillow and NumPy (`pip install pillow numpy`)
- Tuning constants at the top of the script: `SIZE` (256), `FPS` (30), `FRAMES` (16), `CENTER`, `RADIUS`, `HEAD_FROM` / `HEAD_TO` (sweep angles, degrees, 0 = right, clockwise).

## What changed from V1

| V1 (`VFX_Pure_Slash`) | V2 (`VFX_Slash_Stylize_V2`) |
| --- | --- |
| Linear keyed sweep, frame 0 almost empty | Head snaps in with ease-out-expo, visible from frame 0; tail follows with ease-in-out (smear) |
| Uniform white fill, 3 layers | 5 tiers: soft additive glow, inner / outer speed streaks, main blade, bright core edge |
| Thickness fixed per frame | Thickness grows with the swept span (stretch on the fast frames, thin on the slow ones) |
| Fades out as one piece | Tail breaks into 4 drifting fragments plus 7 sparks thrown off the head |
| White only, tint later | Grayscale only, enforced by `enforce_grayscale_only()`; this sheet happens to be all white |

## Steps

1. Open a terminal in `Tools/VfxPreview/scripts`.
2. Run:
   ```bash
   python build_slash_stylize_v2.py
   ```
3. The script checks the grayscale-only rule and writes a `.jsx`. If the `.ecproj` does not exist yet, it builds it with `effectcraft-cli script ... --save-as`; if it exists, it is **kept** (it may hold hand edits) unless you pass `--force`. It then renders all 16 frames with `render-frame --transparent` and exports.
4. Check `output/SlashStylize/VFX_Slash_Stylize_V2/VFX_Slash_Stylize_V2_Contact.png` (8x2 strip on a dark background) or the `_Preview.gif`.
5. To hand-tweak, open `projects/SlashStylize/VFX_Slash_Stylize_V2.ecproj` in `effectcraft.exe`, keep every colour gray, save, then run `python render_vfx.py VFX_Slash_Stylize_V2`.
6. Export to Unity (already done once, 2026-10-18; repeat after a rebuild):
   - Run `python build_slash_stylize_v2.py --unity`, or copy `output/SlashStylize/VFX_Slash_Stylize_V2/VFX_Slash_Stylize_V2_Sheet.png` over `Assets/Image/VFX/VFX_Slash_Stylize_V2_Sheet.png`. Overwrite the PNG only and keep its `.meta`, so the GUID and the 16 sliced sprites (`VFX_Slash_Stylize_V2_Sheet_0` to `_15`, 4x4 grid of 256 px, frame 0 top-left) stay valid.
   - Import settings match the V1 sheet: Texture Type Default, Sprite Mode Multiple, alpha is transparency.
   - Material: `Assets/Materials/Slash_VFX_V2.mat` is a copy of `Slash_VFX.mat` (Particles Standard Unlit) with the V2 sheet as its texture.
   - `Assets/Resources/Prefab/VFX/SlashVFX.prefab` uses `Slash_VFX_V2` on its Particle System Renderer (switched 2026-10-18; set it back to `Slash_VFX` to return to V1). Texture Sheet Animation is 4x4, 1 cycle. Tint with Start Color or the material colour.
   - See [Sprite Sheet VFX Workbench Walkthrough](../Visuals/SpriteSheet_Workbench_Walkthrough.md) for sprite-based use.

## Expected result

- 16 frames at 30 fps (0.53 s). Frames 0 to 7 carry the full slash, 8 to 15 dissolve into fragments and sparks, frame 15 is near empty so the clip ends clean.
- `VFX_Slash_Stylize_V2_Sheet.png`, `_Contact.png`, `_Preview.gif` and `frames/` in `output/`, plus the editable `.ecproj` in `projects/`, all white (grayscale rule).

## Common mistakes

- **`GrayscaleOnlyRuleError` when running.** A layer colour has a hue. Use a gray value; colour belongs in Unity.
- **Tinting with a dark colour.** The sheet uses additive layers, so a dark tint looks muddy. Use a bright tint (HDR colour if the material supports it).
- **Running the script with `--force` after hand edits.** `--force` deletes and rebuilds the project. Without it, the edited project is kept and only re-rendered.
- **Running from another folder.** Paths are relative to the script, so that works, but `effectcraft-cli` must exist at `C:\Program Files\EffectCraft`; update `CLI_PATH` if it moves.
- **Hand-colouring in EffectCraft.** Same rule as the script: a project with hues is not accepted for the game.
