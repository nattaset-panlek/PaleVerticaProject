---
name: figma-to-unity
description: Turn a Figma design into Unity UI for Pale Vertica that matches it to the pixel. Use for ANY task that builds, rebuilds, restyles or "makes it look like Figma" for a window, page, prefab, panel, list item, button or modal, or that mentions a Figma frame, node id, mock image, Talk to Figma, or comparing UI with a design. Gives the required steps, the ready-made scripts in Tools/FigmaToUnity, and the definition of done.
---

# Figma to Unity (Pale Vertica)

Full explanation, examples and pitfall table: `Docs/02-Technical/UI/Figma_To_Unity_Playbook.md`. Read section 7 (layout traps) of it before you build anything.

## Definition of done (all must be true before you say "done")

1. A render of the screen at the Figma position, compared with the Figma export using `compare.py` (blend + zooms). Every remaining difference is listed to the user with its cause.
2. `Logs/UIDebug/<screen>.txt` shows **0 errors**.
3. `unity_batch.sh tests` passes.
4. `compile_check.py` prints OK for both assemblies.
5. A walkthrough exists for any new tool (`Docs/02-Technical/UI/<Name>_Walkthrough.md`), linked from `Docs/README.md`.

If one is not true, you are not done. Say which one and why.

## Never

- Never guess a number. Every position, size, colour, font size and letter spacing comes from Figma node data or from the art's pixels.
- Never hand-write or generate prefab/scene YAML, and never invent GUIDs. Change prefabs only through an editor script (`PrefabUtility.LoadPrefabContents` / `SaveAsPrefabAsset`).
- Never change game formulas for the UI. Read values through existing APIs.
- Never commit, close the user's Unity, or change assets or core code without asking first.
- Never declare a visual result from code reading alone. Look at the PNG.

## Steps

1. **Read**: this file, the playbook, `.claude/skills/pale-vertica-dev/SKILL.md`, the screen's walkthrough if it has one.
2. **Export the design** (Talk to Figma: `join_channel` with the user's channel, usually `flowux`):
   - `export_node_as_image(nodeId, PNG, scale=1)` for every frame, copied to your scratchpad;
   - `get_node_info(nodeId)` saved to a JSON file, then
     `python -I Tools/FigmaToUnity/figma_tree.py node.json 14 --window "<window frame name>"`.
     Positions printed are window-relative: use them directly.
3. **Map assets**:
   - fonts: Kanit weight assets; Novecento = `Novecentosanswide-DemiBold SDF` with **UpperCase** (its lowercase is small caps). `characterSpacing = letterSpacingPx / fontSizePx * 100`. Auto-size off;
   - sprites: search `Assets/Art/UI/New/` first; measure with
     `python -I Tools/FigmaToUnity/measure_art.py <png> --row Y --col X` and set the 9-slice border. Tiled + Full Rect for textured bands;
   - check the art vs frame offset (Skill Manage art is 3 px bigger on each side than the Figma frame);
   - only export a shape from Figma when no art exists.
4. **Inspect the existing prefab**: `python -I Tools/FigmaToUnity/prefab_tree.py "<prefab>" --comps`.
5. **Ask the user** about missing data, which states and frames to cover, and approval for asset or code changes.
6. **Build** with an editor script modelled on `Assets/Script/Editor/UI/SkillManagePrefabBuilder.cs`:
   - menu item + `BuildBatch()`;
   - idempotent (remove the parts you own, then rebuild);
   - `Place()` at Figma coordinates;
   - clone existing headers, scroll views and buttons for an identical look;
   - fixed sizes through `LayoutElement` with flexible 0;
   - serialized refs through `SerializedObject`.
   Code-built windows use the UIBuilder and registry styles instead (`Docs/02-Technical/UI/Light_UI_Kit_Walkthrough.md`).
7. **Code**: Views that listen to events and fill text; data from existing APIs; follow `AGENTS.md` (no comments, `#region`, no runtime Find). Check with
   `python -I Tools/FigmaToUnity/compile_check.py`.
8. **Snapshot**: add the screen to `UISnapshotRenderer.Run`. For a prefab use `ProcessPrefab` at the Figma window centre (anchoredPosition = centre minus (960, 540), y flipped) with backdrop #434343.
9. **Run** (asks the user to close Unity, then waits):
   `Tools/FigmaToUnity/unity_batch.sh <YourBuilder>.BuildBatch CoreSystem.Editor.UI.UISnapshotRenderer.RenderAllBatch`
10. **Compare**:
    `python -I Tools/FigmaToUnity/compare.py figma.png Logs/UIDebug/<screen>.png --box <window box> --zoom <area> --zoom <area> --out <folder>`.
    Read `blend.png`, then every zoom. Write each difference with its size in pixels, find the cause in `<screen>.txt`, fix it in the builder, and go back to step 9. The mean difference should drop every time.
11. **Test**: `Tools/FigmaToUnity/unity_batch.sh tests`
12. **Document**: walkthrough + `Docs/README.md` link. Add any new trap you hit to the playbook's section 7 table, so the next agent knows it.
13. Tell the user they can open Unity again, and list the known differences honestly.

## Quick trap list (details in the playbook)

| You see | Fix |
|---|---|
| element 64 px tall / too big | sliced Image reports its sprite size; add `LayoutElement` with the Figma size |
| pill or tag stretches | layout group force-expand gives flexible 1; set flexible 0 |
| text line vanished | TMP Ellipsis hides lines taller than the box; box height >= line height |
| text smaller than Figma | auto-size on, or Novecento lowercase |
| `NestedFitter` | remove the ContentSizeFitter under a layout group |
| `SiblingOverlap` | negative spacing; use 0 |
| art button tinted | registry entry Keep Prefab Look |
| header texture squashed | Tiled + Full Rect |
