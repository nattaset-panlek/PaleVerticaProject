# AI Icon Crafter & Asset Analyzer (`icon_crafter.py`)

A specialized command-line tool designed for AI collaborators (and human developers) to synthesize high-fidelity 2D icons, complex geometric vector shapes, and analyze existing game assets in **Realm of Hero**.

---

## 1. Architecture & Design Recommendations for AI Tooling

When building tools for LLM/AI interaction, raw pixel manipulation is error-prone. The optimal architectural bridge between LLMs and visual rendering is a **Semantic Vector DSL (Domain-Specific Language)** encoded in structured JSON.

### Why JSON Vector Specs for AI?

- **Mathematical Precision**: AI models naturally excel at calculating spatial geometry, symmetry, bezier control points, and angles.
- **Layer-Based Composition**: By organizing icons into discrete rendering layers (`circle`, `rect`, `polygon`, `star`, `gradient`, `line`), the AI can compose complex designs using **Progressive Disclosure** (building background, silhouette, inner details, and highlight overlays sequentially).
- **Dual Mode Support (Icon & Symbol)**:
  - **Icon Mode (`--mode icon`)**: Full color rendering with background circles, textures, radial/linear gradients, and glows.
  - **Symbol Mode (`--mode symbol`)**: Monochrome white symbol extraction on a transparent canvas (`#00000000`). Automatically strips background frames (`"role": "background"`) and forces foreground shapes to white (`#FFFFFF`) for dynamic tinting in Unity UI.

---

## 2. Asset Analysis Pipeline (`analyze`)

The analysis engine enables the AI to "learn" from existing game art without requiring vision models during generation.

### How It Works

- Scans `F:/GameProject/Realm of Hero/My project/Assets/Addressable` recursively for `.png` files.
- Computes alpha bounding boxes (`bbox`) to strip transparent padding and determine real shape aspect ratios.
- Calculates **Opacity Coverage** (density of the silhouette).
- Quantizes RGB channels into top 5 **Dominant Hex Colors** to establish accurate color palettes.
- Classifies visual style (`flat` vs `smooth`) based on color variance and histogram frequency.
- Indexes assets by filename tokens (e.g., `Skill_Fireball` -> `["skill", "fireball"]`) so the AI can query and combine stylistic elements on demand.

### Running Analysis

```powershell
python Tools/icon_crafter.py analyze --dir "F:/GameProject/Realm of Hero/My project/Assets/Addressable" --output "Tools/Data.json"
```

---

## 3. Drawing & Synthesizing Icons (`draw`)

To render a new icon or combine features from existing PNGs, the AI crafts a JSON specification and executes the draw command.

### CLI Usage (Full Color Icon)

```powershell
python Tools/icon_crafter.py draw --spec "Tools/sample_spec.json" --output "Assets/Addressable/Sprite/Skill/GeneratedIcon.png" --mode icon
```

### CLI Usage (White Symbol Mode)

```powershell
python Tools/icon_crafter.py draw --spec "Tools/sample_spec.json" --output "Assets/Addressable/Sprite/Skill/GeneratedSymbol.png" --mode symbol
```

### Starter Template

Generate a sample JSON specification:

```powershell
python Tools/icon_crafter.py template
```

### Supported Layer Types & Properties

- **Common Properties**: `role` (`background` | `symbol`), `opacity` (0.0 - 1.0), `blur` (int radius for glow/shadow), `blend` (`normal`, `add`, `multiply`, `screen`), `fill` (`#RRGGBBAA`), `outline` (`#RRGGBB`), `width` (outline thickness).
- `rect`: `x`, `y`, `w`, `h`, `radius` (rounded corners).
- `circle` / `ellipse`: `x`, `y`, `r` (or `rx`, `ry`).
- `polygon`: `points` (`[[x1, y1], [x2, y2], ...]`).
- `star`: `x`, `y`, `outer_r`, `inner_r`, `points`, `rotation`.
- `gradient`: `gradient` object (`type: "linear" | "radial"`, `start`, `end`, `angle`, `radius`), `mask_shape` (`circle` | `rect`).
