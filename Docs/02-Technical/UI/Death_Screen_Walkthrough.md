# Death Screen & Fog Aura Walkthrough

The death screen shown when the player's HP reaches 0. It is the Figma design **Death E / Ink Epitaph** (file Whiper-of-pale, node `1786:29432`, page `WockSpace` > `20 · Features` > section "Death Screen"): a MiniHeader window plus a full-screen **Fog Aura** drawn by a moving shader. Added 2026-10-24.

## 1. Where things live

| Piece | File | Role |
|---|---|---|
| `PlayerDeathScreen` | `Assets/Script/CoreSystem/Player/PlayerDeathScreen.cs` | Modal "Death Screen", MiniHeader style "FALLEN": HERE LIES, name + level, YOU ARE DEAD, Fell at / Slain by rows, Town + Revive |
| `PlayerDeathInfo` | `CoreSystem/Player/PlayerDeathInfo.cs` | Name, level, location, killer; `Sample` for renders |
| `PlayerCore` | `CoreSystem/Player/PlayerCore.cs` | Remembers the last attacker, builds `PlayerDeathInfo` (location = active scene name) and calls `Show(info, Revive)` |
| `FogAuraView` | `CoreSystem/UI/Effects/FogAuraView.cs` | `[Runtime] Fog Aura`: full-screen RawImage placed right behind a window. `LoadMaterial(path)` reads a material file; `Create` copies it, starts its clock at 0 and fades it in / out |
| Material files | `Assets/Resources/UI/FogAura/FogAura_Ink.mat` (also `_Crimson`, `_Pale`) | **Edit the look here**: colours, stops, speed, shape, dim. The death screen reads `PlayerDeathScreen.FogMaterialPath` |
| `FogAuraPalette` | `CoreSystem/UI/Effects/FogAuraPalette.cs` | Default colours for new material files (`Ink`, `Crimson`, `Pale`); `ApplyTo(material)` |
| `FogAuraMaterialBuilder` | `Assets/Script/Editor/UI/FogAuraMaterialBuilder.cs` | **Pale Vertica > UI > Create Fog Aura Materials**: creates missing material files, never overwrites |
| Shader | `Assets/Resources/Shaders/UI/UIFogAura.shader` (`Custom/UI/FogAura`) | Procedural aura: noise, tone steps, rim, colour from the ramp |
| Sizes and colours | `UIMiniHeaderStyleRegistry.DeathWindowWidth`, `DeathTitle`, `DeathTitleSpacing` | Static parameters |

## 2. How the aura is drawn

The shader makes a grayscale value and an alpha, then maps the gray to colour with a five-stop gradient (`_RampColor0..4`, `_RampStops`). It needs no texture.

1. **Edge mask**: distance to the nearest screen edge, reach `_Reach` (0.26 of the height), power `_EdgePower`, more at the corners.
2. **Tongues**: noise is sampled along the angle from the screen centre, so the shapes point inward. A low-frequency noise warps them (`_Warp`). Time moves the sample radius, so the tongues flow inward like liquid (`_Speed`). Time counts from `_StartTime`, which `FogAuraView` sets when the aura appears, and the noise cells wrap every 289, so the look stays the same however long the game runs.
3. **Stylized steps**: three hard bands (`_Thresholds`, `_BandAlpha`, `_BandGray`), a bright rim on the first band, a weaker rim on the second and a soft glow around the rim (`_RimWidth`).
4. **Colour**: the gradient map. Ink is black -> #070707 -> #141414 -> #8E1418 -> #FF5A4E at stops 0 / 0.15 / 0.4 / 0.65 / 1.
5. **Screen dim**: `_BaseDim` darkens the whole screen under the aura (0.45). `_Fade` is animated by `FogAuraView`: in over 1.2 s, out over 0.4 s.

## 3. Use the aura elsewhere

```csharp
var fog = FogAuraView.Create(modal.WindowRect, FogAuraView.LoadMaterial("UI/FogAura/FogAura_Crimson"), fadeIn: true);
fog.KeepBehind(modal.WindowRect);
fog.Dismiss();
```

- The window must sit in a full-screen parent (the UIManager modal container does).
- **Change the look**: select `Assets/Resources/UI/FogAura/FogAura_Ink.mat` and edit it in the Inspector. The next aura that appears uses the new values (each aura copies the file, so play-mode edits to the file show on the next death; to see them live, edit the copy on `[Runtime] Fog Aura`).
- **New colour scheme**: duplicate a material file, change its five ramp colours, and point `FogMaterialPath` (or your own call) at it. Colour only through the ramp.
- A missing file falls back to the Ink defaults with a warning.

## 4. Check

1. Compile: `python -I Tools/FigmaToUnity/compile_check.py`.
2. Material files: **Pale Vertica > UI > Create Fog Aura Materials** (or `FogAuraMaterialBuilder.CreateMissingBatch` with Unity closed) once.
3. Render: `deathscreen` in `Logs/UIDebug/` (full frame with the aura), 0 layout issues. Compare it with the Figma export of `1786:29432`.
4. EditMode tests: `DeathScreenTests` (palette stops, material colours, Ink file loads, death info, shader found, the aura layer sits behind its window with its own copy and start time) and `UILayoutRegressionTests`.
5. Play mode: die, revive, and die again a few times. The aura looks the same each time.

## 5. Known gaps

- **Town** is greyed out until a return-to-town system exists. Pass `onReturnToTown` to `Show` to enable it.
- The close button is hidden and Esc does nothing: the player has to choose.
- Location is the scene name until maps have display names.
- Snapshot renders are taken at shader time 0, so they show one frame of the motion.

## 6. Common mistakes

| Mistake | Result | Fix |
|---|---|---|
| Putting the aura inside the window | Layout error "extends outside its parent" and the aura is clipped | Use `FogAuraView.Create(window, ...)`, which adds it as a sibling |
| Colouring the aura in the shader code | Breaks the grayscale + gradient-map rule | Add a `FogAuraPalette` |
| Moving the shader out of `Resources` | `Shader.Find` fails in builds and the aura is missing | Keep it in `Resources/Shaders/UI/` |
| Moving or renaming a material file | Falls back to the Ink defaults | Keep it under `Resources/` and update `FogMaterialPath` |
| Using `_Time.y` directly in a new shader of this kind | Noise breaks up after the game runs for a while | Subtract a start time and wrap the noise cells, as `UIFogAura.shader` does |
