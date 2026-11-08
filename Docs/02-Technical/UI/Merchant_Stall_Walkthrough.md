# Merchant's Stall Walkthrough

The shop window from the Figma section `Merchant's Stall` (page `WockSpace`, `20 · Features`; screens `A Browse` `1786:29602`, `B Quantity` `1786:29661`, `C Item Detail` `1786:29726`, ids as of 2026-11-08). One light window: Wares list on the left, a 6 x 6 Cart grid on the right, footer with the player's credits, the cart total and **Buy all**. Built 2026-11-08.

## 1. How it plays

| Action | Result |
|---|---|
| Left-click a ware | selects it (black card with the Focus ring); the cart does not change |
| Double-click a ware | adds 1 to the cart |
| Drag a ware onto the cart | the Quantity modal opens (title = item name, [-] value [+], One / Half / Max, Cancel / Add xN) |
| Right-click a ware or a cart slot | the existing Item Information window (`ItemInformationManager`) opens with that item |
| Hover a filled cart slot | gradient ring + red remove button; click it to drop the line |
| **Buy all** | spends the credits and adds the items to the inventory in one step (greyed out when the cart is empty or too expensive) |
| Equipment | never stacks: stock 3 shows 3 separate cards, each unit takes its own cart slot, dragging adds 1 without the Quantity modal, and a unit already in the cart greys one card out |
| A ware with no stock left | Disabled card with the label `Sold out` |

Wares are sorted by rarity (Legendary first), then by their order in `Shop.json`. The badge shows the stock left (`x10`) for limited stackable wares, the label shows the price.

## 2. Parts

| Part | File |
|---|---|
| Window prefab | `Assets/Prefab/Game Page/Merchant Stall Window.prefab`, registered as **`Merchant Stall`** in `Assets/Prefab/UIWindowRegistry.asset` |
| Ware list item | `Assets/Prefab/UI/Shop/Ware Card.prefab` (`ItemCard` White + `MerchantWareSlot`) |
| Cart slot | `Assets/Prefab/UI/Shop/Cart Slot.prefab` (`CartSlotView`, nested `ItemSlot Art` at 64 px) |
| Quantity modal | `Assets/Prefab/UI/Shop/Quantity Modal.prefab` (`QuantityModalView`), nested in the window |
| Window script | `Assets/Script/UI/Shop/MerchantStallWindow.cs` (listens to `ShopSession.OnCartChanged` / `OnStockChanged` and `PlayerInventory.OnInventoryChanged`) |
| Wares list | the project's virtual scroller: `VisualizerViewFactory.AttachTo(content, Ware Card)` turns the `Wares Scroll` ScrollRect into a `GlobalVisualizerView` the first time the window opens |
| Shop data and rules | `Assets/Script/CoreSystem/Shop/` (see [Shop Data Walkthrough](../Items/Shop_Data_Walkthrough.md)) |
| Art | `Assets/Art/UI/New/Shop/` (exported from Figma into power-of-two frames) |

The views raise static events (`MerchantWareSlot.OnClicked` / `OnInfoRequested`, `CartSlotView.OnWareDropped` / `OnRemoveRequested` / `OnInfoRequested`); only the open window acts on them, and only for its own `ShopSession`.

## 3. Open it

- In game: put `MerchantShopSystem` on an NPC (next to its `Character`), set **Shop Id** to an id from `Shop.json`. Right-click the NPC, choose **Shop**. The option only shows when the shop id exists.
- Dev Tool (K) > **World**: **Spawn Random Merchant Here** puts a Shopkeeper with a random shop where the player stands (walk away, right-click it, choose **Shop**); **Open Random Shop** opens the window directly; **Add 100,000 Credit** gives money to test buying.
- From a script, data first: `MerchantFactory.OpenShop(shopDefinition)`, for example `MerchantFactory.OpenShop(ShopDatabase.Get("general_merchant"))`. A `ShopDefinition` can come from `Shop.json` or be built in code.
- On a character: `MerchantFactory.SpawnRandom(position, merchant => { })`, or `MerchantFactory.AttachShop(character, shopId)` for a `Character` you already have (the shop must be in `ShopDatabase`; use `ShopDatabase.Register` for one built in code).
- Opening the window with no shop data shows it empty.

## 4. Rebuild

1. **Pale Vertica > UI > Build ItemSlot** and **Build ItemCard** first (the stall nests both).
2. **Pale Vertica > UI > Build Merchant Stall**: rebuilds the four prefabs and adds or updates the registry entry.
3. **Pale Vertica > UI > Render UI Snapshots**: check `Logs/UIDebug/merchant_stall_a.png` (browse, first ware focused, first cart slot hovered) and `merchant_stall_b.png` (drop target + Quantity modal) and their `.txt` (0 errors). State C is the existing Item Information window and has no snapshot of its own. The sample uses `general_merchant` from `Shop.json` and 2,450,000 credits (`MerchantStallSnapshot`).

With Unity closed: `Tools/FigmaToUnity/unity_batch.sh CoreSystem.Editor.UI.MerchantStallPrefabBuilder.BuildBatch CoreSystem.Editor.UI.UISnapshotRenderer.RenderAllBatch`.

## 5. Common mistakes

- Editing the window prefab by hand: the builder replaces every child. Change `MerchantStallPrefabBuilder` (positions are Figma coordinates relative to the window's top-left).
- A ware that does not show: its `ItemId` is not in `Item.json` (the `ShopJson_LoadsAndPointsAtRealItems` test catches this).
- Expecting a separate wallet: the money is the `Xeny Credit` item (id 1000) in the inventory, the same one Refinement and Socket spend.
- Cart full: 36 different lines at most (`ShopSession.CartCapacity`); more of an item already in the cart still fits.
