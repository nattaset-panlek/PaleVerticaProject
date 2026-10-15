# Realm of Hero: Technical Art Setup Guide & TODO

This document details exactly **where the Shader and Master Material are created, where they live, and how they connect to `EntityVisualController`**.

---

## 1. Setup Architecture: Where Shader, Material & Script Connect

```text
[1. Shader File]
Assets/Materials/Shader/Character2DLitMaster.shader
       │
       ▼ (Right click -> Create -> Material)
[2. Master Material Asset]
Assets/Materials/Character2DLitMaster.mat
       │
       ▼ (Assign once into the MasterMaterial field)
[3. Entity Component: EntityVisualController.cs]
       ├── ModelRoot        <-- Assign the character's Model parent transform
       ├── ShadowRenderer   <-- Assign the ground shadow sprite (auto-excluded from flash)
       └── MasterMaterial   <-- Assign Character2DLitMaster.mat here!
```

---

## 2. Automatic Material Injection (Zero Manual SpriteRenderer Work)

When the game starts (`Awake`):
1. `EntityVisualController` automatically finds all `SpriteRenderer` components under `ModelRoot` (Head, Body, Arms, Legs, Weapons).
2. If `MasterMaterial` is assigned, `EntityVisualController` automatically assigns it to all child `SpriteRenderer`s (`renderer.sharedMaterial = _masterMaterial`).
3. **Result**: You never have to manually drag materials into individual sprite renderers inside prefabs!

---

## 3. Runtime Communication via MaterialPropertyBlock

When gameplay events occur, `EntityVisualController` communicates with the Shader via `MaterialPropertyBlock`:

```text
[Damage Event]
       │
       ▼
EntityVisualController.PlayHitFlash()
       │
       ▼
Sets _FlashAmount = 1.0 and _FlashColor = White in MaterialPropertyBlock
       │
       ▼
Master Shader flashes the sprite white for 0.1s (shadow is ignored)
```

```text
[Target Selection Event]
       │
       ▼
EntityVisualController.SetOutline(true, Color.red)
       │
       ▼
Sets _OutlineWidth = 1.0 and _OutlineColor = Red in MaterialPropertyBlock
       │
       ▼
Master Shader draws a glowing outline around all body parts
```

---

## 4. Implementation Steps (Sprint Backlog)

- [ ] **Step 1: Create Master 2D Character Shader**
  - Path: `Assets/Materials/Shader/Character2DLitMaster.shader`
  - Supports: URP 2D Lit, `_FlashAmount`, `_FlashColor`, `_OutlineColor`, `_OutlineWidth`, `_StatusColor`, `_DissolveAmount`.

- [ ] **Step 2: Create Master Material Asset**
  - Path: `Assets/Materials/Character2DLitMaster.mat`
  - Uses `Character2DLitMaster` shader.

- [ ] **Step 3: Create `EntityVisualController.cs`**
  - Path: `Assets/Script/CoreSystem/Visuals/EntityVisualController.cs`
  - Fields: `ModelRoot`, `ShadowRenderer`, `MasterMaterial`.
  - Auto-injects material to child renderers.
  - Implements `PlayHitFlash()`, `SetFacing()`, `SetOutline()`.

- [ ] **Step 4: Create Combat Juice Director (`CombatJuiceDirector.cs`)**
  - Path: `Assets/Script/CoreSystem/Visuals/CombatJuiceDirector.cs`
  - Implements Hit-Stop frame freeze and camera shake.
