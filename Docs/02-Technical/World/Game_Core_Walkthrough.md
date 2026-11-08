# Game Core Walkthrough

Scenes hold only world content. Everything else (Player, Camera, Canvas, UI, Managers) lives in one prefab, `[Game Core]`, which loads by itself and stays across scenes.

## First-time setup

Run **Pale Vertica > World > Convert Scenes To Game Core** once. Save your work and commit before you run it. It:

1. Moves `Player`, `Canvas`, `Main Camera`, `Manager` and `EventSystem` from `SandBox` into `Assets/Prefab/Game Core/[Game Core].prefab`.
2. Removes those objects from `SandBox`, `Town` and `Field`, and adds a `Start Point` where the player stood.
3. Creates `Assets/Resources/GameCoreSettings.asset`.

## Make a new map

1. Create a new scene.
2. Place the ground, lighting, Global Volume, NPCs, spawners and portals.
3. Add an empty GameObject with `MapSpawnPoint` and set its `Id` to `start`.
4. Add the scene in **File > Build Profiles** (Scene List).
5. Press Play.

Do not add a Player, Camera, Canvas, EventSystem or Manager to a map.

## Inputs

| Where | Field | Meaning |
|---|---|---|
| `GameCoreSettings` | `Core Prefab` | The system prefab |
| `GameCoreSettings` | `Scenes Without Core` | Scene names with no Player or game UI, such as `Title` |
| `MapSpawnPoint` | `Id` | `start` is the default entry point of a map. Portals can target other ids |

## Change the Player, UI or Managers

Open `[Game Core].prefab` in Prefab Mode and edit it there.

## Expected result

- Press Play from any map: the system appears and the player stands at `start`.
- Moving between maps keeps the player, inventory and UI as they are.
- Loading a scene in `Scenes Without Core` removes the system and clears player data held in memory.
- Change scenes only with `LoadingManager.LoadScene`, so the core is prepared before the load.

## Common mistakes

- A map with no `start` point: a warning is logged and the player stays where they were.
- A map that still has its own Camera, EventSystem or Canvas with `UIManager`: duplicates cause conflicts.
- A prefab cannot reference objects in a scene. Map objects reach the system through registries, events or `PlayerCore.GetPlayer()`.
- Running the converter twice: it refuses while the prefab exists.
