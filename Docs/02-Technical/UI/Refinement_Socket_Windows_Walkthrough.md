# Refinement and Socket Windows Walkthrough

*Last verified: 2026-10-19*

This walkthrough describes the architecture, prefab configuration, asset reuse pipeline, and operation of the **Refinement Window** and **Socket Window** (16:9 HD modal layouts) matching Figma mockups.

---

## 1. Overview & Architecture

The Refinement and Socket systems provide item progression through stat upgrades (Refine level 1-20) and passive gem infusion (1-4 fragment sockets). Both windows extend `UIWindow` and are registered in `UIWindowRegistry.asset`.

### Component Architecture
- **Refinement Subsystem (`CoreSystem.Refinement`)**:
  - `RefinementRecipeDatabase`: Static registry managing dynamic material recipes (1 to 3 Resource items per recipe) mapped by equipment archetypes (Melee, Magic, Ranged, Armor, Accessory) and unique item overrides (e.g. Dragon Tooth Pendant).
  - `RefinementSystem`: Pure C# service executing validation (`CanRefine`), cost calculation (`(3000 + level * 1200) * mult`), success rate curves (+0..+4 safe 100%, +5..+7 80%, +8..+10 65%, +11..+15 45%, +16..+20 25%), inventory credit and material deductions, and `SmartEquip` auto-refresh.
- **Window Controllers**:
  - `CoreSystem.UI.Refinement.RefinementWindow`: Event-driven UI controller managing the view state, dynamic material slot binding (1-3 active slots), slider progress bar (`0.0` to `1.0`), and virtualized stat rows via `GlobalVisualizerView`.
  - `CoreSystem.UI.Refinement.RefinementCenterSlot`: Center drop target implementing `IDropHandler` and `IPointerClickHandler` accepting `EquipmentInstance` drag payloads from `GlobalDragManager`/`ItemDragHandler` with right-click unslotting.
  - `CoreSystem.UI.Refinement.RefinementStatRowView`: Reusable template row for `GlobalVisualizerView` displaying physical/magic power, attribute bonuses, and conditional refine perks (`[ACTIVE]` vs `[REQ. +N]`).
  - `CoreSystem.UI.Socket.SocketWindow`: Manages socket slot selection, fragment infusion, socket indicators, and credit consumption.
- **Economic Model**:
  - Currency: `Xeny Credit` (`ItemDatabase` ID `1000`, max stack `999,000,000`).
  - Materials: 1 to 3 `Resource` items from `Item.json` (e.g. Iron Ore, Coal, Hardwood, Magic Dust, Thread, Leather, Silver/Gold Ore).
  - Validation: Handled via `RefinementSystem.CanRefine(equipInst, inventory, out string failReason)`.
- **Decoupled Button & Visual Systems**:
  - `Rate Bar Background` utilizes native `UnityEngine.UI.Slider` (`m_Interactable: 0`, `LeftToRight`).
  - `Refine Button` drives the visual frame with baked schematic typography cleanly decoupled from ghost overlays.
  - `Stat Container` integrates `GlobalVisualizerView` (`Packages/com.globalvisualizer.core`) with clamped scrolling and scrollbar integration.

---

## 2. Prefab Structure & Assets

Both prefabs reside in `Assets/Prefab/Game Page/`:

| Prefab Name | GUID | Root Class |
|---|---|---|
| `Refinement Window.prefab` | `c18e1001a11144448888222233334444` | `RefinementWindow` |
| `Socket Window.prefab` | `c28e2002b22255559999333344445555` | `SocketWindow` |

### Shared & Reusable Assets
- **Primary Button Base**: `Assets/Art/UI/New/Button/UI_Primary_Button_Base.png` (GUID: `3e18a202d44f8101a18c89b251a37c99`)
- **Header & Pill Frames**: `Assets/Art/UI/New/Rect/rounded.png` (GUID: `18006f48e9dfbd34080ff908f68dc7a6`, sliced 9-slice)
- **Modal Backgrounds**:
  - Refinement: `Assets/Art/UI/New/Refinement/UI_Refinement_Background.png`
  - Socket: `Assets/Art/UI/New/Socket/UI_Socket_Background.png`
- **Stat Cards**: `Assets/Art/UI/New/Frame/UI_Stat_Card_Light.png` (GUID: `772bd3052af644547bd7dfa7dd569f8b`)
- **Dials / Mandalas**:
  - Refinement: `Assets/Art/UI/New/Refinement/UI_Refinement_Sunburst.png` (GUID: `4c3042e015830010111da33a43be481e`)
  - Socket: `Assets/Art/UI/New/Socket/UI_Socket_Mandala.png` (GUID: `e87674ca24bb978f5209f15c7da1b94d`)
- **Socket Slot Frames & Icons**:
  - Fragment Slot: `Assets/Art/UI/New/Page/Skill Manager Page/UI_Fragment_Slot.png` (Sprite 0: fileID `'-4362983358659938716'`)
  - Gem Icon: `Assets/Art/UI/New/Icon/UI_Fragment_Gem_Icon.png` (GUID: `5a10f011d88bbcc11223344556677881`)
  - Link Icon: `Assets/Art/UI/New/Icon/UI_Fragment_Link_Icon.png` (GUID: `5a20f022d88bbcc11223344556677882`)

---

## 3. How to Open the Windows

### Programmatic Opening (Runtime)
```csharp
// Open Refinement for equipped talisman or specific item
var talisman = GameData.Player.Equipment.GetEquipment(EquipmentSlot.Talisman);
UIManager.Instance.Open("Refinement", talisman);

// Open Socket window
UIManager.Instance.Open("Socket", talisman);
```

### Editor Snapshot & Build Tools
- **Menu Items**:
  - `Pale Vertica > UI > Build Refinement and Socket Windows`: Builds prefabs and registers them in `UIWindowRegistrySO`.
  - `Pale Vertica > UI > Take Snapshots of Refinement and Socket`: Renders 1920x1080 preview renders to `Logs/UIDebug/`.
- **Command Line (Batchmode)**:
```powershell
$p = Start-Process "D:\Unity\6000.4.8f1\Editor\Unity.exe" -ArgumentList "-batchmode", "-quit", "-projectPath", "`"F:\GameProject\Pale Vertica`"", "-executeMethod", "CoreSystem.Editor.UI.RefinementAndSocketPrefabBuilder.TakeSnapshotsBatch", "-logFile", "`"F:\GameProject\Pale Vertica\snapshot_batch.log`"" -PassThru; $p.WaitForExit()
```

---

## 4. Visual Verification Pipeline

A dedicated automated visual comparison pipeline is available in `Tools/UICompare/`:
1. `Tools/UICompare/compare_ui.py`: Loads Unity render snapshots and compares against Figma mockup previews. Computes crop similarity percentage and outputs side-by-side heatmaps:
   - `refinement_comparison.png`
   - `socket_comparison.png`
2. **Executing Comparison**:
```powershell
python Tools/UICompare/compare_ui.py
```

---

## 5. Common Mistakes & Troubleshooting

1. **64-bit Integer FileID Truncation**:
   - `UI_Fragment_Slot` uses 64-bit internal sprite IDs (`-4362983358659938716`).
   - *Fix*: Always treat Unity fileIDs as string literals in JavaScript / Python generators to avoid floating point truncation.
2. **Missing Sprite Fallback to White Square**:
   - For single-sprite textures (`spriteMode: 1`), Unity's YAML sprite fileID is always `21300000`. Do not use internal texture IDs from `spriteSheet` meta fields.
3. **Button Text Wrapping**:
   - Ensure `m_enableWordWrapping: 0` is set on single-line button text components with `m_HorizontalAlignment: 2` and `m_VerticalAlignment: 512` (Center/Middle).
4. **Credit Deduction Failure**:
   - Refine and Socket actions require `Xeny Credit`. Ensure player inventory has sufficient credit instance balance or use `GameData.Player.Inventory.AddCredit(amount)` during testing.
