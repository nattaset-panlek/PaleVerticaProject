# Slash VFX & Melee Weapon Attack Visuals Walkthrough

Date: 2026-10-18  
System: Visuals & Combat Presentation (`CoreSystem.Visuals`, `CoreSystem.Entity`, `CoreSystem.Enums`)

---

## 1. Overview
This system hooks the **2D Stylized Anime Slash** VFX into character combat. When any character equips an edged weapon (**Sword**, **Dagger**, **Blade**, **Axe**) or performs a Slash attack, the attack dynamically spawns a 16-frame stylized anime slash arc oriented along the attack vector.

---

## 2. Core Components & Assets

### Code
| Script | Location | Responsibility |
|---|---|---|
| `WeaponType.cs` | `Assets/Script/CoreSystem/Enums/WeaponType.cs` | Defines weapon types and `IsEdgedWeapon()` extension |
| `Equipment.cs` | `Assets/Script/CoreSystem/Data/Items/Model/Equipment.cs` | Tracks `WeaponType` and evaluates `IsEdgedWeapon()` |
| `SlashVisualEffect.cs` | `Assets/Script/CoreSystem/Visuals/SlashVisualEffect.cs` | Slices and plays the 16-frame 2D slash sprite sheet at 30 FPS |
| `Character.cs` | `Assets/Script/CoreSystem/Entity/Character.cs` | Checks equipped weapon on Melee basic attack and spawns slash VFX |
| `SlashVFXPrefabGenerator.cs` | `Assets/Script/Editor/SlashVFXPrefabGenerator.cs` | Editor utility to generate or regenerate VFX prefabs |

### Primary Prefab & Assets
| Asset | Location | Type & Purpose |
|---|---|---|
| `SlashVFX.prefab` | `Assets/Prefab/VFX/` & `Assets/Resources/Prefab/VFX/` | **Primary Unity Particle System Prefab** (Texture Sheet 4x4, 1-shot burst, editable in Inspector) |
| `Slash_VFX.mat` | `Assets/Materials/Slash_VFX.mat` | Unlit Alpha Blended Particle Material |
| `VFX_Pure_Slash_Sheet.png` | `Assets/Image/VFX/` & `Assets/Resources/Textures/VFX/` | 1024x1024 16-frame 4x4 Sprite Sheet |
| `SlashVisualEffect.cs` | `Assets/Script/CoreSystem/Visuals/SlashVisualEffect.cs` | Alternate 2D Sprite Flipbook renderer (legacy/fallback) |

---

## 3. How to Use / Inputs

### A. Automatic Trigger on Attack
Equip any edged weapon (Sword, Dagger, Blade, Axe) into `EquipmentSlots[0]` (main hand) or `EquipmentSlots[7]` (off hand).
When `Character.BasicAttack(target)` or `StrikeBasicAttack` executes in Melee mode:
- Since 2026-10-18 the attack calls `Character.WeaponVfx.Play(...)` (the data-driven weapon VFX system). The "Edged" profile in `Assets/Resources/VFX/WeaponVfxTable.asset` spawns `SlashVFX.prefab` so that its `CuttingPoint` offset (0.815, 0) lands on the target centre, rotated along the attack direction.
- The slash is tinted with the element ramp's Mid colour from the same table (`Fire`, `Water`, `Earth`, `Electric`, `Holy`, `Ghost`; white for Neutral).
- See [Weapon VFX System Walkthrough](./Weapon_VFX_Walkthrough.md) to change, add or override weapon VFX.

### B. Manual Spawning via Code (Skills / Actions)
Any skill or action can spawn the slash effect directly:
```csharp
// Pure White default:
SlashVisualEffect.Create(spawnPosition, rotation, Color.white, scale: 1.2f);

// Tinted for Elemental Skills:
Color fireTint = new Color(1f, 0.55f, 0.1f, 1f);
SlashVisualEffect.Create(spawnPosition, rotation, fireTint, scale: 1.5f);
```

---

## 4. Expected Results
- A clean, 16-frame anime slash with 3 dynamic speed ribbons appears instantly at the swing location.
- The slash accelerates through 180 degrees during the first 3 frames (snappy anime anticipation), then cleanly dissipates.
- Zero blur and zero diffuse glow ensure crisp pixel clarity.
- The GameObject automatically destroys itself after completion (~0.53 seconds).

---

## 5. Common Mistakes
- **Missing Texture in Resources**: `SlashVisualEffect` loads `Textures/VFX/VFX_Pure_Slash_Sheet` via `Resources.Load`. Ensure the texture is present in `Assets/Resources/Textures/VFX/`.
- **Pivot Offset Confusion**: The slash is oriented along the +X direction (0 degrees = facing right). When rotating, use `Quaternion.AngleAxis(angle, Vector3.forward)`.
