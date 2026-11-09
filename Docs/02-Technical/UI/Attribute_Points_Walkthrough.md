# Attribute Points Walkthrough

Players earn points on level up and spend Attribute Points in a mock window.

## Where to open it

Press **C** in play. The Character Status window moved from C to **U**.

## Point rules

Set in `ProgressionFormula`:

| Level up | Points | Result |
|---|---|---|
| Base level | Attribute Points: `5 + floor(level / 10)` | 51 points by level 10, 112 by level 20 |
| Job level | Passive Points: 1, plus 1 more on every 10th job level | 220 points by job level 200 |

Only the player earns points. Saves keep Attribute Points in `Stats.StatPoints`.

## Steps in order

1. Gain Base EXP until the level rises.
2. Press C. The header shows the available points.
3. Press **+** on STR, VIT, AGI, INT, DEX or LUK. Each press spends one point and adds 1 to the base stat.

## Expected result

- The value rises by 1 and Status shows the new derived stats.
- With no points left, **+** does nothing.

## Common mistakes

- C opens Status as well: you rebound Status to C in Settings. Change it there.
- Existing saves do not get points for levels already gained.
