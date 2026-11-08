# ItemSlot and ItemCard Walkthrough

The square **ItemSlot** (Diagonal Split, 128 px) and the horizontal **ItemCard / Classic Strip** from the Figma section `Shop Components` (file `zZ5QnVfPMSFuiMFjpIGbOC`, page `WockSpace`, section `00 · Foundations` > `Shop Components`). Every window that shows an item uses these two pieces. Built 2026-11-08.

## 1. What is where

| Piece | Prefab | Script | Used by |
|---|---|---|---|
| ItemSlot art (display only) | `Assets/Prefab/UI/ItemSlot Art.prefab` | `UI/Inventory/Utility/ItemSlotArtView.cs` | nested in every slot below |
| Inventory square slot | `Assets/Prefab/UI/ItemSlot.prefab` (same GUID as before) | `ItemSlotSquare` (behaviour from `ItemSlotGlobal`: drag, drop, right-click menu, tooltip) | Inventory grid (`InventoryWindow`, GlobalVisualizer) |
| ItemCard (display only) | `Assets/Prefab/UI/ItemCard.prefab` | `ItemCardView.cs` (theme White / Dark, states Default / Hover / Focus / Pressed / Disabled) | Inventory equipment list, Merchant's Stall wares |
| Inventory wide row | `Assets/Prefab/UI/ItemSlotWide.prefab` (same GUID as before) | `ItemSlotWide` | Inventory equipment list |
| Equipment / Costume slots | `Character Equipment Window.prefab` | `EquipmentSlotUI`, `CostumeSlotUI` (`_art`, `_emptyIcon`) | Character window |
| Refinement / Socket item slot | `Refinement Window.prefab`, `Socket Window.prefab` | `RefinementCenterSlot._art`, `SocketWindow._itemArt` | Refine and Socket NPC windows |
| Cart slot (64 px) | `Assets/Prefab/UI/Shop/Cart Slot.prefab` | `CartSlotView` | Merchant's Stall |

Data goes through two structs, so views never read game objects directly:
- `ItemSlotDisplay.From(ItemInstance)` / `FromItem(Item, quantity)`: kind (Empty / Instance / Stack), icon, rarity, sockets, stars, advancement, quantity, equipped.
- `ItemCardDisplay.From(ItemInstance)`: name, meta (item type), stars, sockets, badge text, label text.

## 2. Data rules

| On the slot | Comes from |
|---|---|
| Rarity wedge colour (bottom right) | `ItemRarityExtensions.GetColor` (the game colours, not the Figma swatches) |
| Stars (5, top right) | `EquipmentInstance.Refine`: one star per 4 levels, rounded up (`ItemSlotDisplay.StarsForRefine`, Refine 1-4 = 1 star, 17-20 = 5) |
| `+N` (bottom left) | `EquipmentInstance.Reforge` (the Advancement number), hidden at 0 |
| `xN` (bottom left) | quantity of anything that is not Equipment, hidden at 1 |
| Socket diamonds (top left) | `Equipment.MaxSockets` (max 4) and `SocketedFragments.Count` |
| White border | the item is equipped (Inventory only; Equipment window slots never show it) |

Card states: Hover, Focus (also used as Selected) and Pressed always use the black body, even on the White theme; Default and Disabled follow the theme. Every state change fades in 0.15 s (`ItemCardView.FadeDuration`, unscaled time); a recycled card showing a different item snaps instead.

The ItemCard uses the same data: badge = `+N` or `xN`, label = `Equipped` in the inventory, price or `Sold out` in the shop.

## 3. Rebuild the prefabs

1. Change art in `Assets/Art/UI/New/ItemSlot/` or `Assets/Art/UI/New/ItemCard/` (exported from Figma at power-of-two sizes; see section 6).
2. **Pale Vertica > UI > Build ItemSlot** rebuilds `ItemSlot Art.prefab` and the children of `ItemSlot.prefab`.
3. **Pale Vertica > UI > Build ItemCard** rebuilds `ItemCard.prefab` and the children of `ItemSlotWide.prefab`.
4. **Pale Vertica > UI > Apply ItemSlot To Windows** puts the ItemSlot art into the Equipment, Refinement and Socket windows (run it again after `Build Refinement and Socket Windows`).
5. **Pale Vertica > UI > Render UI Snapshots**, then open `Logs/UIDebug/itemslot_sheet.png` and `itemcard_sheet.png`.

With Unity closed: `Tools/FigmaToUnity/unity_batch.sh CoreSystem.Editor.UI.ItemSlotPrefabBuilder.BuildBatch CoreSystem.Editor.UI.ItemCardPrefabBuilder.BuildBatch CoreSystem.Editor.UI.ItemSlotAdoptionBuilder.BuildBatch CoreSystem.Editor.UI.UISnapshotRenderer.RenderAllBatch`.

Expected result: the roots, GUIDs and scripts of `ItemSlot.prefab` / `ItemSlotWide.prefab` stay, so `InventoryManager.prefab` keeps working; only the children are new.

## 4. Use the slot in a new window

- Prefab built by an editor script: `ItemSlotPrefabBuilder.AddArt(parent)` nests `ItemSlot Art` and stretches it over the parent. The art is drawn at 128 and scaled to the parent size by `ItemSlotArtView.Fit` (80 px in the inventory, 64 px in the cart).
- Code: keep a `[SerializeField] ItemSlotArtView` and call `Show(ItemSlotDisplay.From(instance))`, or `Clear()` for an empty slot.
- Cards: `ItemCardPrefabBuilder.AddCard(parent, ItemCardTheme.White)`, then `card.Show(display)`; `SetFocused`, `SetInteractable` drive the Focus and Disabled looks. Hover and Pressed come from the pointer.

## 5. Common mistakes

- Editing `ItemSlot.prefab` children by hand: the next **Build ItemSlot** removes them. Change the builder instead.
- Expecting the Figma rarity colours (gold Legendary, blue Rare): the game has seven rarities with its own colours in `ItemRarityExtensions`; change them there if the design should win.
- Using Novecento with UpperCase for `x999`: the `x` is the font's small-cap lowercase on purpose, at 16 px.
- Forgetting **Apply ItemSlot To Windows** after rebuilding the Refinement / Socket windows: they go back to the old icon.

## 6. Figma sources (look up by name; ids as of 2026-11-08)

`ItemSlot` set `1786:17476`, `ItemSlot / Refine` `1786:17020`, `ItemSlot / Sockets` `1786:17078`, `ItemCard / Classic Strip` `1784:111889`, `Cart Slot v2` `1786:17573`. The sprites were exported with Talk to Figma `run_script` (clone the variant, hide the parts that Unity draws itself, export into a power-of-two frame): `UI_ItemSlot_Frame`, `_RarityWedge` (white, tinted in Unity), `_Border`, `_SocketFilled` / `_SocketEmpty` (32 px), `UI_ItemCard_Body_White` / `_Dark`, `_Ring_Hover` / `_Focus` / `_Pressed` (512 x 128, card at 8, 28), `_IconFrame`, `_Socket`, `_Stripe`, `_Pill4`, `_Pill4_Outline`, `_Pill6`.
