# Progression Item & Experience System Walkthrough

This document guides developers on how Progression Items (Base EXP and Job EXP) work, how to add them via GameDataWorkbench or JSON, and how systems should consume and listen to EXP changes via Player APIs.

---

## 1. Where to Access & Edit

1. **GameDataWorkbench**:
   - Unity Editor Menu: `Tools > Pale Vertica > Pale Vertica GameDataWorkbench`
   - Tab: **Items** > select sub-tab **Prog** (Progression)
   - ID Range: `10001 - 10999` (Prefix: `10`)

2. **JSON Data File**:
   - `Assets/Resources/Data/Item.json`
   - Pre-configured entries:
     - `10001`: Base EXP (`ItemType.Progression`, `ProgressionKind: BaseExp`)
     - `10002`: Job EXP (`ItemType.Progression`, `ProgressionKind: JobExp`)

3. **C# Script Entrypoints**:
   - Model: `CoreSystem.Data.Items.Model.Progression`
   - Character Facade: `CoreSystem.Entity.Character`
   - Player Data & Singleton: `CoreSystem.Data.GameData.Player.Experience`
   - Player Controller: `CoreSystem.Player.PlayerCore`

---

## 2. Architecture & Data Flow

```
[Drop / Quest / Reward]
        │
        ▼ (calls playerInventory.AddItem(10001, 250))
[PlayerInventory.AddItem]
        │
        ├─► Checks (template.Type == ItemType.Progression)
        │     └─► Invokes OnProgressionReceived(template, quantity)
        │     └─► Exits early (never occupies bag inventory slots)
        ▼
[GameData.Player.Experience.ReceiveProgressionItem]
        │
        ▼
[ExperienceSystem (Character)]
        │
        ├─► AddBaseExp / AddJobExp
        ├─► CheckBaseLevelUp / CheckJobLevelUp (loops until exp < nextExp)
        ├─► Triggers OnBaseExpChanged(currentExp, nextExp)
        └─► Triggers OnBaseLevelUp(newLevel) when milestone reached
```

---

## 3. Step-by-Step Usage Guide

### A. Awarding EXP to the Player

#### Approach 1: Unified Item Acquisition Pipeline (Recommended for Drops/Quests)
```csharp
// Give 500 Base EXP via standard item pipeline
GameData.Player.Inventory.AddItem(10001, 500);

// Give 200 Job EXP via standard item pipeline
GameData.Player.Inventory.AddItem(10002, 200);
```

#### Approach 2: Direct Player API (Recommended for Level Cheats / Dev Tools / Scripted Cutscenes)
```csharp
// Via PlayerCore static helpers:
PlayerCore.AddBaseExp(500);
PlayerCore.AddJobExp(200);

// Or via possessed Character reference:
Character player = PlayerCore.GetPlayer();
if (player != null)
{
    player.AddBaseExp(500);
    player.AddJobExp(200);
}
```

### B. Binding UI Progress Bars to EXP

In UI views (e.g. HUD, Character Window):
```csharp
private void OnEnable()
{
    var exp = GameData.Player.Experience;
    exp.OnBaseExpChanged += HandleBaseExpChanged;
    exp.OnBaseLevelUp += HandleBaseLevelUp;

    // Initialize display with current values
    UpdateExpBar(exp.CurrentBaseExp, exp.NextBaseExp, exp.BaseExpNormalized);
}

private void OnDisable()
{
    var exp = GameData.Player.Experience;
    exp.OnBaseExpChanged -= HandleBaseExpChanged;
    exp.OnBaseLevelUp -= HandleBaseLevelUp;
}

private void HandleBaseExpChanged(long current, long next)
{
    UpdateExpBar(current, next, GameData.Player.Experience.BaseExpNormalized);
}

private void HandleBaseLevelUp(int newLevel)
{
    levelText.text = $"Lv. {newLevel}";
}
```

---

## 4. Expected Results

1. **Inventory Window Decoupling**:
   - Calling `Inventory.AddItem(10001, 100)` does not create any grid item slots in `InventoryWindow`.
   - Normal bag capacity and visual slots remain untouched.
2. **Immediate Progress Progression**:
   - Character `CurrentBaseExp` increases immediately.
   - If `CurrentBaseExp >= NextBaseExp`, character levels up, derived stats are recalculated, and `OnBaseLevelUp` fires.
3. **Save Persistence**:
   - EXP values are saved under `PlayerDataModel.Stats.CurrentBaseExp` and `CurrentJobExp`.
   - Progression items are filtered out of `InventoryItemRecord` to avoid duplicate data.

---

## 5. Common Mistakes

1. **Accessing inventory directly to count EXP**:
   - **Incorrect**: Checking `Inventory.GetFirstByModelId(10001)?.Quantity` to find the player's EXP (will always be `null` or `0` because it is not stored as bag instances).
   - **Correct**: Read `PlayerCore.GetPlayer().CurrentBaseExp` or `GameData.Player.Experience.CurrentBaseExp`.
2. **Creating consumable EXP potions as ItemType.Progression**:
   - **Progression** is for raw experiential points (Base EXP, Job EXP).
   - If an item should sit in the player's bag and be consumed on click (like an "EXP Elixir"), set its `ItemType` to `Consumable` and implement `OnUse()` to call `PlayerCore.AddBaseExp(amount)`.
3. **Hardcoding formula math outside ExperienceTable**:
   - Do not calculate required EXP in UI scripts. Always reference `exp.NextBaseExp` or `exp.BaseExpNormalized`.
