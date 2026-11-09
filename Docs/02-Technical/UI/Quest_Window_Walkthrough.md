# Quest Window Walkthrough

The Quest window lists active and completed quests and shows the selected quest on the right. It follows the Figma frame `PV / Quest Page (White)`, node `1838-7990`.

## Where to open it

- Build: **Pale Vertica > UI > Build Quest Window**
- In game: press **L**
- Prefabs: `Assets/Prefab/Game Page/Quest Window.prefab`, `Quest Row Slot.prefab`, `Quest Objective Row.prefab`
- Art: `Assets/Art/UI/New/Quest`

## Inputs

Quests come from `QuestDatabase`, and player state comes from `GameData.Player.Quests`. Nothing in the window needs filling in by hand.

## Steps in order

1. Run the build menu once. It imports the art, builds the three prefabs and adds `QuestLog` to the window registry with the L key.
2. Press L in Play mode. The first active quest is selected.
3. Click a quest to see its objectives, description and rewards. The right side scrolls when its content is long.
4. **Track Quest** or **Untrack Quest** adds it to or removes it from the side overlay.
5. Hover a reward slot to see its name and description. Right-click it to open Item Detail. Reward slots cannot be dragged or used.
6. **Reject Quest** asks for confirmation, then removes the quest and its progress.

## Expected result

- A new quest is tracked automatically until 5 are tracked. After that, Untrack another quest first.
- A tracked quest shows the Prismatic badge at the right of its row.
- Untracked quests stay untracked after a save and load.

## Common mistakes

- L does nothing: the build menu has not been run.
- Re-running the build menu replaces all three prefabs, so make layout changes in `QuestWindowPrefabBuilder`.
