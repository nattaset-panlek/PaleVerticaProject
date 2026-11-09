# Class Select Walkthrough

The Class Mastery page lets the player pick one of four starting classes. A class is the origin node of the Skill Tree, so no separate class value is saved.

## Where to open it

- Build the prefab: **Pale Vertica > UI > Build Class Select**
- Prefab: `Assets/Prefab/Game Page/Class Select Modal.prefab`, window name `ClassSelect`
- Art: `Assets/Art/UI/New/ClassSelect`
- Figma: file `zZ5QnVfPMSFuiMFjpIGbOC`, node `424-1090`

## Inputs

| Input | Used for |
|---|---|
| `_classLabel` on each `ClassCardSlot` | Must match a `role = base` node label in `SkillTree.json` |
| `PassiveClassFormula.RequiredJobLevel` | Job level that unlocks the choice (1) |

## Flow in order

1. A new player starts at Base 0 / Job 0. Level 0 needs the same EXP as level 1.
2. Below Job 1, P shows a notice and the Skill Tree stays closed.
3. On reaching Job 1 the page opens by itself. It can be closed with X or Esc.
4. Until a class is chosen, P and the Esc menu open this page instead of the Skill Tree. `ClassSelectGate` does this through `UIManager.OpenRedirect`.
5. Each card is a Button whose Target Graphic is its `Frame`. Hover, press and selected fade the white `Frame` out with Color Tint, which reveals `Prismatic Frame` under it. `PrismaticFlow.mat` (`Custom/UI/PrismaticFlow`) moves the Figma Prismatic colours and uses only the sprite alpha as its shape, so it works on any Image. Change the speed with `Flow Speed` on that material. Click asks for confirmation.
6. Confirm adds the origin node for free, then opens the Skill Tree.

## Resets for NPC events

Call these from the quest or NPC event when it finishes:

| Call | Result |
|---|---|
| `GameData.Player.Class.ResetAllPassives()` | Removes every node except the origin and refunds one point per node |
| `GameData.Player.Class.ChangeClass(originNodeId)` | Same refund, and the origin becomes the new class |

Origin nodes can never be bought with points.

## Expected result

- The save holds the class as its origin node in `PassiveNodes`.
- After a reset, Passive Points equal the points before plus the nodes removed.

## Common mistakes

- Card click logs `No origin node labelled`: the card label does not match the Skill Tree node label.
- P opens nothing and no notice appears: the prefab was not built, so `ClassSelect` is missing from the registry.
- Old saves keep their saved levels. Those at Job 1 or higher get the page on P if they have no origin node.
