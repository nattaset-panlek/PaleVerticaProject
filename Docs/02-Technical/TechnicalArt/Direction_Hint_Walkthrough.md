# Direction Hint Walkthrough

The Direction Hint is a ring under a hostile character whose arrow points the way that character faces. It shows only while the player has a Unique Passive that uses facing (today: Marked Throat, "attacks from behind always crit"), so the player can see where "behind" is.

Design: Figma file `zZ5QnVfPMSFuiMFjpIGbOC`, node `1850:7370` (`Direction_Hint`). The arrow is the front. The `Center` element (`1850:7367`) is the pivot.

## How it works

| Part | Where |
|---|---|
| Shape | `Assets/Resources/Sprite/World/DirectionHint_SDF.png`, a signed distance field made from the Figma export |
| Shader | `Assets/Resources/Shaders/World/DirectionHint.shader` (`Custom/World/DirectionHint`) |
| Material and settings | `Assets/Resources/Materials/DirectionHint.mat`, `Assets/Resources/Settings/DirectionHintSettings.asset` (made by the menu below) |
| Code | `Assets/Script/CoreSystem/Visuals/DirectionHint/` (`DirectionHintView`, `DirectionHintFormula`, `DirectionHintEvents`, `DirectionHintSettings`) |
| Editor | `Assets/Script/Editor/Visuals/DirectionHintAssetBuilder.cs` (import settings and the menu) |
| SDF tool | `Tools/SdfFromPng/make_sdf.js`, source export in `Tools/SdfFromPng/Source/` |

- **Why an SDF:** the ring is drawn by the shader from a distance field (`smoothstep` with `fwidth`), so it stays sharp at any size. A plain PNG would blur when a big monster scales it up.
- **Pivot:** the sprite pivot is the Figma `Center` element: (169.5, 183) in the 338 x 344 group, which is (0.501, 0.468) in Unity. It is in `DirectionHintFormula.Pivot`.
- **Size:** diameter = the longer side of the character's body collider (the first non-trigger `Collider2D`) x **Size Multiplier** (default 1.6), never below **Minimum Diameter** (0.6).
- **Position:** the ring sits on the body collider centre, moved by **Offset** (world units) plus **Offset By Size** (a fraction of the collider size, so tall and short characters move alike). Example: Offset By Size Y = -0.5 puts the ring at the feet.
- **Direction:** every frame the ring turns so its arrow points along `Character.FacingDirection`. Monsters face where they walk and their target; a character with no AI keeps its default facing (right).
- **When it shows:** a passive class overrides `UsesFacing => true`. `UniquePassiveSystem.OnChanged` tells `DirectionHintEvents`, which tells every character to show or hide its hint: shown on living characters hostile to the player, hidden on the player, the dead and allies.
- **Drawn under the body:** it copies the sorting layer of the character's largest sprite and uses its order + **Sorting Order Offset** (default -1).

## How "behind" makes a crit

Marked Throat does not multiply damage itself. It overrides `ForcesCritical`: when the target faces away from the attacker (dot of the target facing and the attacker-to-target direction > 0.3, `MarkedThroatUniquePassive.IsBehind`), `DamageHandler.Offense` replaces the crit roll with `DamageFormula.ForcedCriticalRoll` (-1). The normal crit check then always passes, even with 0 Critical Rate or high Crit Resist, and the hit uses the normal crit damage (1.5 + Critical Damage), crit number and all. Tests: `MarkedThroatCriticalTests`.

## Steps

1. **First time on a machine:** open Unity and run **Pale Vertica > Visuals > Build Direction Hint Assets**. It reimports the SDF with the right settings (not sRGB, uncompressed, trilinear, mipmaps), creates the material and the settings asset, and links them. Without it the game still works with a material made from the shader, but you cannot tune it.
2. **Tune the look:** select `Assets/Resources/Materials/DirectionHint.mat`:
   - **Color** (HDR) and its alpha, **Edge Softness** (higher = softer edge), **Thicken** (makes every line thicker or thinner), **Glow Color** / **Glow Width** for a soft halo.
   - For a new visual, write another shader that reads the same SDF texture (red channel, 0.5 = edge) and assign it in the settings asset's **Material** field.
3. **Tune size, position and order:** select `Assets/Resources/Settings/DirectionHintSettings.asset` and change **Size Multiplier**, **Minimum Diameter**, **Offset**, **Offset By Size**, **Color** or **Sorting Order Offset**. Changes show live in Play mode.
4. **A new passive that cares about facing:** add `public override bool UsesFacing => true;` to its class. Nothing else is needed.
5. **Check it:** run the scenario `core/direction_hint` (Scenario Checklist). It checks the hint is off without the passive, on with Marked Throat, and off again after removing it, and asks you to approve a screenshot.

## Change the shape

1. In Figma, export the shape node (not the group) as PNG at scale 4 with Talk to Figma, and save it to `Tools/SdfFromPng/Source/`.
2. From the project root:
   ```bash
   node Tools/SdfFromPng/make_sdf.js Tools/SdfFromPng/Source/Direction_Hint_x4.png Assets/Resources/Sprite/World/DirectionHint_SDF.png 512 24
   ```
   `512` is the output width, `24` is how far (in source pixels) the field reaches past an edge. A larger spread gives a wider glow range but less precision for thin lines.
3. If the new design moves the `Center` element or changes the group size, update `ArtWidth`, `ArtHeight`, `CenterX`, `CenterY` in `DirectionHintFormula` (and its test).
4. Run the menu in step 1 so the texture keeps its import settings.

## Expected result

With Marked Throat equipped, every living enemy has a sharp white ring at its feet, about 1.6 times its body width, with the arrow pointing where it faces. Without the passive there is no ring. The ring disappears when the enemy dies and with it when the scene changes.

## Common mistakes

- **The ring looks too thick or too thin after import.** The texture was imported as sRGB or compressed. Run the menu in step 1; the postprocessor sets the right import settings.
- **Scaling the sprite to change thickness.** Use the material's **Thicken**; scale only changes size.
- **Exporting the group instead of the shape.** The group adds the grey `Center` dot. Export `Egde_Direction_Hint` (`1850:7366`).
- **The arrow points the wrong way on a test dummy.** Characters with no AI never turn and face right by default; that is expected.
- **The hint stays after a character turns friendly.** Hostility is checked when passives or the player change. A side change (for example Mind Control) shows up at the next change.
