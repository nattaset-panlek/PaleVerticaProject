# Monster Data Walkthrough

Monster stats and AI live in Game Data, so one prefab can serve many monsters.

## Where to open it

**Window > Pale Vertica > Game Data Workbench**, **Characters** tab, **Stats And AI From Game Data** group.

## Inputs

| Field | Meaning |
|---|---|
| `Use Game Data Stats` | On: these values replace the prefab's. Off: the prefab is used as it is |
| `Level` | Also sets the EXP reward when the EXP overrides are 0 |
| `STR` to `LUK` | Core attributes. ATK, MATK, Hit, Flee and ASPD are derived from them by the normal formulas |
| `Base HP`, `Base MP` | Maximum before VIT and INT bonuses |
| `Defense`, `MDEF`, `Weapon ATK`, `Weapon MATK` | Flat values |
| `Move Speed` | `0` keeps the prefab's speed |
| `Element`, `Size`, `Race` | Used by the damage modifiers |
| `Behavior`, `Combat Style` | How the AI acts |
| `Detection Radius`, `Attack Range`, `Can Roam` | AI distances and wandering |
| `AI Skills` | Skills the AI may use |

## Add a monster

1. Add an entry in the Characters tab and set the name and Faction.
2. Drag a prefab into **Drag Prefab Here**.
3. Turn on **Use Game Data Stats** and fill in the group.
4. Choose a **Drop Table**.
5. Add it to an `EntitySpawner`'s Spawn Table.

## Expected result

Monsters from a spawner use their Game Data stats, at full HP. A prefab placed by hand uses the entry chosen in its `Game Data Character`.

## Common mistakes

- `Use Game Data Stats` off: the prefab's stats are used.
- `EntitySpawner` overrides still win over Game Data. Leave them off unless a spawn point really needs a difference.
- Giving every monster the same core attributes: damage comes mostly from STR, INT and Weapon ATK.
- The eight starter monsters have placeholder values and all use the Skeletom art.
