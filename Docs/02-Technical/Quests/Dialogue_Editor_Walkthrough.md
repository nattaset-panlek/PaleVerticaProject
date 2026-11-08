# Dialogue Editor Walkthrough

Write NPC conversations and quest dialogues in the Workbench. It saves to `Assets/Resources/Data/Dialogue.json`.

## Where to open it

**Window > Pale Vertica > Game Data Workbench**, **Dialogues** tab.

## Inputs

| Field | Meaning |
|---|---|
| `Start Node` | The first line played |
| `NPC Says (EN/TH)` | The line the NPC speaks |
| `Answer (EN/TH)` | A button the player can press |
| `Goes To` | The node after that answer, or **Leave** |

## Write a dialogue

1. Add an entry to the list, or press **Create New** next to any dialogue field in the Quests tab.
2. Write the first line in Node 0.
3. Press **+ Node** for each further line.
4. Press **+ Answer** on a node to add a reply, then choose **Goes To**.
5. Leave the last node with no answers.
6. Use **Preview** to play it, then save the Workbench.

## Expected result

- A node with no answers shows **Continue** and ends the conversation.
- On a quest, the objective finishes only at such a node.
- An answer set to **Leave** closes without finishing, and the player can try again.
- For a wrong answer, go to a node that says so, with an answer leading back to the question.

## Use it

- NPC small talk: set `Dialogue Id` in the Characters tab.
- Quest: set `Accept Dialogue`, `Turn-in Dialogue` or a Talk objective's `Quest Dialogue` in the Quests tab.

## Common mistakes

- Warnings at the top of a dialogue mean a broken path, an unreachable node, or no ending. A dialogue with no ending can never finish a quest.
- Removing a node that answers still point to: the answers show `Missing`. Choose a new target.
- Removing a dialogue that a quest uses: the Quests tab warns about it.
