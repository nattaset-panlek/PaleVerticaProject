# Dev Tool Character Spawn Walkthrough

Spawn any character from Game Data near the player, for testing.

## Where to open it

In Play mode press **K**, then open the **Characters** page.

## Inputs

| Control | Meaning |
|---|---|
| Search bar | Filters by name or Id |
| `How Many` | 1 to 20 per press |
| `Spawn` | Creates that character near the player |
| `Remove Spawned` | Removes everything spawned from this page |

## Steps in order

1. Open the Characters page.
2. Set `How Many`.
3. Press `Spawn` on a row.

## Expected result

The character appears about 4 units from the player, using its Game Data stats, AI and drop table. NPCs and monsters both work.

## Common mistakes

- A character is missing from the list: its `Prefab Path` is empty in the Characters tab.
- `Could not spawn`: the prefab is not Addressable, or `Prefab Path` is wrong. See the Console.
- A character that lands on a wall: spawn it again from a clearer spot.
