# Character Asset Analysis (reference PSBs)

Verified 2026-10-17 against `Assets/Image/Character/{Acrxer,Skeleton,zombie}.psb` (raw layers read with psd-tools 1.24), their `.psb.meta` files (PSD Importer 13.0.3, 2D Animation 14.0.4, Unity 6000.4.8f1), the entity prefabs in `Assets/Addressable/Prefab/Entity/`, and the clips in `Assets/Animation/Animation/Character/`.

This is the evidence behind [Character Asset Standard](./Character_Asset_Standard.md). Read the standard for the rules; read this for the reasons.

> **Sections 2.2-2.6 describe the files before 2026-10-17.** On that date the three PSBs were standardized (section 6): layers renamed and reordered, zombie's parts moved out of `Folder 8`, helper groups renamed with a `_` prefix, and real layer IDs added. Pixels and layer bounds did not change.

Coordinates: **canvas px, origin bottom-left** (Unity convention, same as `spritePosition` in the `.meta`). Photoshop/Krita show y from the top: `y_top = 400 - y`.

---

## 1. How a character actually works in the game

The PSB is **only a sprite source**. Nothing uses the prefab that the PSD Importer generates.

```text
Skeletom (Character, Rigidbody2D, CircleCollider2D r=0.25, EntityVisualController ...)
  Model                     Animator (Model.controller), origin = character pivot
    LegR      joint (hip)   scale 0.23428594
      LegR    SpriteRenderer  order 6
    LegL      joint (hip)   scale 0.23428594
      LegL    SpriteRenderer  order 7
    Body      joint (torso) scale 0.23428594
      Body    SpriteRenderer  order 9
      ArmL    joint (shoulder)
        ArmL  SpriteRenderer  order 11
      ArmR    joint (shoulder)
        ArmR  SpriteRenderer  order 8
      Head    joint (neck)  scale 4.2682886 (cancels Body scale)
        Head  SpriteRenderer  order 10, scale 0.23428594
    Shadow    SpriteRenderer  order 1 (rounded.png, black 25%)
```

- **Cutout animation, not skeletal.** Every clip (`Idle`, `walk`, `attack`, `die`, `Awake`) animates `Transform` position/rotation/scale of the joint objects and `SpriteRenderer.m_Color` of the sprite objects. There are no bones, no `SpriteSkin`, no `SpriteResolver`, and no sprite keyframes.
- **The animation contract is the hierarchy path.** Clips bind to `Body`, `Body/Head`, `Body/ArmL`, `Body/ArmR`, `LegL`, `LegR`, `Shadow` and to `Body/Body`, `Body/Head/Head`, `Body/ArmL/ArmL`, `Body/ArmR/ArmR`, `LegL/LegL`, `LegR/LegR`. Any new character prefab must keep these exact names.
- **Sprite pivots are Center.** The prefab places each sprite's **center** at a fixed offset from its joint. Those offsets were measured from **Acrxer** (they match Acrxer's layer centers to 0.01 px), then the sprites were swapped to Skeleton's.
- **All three Addressable prefabs (`Ally`, `Dummy`, `Skeletom`) are the same rig with Skeleton.psb sprites** (identical transforms, sorting and sprite references). Acrxer is used only by legacy `Assets/Prefab/Character/Enemy_SK_BS.prefab`; zombie only by legacy `Enemy_Zb.prefab`.
- **Costumes swap sprites by name.** `CharacterVisualController.ApplyCostumeSprite` maps a multi-sprite costume asset (a PSB) to renderers by lower-case substring: `head`, `body`, `arml`, `armr`, `legl`, `legr`. A head layer called `Layer 5` would not be applied.
- World scale: 1 canvas px = 0.0023428594 world units. A 400 px canvas is 0.937 units; a full character (feet to head top, about 370 px) is about 0.87 units tall.

## 2. Per-file results

### 2.1 Shared by all three

| Item | Value |
|---|---|
| File | PSB (version 2), RGB, 8-bit, **400 x 400 px**, no layer masks, no layer effects, Normal blend on every part |
| Layer IDs | **None in the file** (no `lyid` block). The importer finds every ID = 0, logs "LayerId is not unique. Mapping will be done by Layer's name.", and falls back to `name.GetHashCode()`. Mapping is effectively **by name** |
| Importer | `PSDImporter` (`ScriptedImporter`), Texture Type Sprite, Sprite Mode Multiple |
| PPU | **100** |
| Sprite pivot | **Center (0.5, 0.5)** on every layer, no custom pivots |
| Document | `documentAlignment: 7` = **Bottom Center**, `documentPivot` (0,0) |
| Mosaic | `mosaicLayers: 1`, **padding 4 px**, rects start at (4,4), `spriteSizeExpand 0`, max size 2048. Rects fit in about 500 x 340 |
| Character rig | `characterMode: 1` (Use as Rig) but `characterData.bones: []`, `parts: []`, every `spriteBone`, `vertices`, `indices` empty: **no rig, no weights** |
| Sprite Library | `spriteCategoryList.categories: []`: **no categories or labels** |
| Layer import | `importHiddenLayers 0`, `layerMappingOption 2` (Use Layer ID), `keepDupilcateSpriteName 1`, `generateGOHierarchy 0` (layer grouping off), `paperDollMode 0`, `generatePhysicsShape 0` |
| Texture | Filter **Point**, mipmaps **on**, wrap Clamp, sRGB on, alpha is transparency, not readable |
| Sprite mesh | `spriteMeshType 0` = **Full Rect** (changed from Tight in the current uncommitted edit) |
| Compression | Default platform **Uncompressed** (changed from High Quality in the current uncommitted edit); Standalone/Server not overridden |
| Hidden blocking group | `Folder 1` (hidden): `Layer 1` head circle, `Layer 2` torso (opacity 130), `Layer 3` legs, `Layer 4` arms. A solid black silhouette: the original blocking template |

### 2.2 Acrxer.psb (archer, player-like)

Layer panel, top to bottom (Unity `psdLayers` order):

| # | Layer | Type | Box x,y (w x h) | Imported |
|---|---|---|---|---|
| 1 | `Head` | layer | 62,144 (290 x 249) | yes |
| 2 | `Body` | layer | 149,59 (115 x 136) | yes |
| 3 | `ArmL` | layer | 232,92 (85 x 72) | yes |
| 4 | `ArmR` | layer | 97,92 (85 x 72) | yes |
| 5 | `LegL` | layer | 213,22 (45 x 70) | yes |
| 6 | `LegR` | layer | 153,22 (45 x 70) | yes |
| 7 | `Folder 1` | group, hidden | blocking silhouette (4 layers) | no |

- Arms and legs are exact mirrors. All parts at the root, no groups.
- This file **is the geometric reference**: the shared prefab offsets come from it.

### 2.3 Skeleton.psb (enemy, the sprites every current prefab uses)

| # | Layer | Type | Box x,y (w x h) | Imported |
|---|---|---|---|---|
| 1 | **`Layer 5`** (the head) | layer | 74,160 (254 x 216) | yes |
| 2 | `ArmL` | layer | 227,93 (80 x 78) | yes |
| 3 | `ArmR` | layer | 108,82 (72 x 81) | yes |
| 4 | `Body` | layer | 162,72 (96 x 134) | yes |
| 5 | `LegL` | layer | 216,9 (38 x 97) | yes |
| 6 | `LegR` | layer | 154,10 (41 x 95) | yes |
| 7 | `Folder 1` | group, hidden | blocking silhouette | no |
| 8 | `Folder 2` | group, hidden | Acrxer's six parts merged and renamed `Folder 3` (head), `Folder 4` (body), `Folder 5`/`Folder 5 Copy` (arms), `Folder 6`/`Folder 6 Copy` (legs) | no |
| 9 | `Layer 6` | layer, hidden | full 400 x 400 fill | no |

- Head is misnamed. Arms are not mirrors. Legs are longer (feet at y 9-10 instead of 22).

### 2.4 zombie.psb (enemy)

| # | Layer | Type | Box x,y (w x h) | Imported |
|---|---|---|---|---|
| 1 | `Folder 8` | **group, visible** | contains the six live parts | no (group) |
| 1.1 | `Head` | layer | 74,158 (255 x 222) | yes |
| 1.2 | `ArmL` | layer | 223,87 (83 x 88) | yes |
| 1.3 | `ArmR` | layer | 107,87 (83 x 88) | yes |
| 1.4 | `Body` | layer | 156,69 (107 x 129) | yes |
| 1.5 | `LegL` | layer | 208,20 (53 x 89) | yes |
| 1.6 | `LegR` | layer | 150,20 (53 x 89) | yes |
| 2 | `Folder 1` | group, hidden | blocking silhouette | no |
| 3 | `Folder 7` | group, hidden | a second, older `Head`/`ArmL`/`ArmR`/`Body`/`LegL`/`LegR` set (Skeleton-shaped) | no |
| 4 | `Folder 2` | group, hidden | Acrxer's parts (as in Skeleton) | no |
| 5 | `Layer 6` | layer, hidden | full 400 x 400 fill | no |

- Duplicate part names exist (hidden `Folder 7`). Harmless only because hidden layers are not imported.

### 2.5 Part placement compared with the shared prefab

The prefab expects each sprite **center** at Acrxer's center. Offset of each file's part center from that anchor (px):

| Part | Anchor (center) | Acrxer | Skeleton | zombie |
|---|---|---|---|---|
| Head | 206.9, 268.5 | 0, 0 | -6, -0.5 (`Layer 5`) | -5.4, 0.5 |
| Body | 206.5, 127.0 | 0, 0 | +3.5, +12 | +3, +6.5 |
| ArmL | 274.5, 128.0 | 0, 0 | -7.5, +4 | -10, +3 |
| ArmR | 139.5, 128.0 | 0, 0 | +4.5, -5.5 | +9, +3 |
| LegL | 235.5, 57.0 | 0, 0 | -0.5, +0.5 | -1, +7.5 |
| LegR | 175.4, 57.0 | 0, 0 | -0.9, +0.5 | +1, +7.5 |

Up to 12 px off (about 0.03 world units) in the current Skeleton prefabs. It is barely visible at game scale, but it grows with any character whose limbs differ in length, because a center pivot moves the whole part when the part gets longer.

### 2.6 Draw order

| Source | Front to back |
|---|---|
| Runtime (prefab sorting order) | ArmL 11, Head 10, Body 9, ArmR 8, LegL 7, LegR 6, Shadow 1 |
| Acrxer PSB | Head, Body, ArmL, ArmR, LegL, LegR |
| Skeleton PSB | Head, ArmL, ArmR, Body, LegL, LegR |
| zombie PSB | Head, ArmL, ArmR, Body, LegL, LegR |

No PSB matches the runtime order, so the importer preview never matches the game. The runtime order puts the right-hand side of the image (`ArmL`, `LegL`) nearer the camera, which matches art turned slightly toward the image's left. The art faces left as drawn; `CharacterPresenter` mirrors `Model` (`scale.x = -1`) when the character moves right.

## 3. Inconsistencies that break a shared setup

| # | Issue | Effect | Status |
|---|---|---|---|
| 1 | Skeleton head is named `Layer 5` | Costume name matching (`head`) misses it; any tool keyed on part names fails; the sprite is not named `Head` in the prefab | Fixed 2026-10-17 (renamed `Head`) |
| 2 | zombie parts sit inside a visible group `Folder 8`; the others are at the root | Harmless today (layer grouping off), but turning on layer grouping would change the generated hierarchy for zombie only | Fixed 2026-10-17 (parts at root) |
| 3 | Three different layer orders, none equal to runtime sorting | Importer preview differs from the game; artists judge overlaps wrongly | Fixed 2026-10-17 (runtime order) |
| 4 | No real layer IDs plus mapping set to Use Layer ID | Import warnings on every reimport; renaming a layer silently creates a new sprite and breaks prefab references | Fixed 2026-10-17 (IDs added) |
| 5 | Leftover hidden groups (`Folder 2`, `Folder 7`, `Layer 6`) and duplicate names in zombie | Noise; a future "import hidden layers" or name mapping would collide | Fixed 2026-10-17 (renamed `_Guide`, `_Ref_*`) |
| 6 | Center pivots plus fixed prefab offsets | Parts with a different size than Acrxer shift away from their joints (section 2.5) | Open: Apply Standard sets anchor pivots |
| 7 | Rig mode on, but no bones, weights or Sprite Library | Settings suggest skeletal animation that does not exist; skinning editor work would be ignored by the cutout prefabs | By design (standard section 6.1) |
| 8 | Arms mirrored in Acrxer and zombie, not in Skeleton | Fine for art, but rules for joint overlap must be per side | No action |

## 4. Other findings (outside the asset standard)

- **Attack animation never fires**: `CharacterPresenter.AttackTriggerParam` is `"Attack"` on all three prefabs, but `Model.controller` names the trigger `IsAttack`.
- **No `SortingGroup` on characters**: all parts sit on sorting layer Default with fixed orders 1-11, so two overlapping characters interleave (one character's `ArmL` draws over another's `Head`). Fixed 2026-10-17: `SortingGroup` on `Model` (order 6) and a Sort At Root group on `Model/Shadow` (order 1) in `Ally`, `Dummy` and `Skeletom`; `Renderer2D.asset` Transparency Sort Mode set to Custom Axis (0, 1, 0) so characters sort by Y; the prefab tool sets both groups. The unpacked `Player` rig in `SandBox.unity` still needs the same two components (standard section 7).
- `CharacterVisualController` substring matching treats any name containing `arm` and `l` as `ArmL` (for example `ArmR_Leather`), and `Docs/02-Technical/Items/Costume_System_Walkthrough.md` still says sprites must live in `Resources/` while the controller loads through Addressables.
- Filter mode Point with mipmaps on, drawn at 23% scale, can shimmer on smooth hand-drawn art. Kept as is in the standard because it is the current project choice.

## 5. Atlas distortion

The PSD Importer (Automatic Reslice off) keeps an existing sprite rect's **size** on reimport and only moves it (`PSDImporter.ImportFromLayers`: `r.position = ...`, width and height unchanged unless Sprite Size Expand changed). When a layer was redrawn larger or smaller, its sprite kept the old size:

| PSB | Head layer bounds | Head sprite rect | Effect |
|---|---|---|---|
| Skeleton | 254 x 216 at (74,160) | 248 x 209 | right 6 px and top 7 px cut off |
| zombie | 255 x 222 at (74,158) | 259 x 218 | top 4 px cut off, 4 px of padding on the right |
| Acrxer | 290 x 249 at (62,144) | 291 x 251 | 1-2 px of padding, center pivot off by about 1 px |

Every other part's rect matches its layer exactly. A second, smaller risk: with mipmaps on, 4 px of mosaic padding stops protecting neighbours at mip level 3, so edge pixels can bleed between parts when the camera zooms out. Fix for both: Automatic Reslice on and 8 px padding (**Rebuild Atlas** in the Character Asset Tool). Sprite IDs are kept, so prefab references survive.

## 6. Standardization of the three PSBs (2026-10-17)

Done with psd-tools 1.24 at the user's request. For each file:

- Root order, top to bottom: `ArmL`, `Head`, `Body`, `ArmR`, `LegL`, `LegR` (the prefab's sorting order).
- Skeleton `Layer 5` renamed `Head`. zombie's six parts moved out of the visible group `Folder 8` (group removed).
- Hidden helpers renamed: `Folder 1` to `_Guide` (children `_Guide_Head`, `_Guide_Body`, `_Guide_Arms`, `_Guide_Legs`), `Folder 2` to `_Ref_Acrxer` (children `_Ref_Acrxer_<Part>`), zombie `Folder 7` to `_Ref_OldParts` (children `_Ref_Old_<Part>`), `Layer 6` (white fill) to `_Ref_Background`.
- Every layer got a real layer ID (`lyid`) equal to the ID the importer had already stored in its `.meta` (for example Skeleton `Head` keeps ID 0, the old `Layer 5`). With unique IDs the importer maps old to new by ID, so every sprite ID, and therefore every prefab reference, is kept even though names and order changed.
- Verified by reading the written files back: every layer's pixels and bounds are identical to the original. The composite preview changed only where parts overlap (new draw order).
- Not edited: the `.meta` files (Unity rewrites names and order on reimport) and the prefabs.
- Backups of the original PSBs and `.meta` files (including the uncommitted meta edits of that day): `%APPDATA%/Claude/side-session-notes/local_fec2e6ac-00b2-4af4-8ad5-fb6cfb6b264e/psb_backup_2026-10-17/`. The original PSBs are also in git (`git show HEAD:Assets/Image/Character/Skeleton.psb`).
