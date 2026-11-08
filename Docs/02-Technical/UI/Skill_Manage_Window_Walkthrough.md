# Skill Manage Window Walkthrough (Modifier / Effect / Basic pages)

Verified: **2026-10-18** against `SkillManagePrefabBuilder.cs`, `SkillManageWindow.cs`, `SkillEffectPageView.cs`, `SkillBasicPageView.cs`, `SkillItemListUI.cs` and `UISnapshotRenderer.cs`.

The skill window (`Assets/Prefab/Game Page/Character SkillManager Window.prefab`) is a hand-made prefab page, not a builder window. It follows three Figma frames in [Whiper-of-pale](https://www.figma.com/design/zZ5QnVfPMSFuiMFjpIGbOC/Whiper-of-pale), page Preview:

| Tab | Figma frame | Page in the prefab |
|---|---|---|
| Modifier | `Skill Manage 16:9` (569-1558) | `Right Container/Container/Modifier` (made by hand) |
| Effect | `Skill Manage 16:9 - Effect` (569-1827) | `Right Container/Container/Effect Page` (built by the tool) |
| Basic | `Skill Manage 16:9 - Basic` (569-2084) | `Right Container/Container/Basic Page` (built by the tool) |
| Advance | no Figma frame; same look as Basic | `Right Container/Container/Advance Page` (built by the tool) |

## 1. Where to open it

- Unity menu **Pale Vertica > UI > Build Skill Manage Pages**.
- Batch, with Unity closed:
  ```
  "D:/Unity/6000.4.8f1/Editor/Unity.exe" -batchmode -projectPath "F:/GameProject/Pale Vertica" -executeMethod CoreSystem.Editor.UI.SkillManagePrefabBuilder.BuildBatch -logFile skillbuild.log
  ```
- From code: `CoreSystem.Editor.UI.SkillManagePrefabBuilder.Build()`.

## 2. Inputs

| Input | Path / note |
|---|---|
| Window prefab | `Assets/Prefab/Game Page/Character SkillManager Window.prefab`. The tool needs `Header`, `Right Container/Container/Modifier`, its `SupportSkill Header`, and `Modifier Effect Container/Scroll View` (headers and scroll views are cloned from them, so they keep the same look) |
| Sub-prefabs it tunes | `Assets/Prefab/Page UI/Skill Manage/Item Skill List.prefab`, `Skill Tag Element.prefab` |
| Row art | `Assets/Art/UI/New/Page/Skill Manager Page/UI_Skill_Attribute_Row.png` (496 x 45, exported from the Figma row shape; the tool sets it to Sprite, 9-slice L/R 24, Full Rect) |
| Modifier field prefab | `Assets/Prefab/Page UI/Skill Manage/Modifier Container.prefab` (the tool removes its ContentSizeFitter; the list's layout group sizes it) |
| Fonts | Kanit Regular / Medium / Bold SDF, `Novecentosanswide-DemiBold SDF` |
| Icons | `Icon/UI_StarGylp_Icon.png` (title tag), `Rect/rounded.png` |

## 3. Steps

1. Close Unity if you use the batch command; otherwise just run the menu.
2. Run **Build Skill Manage Pages**. The log lists:
   - the row prefab;
   - "List item tuned";
   - "Tag tuned";
   - "Window: title tag, 'Effect Page', 'Basic Page' and tab pages updated."
3. Run **Pale Vertica > UI > Render UI Snapshots** (or `UISnapshotRenderer.RenderAllBatch`) and open `Logs/UIDebug/`:
   - `skillmanage_1_Modifier.png`, `skillmanage_2_Effect.png`, `skillmanage_3_Basic.png` are rendered at 1920 x 1080 with the window at the same place as in each Figma frame, so you can lay them over the Figma exports;
   - the `.txt` files must report 0 errors (`UILayoutRegressionTests` fails otherwise).
4. In Play mode open the skill window and click the Effect and Basic tabs.

## 4. What it builds

| Part | Result |
|---|---|
| Title tag | `Header/Title Tag`: black tag 136 x 40, star icon, "SKILL" (Kanit Bold 24, caps) at the Figma position |
| Effect page | header "EFFECT", effect text, header "DESCRIPTION", scrolling description with a 4 px track. `SkillEffectPageView` fills it |
| Basic page | header "ATTRIBUTES", scrolling list of `Skill Attribute Row` prefabs. `SkillBasicPageView` fills it |
| Advance page | header "DETAILS", scrolling list with section titles (`Skill Attribute Section` prefab) and rows. `SkillAdvancePageView` fills it |
| Modifier placeholder | `Modifier Effect Container/.../Content/Placeholder`: a dimmed modifier box "NO SUPPORT SKILL" shown while the skill has no linked support (`SkillManageWindow._modifierPlaceholder`) |
| Filter menu | `Filter Button` gets `SkillListFilterView`; clicking it opens the context menu |
| Tabs | `TabGroup` pages: Modifier, Effect Page, Basic Page, Advance Page |
| List item | name Novecento 20 with letter spacing, damage line 16 caps #616161 (no auto-size), level number 32 |
| Tags | fixed 80 x 30 pills, 13 px apart, no content size fitter, no visible scrollbar under the tag row |

Positions come from the Figma nodes: element x/y relative to the Figma window plus 3 px, because the art (`UI_Modal_Skill_Window`, 1089 x 744) has a 3 px outer stroke that the Figma frame (1083 x 738) leaves out. Re-running the tool rebuilds only the parts in this table; the Modifier page and everything else stays as it is.

## 5. Data shown

| Where | Source |
|---|---|
| Effect text | Generated from the skill's `Effects` list: damage, heal, buffs, stun, push, pull, dash, summon. Numbers and keywords use red #E00000, timings and chances use orange #FF6A00 (colours on `SkillEffectPageView`) |
| Description | New field `Description` on the skill: `Skill.json` > `ActiveSkillJsonData.Description` > `ActiveSkill.Description` (copied in `Database.GetSynchronizedActiveSkill` and `SkillConverter`). Edit it in **Pale Vertica GameDataWorkbench > Skills** (Description box). Empty = "No description yet." |
| Basic rows | `SkillCaster.BuildPayload(instance)` vs the skill's base values: Damage, Cooldown, Mana Cost, Cast Time, Cast Delay, AOE Radius, Projectiles, Heal. Shown as `base + change` or `base − change` (change in grey); rows where both are 0 are hidden. No formula was changed |
| List item damage line | "DMG": expected damage of one output (one hit), with ATK/MATK and average crit, times the payload damage multiplier. It replaced the old DPS (damage per second over 60 s) |
| Advance rows | Casting (variable, fixed, total cast, delay, cooldown), Damage (type, element, base, each `OutputScaling` as `+bonus xmultiplier`, damage multiplier, final damage, heal), Area (AOE, cast range, projectiles, area duration and tick for continuous skills), Cost & Support (mana, linked/max support slots, skill level). Empty rows and empty sections are hidden |
| Filter menu | All Skills, Equipped, Active, Toggle, then one entry per element found in the granted skills. The current choice has a dot. The list is rebuilt through `SkillListFilterView.OnFilterChanged` |
| Tags | `Active`/`Toggle` dark #505050, element colour (Fire #FF7C43, others in `SkillManageWindow` > Skill Tag Colours), damage type and skill tags light #B7B7B7 with #505050 text. Duplicate tags (for example Physical twice) are dropped |

Pages refresh through `SkillManageWindow.OnSelectedSkillChanged` (selection and support changes) and when the tab opens.

## 6. Common mistakes

| Symptom | Cause | Fix |
|---|---|---|
| Effect or Basic tab shows an empty right side | the `TabGroup` page entries were cleared or the pages renamed | run the tool again |
| "Window hierarchy changed" in the log | `Modifier`, `SupportSkill Header`, `Modifier Effect Container/Scroll View` or `Header` were renamed or moved | rename them back, or update the paths in `SkillManagePrefabBuilder.Build` |
| A hand edit on the Effect/Basic page is gone | the tool deletes and rebuilds those two pages | change `SkillManagePrefabBuilder` instead, or stop re-running it after hand edits |
| Attribute rows have stretched tips | the row sprite lost its 9-slice border | run the tool (it re-imports the sprite with L/R 24) |
| Description always says "No description yet." | the skill has no `Description` in `Skill.json` | write it in Pale Vertica GameDataWorkbench and save |
| Digits in the damage line look shorter than the letters | the project's `Novecentosanswide-DemiBold` digits are shorter than Figma's Novecento | expected; swap the font asset if this matters |
| `UILayoutRegressionTests` fails on `skillmanage_*` | a text box is shorter than its font, or a fitter sits under a layout group | read `Logs/UIDebug/skillmanage_*.txt`; the fix hint is on each issue |
