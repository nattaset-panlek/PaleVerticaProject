# Game Flow Map for Scenarios

What a player has to do to reach each feature, and which scenario action does it. Read this before writing a scenario that goes past combat. Facts were read from the code on 2026-11-10; if a step stops working, check the file named in the row.

## Start of a game

| Step | What the game does | Scenario action |
|---|---|---|
| Title | Buttons "Continue" (needs save slot 0), "New Game", "Exit" (`UI/TitleMenu/TitleMenuView.cs`) | `StartNewGame` (goes back to Title first if needed) |
| New game | Player reset, Town loads, spawn `start` at (0, -2). No name entry, intro or tutorial | part of `StartNewGame` |
| Job level 0 to 1 | The "ClassSelect" modal opens by itself (`UI/ClassSelect/ClassSelectGate.cs`) | `Level` with `job`, then `ChooseClass` |
| Class | 4 cards: Warrior, Assassin, Mage, Archer. Card, then "Confirm". The Passive Tree opens after | `ChooseClass` (closes the tree again) |
| Hotbar | Tackle (1001) is in skill slot 0 but the hotbar slot looks empty, so its key does nothing | `AssignSkill` skill 1001 slot 1 |
| Stat points | Base level-up gives points; spent with "+" in the Attributes window (C) | `SpendStats` |

The ready-made path `Tests/Scenarios/setup/new_warrior_lv15.json` does all of the above. Start a scenario with `"path": ["setup/new_warrior_lv15.json"]`.

## Windows and keys

Keys come from `Assets/Prefab/UIWindowRegistry.asset` and can be rebound; the actions read the current binding through `KeybindSetting`.

| Window | Key | Action |
|---|---|---|
| CharacterSkillManage (Skills) | J | `OpenWindow` / `CloseWindow` |
| Inventory | E | same |
| CharacterStatus | U | same |
| CharacterEquipment | Q | same |
| CharacterSkillTree (Passive Tree, redirects to ClassSelect without a class) | P | `ChooseClass` |
| Refinement | B | same |
| Socket | N | same |
| QuestLog | L | same |
| Attributes | C (raw key, global modal) | `Key` |

Window keys are ignored while Alt, Ctrl or Shift is held. Escape closes the top modal, never a plain window; close windows with their own key.

## System Menu

Escape with no modal open toggles it (`UI/SystemMenu/SystemMenuWindow.cs`). Items: Character, Equipment, Inventory, Skills, Passive Tree, Keybind, Settings, Save Game, Load Game, Return to Title, Exit Game. Load, Return and Exit ask first: "Cancel" or "Load" / "Return" / "Exit".

Action: `{"do":"Menu","item":"Return to Title","confirm":"Return"}`. Never use "Exit Game": it stops Play mode.

## Clicking and dragging in the UI

Targets for `ClickOn` and `Drag`:

| Target | Finds |
|---|---|
| `{"label":"Equip"}` | a visible button by its text (context menus, dialogs) |
| `{"item":4001}` | the inventory slot holding that item id |
| `{"skill":1001}` | the skill row in the Skills window |
| `{"hotbar":1,"row":0}` | hotbar slot 1 (1-based) |
| `{"equip":"Helm"}` | the equipment slot (Helm, Armor, Glove, WeaponOneHandLeft, WeaponOneHandRight, AccessoryLeft, AccessoryRight, Talisman) |
| `{"name":"..."}` | any UI object by GameObject name |

Rules the game enforces:

- An item or skill drag starts only after the pointer moves 10 px, and that first move must be mostly sideways; a vertical start scrolls the list instead. `Drag` always slides 24 px sideways first.
- A drop lands on whatever drop handler is under the pointer when the button is released. Dropping a hotbar slot on nothing does not clear it.
- Context menus stay open until one of their buttons is clicked.
- World clicks are ignored while the pointer is over UI.

| Goal | Ways the game accepts | Action |
|---|---|---|
| Skill to hotbar | right-click the row, "Slot #N ..."; or drag row to a hotbar slot | `AssignSkill` (`"how":"menu"` or `"drag"`) |
| Equip an item | double-click it; right-click, "Equip"; or drag to the equipment slot (Q open) | `EquipItem` (`"how":"double"`, `"menu"`, `"drag"` with `"slot"`) |
| Unequip | double-click the equipped item; double-click the equipment slot, "Unequip"; drag it onto an inventory slot | `ClickOn` with `"double":true`, then `ClickUI` "Unequip" |
| Consumable to hotbar | drag from inventory to a hotbar slot | `Drag` |
| Use an item | double-click; right-click, "Use" | `ClickOn` |
| Learn a skill | use a Skill Rune item (no skill points for active skills) | `ClickOn` the rune with `"double":true` |

## Combat, death and travel

- Clicking a hostile starts auto-attack: the player walks into range and swings. Clicking a portal walks to it and enters.
- Death opens "Death Screen": "Revive" restores HP in the same place (MP is not restored). "Town" is always disabled.
- `Kill` presses Revive and keeps fighting until the count is reached or the step timeout ends. Deaths go to the `deaths` metric. Give long hunts a `"timeout"` (seconds).
- Maps: Town portal at (25, 0.4) to Field `field_gate` (-23, 1); Field portal at (-27, 0.6) to Town `town_gate` (21, 0.2). Action: `UsePortal`.

## NPCs

Left-click an NPC to talk. NPCs with more than one option (the Alchemist: Talk, Socket) open a menu first; `TalkTo` picks "Talk". Dialogue buttons: "Continue", choice texts, quest names, "Accept", "Complete", "Back", "Close".
