# Kill EXP Walkthrough

How a kill gives EXP, and where to tune level curves and monster rewards. Updated 2026-11-07.

## Where
- Editor: **Window > Pale Vertica > Pale Vertica GameDataWorkbench > DATA > Experience** (replaces the old Tools > Experience Editor window).
  - Level curves (EXP needed per level): `Assets/Resources/Data/ExperienceTable.asset`, saved with **Save Table**.
  - Monster rewards: the "Kill Reward" group lists every character with Base / Job override and EXP multiplier; these write `Character.json` (Ctrl+S). The same fields are also in the Characters tab.
- Code: `Combat/CombatEvents.cs` (`OnKilled`), `Combat/KillCreditRegistry.cs` (damage share), `Combat/Formulas/KillRewardFormula.cs` (all numbers), `Combat/Handlers/Damage/DamageHandler.cs` (records damage, raises the kill), `Entity/Character.cs` `HandleKilled` (pays EXP).
- Tests: `CombatCoreTests` (`KillReward_*`, `KillCredit_*`, `KillingBlow_*`).

## How it works
1. Every hit goes through `CombatCore` -> `DamageHandler.Commit`, which records the HP it removed in `KillCreditRegistry` for the attacker's team leader (a summon's damage counts for its owner; burn ticks count for whoever set the burn).
2. When a hit takes HP from above 0 to 0, `CombatEvents.RaiseKilled` fires once.
3. Each character that dealt damage gets EXP:
   - full EXP = the monster's Base/Job override, or the level curve (`BaseExpForLevel`, Job = 60%) when the override is 0
   - x damage share (`your damage / all damage`)
   - x level gap: +5% per level the monster is above you (max x1.5); no penalty up to 5 levels below; then -30% per 5 levels (min x0.1)
   - x the monster's EXP Multiplier
4. Summons never keep EXP themselves.

## Steps to tune
1. Open the Experience tab.
2. Adjust Max Level, EXP at max and the curves for how much EXP each level needs; check the preview table; press **Save Table**.
3. In "Kill Reward", set a monster's Base/Job override (or leave 0 for the level curve) and multiplier; press Ctrl+S.
4. Make sure the monster prefab's `Character` has the same `CharacterId` (Skeletom.prefab = 1001).
5. Play, kill it, watch the EXP bar.

## Expected result
Killing Skeletom (1001, override 0 = level curve) alone at the same level gives `BaseExpForLevel(its level)` Base EXP and 60% of that as Job EXP (level 1: 12 / 7).

## Common mistakes
- `CharacterId` 0 or missing from `Character.json`: no EXP (the training Dummy on purpose).
- Setting HP directly instead of dealing damage through `CombatCore`: no credit, no kill event.
- Other kill listeners (quests, drops) should use `CombatEvents.OnKilled`, not poll HP.
