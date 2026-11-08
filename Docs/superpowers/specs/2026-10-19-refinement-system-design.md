# Refinement System & Refinement Window Redesign Specification

## 1. Overview
This specification details the comprehensive rework of the Equipment Refinement system and the `Refinement Window.prefab` UI in *Pale Vertica*. The overhaul transitions the refinement mechanics from hardcoded values to a decoupled, data-driven architecture supporting dynamic material recipes (archetype and unique item overrides), inventory drag-and-drop slotted items, and virtualized stat rendering with `GlobalVisualizerView` (`Packages/com.globalvisualizer.core`).

---

## 2. Architecture & Components

```
                +----------------------------+
                |    EquipmentInstance       |
                |  (Target Slotted Item)     |
                +--------------+-------------+
                               |
                               v
            +------------------------------------+
            |      RefinementRecipeDatabase      |
            |  1. Item-Specific Unique Recipes   |
            |  2. Weapon/Armor Archetype Recipes |
            |  3. Fallback Base Recipe           |
            +------------------+-----------------+
                               |
                               v
                     +-------------------+
                     | RefinementSystem  |
                     |  - CanRefine()    |
                     |  - ExecuteRefine()|
                     |  - GetCost()      |
                     |  - GetRates()     |
                     +---------+---------+
                               |
        +----------------------+----------------------+
        |                                             |
        v                                             v
+-------------------------------+         +----------------------------+
|     RefinementWindow (UI)     |         |   PlayerInventory & Entity |
| - CenterItemSlot (IDropHandler|         | - SpendCredit()            |
| - Material Slots 1..3         |         | - RemoveItemByInstance()   |
| - GlobalVisualizerView Stats  |         | - SmartEquip() Refresh     |
| - Rate Bar Slider & Cost      |         +----------------------------+
+-------------------------------+
```

### 2.1 RefinementRecipe & RefinementRecipeDatabase
* **Recipe Data Structure**:
  * `EquipmentFilter`: Specific `ItemId` / `Code` (for unique items) OR `WeaponType` / `EquipmentSlot`.
  * `Materials`: List of `RefinementMaterialRequirement` (1 to 3 items):
    * `ItemId`: Resource item ID from `Item.json` (e.g., 5003 Iron Ore, 5019 Iron Ingot, 5016 Magic Dust).
    * `BaseQuantity`: Starting count.
    * `QuantityScale`: Scaling step based on refine level (e.g., $+1$ per 5 refine levels).
  * `CostMultiplier`: Base cost scalar.
* **Resolution Priority**:
  1. **Item-Specific Unique Recipes**: Mapped directly by `ItemId` (for Epic/Legendary gear requiring special monster drops or rare ingots).
  2. **Archetype Recipes**:
     * *Edged/Melee Weapons (Sword, Dagger, Blade, Axe)*: Iron Ore (5003) / Iron Ingot (5019) + Coal (5017).
     * *Magic Weapons (Staff, Wand)*: Hardwood (5002) + Magic Dust (5016).
     * *Ranged/Agile Weapons (Bow, Spear)*: Wood (5001) / Hardwood (5002) + Thread (5018) + Leather (5007).
     * *Armor/Helm/Glove*: Iron Ore (5003) / Iron Ingot (5019) + Hard Leather (5008).
     * *Accessory/Talisman*: Silver Ore (5006) / Gold Ore (5004) + Magic Dust (5016).
  3. **Fallback Default Recipe**: Iron Ore (5003) x2.

### 2.2 Refinement Economics & Formulas
* **Success Rate Curve**:
  * $+0 \rightarrow +4$: $100\%$ (Safe Level)
  * $+5 \rightarrow +7$: $80\%$
  * $+8 \rightarrow +10$: $65\%$
  * $+11 \rightarrow +15$: $45\%$
  * $+16 \rightarrow +20$: $25\%$
* **Cost Formula**:
  $$\text{Cost}(\text{level}) = 3,000 + (\text{level} \times 1,200)$$
* **Failure Penalty**:
  * Consumed materials and credit fee are lost.
  * Slotted equipment is preserved (no destruction, no level degradation).

---

## 3. UI Window & Prefab Specification

### 3.1 Center Item Slot & Drag-and-Drop
* Component: `RefinementCenterSlot` implementing `IDropHandler` and `IPointerClickHandler`.
* Listens to drops where payload is `EquipmentInstance`.
* **State `None`**:
  * `_itemIcon.enabled = false` (or placeholder outline sprite).
  * Title displays "SELECT EQUIPMENT", type and rarity badges cleared.
* **State `Slotted`**:
  * `_itemIcon.sprite = equipment.Icon; _itemIcon.enabled = true`.
  * Right-click on Center Item Slot clears/unslots the target.

### 3.2 Material Slots (1 to 3 dynamic)
* Slots `Material Slot 1`, `Material Slot 2`, `Material Slot 3`:
  * Each slot contains an `Image` (item icon) and `TMP_Text` (count text).
  * If recipe has 1 material: Slot 1 is active; Slots 2 and 3 are inactive (`gameObject.SetActive(false)`).
  * If recipe has 2 materials: Slots 1 and 2 are active; Slot 3 is inactive.
  * Count formatting: `"{owned} / {required}"`. Colored `#ef4444` (red) if owned < required, `#ffffff` or `#38bdf8` if sufficient.
  * Hover tooltip attached using `HoverableUI` showing material item name.

### 3.3 Stat Container with `GlobalVisualizerView`
* In `Refinement Window.prefab`:
  * `Stat Container` holds `ScrollRect` + `GlobalVisualizerView`.
  * `Container` is the Content `RectTransform`.
  * `Scroll Track` and `Scroll Thumb` wired as `UnityEngine.UI.Scrollbar`.
  * Template item: `Stat Row` (Width 400px, Height 48px).
* **Bound Rows**:
  * Base Physical Power (if $>0$): Current value $+$ Refine bonus, previewing next level bonus `+(+X)`.
  * Base Magic Power (if $>0$): Current value $+$ Refine bonus, previewing next level bonus `+(+X)`.
  * `AttributeBonusEffect` rows (ATK, DEF, MaxHP, Crit, etc.): Base value $+$ refine scaling.
  * `RefineBonusEffect` rows: Special conditional perks showing `[ACTIVE]` (green) or `[Requires +N]` (gray).

### 3.4 Progression Detail & Action Controls
* `Rate Bar Bg`: `UnityEngine.UI.Slider` driven from $0.0$ to $1.0$ (matching success rate).
* `Rate Value`: TMP_Text displaying percentage (e.g. `65%`).
* `Cost Value`: TMP_Text displaying currency requirement (e.g. `5,000 CREDITS`).
* `Refine Button`:
  * Enabled only when equipment is slotted, refine level $<20$, and player has sufficient credits AND required materials.
  * On Click: Calls `RefinementSystem.ExecuteRefine()`, plays audio/visual feedback, and refreshes the UI.
* `Close Button`: Wired to close window via `UIManager.Instance.Close("Refinement")` or `gameObject.SetActive(false)`.

---

## 4. Verification & Testing
* Unit test coverage for `RefinementSystem` (recipe lookup, validation, execution).
* Snapshot generation using `RefinementAndSocketPrefabBuilder` and `trigger_snapshot.txt` to verify visual alignment and prefab integrity.
