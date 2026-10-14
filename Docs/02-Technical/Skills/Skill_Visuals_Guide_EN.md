# Skill Visuals and Handover Guide for VFX Artists (English Version)

This document outlines how the Realm of Hero skill visual system is structured and how visual assets (prefabs, particles, lines, sprites) must be configured to work seamlessly with the game engine.

---

## 1. System Overview

The Realm of Hero skill system is decoupled. Visual effects do not calculate damage or logic. They are spawned on-demand by the Skill Caster and are governed by the `VisualManager` and the `SkillVisualEffect` helper component.

To make a visual effect work with the system, it must:

* Be stored as a **GameObject Prefab**.
* Have the **`SkillVisualEffect`** component attached to its root (if automatic scaling or lifetime cleanup is desired).
* Fit into one of the **5 Visual Categories** defined in the system.

---

## 2. The 5 Visual Categories (VisualCategory)

When setting up an `ActiveSkill` ScriptableObject, you select a `VisualType`. This enum maps directly to a specific spawn and movement behavior in the engine.

### A. Instant Hit (InstantHit)

* **What it is**: Instantaneous visual feedback spawned on top of the target.
* **Usage**: Melee strikes, targeted curses, single-target spells.
* **Engine Behavior**: Spawns the `ImpactVfxPrefab` at the exact transform position of every targeted entity.
* **Art Guidelines**:
  * The prefab's local origin (0,0,0) should represent the target's feet or center depending on the effect.
  * Must handle its own cleanup or use `SkillVisualEffect` to auto-destruct.

### B. Projectile (Projectile)

* **What it is**: Linear moving skill effects.
* **Usage**: Arrows, fireballs, thrown axes.
* **Engine Behavior**:
  * Spawns the `ProjectilePrefab` at the caster's position (`OriginPosition`).
  * Automatically rotates the prefab toward the target. The math uses a -90 degree offset.
  * If `ProjectileCount` is greater than 1, it automatically calculates a fan-shaped spread up to 180 degrees.
* **Critical Art Guidelines**:
  * **Sprite Orientation**: The projectile sprite/particle direction MUST face **Up (Y+)** in the prefab. Because of the internal rotation calculations, any projectile art facing Right (X+) or Left (X-) will appear rotated incorrectly when flying.
  * **Movement**: The projectile prefab must have its own movement script (e.g. traveling forward along its local Y+ axis) or physics controller.

### C. Ray Beam (RayBeam)

* **What it is**: A continuous line drawn between the caster and target.
* **Usage**: Lightning beams, laser beams, channeling draining spells.
* **Engine Behavior**:
  * Spawns the `BeamVfxPrefab` at the caster's position (`OriginPosition`).
  * Searches the prefab for a **`LineRenderer`** component.
  * Automatically sets index 0 of the LineRenderer to the caster's origin and index 1 to the target's position.
* **Critical Art Guidelines**:
  * The prefab MUST contain a `LineRenderer` component on the root or in a child.
  * The LineRenderer's materials should utilize scrolling UV textures or fading shaders to animate the beam.

### D. Ground Eruption (GroundEruption)

* **What it is**: Ground-based area-of-effect spells.
* **Usage**: Meteor impacts, fire pillars, earthquakes, ice spikes.
* **Engine Behavior**:
  * Spawns the `EruptionVfxPrefab` at the target position (or origin if target position is not set).
  * If the skill has an Area-of-Effect (AOE) radius, the system scales the prefab transform to matches the diameter: `localScale = (AOERadius * 2, AOERadius * 2, 1)`.
* **Critical Art Guidelines**:
  * The prefab's pivot point MUST be at the bottom center (ground level) so it doesn't sink into the ground when scaled.
  * All visual components (sprites, particle systems) must support parent scaling.

### E. Aura Buff (AuraBuff)

* **What it is**: Continuous effects parented to characters.
* **Usage**: Shields, healing regenerations, passive stat increases.
* **Engine Behavior**:
  * Spawns the `AuraVfxPrefab` at the target's position.
  * Parent-constrains the prefab to the target's Transform so it moves with the target.
* **Critical Art Guidelines**:
  * The pivot point must be at (0,0,0) (character feet or chest).
  * Design the effect to loop smoothly since buff effects typically remain active for long durations.

---

## 3. The SkillVisualEffect Component

To make life easier, attach the `SkillVisualEffect` script to your prefab root. It automates common tasks:

* **Automatic Scaling (`autoScaleToAOE`)**:
  * When checked, the prefab's localScale automatically scales to match the skill's AOE diameter on spawn.
* **Automatic Destruction (`destroyAfterDuration`)**:
  * When checked, the system calculates when to destroy the prefab.
  * Under **`linkedParticles`**, you can drag and drop your Particle Systems. The system automatically inspects these systems, finds the longest duration + lifetime, and Schedules destruction so that no particles are cut off abruptly.
* **Casting Progression Visual (`SpriteRendererFillSkill`)**:
  * If you assign a SpriteRenderer to this slot, it acts as a visual cast progress bar.
  * The sprite will start at a localScale of zero and smoothly scale up to `Vector3.one` matching the duration of the skill's cast time.
  * Once casting completes, it disappears, leaving the main effect to play.

---

## 4. Skill Targeting Indicators (Indicators)

Before a skill is cast, player-controlled skills display a targeting indicator. These are handled by the `SkillIndicatorManager`.

### Aiming Phase vs. Casting Phase Indicators

The indicator system behaves differently depending on the phase of skill casting:

1. **Aiming Phase (Shortcut Pressed)**:
   * The system always displays the **default system indicators** (Circle, Line, or Cone) to help the player target the area/direction. Any custom `SkillIndicatorPrefab` is ignored during this phase to keep targeting clean and consistent.
2. **Casting Phase (Casting Initiated)**:
   * The aiming indicator is hidden (or kept for direction-based skills).
   * If the skill has a custom **`SkillIndicatorPrefab`** assigned, the system instantiates it at the locked target location on the ground.
   * During the casting duration, the system automatically feeds the cast progress (0.0 to 1.0) into the custom indicator's `SetProgress` method (or `ISkillVisual` script).
   * Once the cast completes or is interrupted, the custom indicator is automatically destroyed.

### Default Indicators

Depending on the skill's Targeting Logic, the system displays one of three built-in templates:

1. **CircleIndicator**: Used for `TargetLocationTargeting` or `SphereAOETargeting`. Displays a range circle centered on the caster, and an AOE disc centered at the target location.
2. **LineIndicator**: Used for `BoxTargeting` or `LinePierceTargeting`. Displays a box indicating width and length. Uses a shader property `_FillAmount` for charging visual feedback.
3. **ConeIndicator**: Used for `ConeTargeting`. Displays a cone matching the radius and cone angle. Uses the shader property `_ConeAngle` and scales up from the origin during casting.

### Shaders and Materials for Indicators

If you customize indicators or write shaders for them, ensure they support:

* **`_FillAmount`**: Float value from 0 to 1 representing cast/charge progress.
* **`_ConeAngle`**: Float value representing the angle of the cone segment.
* **`_HeadTex`**: Texture representing the front arrow head/indicator line cap.

---

## 5. Handover Checklist for Visual-sa

When creating new visuals, follow this checklist to ensure they work without programmer intervention:

1. [ ] **Pivot Point**: Confirm the prefab pivot is at ground center (0,0,0).
2. [ ] **Projectile Orientation**: Double-check that projectile sprites and particle velocities point **directly Up (Y+)** in local space.
3. [ ] **LineRenderer**: Ensure RayBeam effects have a `LineRenderer` component on the root or children.
4. [ ] **No Logic Components**: Do not add Colliders, Rigidbodies, or custom damage-dealing scripts to visuals. Keep visual prefabs clean of gameplay logic.
5. [ ] **Auto-Cleanup**: Add `SkillVisualEffect` to the root, check `destroyAfterDuration`, and assign all particle systems to `linkedParticles` so they clean up automatically.
