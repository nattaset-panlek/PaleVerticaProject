---
name: figma-to-unity
description: Turn a Figma design into Unity UI for Pale Vertica that matches it to the pixel. Use for ANY task that builds, rebuilds, restyles or "makes it look like Figma" for a window, page, prefab, panel, list item, button or modal, or that mentions a Figma frame, node id, mock image, Talk to Figma, or comparing UI with a design.
---

# Figma to Unity UGUI: Pixel-Perfect Engineering Playbook

> [!IMPORTANT]
> **Antigravity Agent Directive**: This skill is strictly configured for the **Antigravity (Google DeepMind)** pair-programming agent within the *Pale Vertica* codebase. It provides the definitive mathematical and structural playbook to achieve visual and functional parity without trial-and-error regression loops.
> Reference docs: `Docs/02-Technical/UI/Figma_To_Unity_Playbook.md` and `Docs/02-Technical/UI/Figma_To_Unity_Pipeline_Logic.md`.

---

## 1. The Core Rules & Definition of Done

### Never:
1. **Never guess a number**: Every position, size, colour, font size, and letter spacing comes from Figma node data or the art's pixels.
2. **Never hand-write or generate YAML prefabs**: Never invent GUIDs or generate synthetic YAML strings. Change prefabs exclusively through C# Editor scripts (`PrefabUtility.LoadPrefabContents` / `SaveAsPrefabAsset`).
3. **Never change game formulas for the UI**: Read values through existing APIs (e.g. `SkillCaster.BuildPayload`).
4. **Never upscale or assume scale**: Export assets strictly at **1:1 scale** (`1x`). Do not upscale unless ordered.
5. **Never declare victory from code reading alone**: Always inspect the rendered PNG and run diff verification.

### Definition of Done (All 5 must be true):
1. **Visual Match**: Rendered snapshot compared with Figma export using `Tools/FigmaToUnity/compare.py` (blend + zooms) and `Tools/UICompare/generate_socket_analysis.py` (minimal light theme). Every remaining delta is explained.
2. **0 Layout Errors**: `Logs/UIDebug/<screen>.txt` layout report reports **0 errors**.
3. **Tests Pass**: `UILayoutRegressionTests` pass in EditMode.
4. **Compile Check**: `python -I Tools/FigmaToUnity/compile_check.py` prints OK for runtime and editor assemblies.
5. **Walkthrough Documented**: Walkthrough exists at `Docs/02-Technical/<System>/<Name>_Walkthrough.md` and is indexed in `Docs/README.md`.

---

## 2. Tooling Architecture

You have ready-made, verified scripts in two tool suites:

### Suite A: `Tools/FigmaToUnity/`
* `figma_tree.py`: Parses Figma node JSON and prints window-relative coordinates, typography, and styles.
* `measure_art.py`: Samples sprite pixels to find exact outer strokes, inner panels, and 9-slice borders.
* `prefab_tree.py`: Inspects Unity prefab components and RectTransforms without launching Unity.
* `compare.py`: Pillow-based 50/50 blend (`Image.blend`) where misaligned parts ghost, plus zoomed crops.
* `compile_check.py`: Checks C# compilation via `dotnet build` without opening Unity.
* `unity_batch.sh`: Monitors Unity process and lockfile, running batchmode build and render cleanly.

### Suite B: `Tools/UICompare/`
* `figma_client.js`: WebSocket client for TalkToFigma MCP (`ws://localhost:3055`).
* `generate_socket_analysis.py`: Minimal simple light theme OpenCV difference map, subpixel centroid drift analysis, and zone scorecards.

---

## 3. Failure Post-Mortem & Layout Traps Table

Learn these pitfalls before writing a single line of code:

| Symptom / Trap | Root Cause | Exact Remediation |
|---|---|---|
| **Element 64px tall / too big** | An `Image` with a sliced sprite reports its 9-slice border sum as preferred size (priority 0) and overrides layout | Attach a `LayoutElement` with `minHeight`, `preferredHeight` set to Figma height, and `flexibleHeight = 0` (priority 1 wins) |
| **Pill / tag stretches across row** | Parent layout group has `childForceExpand = true`, giving children `flexible = 1` | Turn force-expand off, or set `LayoutElement.flexibleWidth = 0` |
| **Tags 40px tall instead of 30px** | Flexible size leak on the vertical axis | Add `LayoutElement` with `flexibleHeight = 0` and `preferredHeight = 30` |
| **`NestedFitter` error** | `ContentSizeFitter` inside a child of a LayoutGroup | Remove inner fitter; let parent LayoutGroup control child dimensions (`childControlWidth/Height = true`) |
| **Text line vanishes completely** | TMP **Ellipsis / Truncate** silently hides lines when box height is shorter than font line height | Make RectTransform box height at least the font line height (e.g. Novecento 16pt needs box height >= 24px) |
| **Novecento looks tiny / broken** | Novecento lowercase characters render as tiny small caps | Always uppercase Novecento text (`FontStyles.UpperCase` or `.ToUpperInvariant()`) |
| **Double-Tinting Multiplier Bug** | Baked color in PNG multiplied by Unity `Image.color` ($G_{\text{final}} = G_{\text{tex}} \times G_{\text{tint}}$) | Keep base 9-slice sprites pure white (`#ffffff`). Apply tinting exclusively via `Image.color` |
| **Canvas Coordinate Offset Trap** | Figma canvas window center vs Unity screen center ($x=394$ vs $(1920-1083)/2$) | Always crop and calculate positions relative to the **modal root bounding box**, not full-screen absolute |
| **Art vs Frame Offset (`ArtInset`)** | Figma draws outer stroke outside frame; art is larger than frame (e.g. 1089x744 vs 1083x738) | Apply $\text{art coordinate} = \text{Figma coordinate} + \text{ArtInset}$ (e.g. +3px) |
| **Transparent Padding Shrinkage** | Exported sprite has transparent margins, but RectTransform matched a tight inner sub-node with `preserveAspect = true` | Tight-crop transparent padding before import OR set RectTransform to match the exported slice frame bounds 1:1 |
| **Header texture squashed** | Sliced image on a narrow window | Use `Image.Type.Tiled` + `spriteMeshType: FullRect` |
| **Button text smudged / uneditable** | Exporting button as a flattened monolithic raster | Inpaint text from background (`btn_primary_clean_290x50.png`) and add standalone child `TextMeshProUGUI` |
| **Invisible text** | SDF Font asset created with empty atlas table (`m_GlyphTable: []`) | Verify character metrics in Unity Editor Font Asset Creator |

---

## 4. Typography & Mathematical Formulas

### A. Canonical Font Registry
* `Novecentosanswide-DemiBold SDF` (`ed472c2881e9b894fb40179e7c9b4332`): Titles, stat labels, emphasis
* `Novecentosanswide-Normal SDF` (`a66aeea0855f6544883cbd79dc661f3a`): Body, item types, consumption costs
* `Kanit-Bold SDF` (`0063cb4c629a50d499805b5a4274846a`): Header tags, skill titles
* `Kanit-ExtraBold SDF` (`ab7855212e5d209409402ec8a49664dd`): Numeric badges
* `NotoSansThai-SemiBold SDF` (`57d8cea201ff3e04b8aef8e01a97988a`): Action button labels

### B. Letter Spacing Mathematical Formula
TextMesh Pro `characterSpacing` is expressed in **1/100 em**:
$$\text{characterSpacing} = \frac{\text{letterSpacing}_{\text{px}}}{\text{fontSize}_{\text{px}}} \times 100$$
*Example*: 4.8px letter spacing on 32px font = $\frac{4.8}{32} \times 100 = 15$.

### C. Top-Left Mathematical Coordinate Mapping
$$\text{pos.x} = \text{child.x} - \text{modal.x} + \text{ArtInset}$$
$$\text{pos.y} = -(\text{child.y} - \text{modal.y} + \text{ArtInset})$$
Anchors: `anchorMin = (0, 1)`, `anchorMax = (0, 1)`, `pivot = (0, 1)`.

---

## 5. Standard Step-by-Step Execution Sequence

1. **Query & Dump Tree**:
   - `export_node_as_image(nodeId, PNG, scale=1)`
   - Run `python -I Tools/FigmaToUnity/figma_tree.py node.json 14 --window "<window name>"` to get window-relative coordinates.
2. **Asset Triage**:
   - Reuse `Assets/Art/UI/Old/rounded.png` for all standard rounded pills/tags/stat rows.
   - Reuse `Circle.png` and clean decoupled button backgrounds.
   - Measure 9-slice borders using `python -I Tools/FigmaToUnity/measure_art.py`.
3. **C# Editor Builder Implementation**:
   - Create/update `<Window>PrefabBuilder.cs` with `#region` organization.
   - Use `PrefabUtility.LoadPrefabContents` / `SaveAsPrefabAsset`.
   - Clone existing headers, scrollviews, and buttons instead of rebuilding from scratch.
   - Wire all serialized properties through `SerializedObject` and `FindProperty`.
4. **Headless Batchmode Snapshot**:
   - Ensure Unity is closed (`Temp/UnityLockfile` does not exist).
   - Execute batch build and snapshot renderer via PowerShell.
5. **Quantitative Comparison**:
   - Run `python -I Tools/FigmaToUnity/compare.py` (blend + zoom) and `generate_socket_analysis.py` (minimal light theme).
   - Check layout report `Logs/UIDebug/<screen>.txt` for 0 errors.
6. **Closed-Loop Convergence**:
   - Read subpixel deltas from blend/heatmap, adjust C# builder, and re-run until fidelity reaches >93-95%.
