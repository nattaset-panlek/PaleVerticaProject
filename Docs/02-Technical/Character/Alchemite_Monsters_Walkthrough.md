# Alchemite Monsters and Environment Walkthrough

Slime, Gas and Golem are non-humanoid monsters from AlchemiteReturn. Trees, rocks and houses from the same project dress Town and Field.

## Where to open it

**Pale Vertica > World > Import Alchemite Monsters And Environment**

## Inputs

| Path | Contents |
|---|---|
| `Assets/Image/Environment` | Sliced, trimmed rocks and houses. Trees use the MapLab default tree art |
| `Assets/Image/Character` | `gas.psb`, `golem.psb`, `slime.psb` |
| `Assets/Animation/Animation/Monster/<Name>` | Controller, idle and walk |
| `Assets/Prefab/Imported/AlchemiteReturn` | Original prefabs, used as the source |

## Steps in order

1. Let Unity finish importing, then run the menu once.
2. It sets the sprite import settings, with the pivot at bottom center.
3. It builds `Slime`, `Gas` and `Golem` in `Prefab/Entity/Enemy` from the Skeletom prefab, swapping in each `Model`, and makes them Addressable.
4. In Town and Field it replaces each `Tree` and `Boulder` in `Props`, where they already stand, adds two houses in Town, and scatters grass tufts over the ground in both maps. Roads and the plaza draw over the grass.
5. It adds a spawner for each monster to Field and bakes the walkable area again.

Your other scene edits are kept. Running it again rebuilds the prefabs and does not add duplicates.

## Expected result

- Each monster plays idle and walk, fights, and has its own stats in the Characters tab (Ids 1009 to 1011).
- The player walks behind trees and houses, and they fade.

## Add another non-humanoid monster

Add its name and Id to `Monsters` in `AlchemiteContentBuilder`, and put its prefab with a child called `Model` that has the Animator in the source folder. Then add its entry in the Characters tab.

## Common mistakes

- No `Attack` or `IsDead` animation: this is allowed. The monster still deals damage, and it disappears after the normal death delay.
- A monster that faces the wrong way: its art was drawn facing the other direction.
- Moving props by hand into a spot used by `Walkable Grid`: bake the walkable area again in MapLab.
