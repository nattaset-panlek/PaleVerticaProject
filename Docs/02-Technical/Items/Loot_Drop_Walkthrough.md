# Loot Drop Walkthrough

Monsters drop items on the ground when they die. Each drop shows a name label; press Z to show or hide all labels. Clicking a label picks the item up: within 1 unit it is immediate, otherwise the character walks a found path to it.

## Where to edit

- Drop tables: **Window > Pale Vertica > Game Data Workbench**, **Drop Tables** tab. It saves to `Assets/Resources/Data/DropTable.json`.
- Which monster uses which table: **Drop Table** in the Characters tab. `None` means no drops.

## First-time setup

Run **Pale Vertica > Loot > Build Loot Visuals** once. It creates `Resources/Loot/LootVisualSettings.asset`, `Prefab/Loot/Ground Item Default.prefab` (placeholder dot), `Prefab/Game Page/Loot Label Window.prefab`, and the `LootLabels` entry (key Z) in `UIWindowRegistry`. Running it again rebuilds the window prefab only.

## Choose what a drop looks like

Open `Resources/Loot/LootVisualSettings.asset`:
- **Default Prefab**: shown on the ground for every item.
- **Unique Visuals**: add an element with an `Item Id` and a `Prefab` to give that item its own look.

## Inputs

| Field | Meaning |
|---|---|
| `GroundLifetimeSeconds` | How long an item stays on the ground (default 60) |
| `OwnerLockSeconds` | How long only the top damage dealer can pick it up (default 10) |
| `Tables[].Id` | The id that `DropTableId` points to |
| `Entries[].ItemId` | Item id from `Item.json` |
| `Entries[].Chance` | 0 to 1. Each entry rolls on its own |
| `Entries[].MinQty`, `MaxQty` | Random quantity, inclusive |

## Add drops to a monster

1. In the Drop Tables tab, add a table and its items.
2. In the Characters tab, choose it as the monster's **Drop Table**.
3. Enter Play mode and kill the monster.

## Expected result

- Items appear scattered around the corpse.
- Labels sit above their items and never overlap; crowded labels stack upward.
- Click a label: the character walks to within 1 unit and the item goes into the inventory.
- Moving with the keyboard cancels the walk.
- Items disappear after `GroundLifetimeSeconds`.

## How it works

`LootManager` listens to `CombatEvents.OnKilled`, rolls with `LootFormula`, and spawns `GroundItem` objects. The owner is `KillCreditRegistry.TopAttacker`. `LootLabelWindow` is a normal `UIWindow` that draws one `LootLabelView` per `GroundItem` and spaces them with `LootLabelLayout`. A label click calls `LootManager.RequestPickup`; `PlayerLootPickup` walks the path and collects.

## Common mistakes

- `DropTableId` left at 0: nothing drops.
- An `ItemId` that does not exist: a warning is logged and that entry is skipped.
- Forgetting the Build Loot Visuals menu: no labels, and drops fall back to the item icon.
- Clicking a label another character still has locked does nothing.
- A label whose item has no walkable route does nothing.
