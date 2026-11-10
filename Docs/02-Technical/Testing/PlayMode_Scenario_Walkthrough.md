# Scenario Test Walkthrough

A scenario is a JSON file that lists steps: press keys, click things, set up a test, then check values. The runner plays it in Play mode while you watch, records what happened, and marks every check as Pass, Review or Fail. AI agents write scenarios to prove their work; people review the ones marked Review.

The framework only sends input, lets the game act, and records the result. Game code knows nothing about it, and everything is compiled out of release builds.

## Where things are

| Path | What it is |
|---|---|
| `Tests/Scenarios/<area>/<name>.json` | Scenario files (outside `Assets/`) |
| `Tests/Scenarios/_reviews.json` | Verdicts people gave to Review checks |
| `Tests/Results/<agent>/<name>.json` | Report of the last run, plus Review screenshots |
| `Tests/Results/<agent>/_summary.md` | Rebuilt after every run: status of every scenario, then each Fail and Review check with expected and actual values. Read this first |
| `Tests/Results/<agent>/history/<name>/` | One copy of the report per run, named by time and status |
| `Assets/Script/ScenarioTesting/` | Runner (`ScenarioManager`), virtual input (`ScenarioInput`), state reader (`ScenarioProbe`), `ScenarioLab`, expressions, reports |
| `Assets/Script/ScenarioTesting/Actions/` | Built-in actions, and the place for new C# actions |
| `Assets/Script/Editor/ScenarioTesting/ScenarioChecklistWindow.cs` | **Pale Vertica > Testing > Scenario Checklist** |
| `Assets/Script/Editor/ScenarioTesting/ScenarioBatchRunner.cs` | Batch entry: enters Play mode, runs the scenarios, exits Unity with 0 (no failures) or 1 |
| `Assets/Script/Editor/Tests/ScenarioFormatTests.cs` | EditMode check that every file parses, uses known actions and valid expressions |

Everything in `ScenarioTesting` is wrapped in `#if UNITY_EDITOR || PV_SCENARIO_TESTS`. Add the `PV_SCENARIO_TESTS` scripting define only to a development player build that should run scenarios.

## Watch a scenario

1. Open **Pale Vertica > Testing > Scenario Checklist**.
2. Pick a scenario on the left. The right side shows its steps, last result and checks.
3. Set **Speed** if you want it slower or faster, then press **Run in Play mode**.
4. Unity enters Play mode and the game plays itself. A panel at the bottom left shows the progress (`Scenario 40%`, and with more than one queued `Run 3/15, overall 17%`) with a bar, then the current step, its note and the counts so far. The selected scenario in the Checklist window shows the same progress bar while it runs. The percentage counts finished steps, so a long step such as `StartNewGame` holds it still for a few seconds.
5. When it ends the list refreshes. Green is Pass, yellow is Review, red is Fail.
6. For each Review check open **Screenshot**, type a note if you want, then **Approve** or **Reject**. A scenario with every Review approved counts as Pass.

Run several at once with the checkboxes and **Run checked**, or **Run all** / **Run not passed**. **Stop play when done** leaves Play mode after the queue ends. While scenarios run, your real mouse and keyboard are switched off for the game (the Editor and this window still work), so only the virtual mouse clicks. They come back when the queue ends or you leave Play mode.

### Copy a result to share

- **Copy** (next to **Open report**) copies the selected run: status, every check and step that did not pass with expected and actual values, the first errors, variables and the end of the log. If you are viewing an older run from History, it copies that run.
- **Copy fails** (toolbar) copies a short list of every scenario that did not pass, with its failed and review checks.

### Clean up history

Each run adds a file to `Tests/Results/<agent>/history/<name>/`. The latest result of each scenario (`Tests/Results/<agent>/<name>.json`) is never deleted, so the list keeps its status.

- **One scenario:** open its History section and press **Keep latest only** or **Delete all**.
- **Every scenario:** toolbar **History > Keep the latest run of every scenario** or **History > Delete all history**.
- **Automatically:** toolbar **History > After each run, keep** and pick **Latest 1**, **5**, **20** or **All runs** (the default). The choice is saved for this computer and also applies to batch runs.

Each delete asks first and cannot be undone, except through git for history files that were committed.

## Write a scenario

```json
{
  "name": "skill_slot0_damage",
  "description": "Press hotbar slot 1 and click a frozen Skeletom.",
  "tags": ["skill"],
  "seed": 1337,
  "steps": [
    { "do": "StartNewGame" },
    { "do": "SpawnTarget", "tank": false },
    { "do": "Record", "name": "hp", "value": "target.HP" },
    { "do": "Cast", "slot": 0, "at": "target", "wait": 2, "note": "presses the hotbar key, then clicks" },
    { "do": "Expect", "name": "target lost hp", "value": "target.HP", "op": "<", "to": "var.hp" },
    { "do": "Review", "name": "the skill VFX looked right", "note": "Check the slash in the screenshot." }
  ]
}
```

Top-level fields: `name` (unique, used for the report), `description`, `tags`, `seed` (fixes every random roll), `agent` (save profile, default `claude`), `clearSaves`, `continueOnFail`.

### Scenario Path (shared setup)

`"path": ["setup/new_warrior_lv15.json"]` runs the steps of those files first, in order, then this file's steps. Inside `steps`, `{"do":"Include","file":"setup/x.json"}` inserts another file's steps at that point. Paths are relative to `Tests/Scenarios/` (or to the including file). Included files may have their own `path`; a loop is reported as an error. In the checklist, included steps show their source as `[setup/x.json] Step ...`.

Shared setups live in `Tests/Scenarios/setup/`. Use one instead of repeating StartNewGame, Level, SpendStats and ChooseClass in every scenario.

Every step has `do` and may have `note` (shown while it runs), `timeout` (seconds, default 120) and `continueOnFail`.

The full list of actions with an example each is in the window under **Actions you can use in a scenario**. In short:

| Category | Actions |
|---|---|
| Input | `Key`, `Walk`, `WalkTo`, `Click`, `ClickUI`, `Idle` |
| NPC | `TalkTo`, `Choose`, `Continue`, `CloseDialogue`, `AcceptQuest`, `TurnInQuest`, `QuestTalk` |
| Combat | `SpawnTarget`, `Spawn`, `Attack`, `Kill` (presses Revive on death, `"revives"`, default 3; deaths go to the `deaths` metric), `Revive`, `Cast`, `TakeHits`, `Impulse` |
| Progression | `Level` (`base`, `job`: gives EXP so the game grants stat and passive points), `SpendStats` (`"VIT":20,"STR":"rest"`, fails without enough points), `SetBase` (free mode, writes base stats directly), `ChooseClass` (`"class":"Warrior"`: opens the Passive Tree by its key, clicks the card, Confirm) |
| UI | `OpenWindow`, `CloseWindow`, `Menu`, `ClickOn` (right, double), `Drag`, `AssignSkill`, `EquipItem`. Targets and game rules: [Game Flow Map](Game_Flow_Map.md) |
| Game | `StartNewGame`, `UsePortal`, `Travel`, `Save`, `Load`, `ContinueFromSlot` |
| Setup | `Give`, `Take`, `ClearPassives`, `SetStat`, `ClearStat`, `SetHp`, `SetMp`, `Refill`, `UseItem`, `AddItem`, `Reset` |
| Check | `Record`, `Measure`, `Expect`, `ExpectNoErrors`, `Snapshot`, `ExpectSameStats`, `Review` |
| Flow | `Wait`, `WaitUntil`, `Repeat`, `Watch`, `Log`, `Speed`, `Seed` |
| Script | `StandardRoutine`, `SmokeAllPassives`, `RandomLoadouts`, `CastEverySkill`, `SkillPassiveCombos` |

Input actions use a virtual keyboard and mouse through the Input System: W/A/S/D to walk, left click to attack or talk, the hotbar key to aim a skill, mouse clicks on UI buttons by their text. Setup actions use existing game APIs directly, because they arrange the test rather than play it.

### Active skill smoke tests

Two slow scenarios in `Tests/Scenarios/skills/` cast every Active skill in `Database.Skills`:

- `every_skill.json` (`CastEverySkill`): each skill alone.
- `skill_passive_combos.json` (`SkillPassiveCombos`): each skill with every Unique Passive at once (`"mode":"all"`), again without Oath of Silence (`"skipPassives"`), then with seeded random sets (`"mode":"random","count":2,"size":5`).

For each skill the action puts it on hotbar slot 1, presses the key and clicks the target like a player, then records:

- **cast started**. The harness counts a cast when the caster leaves Idle, the skill goes on cooldown, MP drops, or the skill actually executes (a harness-only probe passive, `ScenarioCastProbe`, is attached during the cast and is not registered with the game). If a passive refuses the cast on purpose (`CanCast` is false, for example Oath of Silence on a skill with a cast time) it passes with that note. Anything else is a Review, because some skills need a condition (an incoming hit, an ally, a free spot).
- **caster returns to Idle** within 10 seconds, so a stuck cast fails.
- **toggles off** for toggle skills, after a second cast.
- **hp, mp and max hp are valid** for the player and the target, and **no errors** while that skill ran.

Between skills it refills hp/mp (max MP is raised to 100000 for the run), walks back to the target if a skill moved the player, and spawns a new target if the old one died or changed side. Limit a run with `"skills":[1001,1002]` or leave some out with `"skip":[1019]` and `"skipPassives":["Rock"]`.

## Values and checks

`value`, `to`, `min`, `max`, `until` and most numbers accept expressions with `+ - * / ( )`:

| Value | Meaning |
|---|---|
| `player.HP`, `target.MaxHP`, `victim.ATK` | Any `AttributeType` of the player, the lab target or a character named with `"as"` |
| `player.x`, `.y`, `.dead`, `.exists`, `.level`, `.baseExp`, `.passivePoints` | Other character values |
| `var.name` | A value saved with `Record` (`var.i` inside `Repeat`, `var.name_min` / `_max` after `Watch`) |
| `hit.0`, `hits.count`, `hits.total`, `hits.avg` | Hits of the last `Attack` or `Cast` |
| `dealt`, `taken`, `kills`, `errors`, `summons`, `time` | Running totals |
| `quest.1.status`, `quest.1.count.0`, `status.Active` | Quest state and status names |
| `dialogue.open`, `.node`, `.choices`, `.character`, `.quest` | The open conversation |
| `items.2001`, `potion`, `cast`, `crit`, `scene.Town`, `passive.Vampire`, `distance.player.target` | Inventory count, last potion kept, last cast started, crit multiplier, current scene, owned passive, distance |

`Expect` ops: `==`, `!=`, `<`, `<=`, `>`, `>=`, `~=` (with `tolerance`) and `between` (`min`, `max`).

- Inside the tolerance: **Pass**.
- Outside it but within `review`: **Review**, for borderline numbers.
- `"severity": "review"`: a miss becomes **Review** instead of Fail, for checks that depend on content.
- `Review` steps are always Review and take a screenshot for a person. In batch mode (`unity_batch.sh playtests`) they skip the screenshot, because nothing is rendered there; run them in the Editor to get one.

## Add a C# action

When JSON cannot express something, add a class in `Assets/Script/ScenarioTesting/Actions/` inside `#if UNITY_EDITOR || PV_SCENARIO_TESTS`:

```csharp
[ScenarioAction("OpenInventory", "Input", "{\"do\":\"OpenInventory\"}  presses I and waits for the window")]
internal sealed class OpenInventoryAction : ScenarioAction
{
    public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
    {
        yield return run.Input.Tap(Key.I);
        yield return run.WaitFor(() => ScenarioInput.FindButton("Sort") != null, 5f, "the inventory window");
    }
}
```

Read arguments with `step.Int`, `step.Float`, `step.String`, `step.Strings`, `run.Number` (expressions) and `run.Entity`. Record with `run.Record`, `run.Measure`, `run.SetVariable`; stop the step with `run.FailStep`. It shows up in the window and in `ScenarioFormatTests` by itself.

## Run without the window

- With Unity closed: `Tools/FigmaToUnity/unity_batch.sh playtests` runs every scenario. `playtests:core/,quests/` runs only files whose path contains one of the parts. `SCENARIO_SPEED=4` plays faster (default 2). It prints each scenario's status and every Review or Fail check.
- With Unity open: use the Checklist window.
- `Tools/FigmaToUnity/unity_batch.sh tests` includes `ScenarioFormatTests`, which catches a misspelt action or a broken expression without playing.
- Every step writes `[Scenario] <name> step <n>/<steps> (run <i>/<runs>): <action>` to the Unity log (`Editor.log`, or `Logs/Batch/playtests.log` in batch). If Unity freezes during a run, the last of these lines shows which step it froze on.

## Save profiles

Each scenario saves in `Saves/Profiles/<agent>/`, never in the player's own saves. `"clearSaves": true` empties that profile first. Only the player is saved for now.

## Common mistakes

- **A click does nothing:** the Game view lost the virtual mouse because the real mouse moved over it, or the target was off screen. `ClickCharacter` walks closer first, but walls can stop W/A/S/D steering.
- **`ClickUI` cannot find a button:** the message lists every visible button label. Labels come from the UI text, so match the current language.
- **"unknown action":** check the spelling against the window's action list; `ScenarioFormatTests` catches this before playing.
- **A Review stays yellow after a new run:** verdicts belong to one run. Review the new screenshots again.
- **A check was loosened to pass:** do not. Fix the game or ask why the number differs.
