# Sprint Skill Walkthrough

Sprint (skill 1021) works like FFXIV's Sprint: a burst of speed that eases into a jog and then slows back to walking. It is a normal active skill (instant, self, no MP, 30 s cooldown) built on a reusable `SprintEffect`.

## How it behaves

With the default data (Value 60, Duration 10 s):

| Time | Phase | Move speed |
| --- | --- | --- |
| 0 - 4 s | Sprint | +60 % |
| 4 - 5 s | Sprint to Jog | eases from +60 % to +30 % |
| 5 - 8 s | Jog | +30 % |
| 8 - 10 s | Jog to Walk | eases from +30 % to 0 |
| 10 s | Walk | buff removed |

Casting it again while active restarts it at full sprint (one stack only). The buff shows in the buff bar as "Sprint" for its whole duration.

## Where

| What | Path |
| --- | --- |
| Skill data | `Assets/Resources/Data/Skill.json`, `SkillID` 1021, effect `SprintEffect` |
| Skill asset (generated) | `Assets/Resources/Skill/ActiveSkill/Sprint_1021.asset` |
| Effect | `Assets/Script/CoreSystem/Skills/Logic/Effects/SprintEffect.cs` |
| Curve | `Assets/Script/CoreSystem/Combat/Formulas/MovementFormula.cs` (`SprintBonus`, `GetSprintPhase`), phases in `Assets/Script/CoreSystem/Movement/SprintPhase.cs` |
| Buff support | `BuffSystem.GetBuff` / `ReplaceModifier` (the modifier is swapped every tick) |
| Converter | `SkillConverter` case `SprintEffect`; batch entry `SkillConverter.ConvertSkillIdsBatch` (ids from env `PV_SKILL_IDS`) |
| Workbench | Skills tab: effect type `SprintEffect` with Sprint Bonus %, Duration, Buff Name, Buff Code |
| Tests | `Assets/Script/Editor/Tests/SprintTests.cs` |

## Inputs (effect fields)

| Field | JSON | Default | Meaning |
| --- | --- | --- | --- |
| `SprintBonusPercent` | `Value` | 60 | Move speed bonus at full sprint (PercentAdd on MoveSpeed). |
| `Duration` | `Duration` | 10 | Seconds from sprint to walk. |
| `JogRatio` | asset only | 0.5 | Jog bonus as a share of the sprint bonus. |
| `SprintShare` / `RampShare` / `JogShare` | asset only | 0.4 / 0.1 / 0.3 | Share of `Duration` for sprint, the sprint-to-jog ease and jog; the rest is the fade to walk. |
| `TickInterval` | asset only | 0.1 | How often the speed is updated. |
| `BuffName`, `BuffCode` | `BuffName`, `BuffCode` | Sprint | Buff label and id. |
| `BuffIconPath` / `Icon` | `BuffIconPath` | none | Buff icon. |

## Steps

### A. Give Sprint to the player
1. Play mode, press **K** (dev tool), Skills page, grant **Sprint**, or grant it from code: `character.skillCaster.GrantSkill(1021)`.
2. Put it on the hotbar and use it while moving.

### B. Tune it
1. Open **Window > Pale Vertica > Pale Vertica GameDataWorkbench**, Skills tab, skill 1021, or edit `Skill.json`.
2. Change `Value` (sprint %), `Duration`, `BaseCooldown`.
3. Re-generate the asset: in the Workbench convert the skill, or with Unity closed run
   ```bash
   PV_SKILL_IDS=1021 bash Tools/FigmaToUnity/unity_batch.sh SkillConverter.ConvertSkillIdsBatch
   ```
4. Phase shares and jog ratio live on the asset (`Sprint_1021.asset` > Effects > SprintEffect); a JSON re-convert resets them to the defaults.

### C. Reuse the effect
Add `SprintEffect` to any skill (for example a mount call or a dash with a speed tail). It always targets the caster.

## Expected result

- Move speed (Inspect window, MOVE) shows 160 right after casting, 130 during the jog, then drops smoothly to 100 at 10 s.
- `SprintTests` pass.

## Common mistakes

- **Speed jumps instead of easing**: `TickInterval` too large; keep it at 0.1 or lower.
- **Sprint stacks or never ends**: another buff uses the same `BuffCode` on MoveSpeed. Keep the code unique.
- **Changes in the asset disappear**: a JSON re-convert rebuilds the effect from `Skill.json`; set phase shares again or add them to the converter.
- **No animation change**: the character still plays the walk animation, only faster. There is no jog/sprint animation or animator speed parameter yet.
