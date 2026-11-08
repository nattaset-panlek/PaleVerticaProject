# Cursor and UI Click Walkthrough

How the mouse cursor picks its look, and how clicks on UI are kept from reaching the game world.

## Where

| What | Path |
| --- | --- |
| Cursor logic | `Assets/Script/UI/CursorManager.cs` (scene object `CursorManager` in `SandBox.unity`) |
| Cursor sprites | `CursorManager > Cursor States` list in the scene, one sprite + hotspot per `CursorType` |
| UI hit test | `UIManager.IsPointerOverUI()` in `Assets/Script/CoreSystem/UI/Manager/UIManager.cs` |
| World click input | `PlayerCore.HandleSkillInput()` in `Assets/Script/CoreSystem/Player/PlayerCore.cs` |

## Cursor rules (checked every frame, first match wins)

1. A forced cursor set by code (`CursorManager.Instance.SetCursor(type)`, cleared with `ResetCursor()`), for example the modal resize handle.
2. Pointer over UI: resize handle cursor on resize handles, **Interact** on buttons and other selectables, otherwise **Default**.
3. Pointer over a world collider on the attackable or interactable layers:
   - **Dialogue** when the character has an interaction component (any `MonoBehaviour` implementing `IInteractionProvider`, such as `RefinementMerchantSystem`) or is tagged `Merchant` or `Villager`;
   - else **Attack** on the attackable layer, **Interact** on the interactable layer.
4. Otherwise **Default**.

## UI click rule

`PlayerCore` reads left click (Attack action) and right click once per frame. If either was pressed while `UIManager.IsPointerOverUI()` is true (an EventSystem raycast hits any UI element with Raycast Target), the click is ignored by the world: no attack target, no move, no skill cast, no right-click menu, and skill aiming is not cancelled. The UI element still receives the click.

## Left click on an NPC you can interact with

`PlayerCore` asks `PlayerInteraction.TryPrimaryInteract` before choosing an attack target. It collects the character's options (`InteractionCore.GetOptions`) and drops the core ones (ids starting with `core.`: Inspect, Attack, Follow Me, Stop Following):

- **no interaction**: the click works as before (hostile = attack target, others = nothing);
- **one interaction** (for example Refine): it runs at once, the player walks into range and the window opens;
- **two or more**: a context menu with only those interactions opens at the mouse.

The click never starts an attack on such an NPC. Right click still opens the full menu, Inspect included. Tests: `PrimaryInteractTests`.

## Steps

### A. Make an NPC show the Dialogue cursor
1. Give it an interaction component (see [Blacksmith NPC Walkthrough](../Character/Blacksmith_NPC_Walkthrough.md)), or set its Character **Tags** to Merchant or Villager.
2. Keep its collider on a layer included in `CursorManager > Attackable Layer` or `Interactable Layer`.
3. Play and hover it: the Dialogue cursor appears.

### B. Change a cursor image
1. Select `CursorManager` in the scene.
2. In **Cursor States**, find the entry with the type (Dialogue = 8) and set its sprite and hotspot.
3. The sprite texture must have **Read/Write Enabled**.

### C. Make a UI panel let clicks through to the world
Turn off **Raycast Target** on its Images/Texts (or add a `CanvasGroup` with Blocks Raycasts off). Anything that receives raycasts blocks world clicks.

## Expected result

- Clicking a hotbar button, window or modal that sits over an enemy never starts walking to or attacking that enemy.
- Hovering an NPC you can talk to or trade with shows the Dialogue cursor instead of the sword.

## Common mistakes

- **World clicks never work**: a full-screen UI element with Raycast Target on (often an invisible background Image) is covering the screen. Turn its Raycast Target off.
- **Dialogue cursor shows the default arrow**: the Dialogue entry in Cursor States has no sprite, or the texture is not readable (the Console logs this).
- **Hostile NPC with an interaction component shows Dialogue**: Dialogue wins over Attack by design; remove the component or the tag if it should look attackable.
