# Antigravity Agent Directives & Case Learnings (GEMINI.md)

> [!IMPORTANT]
> This configuration and case learning is exclusively scoped to the **Antigravity (Google DeepMind)** pair-programming agent. It must not be merged into general agent configs or Claude guidelines.

---

## 1. UI Porting & Asset Consolidation Rules

### A. Asset Reusability Before Exporting
- **Check Existing Project Assets First**: Never export a new 9-slice sprite or rounded rectangle if a standard one exists in the project.
- **Canonical Rounded Sprite**: Use `Assets/Art/UI/Old/rounded.png` (GUID: `9df5b6060bad4fb4b9b050483f64c324`, 9-slice border `20, 20, 20, 20`) for general pills, tags, badge frames, and stat row containers.
- **Dynamic Color Tinting**: Keep base 9-slice sprites pure white and apply tint colors at runtime via `Image.color` (e.g., `{r: 0, g: 0.582, b: 1, a: 1}` for rarity badges) instead of baking color into separate PNG files.

### B. Interactive & Dynamic UI Controls
- **Progress Bars**: Do not rely on static `Image` width manipulation for progress indicators. Use a native `UnityEngine.UI.Slider` component:
  - `m_Interactable: 0`
  - `m_Direction: 0` (Left-to-Right)
  - `m_FillRect`: points to the fill child `RectTransform` (anchors `(0, 0)` to `(0, 0)`)
  - Value driven natively between `0.0` and `1.0`.

### C. TextMesh Pro SDF Font Atlas Verification
- When TextMesh Pro text renders invisible despite correct references, check if `m_GlyphTable: []` is empty in the SDF font asset (e.g. `NotoSansThai-SemiBold SDF.asset`).
- The font atlas must have character metrics generated in the Unity Editor to display text glyphs properly.

### D. Reusable Button Decoupling
- **Never Bake Text into Button Sprites**: The button container must only hold the background visuals and decorative chrome (such as circuit traces and icons).
- **Standalone Label**: Button label text must be a separate child `TextMeshProUGUI` component positioned between visual adornments (e.g., `pos: (125, 0)` for `290 x 50px` buttons).

### E. Top-Left Mathematical Coordinate Mapping
- Use Top-Left anchors `(anchorMin = (0, 1), anchorMax = (0, 1), pivot = (0, 1))` for window contents.
- Coordinates translate directly from Figma:
  $$\text{pos.x} = \text{figma.x} - \text{parent.x}$$
  $$\text{pos.y} = -(\text{figma.y} - \text{parent.y})$$

### F. Strictly 1:1 Export Scale
- Export assets strictly at **1:1 scale** (`1x`). Never assume or upscale (such as 2x) unless explicitly ordered by the user.

### G. Transparent Padding & Aspect Shrinkage Audit
- When exporting assets (e.g. dials, emblems, icons), verify if the file has transparent outer margins.
- If transparent margin exists, either perform a tight alpha crop or set the Unity `RectTransform` size to match the exact slice bounds. Never pair a tight inner JSON bounding box with a loosely-padded sprite under `preserveAspect = true`, as this causes drastic unwanted art shrinkage (Case: Mandala dial).

### H. Minimal Simple Light Theme for Analysis Images
- UI audit and comparison images must use a **minimal, simple light theme** (clean white background, readable dark typography, clean subtle borders) instead of flashy dark sci-fi decorations.

### I. Never Hand-Write or Synthesize YAML Prefabs
- Never invent GUIDs or generate synthetic YAML string blocks. Always build and modify prefabs through C# Editor scripts (`PrefabUtility.LoadPrefabContents` / `SaveAsPrefabAsset`).

### J. LayoutElement Size Priority Over Sliced Sprites (64px Bug)
- Sliced sprite images inside LayoutGroups report their border sums as preferred size (priority 0) and can blow up pills/badges to 64px.
- Always attach a `LayoutElement` with `minHeight/preferredHeight` matching Figma, and set `flexibleWidth = 0` / `flexibleHeight = 0` (priority 1 overrides image borders).

### K. TMP Ellipsis Vanishing Line Prevention
- When using `OverflowMode.Ellipsis` or `Truncate`, the RectTransform box height must be at least the font's line height (e.g. Novecento 16pt requires box height >= 24px), otherwise TMP silently hides the entire line of text.

### L. Novecento Lowercase is Small Caps
- Novecento lowercase renders as tiny small caps. Always force `FontStyles.UpperCase` or `.ToUpperInvariant()` on Novecento text.

### M. Leverage Toolkit in `Tools/FigmaToUnity/`
- `figma_tree.py`: Window-relative coordinates directly from Figma node JSON.
- `measure_art.py`: Pixel-sampling of 9-slice borders and outer strokes.
- `compare.py`: 50/50 image blend where misaligned components ghost.
- `compile_check.py`: C# compile validation via `dotnet build` without opening Unity.

---

## 2. Mandatory Subagent-Driven Execution Policy

> [!IMPORTANT]
> Antigravity must strictly operate under a **Subagent-Driven Development (SDD)** paradigm for all multi-step feature implementations, code refactoring, and multi-file tasks within this project.

### A. Controller Role & Context Isolation
- **Orchestrator Role**: Antigravity functions strictly as the high-level controller/orchestrator. Never pollute the main session context with large code blocks, full file writes, or iterative debugging churn.
- **Context Hygiene**: Retain controller context exclusively for task decomposition, dependency tracking, architecture alignment, and reviewing outcomes.

### B. Subagent Dispatch & Delegation
- **Fresh Subagent per Task**: Dispatch dedicated subagents (`invoke_subagent`) for each distinct implementation task.
- **Bounded Task Briefs**: Subagents must receive lean, self-contained briefs containing explicit requirements, relevant file paths, and interface signatures. Never dump the entire conversation history into a subagent prompt.
- **Batching Equivalent Work**: Only small, same-shape mechanical edits across multiple files may be grouped into a single subagent dispatch.

### C. Review & Quality Gates
- **Task Review Verification**: Every subagent deliverable must undergo verification (spec compliance and code quality) before marking a task complete.
- **Fix Loop**: If verification reveals issues or regressions, resume the subagent or dispatch a targeted fix subagent with concrete failure points. Escalate model tier if an implementer stalls across rounds.
- **No Controller Fixes**: Never bypass the subagent workflow to make direct code fixes in the controller session. Controller-level fixes skip proper review gates and compromise context purity.

### D. Autonomous Flow & Non-Stalling Execution
- **Continuous Progress**: Execute planned tasks end-to-end without stopping to ask routine "Should I continue?" or permission checks.
- **Halt Conditions**: Pause only for irreversible destructive actions, critical security boundaries, or fundamental plan defects where all possible paths forward are guesses.

