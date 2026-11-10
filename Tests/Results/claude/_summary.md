# Scenario results: claude

Updated 2026-11-10 22:43:57. Failed 0, needs review 4, passed 15.

| Status | Scenario | File | Pass | Review | Fail | Last run |
|---|---|---|---|---|---|---|
| NeedsReview | unique_combo_weird | combos/weird.json | 10 | 1 | 0 | 2026-11-10 21:53:10 |
| NeedsReview | dialogue_alchemist_choice | core/dialogue_alchemist.json | 4 | 1 | 0 | 2026-11-10 21:09:22 |
| NeedsReview | input_check | core/input_check.json | 2 | 1 | 0 | 2026-11-10 21:09:52 |
| NeedsReview | active_skill_passive_combos | skills/skill_passive_combos.json | 476 | 5 | 0 | 2026-11-10 22:11:48 |
| Passed | unique_chaos_loadouts | chaos/random_loadouts.json | 51 | 0 | 0 | 2026-11-10 20:58:39 |
| Passed | unique_combo_conflicts | combos/conflicts.json | 10 | 0 | 0 | 2026-11-10 21:44:41 |
| Passed | unique_combo_order | combos/order.json | 7 | 0 | 0 | 2026-11-10 21:47:25 |
| Passed | save_load_round_trip | core/save_load.json | 3 | 0 | 0 | 2026-11-10 21:00:26 |
| Passed | skill_slot0_damage | core/skill_slot0.json | 4 | 0 | 0 | 2026-11-10 21:33:30 |
| Passed | status_bar_scene_change | core/status_bar_scene.json | 4 | 0 | 0 | 2026-11-10 22:43:44 |
| Passed | ui_flow | core/ui_flow.json | 2 | 0 | 0 | 2026-11-10 21:37:08 |
| Passed | unique_defense | passives/defense.json | 5 | 0 | 0 | 2026-11-10 21:00:54 |
| Passed | unique_hit_modifiers | passives/hit_modifiers.json | 12 | 0 | 0 | 2026-11-10 21:00:59 |
| Passed | unique_potions | passives/potions.json | 8 | 0 | 0 | 2026-11-10 21:01:16 |
| Passed | unique_self_stats | passives/self.json | 3 | 0 | 0 | 2026-11-10 21:52:39 |
| Passed | unique_passive_smoke | passives/smoke_all.json | 244 | 0 | 0 | 2026-11-10 21:01:33 |
| Passed | quest_bones_on_the_road | quests/bones_on_the_road.json | 6 | 0 | 0 | 2026-11-10 21:13:30 |
| Passed | setup_new_warrior_lv15 | setup/new_warrior_lv15.json | 1 | 0 | 0 | 2026-11-10 21:03:37 |
| Passed | active_skill_smoke | skills/every_skill.json | 121 | 0 | 0 | 2026-11-10 22:02:32 |

## NeedsReview: unique_combo_weird

Report: `unique_combo_weird.json`, scenario `Tests/Scenarios/combos/weird.json`

- **Review** step 23: summon pack looks right. Expected `a person approves`, got `waiting for review` (Is there at most one, larger summon next to the player?)

## NeedsReview: dialogue_alchemist_choice

Report: `dialogue_alchemist_choice.json`, scenario `Tests/Scenarios/core/dialogue_alchemist.json`

- **Review** step 9: the dialogue window looked right. Expected `a person approves`, got `waiting for review` (Check the speaker name, text and buttons in the screenshot.)

## NeedsReview: input_check

Report: `input_check.json`, scenario `Tests/Scenarios/core/input_check.json`

- **Review** step 8: the player walked and attacked on screen. Expected `a person approves`, got `waiting for review` (Look at the screenshot: the player should stand next to the Skeletom.)

## NeedsReview: active_skill_passive_combos

Report: `active_skill_passive_combos.json`, scenario `Tests/Scenarios/skills/skill_passive_combos.json`

- **Review** step 6: 1001 Tackle + every passive: cast started. Expected `started`, got `did not start (skill 1001 on Player#-1057806, caster Idle)` (Some skills need a condition (an incoming hit, an ally, a free spot). Look at the log if this one should have started.)
- **Review** step 6: 1008 Aura of Flames + every passive: cast started. Expected `started`, got `did not start (skill 1008 on Player#-1057806, caster Idle)` (Some skills need a condition (an incoming hit, an ally, a free spot). Look at the log if this one should have started.)
- **Review** step 6: 1019 Mind Control + every passive: cast started. Expected `started`, got `did not start (skill 1019 on Player#-1057806, caster Idle)` (Some skills need a condition (an incoming hit, an ally, a free spot). Look at the log if this one should have started.)
- **Review** step 6: 3001 Wind Slash + every passive: cast started. Expected `started`, got `did not start (skill 3001 on Player#-1057806, caster Idle)` (Some skills need a condition (an incoming hit, an ally, a free spot). Look at the log if this one should have started.)
- **Review** step 7: 1019 Mind Control + HereticsPyre+LivingPhilosopher+SixFates+VenomLedger+Vampire: cast started. Expected `started`, got `did not start (skill 1019 on Player#-1057806, caster Idle)` (Some skills need a condition (an incoming hit, an ally, a free spot). Look at the log if this one should have started.)
