# Map Portal and Respawn Walkthrough

Maps are separate scenes joined by portals, with the global loading screen. The player's data travels with them.

## Build the prototype maps

Run **Pale Vertica > World > Build Prototype Town and Field**. It copies `SandBox.unity` into `Town.unity` and `Field.unity`, turns off spawners in Town, places the four NPCs, a portal and arrival point in each map, adds a Field spawner (melee, ranged, mini boss), and adds both scenes to Build Settings. Afterward, move anything that landed off walkable ground. It refuses to run if either scene already exists.

## Make a new map

1. Duplicate `Assets/Scenes/SandBox.unity`, since every map needs its own camera, canvases, managers and player.
2. Add the scene in **File > Build Profiles** (Scene List).

## Add a portal

1. In the source map, add a GameObject with a sprite and a `Collider2D`.
2. Add `MapPortal`. Set `Target Scene` to the scene name and `Target Spawn Id` to an id.
3. In the target map, add an empty GameObject where the player should appear, add `MapSpawnPoint`, and set the same `Id`.

## Inputs

| Component | Field | Meaning |
|---|---|---|
| `MapPortal` | `Target Scene` | Scene name in Build Settings |
| `MapPortal` | `Target Spawn Id` | Matches a `MapSpawnPoint` in that scene |
| `MapPortal` | `Range` | How close the player must be (default 1.5) |
| `MapSpawnPoint` | `Id` | Unique within the scene |
| `EntitySpawner` | `Respawn Delay` | Seconds before a dead monster is replaced. `0` keeps the wave behaviour |
| `EntitySpawner` | `Max Alive` | Cap on living monsters. `0` means no cap |

## Expected result

- Left click a portal with no skill aimed: the character walks a path to it, the loading screen appears, and the new map loads.
- Inventory, stats and equipment carry over. The player appears at the spawn point.
- With `Respawn Delay` above 0, the first wave fills the zone, then each kill is replaced one at a time after the delay.

## Common mistakes

- Scene missing from Build Settings: an error is logged and nothing happens.
- A spawn id that does not exist: a warning is logged and the player stays at the scene's default position.
- A portal without a `Collider2D` cannot be clicked.
- A map with no player character: the loading screen stays up.
