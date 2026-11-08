# Antigravity UI Porting & Optimization Walkthrough (Figma to Unity UGUI)

> [!NOTE]
> **Scope**: Recorded for the **Antigravity (Google DeepMind)** Agent. This document preserves the complete systemic learnings, failure post-mortems, anti-patterns, and shortcut playbooks discovered while migrating `Refinement 16:9` (`569:2453`, now `Screen / Refinement` `1786:24436`) from Figma into Unity UGUI.

---

## 1. Overview & Core Philosophy

Porting Figma UI to Unity UGUI is an **engineering science**, not an artistic guessing game. 

* **The 80/20 Reality**: Automated coordinate conversion reaches ~70-80% fidelity quickly, but the remaining 20-30% involves component semantics (9-slice borders, dynamic controls, typography alignment, and atlas baking) that determine whether the UI looks production-ready or amateurish.
* **The Goal**: Reach 95%+ automated fidelity using mathematical coordinate mapping, asset consolidation, component decoupling, and diagnostic delta heatmaps.

---

## 2. Tooling Architecture (`Tools/UICompare/`)

All comparison and automation tools reside strictly within the project repository at `Tools/UICompare/`:

```
Tools/UICompare/
├── figma_client.js            # WebSocket client communicating with TalkToFigma MCP (ws://localhost:3055)
├── build_refinement_perfect.js# Deterministic Unity YAML Prefab generator
├── compare_ui.py              # Quantitative Pillow-based crop similarity comparator
├── generate_analysis_image.py # OpenCV diagnostic heatmap generator (bounding boxes, insets, scores)
└── setup_sprites.js           # Automated .meta generator with exact 9-slice border definitions
```

---

## 3. Failure Post-Mortem: Why Initial Ports Look Terrible

During initial iterations, several severe visual defects occurred. Understanding their root causes prevents repeating these anti-patterns:

### Defect 1: Blurry, Stretched, or Upscaled Bitmaps
* **Symptom**: Borders look muddy; circular corners become pixelated or stretched unevenly.
* **Root Cause**: Exporting vector frames at 1x resolution without 9-slice slicing, or resizing them nonuniformly.
* **Remediation**:
  * Export all vector shapes at **2x resolution** (`@2x`).
  * Always configure `spriteBorder: {x: L, y: B, z: R, w: T}` in the `.meta` file.
  * Check existing project assets first (see Section 4).

### Defect 2: Baked Text on Action Buttons (Smudged / Non-Reusable)
* **Symptom**: Primary action button ("Refinement") has text overlapping decorative circuit lines, blurred edges, and unchangeable copy.
* **Root Cause**: Trying to capture the button as a single flattened raster image from Figma.
* **Remediation**:
  * **Frame Asset**: Inpaint/remove the text from the button background (`btn_primary_clean_290x50.png`). Keep only circuit lines and decorative badges (chevron circle).
  * **Text Child**: Add a standalone child `TextMeshProUGUI` component anchored between decorative markers (`pos: (125, 0)` for a 290px button).

### Defect 3: Font Asset GUID Confusion & Typography Mismatches
* **Symptom**: Text appears blocky, wrong weight, or uses fallback fonts.
* **Root Cause**: Assuming font asset GUIDs from names or confusing Thai fonts (`Kanit`) with European geometric fonts (`Novecento`).
* **Canonical Font Registry for Pale Vertica**:
  * `Novecentosanswide-DemiBold SDF`: `ed472c2881e9b894fb40179e7c9b4332` (Titles, stat labels, emphasis)
  * `Novecentosanswide-Normal SDF`: `a66aeea0855f6544883cbd79dc661f3a` (Body, secondary details)
  * `Kanit-Bold SDF`: `0063cb4c629a50d499805b5a4274846a` (Header tags)
  * `Kanit-ExtraBold SDF`: `ab7855212e5d209409402ec8a49664dd` (Numeric badges)
  * `NotoSansThai-SemiBold SDF`: `57d8cea201ff3e04b8aef8e01a97988a` (Action button labels)

### Defect 4: Invisible Text Due to Empty Glyph Table
* **Symptom**: `TextMeshProUGUI` component exists with correct text string, but renders blank in Game View.
* **Root Cause**: The `.asset` font file was created with an empty atlas/glyph table (`m_GlyphTable: []`).
* **Remediation**: Always verify font atlas metrics in Unity Editor. If empty, re-bake font characters in TextMesh Pro Font Asset Creator.

### Defect 5: Double-Tinting Multiplier Bug
* **Symptom**: Blue badge or stat row background appears black or muddy dark navy.
* **Root Cause**: Baking blue color (`#0048ff`) into the PNG file, and then applying blue `Image.color` (`#0048ff`) in Unity. The channels multiply:
  $$G_{\text{final}} = G_{\text{texture}} \times G_{\text{tint}} = 72 \times 0.282 = 20$$
* **Remediation**: Keep base 9-slice sprites pure white (`#ffffff`). Apply color tinting exclusively via `Image.color`.

### Defect 6: Canvas Coordinate Offset Trap
* **Symptom**: Unity modal appears shifted 27px to the right compared to Figma.
* **Root Cause**: Figma's 1920x1080 canvas placed the modal at `x = 391`, whereas centering a 1083px window in 1920px puts it at `x = 418.5`.
* **Remediation**: When comparing UI, always crop relative to the **modal bounding box** (`1083 x 705`), not full-screen absolute coordinates.

### Defect 7: Phantom Masked Nodes in Figma JSON
* **Symptom**: Delta heatmap shows mismatches for elements you thought were in Figma.
* **Root Cause**: Figma JSON trees often contain hidden or masked layers (e.g. quantity badges "17" underneath material slots that were masked by a parent rect).
* **Remediation**: Never trust JSON alone. Cross-reference node trees against the rendered PNG preview to verify layer visibility.

---

## 4. Developer Manual Patterns (The Missing 20%)

These patterns were implemented by the developer to transform a static mockup into a production-grade UI:

### A. Consolidate to Standard 9-Slice (`rounded.png`)
* Never export new rounded rectangle PNGs if a standard one exists in the project.
* **Standard Sprite**: `Assets/Art/UI/Old/rounded.png`
* **GUID**: `9df5b6060bad4fb4b9b050483f64c324`
* **9-Slice Border**: `(20, 20, 20, 20)`
* Use this single sprite for header pills, rarity tags, and stat row backgrounds.

### B. Native `UnityEngine.UI.Slider` for Progress Bars
* Do not fake progress bars using static `Image` width.
* Attach a `UnityEngine.UI.Slider` to `Rate Bar Bg`:
  * `m_Interactable: 0`
  * `m_Direction: 0` (Left-to-Right)
  * `m_FillRect`: points to the fill child `RectTransform` (anchors `(0, 0)` to `(0, 0)`)
  * `m_Value`: driven programmatically between `0.0` and `1.0`.

---

## 5. The "Shortcut" Playbook (Step-by-Step Execution)

Follow this sequence to port any future Figma window in 1-2 passes:

### Step 1: Query Figma Dimensions & Anchors
1. Connect to TalkToFigma MCP (`Tools/UICompare/figma_client.js`).
2. Read the root modal's bounding box: `(w, h)`.
3. Create a parent container RectTransform with `pivot = (0.5, 0.5)` and `sizeDelta = (w, h)`.

### Step 2: Top-Left Anchor Mapping (Zero Guesswork)
For every child element inside the modal, set:
* `anchorMin: (0, 1)`
* `anchorMax: (0, 1)`
* `pivot: (0, 1)`

Calculate positions mathematically:
$$\text{anchoredPosition.x} = \text{child.x} - \text{modal.x}$$
$$\text{anchoredPosition.y} = -(\text{child.y} - \text{modal.y})$$

### Step 3: Asset Reuse Check
* Does the element use a simple rounded container? Use `rounded.png` (`9df5b6060bad4fb4b9b050483f64c324`) with `Image.color` tint.
* Is it a primary button? Use `btn_primary_clean_290x50.png` with a standalone text child.
* Is it a level badge? Use `badge_level_frame.png` with text split (`Lv.12<size=75%><color=#737373>/20</color></size>`).

### Step 4: Font & Typography Verification
* Assign correct font GUIDs from the Canonical Font Registry.
* Match text case: if Figma text has uppercase styling, specify all-caps in TMP text string.
* Check overflow: use `m_overflowMode: 0` (Overflow) or ensure box height exceeds font line height before using `m_overflowMode: 1` (Ellipsis).

### Step 5: Verification & Diagnostic Audit
1. Run Unity batchmode snapshot.
2. Run `compare_ui.py` and `generate_analysis_image.py`.
3. Inspect `refinement_advanced_analysis.png`:
   * Green zones (>90%): verified.
   * Orange/Red zones (<90%): consult Section 3 to apply exact remediation.
