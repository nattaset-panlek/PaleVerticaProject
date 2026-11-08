# Weapon VFX System Walkthrough

Basic attacks play a VFX picked by the attacker's weapon type. Everything is data in one asset, so new weapons, extra effects on a specific item, or several variants per weapon (for example a spear that alternates thrust and slash) need no code.

## Where

| What | Path |
| --- | --- |
| Data table (edit this) | `Assets/Resources/VFX/WeaponVfxTable.asset` (loaded from `Resources/VFX/WeaponVfxTable`) |
| VFX prefabs | `Assets/Resources/Prefab/VFX/Weapon/VFX_Fist_Jab_V2`, `VFX_Fist_Hook_V2`, `VFX_Spear_Thrust_V2`; edged weapons use `Assets/Resources/Prefab/VFX/SlashVFX.prefab` |
| Sprite sheets | `Assets/Image/VFX/Weapon/*_Sheet.png` (grayscale, 4x4, 256 px frames, sliced) |
| Material / shader | `Assets/Materials/VFX/VFX_SpriteGradientMap.mat`, shader `Pale Vertica/VFX/Sprite Gradient Map` (`Assets/Materials/Shader/SpriteGradientMap.shader`) |
| Code | `Assets/Script/CoreSystem/Visuals/WeaponVfx/` |
| Source projects | `Tools/VfxPreview/projects/WeaponV2Style/*.ecproj` (see [VFX Preview Tool Walkthrough](../TechnicalArt/VFX_Preview_Tool_Walkthrough.md)) |

Code map:

| Type | Role |
| --- | --- |
| `WeaponVfxTable` (ScriptableObject) | Profiles per weapon type, per-item overrides, element colour ramps. `Load()` caches it. |
| `WeaponVfxProfile` | Weapon types it covers, `ExcludedTags`, `SelectMode` (Cycle / Random), `ComboResetSeconds`, list of variants. |
| `WeaponVfxVariant` | One effect: prefab, anchor (Target / Attacker), `ImpactOffset`, scale, align to aim, mirror when aiming left, `FlipVertical` (play upside down: a top-to-bottom slash becomes bottom-to-top), colour mode. |
| `WeaponVfxItemOverride` | Item ID + `Replace` (use these variants instead) or `Add` (play these on top of the weapon's variant). |
| `WeaponVfxSystem` | Per character (`Character.WeaponVfx`). Resolves weapon, picks a variant, raises `OnBeforePlay` and `OnSpawned`. |
| `WeaponVfxFactory` | Spawns and places the prefab under `[Runtime]`, applies the colour ramp. |
| `SpriteSheetVfxController` | Plays the sliced frames on a SpriteRenderer, sets the gradient-map colours, destroys itself; also an `ISkillVisual` for skills. |

## Current setup

| Profile | Weapon types | Variants | Notes |
| --- | --- | --- | --- |
| Unarmed | None (no weapon equipped) | Jab, Hook (cycle; resets after 1.2 s idle) | `ExcludedTags` = Monster, Boss, so unarmed monsters stay silent |
| Edged | Sword, Dagger, Blade, Axe | SlashDown, SlashUp (cycle) | Both use `SlashVFX.prefab`; SlashUp has `FlipVertical` on, so hits alternate top-to-bottom and bottom-to-top |
| Spear | Spear | Thrust | The thrust tip (0.85 units ahead of the sprite centre) lands on the target |

An equipped weapon whose `WeaponType` is `None` but whose name looks edged (old items) is treated as a Sword. Any other untyped weapon plays nothing.

Colour: sheets are grayscale. The shader maps black -> `Shadow`, gray -> `Mid`, white -> `Highlight` from the attacker's element ramp in the table (Neutral = white).

## Steps

### A. Change which VFX a weapon type plays
1. Select `Assets/Resources/VFX/WeaponVfxTable.asset`.
2. Open `_profiles`, find the profile (or add one) and set `WeaponTypes`.
3. Add variants: drag a prefab into `Prefab`, set `ImpactOffset` (local units, the point that should land on the target), `Scale`.
4. Two or more variants with `SelectMode` = Cycle play in order on consecutive hits (combo); Random picks one each hit. To get a mirrored swing from the same prefab, add the variant twice and tick `FlipVertical` on the second (sprite sheets flip their scale, particle prefabs flip `ParticleSystemRenderer.flip.y`).
5. Enter Play mode, equip the weapon, attack a dummy.

### B. Give one item an extra or different effect
1. In `_itemOverrides`, add an entry with the item's `ItemId` (from Item.json).
2. `Mode` = Add plays the listed variants together with the weapon's normal one (for example a fire trail on a legendary spear). `Mode` = Replace uses only the listed variants.

### C. Add a new VFX prefab
1. Make the effect in `Tools/VfxPreview` (grayscale only) and export with `python render_vfx.py <Name> --unity`.
2. Duplicate `VFX_Spear_Thrust_V2.prefab`, rename it, and on `SpriteSheetVfxController` replace `_frames` with the 16 sliced sprites of the new sheet (select the sheet's sprites `_0` to `_15` in order and drag them onto the array).
3. Set `_visibleLength` to the drawn length in units (used to fit box skills).
4. Reference it from a variant (A) or a skill (D).

### D. Use the spear thrust for a skill (straight, short box)
1. Open the skill's `ActiveSkill` asset.
2. Set `VisualType` = `GroundEruption` and `Eruption Vfx Prefab` = `VFX_Spear_Thrust_V2`.
3. With `BoxTargeting`, the prefab places itself at the box centre, rotates to the aim and stretches so its length matches `BoxSize.y`. The element ramp comes from the skill element.

### E. React from code (buffs, enchants)
Subscribe to `character.WeaponVfx.OnBeforePlay` to add variants (`request.Variants.Add(...)`), change `request.Element`, or set `request.Cancelled = true`. `OnSpawned` gives each spawned instance.

## Expected result

- No weapon: each hit alternates Jab and Hook at the target, mirrored correctly when attacking left.
- Sword / Dagger / Blade / Axe: the slash alternates downward and upward on consecutive hits; after 1.2 s without attacking the next hit starts downward again.
- Spear: a straight thrust ending at the target.
- Ranged attacks only play variants anchored to the Attacker (none by default).
- Colours follow the attacker's element.

## Common mistakes

- **Pink sprite.** The gradient-map shader failed to compile on this platform; check the Console for `SpriteGradientMap` errors. As a stopgap, set the prefab's SpriteRenderer material to `Sprite-Unlit-Default`; the effect then shows white.
- **Effect floats away from the target.** `ImpactOffset` is in local units before scale; for a 256 px frame at 100 PPU the sheet is 2.56 units wide, centre = (0, 0).
- **Frames out of order** after replacing `_frames`: drag the sprites sorted by name `_0` ... `_15`.
- **Monsters punching with fist VFX.** Add their tag to the Unarmed profile's `ExcludedTags`.
- **Nothing plays.** The table must stay at `Resources/VFX/WeaponVfxTable`; a missing table logs one warning and disables weapon VFX.
- **Coloured sheet.** Sheets must be grayscale (see the VFX Preview Tool rule); colour comes from the ramp.
