# VFX Preview Tool Walkthrough (EffectCraft)

`Tools/VfxPreview` builds 2D sprite-sheet VFX in EffectCraft, keeps an editable `.ecproj` for each effect, renders sheets and GIFs, and shows everything in one gallery page with gradient-map colouring. Slash-specific details are in [Slash Stylize V2 Walkthrough](./Slash_Stylize_V2_Walkthrough.md).

## Rule: grayscale only (blocking)

Every sheet is grayscale (black to white). Colour comes from a gradient map or tint in Unity, never from the sheet. This is checked twice:
- before a script builds a project: `enforce_grayscale_only()` refuses any layer colour with a hue;
- on every render, including projects edited by hand: `check_grayscale_pixels()` refuses any rendered pixel whose R, G and B differ by more than 3.

Both raise `GrayscaleOnlyRuleError`, and nothing is exported.

## Where

| Path | What it holds |
| --- | --- |
| `Tools/VfxPreview/index.html` | Gallery: every effect, animated, with gradient-map, speed, background and frame-strip controls. Generated. |
| `Tools/VfxPreview/projects/<Set>/<Effect>.ecproj` | **Editable masters.** One EffectCraft project per effect. Open and edit these. |
| `Tools/VfxPreview/output/<Set>/<Effect>/` | Renders: `_Sheet.png` (4x4, transparent), `_Contact.png`, `_Preview.gif`, `frames/`, and the generator `.jsx`. Regenerated, do not edit. |
| `Tools/VfxPreview/scripts/` | Python: `vfx_effectcraft.py` (shared runtime), `build_*.py` (generators per set), `render_vfx.py` (render edited projects), `make_vfx_gallery.py` + `vfx_gallery_template.html` (gallery). |
| `Tools/VfxPreview/_archive/` | Early experiments (`legacy_antigravity/`) and old gallery copies. Kept for reference, not used. |
| `C:\Program Files\EffectCraft\effectcraft.exe` | EffectCraft editor (After Effects-style). `effectcraft-cli.exe` is the headless CLI / MCP server the scripts call. |

Sets in `projects/`:

| Set | Script | Look |
| --- | --- | --- |
| `SlashStylize` | `build_slash_stylize_v2.py` | Sword slash V2 (in use by `SlashVFX.prefab`) and the original V1 |
| `WeaponV2Style` | `build_weapon_vfx_v2style.py` | Circles and tapered arcs in the Slash V2 language (current direction) |
| `WeaponArtStylized` | `build_weapon_vfx_art.py` | Jagged anime hit sparks |
| `WeaponPainterly` | `build_weapon_vfx_paint.py` | Dark-backed painterly spikes (rejected direction, kept for reference) |
| `WeaponClean` | `build_weapon_vfx.py` | First geometric pass |

## Inputs

- Python 3 with Pillow and NumPy (`pip install pillow numpy`).
- EffectCraft installed at `C:\Program Files\EffectCraft` (change `CLI_PATH` in `scripts/vfx_effectcraft.py` if it moves).

## Steps

### A. Look at the effects
1. Open `Tools/VfxPreview/index.html` in a browser.
2. Pick a gradient map (Fire, Arcane, Void, Poison, Holy, Ice, Raw) to see how one grayscale sheet looks per element.
3. Tick "เลือก" on the effects you want; the box at the bottom lists them to copy.

### B. Edit an effect by hand
1. Open `projects/<Set>/<Effect>.ecproj` in `effectcraft.exe`.
2. Edit layers, keyframes and effects. Keep every colour gray.
3. Save (Ctrl+S) to the same file.
4. In a terminal at `Tools/VfxPreview/scripts`:
   ```bash
   python render_vfx.py VFX_Fist_Jab_V2
   ```
   A target can be an effect name, a set name (`WeaponV2Style`), or a `.ecproj` path. With no target, every project is rendered.
5. The sheet, contact strip, GIF and `index.html` are updated.

### C. Regenerate an effect from its script
1. Change the recipe function in the set's `build_*.py`.
2. Run, for example:
   ```bash
   python build_weapon_vfx_v2style.py VFX_Magic_Burst_V2 --force
   ```
   Without `--force` an existing `.ecproj` is **kept** (it may hold hand edits) and only re-rendered. `--force` deletes and rebuilds it from the script.
3. Run `python make_vfx_gallery.py` (or `render_vfx.py`) to refresh the gallery.

### D. Add a new effect
1. Add a recipe function that returns layers (use `poly_layer`, `shape_layer`, `multi_layer` with `value=` gray levels) and register it in that script's `RECIPES`.
2. Name it `VFX_<Category>_<Move>_<SetSuffix>`; the gallery groups by the `<Category>` part (Fist, Magic, Spear, Sword).
3. Run the script with the new name, then edit the `.ecproj` if needed (B).

### E. Send a sheet to Unity
```bash
python render_vfx.py VFX_Fist_Jab_V2 --unity
```
This copies the sheet to `Assets/Image/VFX/Weapon/` and writes a sliced 4x4 `.meta` the first time; later exports overwrite only the PNG, so GUIDs and references stay valid.

## Expected result

- `projects/` holds only `.ecproj` files you can open and edit; `output/` holds only generated files.
- Each effect renders 16 frames at 30 fps into a 1024x1024 transparent 4x4 sheet.
- `index.html` lists every effect grouped by category and set, with the project path under each card.

## Common mistakes

- **Editing files in `output/`.** They are overwritten on every render. Edit the `.ecproj` in `projects/`.
- **Running a build script with `--force` after hand edits.** That discards the edits. Without `--force` the project is kept.
- **`GrayscaleOnlyRuleError` after a hand edit.** A fill, stroke or effect (for example Glow colour, Roughen Edges "Color" types) added a hue. Set it back to gray.
- **Changing the comp name inside a project.** `render_vfx.py` renders the comp named like the file, otherwise the active comp. Keep the names matched, or pass `--comp`.
- **Changing duration or frame rate.** Allowed, but the sheet grid becomes `ceil(sqrt(frames))` square, and Unity flipbooks set to 4x4 must be updated to match.
- **`WARN ... touches the frame edge`.** Content reaches the 256 px border and will look cropped in game. Move or shrink it so it keeps about 16 px of margin.
- **Turbulent Displace on thin shapes.** It turns them into wavy "tentacles"; keep it off for the V2 style.
