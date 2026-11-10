# Scenario results: claude

Updated 2026-11-10 19:35:23. Failed 8, needs review 1, passed 6.

| Status | Scenario | File | Pass | Review | Fail | Last run |
|---|---|---|---|---|---|---|
| Failed | unique_combo_weird | combos/weird.json | 5 | 1 | 5 | 2026-11-10 16:01:25 |
| Failed | dialogue_alchemist_choice | core/dialogue_alchemist.json | 0 | 0 | 0 | 2026-11-10 16:03:45 |
| Failed | skill_slot0_damage | core/skill_slot0.json | 1 | 0 | 3 | 2026-11-10 16:01:35 |
| Failed | unique_defense | passives/defense.json | 4 | 0 | 1 | 2026-11-10 16:01:35 |
| Failed | unique_hit_modifiers | passives/hit_modifiers.json | 5 | 0 | 7 | 2026-11-10 16:01:36 |
| Failed | unique_self_stats | passives/self.json | 1 | 0 | 2 | 2026-11-10 16:01:43 |
| Failed | unique_passive_smoke | passives/smoke_all.json | 243 | 0 | 1 | 2026-11-10 16:01:45 |
| Failed | quest_bones_on_the_road | quests/bones_on_the_road.json | 0 | 0 | 1 | 2026-11-10 19:31:15 |
| NeedsReview | input_check | core/input_check.json | 2 | 1 | 0 | 2026-11-10 19:30:12 |
| Passed | unique_chaos_loadouts | chaos/random_loadouts.json | 51 | 0 | 0 | 2026-11-10 19:32:51 |
| Passed | unique_combo_conflicts | combos/conflicts.json | 10 | 0 | 0 | 2026-11-10 19:34:34 |
| Passed | unique_combo_order | combos/order.json | 7 | 0 | 0 | 2026-11-10 19:34:52 |
| Passed | save_load_round_trip | core/save_load.json | 3 | 0 | 0 | 2026-11-10 16:01:34 |
| Passed | unique_potions | passives/potions.json | 8 | 0 | 0 | 2026-11-10 16:01:42 |
| Passed | setup_new_warrior_lv15 | setup/new_warrior_lv15.json | 1 | 0 | 0 | 2026-11-10 19:30:39 |

## Failed: unique_combo_weird

Report: `unique_combo_weird.json`, scenario `Tests/Scenarios/combos/weird.json`

- **Failed** step 11: Thorn vs Thorn: reflections do not loop. Expected `0 errors`, got `191` (errors are listed in the report)
- **Failed** step 22: summon pack: no errors. Expected `0 errors`, got `329` (errors are listed in the report)
- **Review** step 23: summon pack looks right. Expected `a person approves`, got `waiting for review` (Is there at most one, larger summon next to the player?)
- **Failed** step 28: caster soup: no errors. Expected `0 errors`, got `573` (errors are listed in the report)
- **Failed** step 34: damage soup: no errors. Expected `0 errors`, got `754` (errors are listed in the report)
- **Failed** step 37: stat soup: no errors. Expected `0 errors`, got `175`
- **Error**: Exception: MissingReferenceException: The object of type 'CoreSystem.Entity.Character' has been destroyed but you are still trying to access it.
- **Error**: Exception: MissingReferenceException: The object of type 'CoreSystem.Entity.Character' has been destroyed but you are still trying to access it.
- **Error**: Exception: MissingReferenceException: The object of type 'CoreSystem.Entity.Character' has been destroyed but you are still trying to access it.
- **Error**: Exception: MissingReferenceException: The object of type 'CoreSystem.Entity.Character' has been destroyed but you are still trying to access it.
- **Error**: Exception: MissingReferenceException: The object of type 'CoreSystem.Entity.Character' has been destroyed but you are still trying to access it.

## Failed: dialogue_alchemist_choice

Report: `dialogue_alchemist_choice.json`, scenario `Tests/Scenarios/core/dialogue_alchemist.json`


## Failed: skill_slot0_damage

Report: `skill_slot0_damage.json`, scenario `Tests/Scenarios/core/skill_slot0.json`

- **Failed** step 9: the cast started. Expected `== 1`, got `0`
- **Failed** step 10: target lost hp. Expected `< 1000124`, got `1000124`
- **Failed** step 11: damage was recorded on the target. Expected `> 0`, got `0`

## Failed: unique_defense

Report: `unique_defense.json`, scenario `Tests/Scenarios/passives/defense.json`

- **Failed** step 22: Last Banner: the shield absorbs near half hp. Expected `< 191.8`, got `191.8`

## Failed: unique_hit_modifiers

Report: `unique_hit_modifiers.json`, scenario `Tests/Scenarios/passives/hit_modifiers.json`

- **Failed** step 9: baseline hits are identical. Expected `1 +/- 0.01`, got `0`
- **Failed** step 19: One Clean Cut: first hit 2x. Expected `2 +/- 0.03`, got `0`
- **Failed** step 20: One Clean Cut: second hit 0.7x. Expected `0.7 +/- 0.03`, got `0`
- **Failed** step 31: Windreader: distance bonus. Expected `1.001..1.6`, got `0`
- **Failed** step 36: Anvil Heart: hits are the same size. Expected `1 +/- 0.01`, got `0`
- **Failed** step 37: Anvil Heart: hit equals the average crit. Expected `1.25 +/- 0.03`, got `0`
- **Failed** step 44: Twin Conduit: adds half of M.ATK. Expected `1.0882 +/- 0.03`, got `0`

## Failed: unique_self_stats

Report: `unique_self_stats.json`, scenario `Tests/Scenarios/passives/self.json`

- **Failed** step 6: Hollow Crown: max hp is halved. Expected `0.5 +/- 0.03`, got `1`
- **Failed** step 11: Whisper Pact: repeated casts lower max hp. Expected `< 55101`, got `55101`

## Failed: unique_passive_smoke

Report: `unique_passive_smoke.json`, scenario `Tests/Scenarios/passives/smoke_all.json`

- **Failed** step 5: AlphasBurden: no errors. Expected `0 errors`, got `177`
- **Error**: Exception: MissingReferenceException: The object of type 'CoreSystem.Entity.Character' has been destroyed but you are still trying to access it.
- **Error**: Exception: MissingReferenceException: The object of type 'CoreSystem.Entity.Character' has been destroyed but you are still trying to access it.
- **Error**: Exception: MissingReferenceException: The object of type 'CoreSystem.Entity.Character' has been destroyed but you are still trying to access it.
- **Error**: Exception: MissingReferenceException: The object of type 'CoreSystem.Entity.Character' has been destroyed but you are still trying to access it.
- **Error**: Exception: MissingReferenceException: The object of type 'CoreSystem.Entity.Character' has been destroyed but you are still trying to access it.

## Failed: quest_bones_on_the_road

Report: `quest_bones_on_the_road.json`, scenario `Tests/Scenarios/quests/bones_on_the_road.json`

- **Failed** step 5: step 5 AssignSkill completes. Expected `completes`, got `NullReferenceException: Object reference not set to an instance of an object` (a new game has Tackle in skill slot 0 but not on the hotbar)
- **Error**: Exception: NullReferenceException: Object reference not set to an instance of an object ScenarioTesting.Actions.AssignSkillAction+<Run>d__0.MoveNext () (at Assets/Script/ScenarioTesting/Actions/UIActions.cs:175)

## NeedsReview: input_check

Report: `input_check.json`, scenario `Tests/Scenarios/core/input_check.json`

- **Review** step 8: the player walked and attacked on screen. Expected `a person approves`, got `waiting for review` (Look at the screenshot: the player should stand next to the Skeletom.)
