# NPC Dialogue Walkthrough

NPCs with a dialogue get a **Talk** option. The conversation opens in a Global Modal.

## Where to edit

- Lines: `Assets/Resources/Data/Dialogue.json`
- Which NPC says what: `DialogueId` on the character, in **Window > Pale Vertica > Game Data Workbench**, Characters tab, or in `Character.json`. `0` means no Talk option.

## Inputs

| Field | Meaning |
|---|---|
| `Dialogues[].Id` | The id that `DialogueId` points to |
| `StartNode` | First node shown |
| `Nodes[].Text` | `EN` and `TH` text |
| `Choices[].Text` | Button label |
| `Choices[].Next` | Node to go to. `-1` ends the conversation |

## Add a conversation

1. Add an entry with a new `Id` to `Dialogue.json`.
2. Set that `Id` as the NPC's `DialogueId`.
3. Enter Play mode and click the NPC.

## Expected result

- The character walks into range (1.5 units) and the modal opens with the NPC's name as the title.
- Each choice moves to its next node.
- A node with no choices shows a Close button.
- An NPC with other options (Shop, Refine) shows a menu that includes Talk.

## Common mistakes

- `DialogueId` left at 0: no Talk option.
- `StartNode` that matches no node: the modal does not open.
- A `Next` that matches no node ends the conversation.
