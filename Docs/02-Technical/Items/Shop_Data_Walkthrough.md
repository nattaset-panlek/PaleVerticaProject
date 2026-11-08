# Shop Data Walkthrough

What a merchant sells and for how much. Prices belong to the merchant, not to the item: two merchants can sell the same item at different prices. Built 2026-11-08 with the [Merchant's Stall](../UI/Merchant_Stall_Walkthrough.md).

## 1. The file

`Assets/Resources/Data/Shop.json`:

```json
{
  "Shops": [
    {
      "Id": "general_merchant",
      "Name": "Merchant",
      "Items": [
        { "ItemId": 2002, "Price": 120, "Stock": -1 },
        { "ItemId": 3005, "Price": 2400, "Stock": 3 }
      ]
    }
  ]
}
```

| Field | Meaning |
|---|---|
| `Id` | the shop id that `MerchantShopSystem` points at |
| `Name` | the shop's name in data; the window tag always says Merchant |
| `ItemId` | an item from `Item.json` |
| `Price` | credits for one unit (negative prices are dropped when loading) |
| `Stock` | units for sale per opening of the window; `-1` or missing = unlimited |

Stock is counted per `ShopSession` (one opening of the window). It is not saved yet, so it refills the next time the window opens.

## 2. Code

| Type | Job |
|---|---|
| `ShopLoader` | reads and writes the JSON (`Parse`, `ToJson`) |
| `ShopDatabase` | static lookup by id, loads `Shop.json` the first time (`Reload`, `Set` for tests) |
| `ShopSession` | one visit: stock left, the cart (max 36 lines), `AddToCart`, `SetCartQuantity`, `RemoveLine`, `Total`, `CanAfford`, `Checkout(PlayerInventory)`; events `OnCartChanged`, `OnStockChanged`, `OnPurchased` |
| `MerchantShopSystem` | `IInteractionProvider` on an NPC: adds the **Shop** option and opens `Merchant Stall` |

Money is the inventory credit (`PlayerInventory.GetCreditAmount` / `SpendCredit`, item 1000 `Xeny Credit`). `Checkout` checks the total first, then spends and adds all items inside one inventory batch, so the UI refreshes once. It returns `Success`, `EmptyCart`, `NotEnoughCredit` or `NoInventory`.

## 3. Add a merchant

1. In **Game Data Workbench**, **Shops** tab, add a shop and its items.
2. On the NPC prefab, add **Merchant Shop System**, set **Character** (filled by Reset) and **Shop Id**.
3. Play, right-click the NPC, choose **Shop**.
4. Run the EditMode tests: `ShopSessionTests.ShopJson_LoadsAndPointsAtRealItems` fails if an `ItemId` does not exist.

## 4. Common mistakes

- Putting `Price` on the item in `Item.json`: it is not read; prices live in `Shop.json`.
- `Stock: 0`: the ware shows as Sold out from the start; use `-1` for unlimited.
- A merchant with no matching shop id gets no Shop option (no error).
- The Workbench has no Shop tab yet; edit `Shop.json` directly.
