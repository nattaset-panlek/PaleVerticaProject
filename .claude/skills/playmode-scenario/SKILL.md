---
name: playmode-scenario
description: Verification protocol for Pale Vertica. Use after ANY gameplay change (skills, passives, Skill Tree, quests, dialogue, NPCs, items, combat, save data, scenes) to prove it works by writing a JSON scenario in Tests/Scenarios that sends input, lets the game act and checks the recorded values. Also use when asked to "test", "check it works", "play it for me", write a scenario, add a scenario action, or read scenario reports.
---

# Scenario Test Protocol

You verify your work by writing a scenario file, not by reading code. A scenario sends input (virtual keyboard and mouse), lets the game act, and records results. Every check ends as **Pass**, **Review** (a person must look) or **Fail**.

Full reference: `Docs/02-Technical/Testing/PlayMode_Scenario_Walkthrough.md`. The action list with examples is also in **Pale Vertica > Testing > Scenario Checklist**.

## Definition of done

1. `python Tools/FigmaToUnity/compile_check.py` prints OK for both assemblies.
2. `Tools/FigmaToUnity/unity_batch.sh tests` passes, including `ScenarioFormatTests` (your file parses, every action exists, every expression is valid).
3. Your scenario ran and its report `Tests/Results/<agent>/<name>.json` has **no Fail**. Read `Tests/Results/<agent>/_summary.md` first: one table of every scenario plus each Fail and Review check with expected and actual values. Run it with `unity_batch.sh playtests:<area>/<file>` (Unity closed) or ask the user to press **Run in Play mode** in the Checklist window.
4. Review checks are listed in your reply so the user knows what to look at. Never mark them approved yourself.

If one is not true, say which one and why. Never claim the change works from reading code.

## Rules

- **Input, action, record.** Never add test hooks, flags or commands to game code. If the game needs a new way to be driven, drive it through input; if that is impossible, write a C# action in `Assets/Script/ScenarioTesting/Actions/` that uses existing public game APIs.
- Everything under `Assets/Script/ScenarioTesting/` stays inside `#if UNITY_EDITOR || PV_SCENARIO_TESTS`.
- One file per scenario in `Tests/Scenarios/<area>/`, unique `name`, a `description` a person can follow while watching, a `note` on non-obvious steps.
- Every scenario has at least one `Expect` on a value the change should affect. Use `tolerance` for exact numbers, `review` for a borderline band, `"severity": "review"` when the expected number depends on content, and a `Review` step for anything only eyes can judge (VFX, animation, layout).
- Write the expected number from the design or the description, not from what the game currently does. Never loosen a check to pass; if the design number is unclear, ask.
- Before a scenario that uses menus, items, skills or travel, read `Docs/02-Technical/Testing/Game_Flow_Map.md`: it lists what the player must do first (class, hotbar, windows) and the action for each (`OpenWindow`, `Menu`, `ClickOn`, `Drag`, `AssignSkill`, `EquipItem`).
- Start from a shared setup with `"path": ["setup/new_warrior_lv15.json"]` (or `{"do":"Include","file":...}` mid-way) instead of copying StartNewGame, Level, SpendStats and ChooseClass. Add new setups to `Tests/Scenarios/setup/`.
- Set `seed` (or rely on the default) so random rolls repeat. Use `continueOnFail` for lists of independent cases.
- Ids come from `Character.json`, `Quest.json`, `Item.json`, `UniquePassive.json`. Name spawned characters with `"as"` and refer to them by name.

## Choose the check for the change

| You changed | Scenario |
|---|---|
| A formula | EditMode test of the formula, plus a scenario that measures it in play with `Record` and `Expect` |
| A skill | `SpawnTarget`, `Record` HP/MP, `Cast`, `Expect` on `target.HP`, `hits.total`, `cast`; `Review` for VFX |
| A Unique Passive or Notable | `Reset`, baseline `Attack`, `Give`, `Attack` again, `Expect` the ratio from its description; `Snapshot` + `ExpectSameStats` after `Take` for leaks; add combo cases with passives on the same hook |
| Quest, dialogue, NPC | `TalkTo`, `Choose`, `AcceptQuest`, `UsePortal`, `Kill` (revives on death, counted in the `deaths` metric), `TurnInQuest`, then `quest.<id>.status` |
| Player build | `Level`, `SpendStats`, then `ChooseClass` (by the rules: EXP gives points, points buy stats), or `SetBase` when the rules do not matter. Use this when the player dies too fast for the scenario |
| Save data | `Record`, `Save`, change state, `Load`, `Expect` it came back |
| Items | `UseItem` or `AddItem`, then HP, MP, `items.<id>`, `potion` |
| UI layout only | Not a scenario: use the figma-to-unity skill |

## Minimal template

```json
{
  "name": "area_what_it_proves",
  "description": "One or two sentences a person reads while watching.",
  "tags": ["area"],
  "steps": [
    { "do": "StartNewGame" },
    { "do": "SpawnTarget" },
    { "do": "Record", "name": "before", "value": "target.HP" },
    { "do": "Attack", "hits": 3 },
    { "do": "Expect", "name": "target lost hp", "value": "target.HP", "op": "<", "to": "var.before" }
  ]
}
```

## Read results

- `unity_batch.sh playtests` prints `scenario claude/<name>: <Status>` and every Review or Fail check with expected and actual.
- The report also has `StepResults` (where a step failed and why), `Metrics`, `Variables`, `Errors` (anything logged as an error) and `Combat` (hits, damage taken, kills).
- A failed step stops the scenario unless `continueOnFail` is set. Fix the cause or the scenario, then run again.
