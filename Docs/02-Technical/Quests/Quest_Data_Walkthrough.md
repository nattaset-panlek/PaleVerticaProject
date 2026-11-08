# Quest Data Walkthrough

Quests are defined in JSON. Each player's progress lives in `GameData.Player.Quests` and is saved with the character.

This covers data, state, objective tracking, accepting and completing in dialogue, the Quest Log and the on-screen tracker.

## Where to edit

**Window > Pale Vertica > Game Data Workbench**, **Quests** tab. It saves to `Assets/Resources/Data/Quest.json`.

NPCs, items, scenes and required quests are picked from lists. Problems such as a missing target or an NPC with no Dialogue Id appear as warnings at the top of the quest.

## Inputs

| Field | Meaning |
|---|---|
| `Id` | Unique quest id |
| `Name`, `Description` | `EN` and `TH` text |
| `GiverCharacterId` | NPC that offers the quest |
| `TurnInCharacterId` | NPC that completes the quest |
| `RequiresQuestIds` | Quests that must all be completed first. Empty means none |
| `Objectives[].Type` | `0` Kill, `1` Collect, `2` Talk, `3` ReachMap |
| `Objectives[].TargetId` | Character id for Kill and Talk, item id for Collect |
| `Objectives[].TargetScene` | Scene name for ReachMap |
| `Objectives[].Required` | Count needed |
| `Objectives[].Text` | Line shown to the player |
| `Reward.BaseExp`, `Reward.JobExp` | EXP given on completion |
| `Reward.Items[]` | `ItemId` and `Quantity`. Money is a currency item, such as 1003 Copper Coin |

## Add a quest

1. In the Quests tab, add a new entry to the list.
2. Set the name, description, giver and turn-in NPC.
3. Press **+ Objective** and choose its type, target and required count.
4. Add EXP and reward items. To chain it, set **Requires Quest**.
5. Fix any warnings, then save the Workbench.

## Expected result

A quest moves through `Locked`, `Available`, `Active`, `ReadyToTurnIn` and `Completed`. It can be taken only once.

## How objectives count

`QuestManager` updates active quests by itself:

| Type | Counts when |
|---|---|
| Kill | A monster with that character id dies and the player dealt damage to it |
| Collect | The inventory holds that item. The count goes down if the item leaves the inventory |
| Talk | The player opens that NPC's dialogue. The NPC needs a `DialogueId` |
| ReachMap | That scene loads, or the player is already in it |

## Accept and complete

1. Click the giver NPC and choose **Talk**. Offered quests appear as buttons above the conversation choices.
2. Click a quest to see its objectives and rewards, then press **Accept**.
3. When the objectives are done, Talk to the turn-in NPC, open the quest and press **Complete**.

On completion the player receives the EXP and items. Items required by Collect objectives are taken from the inventory.

## Quest Log and tracker

Run **Pale Vertica > Quests > Build Quest Tracker** once. It creates `Prefab/Game Page/Quest Tracker Window.prefab` and registers `QuestTracker` in `UIWindowRegistry`.

- Press **L** to open or close the Quest Log. Esc also closes it.
- The tracker at the top right lists up to 5 active quests and updates by itself.

## Story quests

- **Scenario order:** add every earlier quest under **Requires Quests**. Quest 4 with quests 2 and 3 listed stays locked until both are completed.
- **Objectives In Order:** only the current objective counts, and the Quest Log shows finished steps plus the current one. Example: talk, kill, talk, collect, kill, talk.
- **Quest Dialogue on a Talk objective:** the NPC shows a quest button that plays that dialogue. The objective finishes when the player reaches a node with no choices and presses Continue. A choice with `Next` of `-1` leaves without finishing, so wrong answers can send the player away or back to another node.
- **Accept Dialogue / Turn-in Dialogue:** plays before the Accept or Complete page.
- Quest 3, "The Old Road", in `Quest.json` is a working example.

## Use it from code

```csharp
QuestModel quest = QuestDatabase.Get(1);
GameData.Player.Quests.Accept(quest);
GameData.Player.Quests.Report(QuestObjectiveType.Kill, 1001);
QuestManager.TurnIn(quest);
```

## Common mistakes

- A required quest that does not exist, or a loop of required quests: the quest stays `Locked`. The Workbench warns about both.
- `Type` is a number if you edit the JSON by hand.
- `Required` of 0 makes the objective finished as soon as the quest is accepted.
- A Talk objective on an NPC with `DialogueId` 0 can never finish.
- Forgetting the Build Quest Tracker menu: a warning is logged and no tracker appears. The Quest Log still works.
- Calling `PlayerQuests.Complete` directly gives no rewards. Use `QuestManager.TurnIn`.
- A giver or turn-in NPC with `DialogueId` 0 has no Talk option, so it cannot offer the quest.
