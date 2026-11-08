# Hit VFX System Walkthrough (Melee & Magic)

Date: 2026-10-18  
System: Combat Visuals & Impact Feedback (`CoreSystem.Visuals`, `CoreSystem.Combat`)

---

## 1. Overview
The **Hit VFX System** triggers stylized visual impact effects whenever an entity receives damage. It differentiates between **Melee (Physical)** impacts and **Magic / Elemental** impacts:
1. **Melee Hit (`HitMeleeVFX`)**: A sharp stylized cut stroke from left to right with a leading diamond fragment and randomized 360-degree rotation.
2. **Magic Hit (`HitMagicVFX`)**: A bidirectional outward explosion featuring a central impact core and two outward particle bursts (left and right) radiating away from the target.

---

## 2. Core Assets & Prefabs

| Asset | Location | Type & Purpose |
|---|---|---|
| `HitMeleeVFX.prefab` | `Assets/Resources/Prefab/VFX/HitMeleeVFX.prefab` | Melee impact cut stroke with random rotation (0-360°) |
| `HitMagicVFX.prefab` | `Assets/Resources/Prefab/VFX/HitMagicVFX.prefab` | Magic impact burst with LeftBurst, RightBurst, and central flash |
| `HitVFXManager.cs` | `Assets/Script/CoreSystem/Visuals/HitVFXManager.cs` | Runtime spawning API and elemental color tinting |
| `HitVFXPrefabGenerator.cs` | `Assets/Script/Editor/HitVFXPrefabGenerator.cs` | Editor generation utility (`Tools > VFX > Generate Hit VFX Prefabs`) |
| `VFX_Hit_Melee.png` | `Assets/Image/VFX/` & `Assets/Resources/Textures/VFX/` | Sharp stylized anime cut mark texture |
| `VFX_Hit_Magic_Spark.png` | `Assets/Image/VFX/` & `Assets/Resources/Textures/VFX/` | Directional magic shard spark texture |
| `VFX_Hit_Magic_Core.png` | `Assets/Image/VFX/` & `Assets/Resources/Textures/VFX/` | 4-point impact star core texture |

---

## 3. How It Works

### Automatic Trigger on Damage
In `DamageHandler.Commit()`, when damage calculation finishes:
```csharp
HitVFXManager.PlayHitVFX(target.transform.position, payload.DamageType, payload.SkillElement);
```
* **If `DamageType.Physical`**: Spawns `HitMeleeVFX` at target position. The particle picks a randomized rotation between $0^\circ$ and $360^\circ$ so consecutive hits cut across at varying dynamic angles.
* **If `DamageType.Magic` or `DamageType.Elemental`**: Spawns `HitMagicVFX` at target position. Two directional emitters shoot sparks outwards to the left ($180^\circ$) and right ($0^\circ$) away from the target center.
* **Elemental Tinting**: The particles automatically tint according to `SkillElement` (Fire = Orange-Red, Water = Cyan, Earth = Amber, Electric = Bright Yellow, Holy = Gold, Ghost = Violet, Neutral = Pure White).

### Manual Spawning via Code
Any custom skill or action can spawn the hit effects directly:
```csharp
// Spawn Melee cut at position:
HitVFXManager.SpawnMeleeHit(targetPosition, ElementType.Neutral);

// Spawn Magic burst at position with element tint:
HitVFXManager.SpawnMagicHit(targetPosition, ElementType.Fire);
```

---

## 4. Expected Results
* Melee hits show instantaneous snappy cut slashes (0.28s) that randomly tilt on each impact.
* Magic hits show sharp energy shards blasting outward horizontally to the left and right away from the target (0.35s) with a bright impact star in the center.
* Both prefabs auto-destroy themselves upon finishing (`StopAction: Destroy`).
