# Sprite Sheet VFX Workbench Walkthrough

Date: 2026-10-18  
System: VFX & Editor Tools (`PaleVertica.Editor`, `CoreSystem.Visuals`)

---

## 1. Overview
The **Sprite Sheet VFX Workbench** is an in-engine editor utility to assemble individual sprite animation frames from a folder into a unified Sprite Sheet grid. It allows real-time global rotation, offset, and scale adjustment across all frames simultaneously to perfectly align cut points, blade angles, and pivots before baking to PNG.

---

## 2. Where to Open
In Unity Editor's top menu bar:
> `Tools` > `VFX` > `Sprite Sheet Workbench`

---

## 3. Workflow & Controls

### Step 1: Select Frames Folder
* Drag-and-drop any Unity project folder containing sequence images into the **Folder Asset** field, or click **Browse...** to select any folder on disk.
* The tool automatically loads and naturally sorts all frames (`slash_1.png`, `slash_2.png` ... `slash_10.png`).
* The **Frame Sequence Filmstrip** displays the ordered frames. Clicking any thumbnail jumps directly to that frame.

### Step 2: Set Grid Layout (Columns & Rows)
* **Columns**: Choose how many horizontal columns (e.g. 4).
* **Auto Calculate Rows**: Automatically determines rows based on the total number of frames (e.g. 16 frames / 4 columns = 4 rows).
* **Export Resolution**: Select 512, 1024, 2048, or 4096 px.

### Step 3: Global Transform (Rotates & Shifts All Frames Together)
* **Rotation (Deg)**: Rotates all frames simultaneously around the center pivot (e.g. -180° to +180°).
* **Offset (X, Y)**: Moves all frames together to shift the impact/cut center onto the crosshair pivot.
* **Scale**: Uniformly zooms in or out.
* **Flip X / Flip Y**: Flips all frames horizontally or vertically.

### Step 4: Previewing
* **Live Animation Player Tab**:
  * Real-time playback loop at custom FPS (1 to 60 FPS).
  * Scrub slider to inspect individual frames.
  * **Crosshair Pivot**: Shows red center lines and a green forward direction arrow to verify attack alignment.
* **Full Sheet Grid Preview Tab**:
  * Displays the entire stitched sprite sheet with all rotations and offsets applied, showing the final PNG result before baking.

### Step 5: Bake & Save
* Set the target save path (defaults to `Assets/Image/VFX/VFX_Pure_Slash_Sheet.png`).
* Toggle **Assign to Slash_VFX.mat** (enabled by default).
* Click **Bake & Save Sprite Sheet**:
  * Bakes each frame using bilinear interpolation.
  * Automatically sets Unity TextureImporter (`Default`, `Alpha is Transparency`, `Read/Write Enabled`, `Uncompressed`).
  * Re-assigns the baked texture to `Slash_VFX.mat` and updates `SlashVFX.prefab`.
