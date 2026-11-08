# Item Insert Target Walkthrough

Shared core for windows that have slots an item is put into: Refinement (center equipment slot) and Socket (center equipment slot + 4 fragment slots). It covers both ways to put an item in:

- **Drag and drop:** drag an item from the inventory onto the slot.
- **Right-click "Insert":** while such a window is open, right-clicking an item slot in the inventory adds an entry such as **Insert to Refine** at the top of the menu.

Added 2026-10-20.

## 1. Pieces

| Type | File | Role |
|---|---|---|
| `IItemInsertTarget` | `Assets/Script/UI/Utility/IItemInsertTarget.cs` | Implemented by a window. `CanInsert(item, slotIndex)`, `TryInsert(item, slotIndex)`, `ClearInsert(slotIndex)`, `GetInsertLabel(item)` |
| `ItemInsertRegistry` | `Assets/Script/UI/Utility/ItemInsertRegistry.cs` | Static list of the open targets. `Register` / `Unregister`, `GetTargetsFor(item)`, `BuildContextMenuItems(item)`, `OnTargetsChanged`, `AnySlot = -1` |
| `ItemInsertSlot` | `Assets/Script/UI/Utility/ItemInsertSlot.cs` | Put on a slot GameObject. On drop it calls `TryInsert(item, slotIndex)`; on right-click it calls `ClearInsert(slotIndex)`. Also `ResolveDraggedItem()` / `ResolveItem(payload)` |
| Context menu hook | `ItemSlotGlobal.GetDefaultContextMenuItems()` | Starts the list with `ItemInsertRegistry.BuildContextMenuItems(item)` |

The item slot never references Refinement or Socket. It only asks the registry "who accepts this item?", and windows add or remove themselves when they are enabled or disabled. This follows the project rules: the dependency points one way (to an interface), there is no runtime Find, and the registry is the only shared state.

`slotIndex` values: `ItemInsertRegistry.AnySlot` (-1) means "wherever it fits" (used by the context menu). Each window defines its own other values (Socket: `CenterSlot` = -2, sockets 0 to 3).

## 2. Behaviour in the two windows

### Refinement (`RefinementWindow`)

- Opening the window always starts empty ("SELECT EQUIPMENT"). It no longer picks the equipped Talisman, weapon or first inventory item. `UIManager.Open("Refinement", equipment)` still opens it with that item.
- Accepts any `EquipmentInstance` (drop on the center slot or **Insert to Refine**). Refuses while the refine loading bar runs.
- Right-click the center slot clears it.

### Socket (`SocketWindow`)

- Opening starts empty as well.
- **Center slot:** accepts equipment only (drop or **Insert to Socket**). Changing the equipment drops any staged fragment.
- **Socket slots:** accept a `Fragment` only, and only when equipment is set and it has a free unlocked socket. The fragment is **staged**: it shows at the next free socket, dimmed, with the highlight. The menu entry is **Insert Fragment**.
- **Staged look:** the target socket gets a #FAFF00 outline in the socket's own shape (`Active Highlight`, `UI_Fragment_Slot_Highlight.png`, 102 x 102), and the fragment icon shows black at 55 % on the backing circle. Socketed fragments show black at full opacity.
- **Infuse Socket** is enabled when: equipment set + fragment staged + fragment still in the inventory + free socket + 5,000 credits. Clicking it shows the global loading bar (`ActionProgressWindow`, 0.5 s, type `SOCKET`); when the bar completes it spends the credits and calls `PlayerEquipment.SocketFragment`. If socketing fails, the credits are refunded. While the bar runs, inserts and clears are refused.
- Right-click the staged socket (or the center) clears it.
- Unlocked sockets = `Equipment.MaxSockets`, at most 4. **0 means no sockets** (before, 0 was treated as 4, so Infuse spent 5,000 credits and then `SocketFragment` refused).
- The window listens to `PlayerInventory.OnInventoryChanged`, so credits and the staged fragment stay current.

Fragments always fill the list in order (`SocketedFragments` is a list), so a fragment dropped on socket 3 while socket 2 is empty goes into socket 2.

## 3. Add insert support to a new window

1. Make the window implement `IItemInsertTarget`.
2. In `OnEnable` call `ItemInsertRegistry.Register(this)`; in `OnDisable` call `ItemInsertRegistry.Unregister(this)`.
3. In `OnOpen(data)` reset the slots (start empty unless `data` is the item).
4. `CanInsert`: type-check the item and the slot index. For `AnySlot`, return true if it fits anywhere. This decides whether the menu entry shows, so keep it cheap and free of side effects.
5. `TryInsert`: call `CanInsert` again, then store the item and refresh the view.
6. `GetInsertLabel`: short verb + window ("Insert to Refine").
7. Put an `ItemInsertSlot` on each slot through the window's prefab builder: `slotObj.AddComponent<ItemInsertSlot>().Setup(windowComp, index)`. Do not add it at runtime.

## 4. Expected result

- Open Refinement or Socket: the center slot is empty.
- With the inventory open, right-click an equipment item: the first menu entry is **Insert to Refine** or **Insert to Socket**. Choosing it fills the center slot.
- In Socket with equipment set, right-click a fragment: **Insert Fragment** stages it; Infuse becomes clickable if you have 5,000 credits.
- Close the windows: the Insert entries disappear from the menu.
- Tests: `ItemInsertRegistryTests` (EditMode).

## 5. Common mistakes

| Mistake | Result | Fix |
|---|---|---|
| Registering in `Awake` or `OnOpen` only | The entry stays in the menu after the window closes | Register in `OnEnable`, unregister in `OnDisable` |
| `CanInsert` changes state | Right-clicking an item changes the window | Keep `CanInsert` pure; change state only in `TryInsert` |
| `ItemInsertSlot` without a target on a prefab outside the window | Drops do nothing | `Setup(window, index)` in the builder, or keep the slot under the window so `GetComponentInParent` finds it |
| Socket prefab not rebuilt after this change | Socket slots ignore drops (no `ItemInsertSlot` on them) | Run **Pale Vertica > UI > Build Refinement and Socket Windows** or the `BuildSocketBatch` batch method |
| Expecting an item with `MaxSockets` 0 to have sockets | All sockets locked, Insert Fragment does not show | Set Max Sockets in the Pale Vertica GameDataWorkbench |
