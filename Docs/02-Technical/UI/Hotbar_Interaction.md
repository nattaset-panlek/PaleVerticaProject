# Unified Hotbar Interaction & Aiming System (Doc.md)

## Overview

This document outlines the architectural refactoring of the Hotbar Interaction, Combat Aiming, and Consumable Item Assignment systems in **Realm of Hero**. The refactor resolves overlapping keyboard handling between `PlayerCore` and `HotbarManager` while unifying all action executions (Skills, Consumables, Items, and future interactive systems) under a centralized Hotbar gateway.

---

## 1. Resolution of Redundant Keybinding Handling

Previously, both `PlayerCore.HandleSkillInput()` and `HotbarManager.HandleHotkeyInputs()` actively polled digit keys (`1` through `0`). This resulted in redundant executions and execution race conditions when casting skills or switching rows.

### Architecture Changes

- **`PlayerCore` Decoupled**: Direct polling of digit keys (`digit1Key` to `digit0Key`) has been completely removed from `PlayerCore.HandleSkillInput()`.
- **`HotbarManager` as Central Input Gateway**: All keyboard hotkey checks (`1-0`, `Alt+1-0`, `Ctrl+1-0`, `Shift+1-0`) are now managed exclusively by `HotbarManager.HandleHotkeyInputs()`.
- **Event Routing**: When a hotkey is pressed or a slot is clicked, `HotbarManager` calls `HotbarSlotView.ExecuteAction()` on the target slot.

---

## 2. Universal Row-Agnostic Skill Aiming

Previously, aiming mode (`TryEnterAimingMode`) required an integer index into `player.skillCaster.EquippedSkills` (which only maps to Hotbar Row 0). Skills assigned to Rows 1 through 9 either bypassed Aiming Mode (firing instantly as fallback) or failed to execute.

### The `SkillInstance` Aiming Pipeline

1. **Direct Instance Targeting**: Added `public SkillInstance AimedSkillInstance` to `SkillCaster` and `PlayerCore`.
2. **Row-Agnostic Activation**: `TryEnterAimingMode` now accepts a `SkillInstance` directly:

   ```csharp
   public void TryEnterAimingMode(SkillInstance instance, Vector3 mousePos)
   ```

   Regardless of whether a skill resides on Row 0, Row 5, an Inventory Item button, or an external UI macro, executing the skill calls `TryEnterAimingMode(instance, mousePos)`.
3. **Guaranteed Aiming First**: If `instance.SkillData.BaseTargeting.RequiresSelection` is true, the skill enters Aiming Mode, displays targeting indicators (Cone, Box, AOE, Line Pierce), and highlights `SkillSelected` across all UI slots representing that skill.
4. **Execution on Click**: Left-clicking confirms the aim and triggers `player.skillCaster.CastSkill(instance, mousePos, true)`. Right-clicking cancels aiming.

---

## 3. Unified Execution Gateway (`HotbarSlotView.ExecuteAction`)

All interactions now funnel through `HotbarSlotView.ExecuteAction()`, making the Hotbar system the primary interactive bridge for all gameplay actions.

```text
[Keybinding Press / Left-Click] -> HotbarSlotView.ExecuteAction()
                                         |
         +-------------------------------+-------------------------------+
         | (HotbarActionType.Skill)                                      | (HotbarActionType.Item)
         v                                                               v
PlayerCore.TryEnterAimingMode(SkillInstance)                Consumable.OnUse()
         |                                                               |
         v                                                               v
[Aiming Indicator / Highlight] -> Left Click -> CastSkill()     [Inventory Decrement & Refresh]
```

---

## 4. Consumable Assignment & Instant Cost Display

Any consumable item in the player's inventory can be dragged directly onto a Hotbar slot.

### Key Implementation Details

- **Inventory Dragging (`OnDrop`)**: When an item is dragged from the inventory via `ItemDragHandler` and dropped onto `HotbarSlotView`, the slot verifies if `draggedItem.Item is Consumable`.
- **Future Subcategory Filtering**: A placeholder comment (`[FUTURE SUBCATEGORY]`) is maintained in `HotbarSlotView.OnDrop` where specific consumable subcategories will be filtered or restricted in future development phases.
- **Cost Display**: While skills display their resource requirement (e.g., `10 MP`), consumable items display the word **`Instant`** in their cost UI label (`_costText`). This signifies immediate execution without MP or HP resource expenditure.
- **Automatic Exhaustion Cleanup**: When an item is used via `ExecuteAction()` or when `RefreshUI()` evaluates inventory state, if all instances of the item are exhausted (`Quantity <= 0` or missing from inventory), the slot calls `ClearSlot()`. This resets the slot to `Empty`, removing all icons and text.

---

## 5. Expanding for Future Systems

To integrate new game mechanics (e.g., Mounts, Emotes, Pet Commands, or Gadgets) into the Hotbar:

1. **Add Action Type**: Extend `HotbarActionType` in `HotbarSlotData.cs`:

   ```csharp
   public enum HotbarActionType
   {
       Empty = 0,
       Skill = 1,
       Item = 2,
       Custom = 3,
       Mount = 4, // New System
       PetCommand = 5
   }
   ```

2. **Handle in Gateway**: Add a switch case in `HotbarSlotView.ExecuteAction()`:

   ```csharp
   case HotbarActionType.Mount:
       MountManager.Instance.ToggleMount(_slotData.CustomActionId);
       break;
   ```

3. **UI Refresh**: Update `HotbarSlotView.RefreshUI()` and `UpdateTimersAndSelection()` to bind icons, cooldown overlays, and active toggle highlights for the new action type.

---

## 6. Assigning New Systems to Hotbar (Drag & Drop and Context Menu)

When implementing a new interactive system (such as Mounts, Emotes, or Pet Commands), the UI icons representing those features must be assignable to the Hotbar. Developers can implement assignment through **Drag-and-Drop** (via `GlobalDragManager`) and **Context Menu** workflows.

### A. Drag-and-Drop Assignment (via `GlobalDragManager`)

*(For comprehensive architectural details and general system integration, see [GlobalDrag.md](./GlobalDrag.md)).*

To enable dragging an icon from a custom UI window directly onto any Hotbar slot:

1. **Use `GlobalDragManager` on Source UI**: Implement `IBeginDragHandler`, `IDragHandler`, and `IEndDragHandler` on the source UI component, delegating visual creation and position tracking to `GlobalDragManager`:

   ```csharp
   using CoreSystem.UI.Utility;

   public class MountSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
   {
       [SerializeField] private MountData _mountData;
       [SerializeField] private Sprite _icon;

       public void OnBeginDrag(PointerEventData eventData)
       {
           if (eventData.button != PointerEventData.InputButton.Left) return;
           
           GlobalDragManager.Instance.StartDrag(
               source: this,
               payload: _mountData,
               icon: _icon,
               screenPos: eventData.position
           );
       }

       public void OnDrag(PointerEventData eventData)
       {
           GlobalDragManager.Instance.UpdatePosition(eventData.position);
       }

       public void OnEndDrag(PointerEventData eventData)
       {
           GlobalDragManager.Instance.FinishDrag();
       }
   }
   ```

2. **Intercept Drop in `HotbarSlotView.OnDrop`**: Extend `HotbarSlotView.OnDrop(PointerEventData eventData)` to inspect `GlobalDragManager.Payload`:

   ```csharp
   if (GlobalDragManager.Payload is MountData mountData)
   {
       AssignMountAction(mountData.Id);
       return;
   }
   ```

### B. Context Menu Assignment (Right-Click)

To provide fast assignment without requiring manual dragging across the screen, implement right-click context menu options on the source UI icon:

1. **Implement `IPointerClickHandler` on Source UI**: Listen for right-click events on the feature's icon:

   ```csharp
   public class MountSlotUI : MonoBehaviour, IPointerClickHandler
   {
       public void OnPointerClick(PointerEventData eventData)
       {
           if (eventData.button == PointerEventData.InputButton.Right)
           {
               OpenAssignmentMenu(eventData.position);
           }
       }
   }
   ```

2. **Generate Menu Options via `ContextMenuManager`**: Query `HotbarManager.Instance.ActiveRows` to generate assignment options dynamically:

   ```csharp
   private void OpenAssignmentMenu(Vector2 screenPos)
   {
       List<ContextMenuItem> items = new List<ContextMenuItem>();
       var hotbarMgr = HotbarManager.Instance;

       if (hotbarMgr != null)
       {
           for (int i = 0; i < hotbarMgr.ActiveRows.Count; i++)
           {
               int rowIndex = i;
               items.Add(new ContextMenuItem($"Assign to Hotbar {rowIndex + 1} (Slot 1)", () =>
               {
                   var targetSlot = hotbarMgr.ActiveRows[rowIndex].GetSlot(0);
                   if (targetSlot != null)
                   {
                       targetSlot.AssignCustomAction(_mountId);
                   }
               }));
           }
       }

       ContextMenuManager.Instance.Show(items, screenPos);
   }
   ```
