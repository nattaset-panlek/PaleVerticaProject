# Event System Walkthrough

An event is "when this happens, do these things". Events are data, and their progress is saved with the player.

## Where to open it

**Window > Pale Vertica > Game Data Workbench**, **Events** tab. It saves to `Assets/Resources/Data/Event.json`.

## Inputs

| Trigger | Needs |
|---|---|
| `EnterZone` | Zone Id |
| `QuestAccepted`, `QuestCompleted` | Quest |
| `QuestObjectiveDone` | Quest and objective number, counted from 0 |
| `KillCount` | Monster and count. The player must have damaged each one |
| `DialogueEnded` | Dialogue. Fires when it reaches a node with no answers |

| Action | Needs |
|---|---|
| `PlayDialogue` | Dialogue and speaker |
| `SpawnMonster` | Monster, count and Spawn Point Id |
| `GiveReward` | Item and quantity, and/or EXP |
| `SetObjectActive` | Object Id and show or hide |

`Only Once` means once per save file.

## Set up the scene

- **Zone:** add a GameObject with a `Collider2D` and `EventZone`, then set `Zone Id`.
- **Spawn point:** add `MapSpawnPoint` and set its `Id`.
- **Show or hide an object:** add `EventObject` to an object that stays active, set `Object Id`, and drag the door or gate into `Target`. The Target can start hidden.

## Steps in order

1. Add an event in the Events tab.
2. Choose the trigger and fill in its fields.
3. Press **+ Action** for each result.
4. Fix any warnings and save the Workbench.
5. Add the scene pieces it names.

## Expected result

- The actions run the frame after the trigger.
- Events and object states stay after save and load.
- Event 1 in `Event.json` is an example: accepting quest 3 gives a Minor Health Potion.

## Common mistakes

- `EventObject` placed on the object it hides: use a separate object that stays active.
- A Zone Id or Spawn Point Id that matches nothing: IDs in scenes are typed by hand, so check the spelling. A missing spawn point logs a warning.
- A zone with `Is Trigger` off: the player bumps it and nothing fires.
