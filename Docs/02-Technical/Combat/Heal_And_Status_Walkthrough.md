# Heal and Status Walkthrough

Healing and status ailments run through `CombatCore`, using `HealHandler` and `StatusHandler`.

## Heal a target from a skill

1. On the `ActiveSkill` asset add a `HealEffect`.
2. Set `Target Attribute` (HP or MP), `Base Heal Amount`, and `Matk Scale` (default 0.5).

Result: HP heal = `Base + MATK x Matk Scale`, capped at max. MP heals ignore MATK.

## Make a consumable heal

In **Window > Pale Vertica > Game Data Workbench**, Items tab, set `Heal HP` and `Heal MP` on the item, or set `HealHP` and `HealMP` in `Item.json`. Potions restore a fixed amount and do not use MATK.

## Apply a status from a skill

Add an `ApplyStatusEffect` to the skill and set:

| Field | Used by | Meaning |
|---|---|---|
| `Status` | all | Stun, Freeze, Slow, Burn or Poison |
| `Duration` | all | Seconds |
| `Tick Damage`, `Tick Interval` | Burn, Poison | Damage per tick |
| `Slow Percent` | Slow | Move speed reduction, in percent |

From code: `StatusHandler.Apply(caster, target, new StatusSpec { ... }, procSkillId)`.

## Expected result

- Reapplying a status adds its duration to the time left. It never stacks strength.
- Burn and Poison ticks are Status damage and credit the caster.
- Dead targets are rejected by the Gate.

## Common mistakes

- `Tick Damage` left at 0: Burn or Poison does nothing.
- `Slow Percent` left at 0: Slow does nothing.
- Item heal values of 0: the consumable is used up with no effect.
- Adding a new `StatusKind` without a case in `StatusHandler.Commit` does nothing.
