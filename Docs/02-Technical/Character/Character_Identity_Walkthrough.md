# Character Identity Walkthrough

A character has one identity: its entry in Game Data. The prefab links to that entry through **Game Data Character**. Quests, dialogue, drops and the displayed name all read from it.

## Where to open it

- On a prefab: the `Character` component, **Character Identity** section.
- In data: **Window > Pale Vertica > Game Data Workbench**, **Characters** tab, **Prefab Settings** group.

## Inputs

| Field | Meaning |
|---|---|
| `Game Data Character` | Choose the entry from the list, shown as `[2004] Old Man` |
| `Prefab Path` (Workbench) | The prefab that entry uses |

## Add a character

1. In the Characters tab, add an entry and set its name.
2. Drag the prefab into **Drag Prefab Here**.
3. If a warning appears, press **Set Prefab To This Character**.

## Expected result

- The Workbench shows no warning for the entry.
- In Play mode the name shown in the Agent Core window comes from Game Data.
- Several entries may share one prefab, such as Skeletom, Archer and Captain. Spawners assign the right entry when they spawn.

## Common mistakes

- A red error on the `Character` component: the prefab points to no entry. Choose one.
- A console warning in Play mode: a non-player character has no Game Data entry, so quests, dialogue and drops skip it.
- Placing a shared prefab in a scene by hand: it counts as the entry stored on the prefab. Use a spawner for the others.
