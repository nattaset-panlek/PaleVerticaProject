# SandBox Walkthrough

`Assets/Scenes/Dev/SandBox.unity` is the scene for testing anything. Players cannot reach it: it is not in the build, and no map links to it.

## First-time setup

Run **Pale Vertica > World > Set Up SandBox Test Scene** once. It:

1. Moves `SandBox.unity` and its `SandBox_MapLab` data to `Assets/Scenes/Dev`.
2. Adds the missing NPCs, portals to Town and Field, and three test spawners.
3. Removes SandBox from Build Settings.

It refuses to run if the scene is already in `Dev`. Afterward, move anything that landed off walkable ground.

## Use it

1. Open `Assets/Scenes/Dev/SandBox.unity`.
2. Press Play. `[Game Core]` loads by itself.
3. Press **K** for the Dev Tool to add items, skills and levels.

## Inputs

| Object | What to change |
|---|---|
| `Test Melee`, `Test Ranged`, `Test Mini Boss` | `Spawn Table`, `Max Alive`, `Respawn Delay` |
| `Dummy` | Training target |
| `Portal To Town`, `Portal To Field` | Test map travel |

## Expected result

- All four NPCs can be talked to, and the Shopkeeper's shop opens if its prefab has one.
- Monsters of each type spawn, respawn and drop loot.
- Portals load Town and Field through the loading screen.

## Common mistakes

- Loading SandBox with `LoadingManager.LoadScene`: it is not in the build, so an error is logged. Open it in the Editor.
- A save made in SandBox: Continue uses `New Game Scene`, because SandBox cannot be loaded from a menu.
- Adding a Player, Camera or Canvas to SandBox: the system already comes from `[Game Core]`.
