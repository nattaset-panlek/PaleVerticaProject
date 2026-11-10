# Skill Tree Layout Walkthrough

`Tools/SkillTree/build_balanced_tree.py` builds `Assets/Resources/Data/SkillTree.json`. All 12 classes get the same 69-node sector, so each class has the same reach and the same distance to its first Unique Passive.

## Where to open it

Run it from any folder. It works from the project root by itself.

```bash
python -I Tools/SkillTree/build_balanced_tree.py --render Logs/SkillTree.png
```

| Option | Result |
|---|---|
| none | Rebuilds and writes `SkillTree.json` |
| `--check-only` | Reports on the current file and changes nothing |
| `--render <png>` | Also draws the tree to an image |

## Inputs

| Input in the script | Used for |
|---|---|
| `CLASSES` | Order around the circle, main stat and second stat. Neighbours share a stat so builds can mix |
| `UNIQUE_SLOTS` | Which class and `C1` or `C2` slot holds each finished Unique Passive |
| `Docs/01-Design/Unique_Node_Concepts_TH.md` | The table of concept nodes for the other slots. Its Class, slot and name columns are read by the script |
| `NOT_FOR_TREE` | Unique Passives that must never be on the tree |
| `CENTER`, `PAIRED`, `BOUNDARY` | Node positions inside one sector, as distance outward and sideways |
| `CENTER_LINKS`, `FORK_LINKS`, `SIDE_LINKS`, `BOUNDARY_LINKS` | Lines inside one sector and across to the next sector |

## Layout

- Each sector has a central column of hexagon clusters, with big slots `C1` (inner), `C2` (middle) and `C4` (outer). `C3` is a pair of side clusters with a big slot each.
- Wings run along both sides and use the class second stat.
- Four boundary nodes per sector join neighbouring classes and use the stat they share.
- A slot with no Unique Passive stays a small Attribute node.
- A concept with an entry in `UniquePassive.json` but no script shows "Coming soon" in its tooltip and has no effect when bought.

## Crest shape

`SILHOUETTE` sets how far each direction reaches, which draws the top spire, shoulders, waist and lower point. `CLUSTER_STYLES` and `STYLE_BY_ANGLE` give each pair of classes differently shaped clusters. These only move nodes. Every class keeps the same links, so balance does not change.

## Stats and Notable nodes

- `balanced_stats` keeps the inner part of each sector on the class stats, then gives outer groups to the stat with the fewest nodes, so the six stats end up about even.
- `NOTABLES` lists each class's five Notable nodes at `H1t`, `H2t`, both `T3t` and `H4t`. Each gives double the stat of a small node plus its special bonus.
- `BONUS_POINT` sets one point of each special bonus, sized at about a third of the same bonus on an item. The first four Notables have one point and `H4t` has two, so every class gets the same budget.

## How a big node gives its Unique Passive

- A node with `uniquePassiveId` in the JSON grants that passive when bought.
- `TreeUniquePassiveSystem.Sync` runs in `Character.RecalculateStats`, so buying, resetting, changing class and loading a save all stay correct.
- Passives granted by the tree are not written to `ActiveUniquePassiveIds`, because the bought nodes already record them.

## Steps in order

1. Add the passive class and its entry in `UniquePassive.json`.
2. Add a line to `UNIQUE_SLOTS`, for example `('Warrior', 'C2'): 'Name'`.
3. Back up the tree to `Backup/SkillTree/`.
4. Run the command above.
5. Open the image, then check the tree in Unity after it reimports the JSON.

## Expected result

The script prints a report:

- `crossings` is 0 and `edges_through_nodes` is empty
- `class_reach_within15_firstUnique` has exactly one entry, meaning all classes are equal
- `big_not_unique` and `unique_without_node` are empty

The script refuses to write the file if any of the layout checks fail.

## Common mistakes

- **Saved characters:** node ids and links changed. Use Reset All Passive on an old save.
- **`Bleeding` is in `NOT_FOR_TREE`:** it is the damage-over-time effect that sits on the victim, so owning it hurts the owner.
- **The name in `UNIQUE_SLOTS` must match `Name` in `UniquePassive.json`,** or the build stops with a `KeyError`.
- **Do not run `generate_expanded_tree.py`.** It overwrites this tree with the old one.
- **Do not edit `SkillTree.json` by hand** if you plan to run this script again. Change the script instead.
