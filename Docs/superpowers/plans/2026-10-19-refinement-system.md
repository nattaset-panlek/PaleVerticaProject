# Refinement System & Refinement Window Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rework the Equipment Refinement system with dynamic material recipes (archetype and unique item overrides, 1 to 3 materials), drag-and-drop center item slot with placeholder support, and virtualized stat display via `GlobalVisualizerView` in `Refinement Window.prefab`.

**Architecture:** Pure C# data and logic layer (`RefinementRecipeDatabase` + `RefinementSystem`) decoupled from UI, an event-driven `RefinementWindow` UI controller utilizing `GlobalVisualizerView` (`Packages/com.globalvisualizer.core`) for stat rows, and `RefinementCenterSlot` handling `IDropHandler` for equipment drag-and-drop.

**Tech Stack:** Unity 2022/6000+, TextMesh Pro, Unity UGUI, `GlobalVisualizer` (`Packages/com.globalvisualizer.core`), C# .NET.

**Spec:** `Docs/superpowers/specs/2026-10-19-refinement-system-design.md`

## Global Constraints
- Every new and refactored script MUST strictly use `#region` grouping (`[Variables/Fields]`, `[Properties]`, `[Unity Methods]`, `[Public Methods]`, `[Private Methods]`).
- Do NOT write monolithic God classes; separate data/recipes, refinement logic, and UI view.
- No code comments unless specifically requested.
- Use `UnityEngine.UI.Slider` for the progression rate bar per `GEMINI.md`.
- No `cmd` or `cmd /c` in terminal commands (use PowerShell).

---

### Task 1: Refinement Data & Recipe Architecture

**Files:**
- Create: `Assets/Script/CoreSystem/Refinement/RefinementRecipe.cs`
- Create: `Assets/Script/CoreSystem/Refinement/RefinementRecipeDatabase.cs`

**Interfaces:**
- Consumes: `CoreSystem.Data.Items.Model.Equipment`, `CoreSystem.Enums.WeaponType`, `CoreSystem.Data.Items.Model.EquipmentSlot`
- Produces: `RefinementRecipe`, `RefinementMaterialRequirement`, `RefinementRecipeDatabase.GetRecipe(Equipment equipment)`

- [ ] **Step 1: Create `RefinementRecipe.cs` with material requirement and recipe definition**
```csharp
using System;
using System.Collections.Generic;

namespace CoreSystem.Refinement
{
    [Serializable]
    public class RefinementMaterialRequirement
    {
        public int ItemId;
        public int BaseQuantity = 1;
        public int QuantityScaleInterval = 5;

        public int GetRequiredQuantity(int currentRefine)
        {
            if (QuantityScaleInterval <= 0) return BaseQuantity;
            return BaseQuantity + (currentRefine / QuantityScaleInterval);
        }
    }

    [Serializable]
    public class RefinementRecipe
    {
        public string RecipeId;
        public int TargetItemId = -1;
        public List<RefinementMaterialRequirement> Materials = new List<RefinementMaterialRequirement>();
        public float CostMultiplier = 1.0f;
    }
}
```

- [ ] **Step 2: Create `RefinementRecipeDatabase.cs` with archetype and unique item recipe resolvers**
```csharp
using System.Collections.Generic;
using CoreSystem.Data.Items.Model;
using CoreSystem.Enums;
using UnityEngine;

namespace CoreSystem.Refinement
{
    public static class RefinementRecipeDatabase
    {
        #region [Variables/Fields]
        private static readonly Dictionary<int, RefinementRecipe> _uniqueItemRecipes = new Dictionary<int, RefinementRecipe>();
        private static readonly Dictionary<WeaponType, RefinementRecipe> _weaponRecipes = new Dictionary<WeaponType, RefinementRecipe>();
        private static readonly Dictionary<EquipmentSlot, RefinementRecipe> _armorRecipes = new Dictionary<EquipmentSlot, RefinementRecipe>();
        private static RefinementRecipe _defaultRecipe;
        private static bool _initialized = false;
        #endregion

        #region [Properties]
        public static bool IsInitialized => _initialized;
        #endregion

        #region [Public Methods]
        public static void Initialize()
        {
            _uniqueItemRecipes.Clear();
            _weaponRecipes.Clear();
            _armorRecipes.Clear();

            // Archetype: Edged / Melee Weapons (Iron Ore 5003, Coal 5017)
            var meleeRecipe = CreateRecipe("Archetype_Melee",
                new RefinementMaterialRequirement { ItemId = 5003, BaseQuantity = 2, QuantityScaleInterval = 5 },
                new RefinementMaterialRequirement { ItemId = 5017, BaseQuantity = 1, QuantityScaleInterval = 7 });
            _weaponRecipes[WeaponType.Sword] = meleeRecipe;
            _weaponRecipes[WeaponType.Dagger] = meleeRecipe;
            _weaponRecipes[WeaponType.Blade] = meleeRecipe;
            _weaponRecipes[WeaponType.Axe] = meleeRecipe;
            _weaponRecipes[WeaponType.Mace] = meleeRecipe;

            // Archetype: Magic Weapons (Hardwood 5002, Magic Dust 5016)
            var magicRecipe = CreateRecipe("Archetype_Magic",
                new RefinementMaterialRequirement { ItemId = 5002, BaseQuantity = 2, QuantityScaleInterval = 5 },
                new RefinementMaterialRequirement { ItemId = 5016, BaseQuantity = 1, QuantityScaleInterval = 6 });
            _weaponRecipes[WeaponType.Staff] = magicRecipe;
            _weaponRecipes[WeaponType.Wand] = magicRecipe;

            // Archetype: Ranged / Agile (Wood 5001, Thread 5018, Leather 5007)
            var rangedRecipe = CreateRecipe("Archetype_Ranged",
                new RefinementMaterialRequirement { ItemId = 5001, BaseQuantity = 3, QuantityScaleInterval = 5 },
                new RefinementMaterialRequirement { ItemId = 5018, BaseQuantity = 2, QuantityScaleInterval = 5 },
                new RefinementMaterialRequirement { ItemId = 5007, BaseQuantity = 1, QuantityScaleInterval = 8 });
            _weaponRecipes[WeaponType.Bow] = rangedRecipe;
            _weaponRecipes[WeaponType.Spear] = rangedRecipe;

            // Archetype: Armors (Iron Ingot 5019 / Iron Ore 5003, Hard Leather 5008)
            var armorRecipe = CreateRecipe("Archetype_Armor",
                new RefinementMaterialRequirement { ItemId = 5003, BaseQuantity = 2, QuantityScaleInterval = 5 },
                new RefinementMaterialRequirement { ItemId = 5008, BaseQuantity = 1, QuantityScaleInterval = 6 });
            _armorRecipes[EquipmentSlot.Helm] = armorRecipe;
            _armorRecipes[EquipmentSlot.Armor] = armorRecipe;
            _armorRecipes[EquipmentSlot.Glove] = armorRecipe;

            // Archetype: Accessories & Talismans (Silver Ore 5006, Magic Dust 5016)
            var accRecipe = CreateRecipe("Archetype_Accessory",
                new RefinementMaterialRequirement { ItemId = 5006, BaseQuantity = 2, QuantityScaleInterval = 5 },
                new RefinementMaterialRequirement { ItemId = 5016, BaseQuantity = 1, QuantityScaleInterval = 5 });
            _armorRecipes[EquipmentSlot.Accessory] = accRecipe;
            _armorRecipes[EquipmentSlot.AccessoryLeft] = accRecipe;
            _armorRecipes[EquipmentSlot.AccessoryRight] = accRecipe;
            _armorRecipes[EquipmentSlot.Talisman] = accRecipe;

            // Unique Item Recipe Example: Dragon Tooth Pendant (Talisman 3015 or high-tier gear)
            RegisterUniqueRecipe(3015, CreateRecipe("Unique_DragonPendant",
                new RefinementMaterialRequirement { ItemId = 5016, BaseQuantity = 2, QuantityScaleInterval = 4 },
                new RefinementMaterialRequirement { ItemId = 5020, BaseQuantity = 1, QuantityScaleInterval = 8 },
                new RefinementMaterialRequirement { ItemId = 5014, BaseQuantity = 2, QuantityScaleInterval = 6 }));

            _defaultRecipe = CreateRecipe("Fallback_Default",
                new RefinementMaterialRequirement { ItemId = 5003, BaseQuantity = 2, QuantityScaleInterval = 5 });

            _initialized = true;
        }

        public static void RegisterUniqueRecipe(int itemId, RefinementRecipe recipe)
        {
            recipe.TargetItemId = itemId;
            _uniqueItemRecipes[itemId] = recipe;
        }

        public static RefinementRecipe GetRecipe(Equipment equipment)
        {
            if (!_initialized) Initialize();
            if (equipment == null) return _defaultRecipe;

            if (_uniqueItemRecipes.TryGetValue(equipment.Id, out var uniqueRecipe))
            {
                return uniqueRecipe;
            }

            if (equipment.WeaponType != WeaponType.None && _weaponRecipes.TryGetValue(equipment.WeaponType, out var wRecipe))
            {
                return wRecipe;
            }

            WeaponType inferred = InferWeaponType(equipment);
            if (inferred != WeaponType.None && _weaponRecipes.TryGetValue(inferred, out var infRecipe))
            {
                return infRecipe;
            }

            if (_armorRecipes.TryGetValue(equipment.Slot, out var aRecipe))
            {
                return aRecipe;
            }

            return _defaultRecipe;
        }
        #endregion

        #region [Private Methods]
        private static RefinementRecipe CreateRecipe(string id, params RefinementMaterialRequirement[] materials)
        {
            var recipe = new RefinementRecipe { RecipeId = id };
            if (materials != null)
            {
                recipe.Materials.AddRange(materials);
            }
            return recipe;
        }

        private static WeaponType InferWeaponType(Equipment equipment)
        {
            string name = equipment.Name ?? string.Empty;
            if (name.IndexOf("Sword", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Sword;
            if (name.IndexOf("Dagger", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Dagger;
            if (name.IndexOf("Blade", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Blade;
            if (name.IndexOf("Axe", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Axe;
            if (name.IndexOf("Staff", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Staff;
            if (name.IndexOf("Wand", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Wand;
            if (name.IndexOf("Bow", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Bow;
            if (name.IndexOf("Spear", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Spear;
            if (name.IndexOf("Mace", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Mace;
            return WeaponType.None;
        }
        #endregion
    }
}
```

---

### Task 2: Pure C# Refinement System Logic

**Files:**
- Create: `Assets/Script/CoreSystem/Refinement/RefinementSystem.cs`

**Interfaces:**
- Consumes: `RefinementRecipeDatabase`, `PlayerInventory`, `EquipmentInstance`
- Produces: `RefinementSystem.CanRefine()`, `RefinementSystem.ExecuteRefine()`, `RefinementResult`

- [ ] **Step 1: Implement `RefinementSystem.cs`**
```csharp
using System;
using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using UnityEngine;

namespace CoreSystem.Refinement
{
    public struct RefinementResult
    {
        public bool Success;
        public bool RolledSuccess;
        public int PreviousLevel;
        public int NewLevel;
        public string Message;
    }

    public static class RefinementSystem
    {
        #region [Variables/Fields]
        public const int MaxRefineLevel = 20;
        public const int SafeRefineLevel = 4;
        public const int BaseCost = 3000;
        public const int CostPerLevel = 1200;

        public static event Action<RefinementResult> OnRefinementExecuted;
        #endregion

        #region [Properties]
        #endregion

        #region [Public Methods]
        public static float CalculateSuccessRate(int currentLevel)
        {
            if (currentLevel >= MaxRefineLevel) return 0f;
            if (currentLevel < SafeRefineLevel) return 100f;
            if (currentLevel < 7) return 80f;
            if (currentLevel < 10) return 65f;
            if (currentLevel < 15) return 45f;
            return 25f;
        }

        public static int CalculateCost(Equipment equipment, int currentLevel)
        {
            if (equipment == null) return 0;
            var recipe = RefinementRecipeDatabase.GetRecipe(equipment);
            float mult = recipe != null ? recipe.CostMultiplier : 1.0f;
            return Mathf.RoundToInt((BaseCost + (currentLevel * CostPerLevel)) * mult);
        }

        public static List<(int itemId, int required, int owned)> GetMaterialStatus(Equipment equipment, int currentLevel, PlayerInventory inventory)
        {
            var status = new List<(int, int, int)>();
            if (equipment == null) return status;

            var recipe = RefinementRecipeDatabase.GetRecipe(equipment);
            if (recipe == null || recipe.Materials == null) return status;

            foreach (var mat in recipe.Materials)
            {
                int req = mat.GetRequiredQuantity(currentLevel);
                int owned = inventory != null ? inventory.GetItemCount(mat.ItemId) : 0;
                status.Add((mat.ItemId, req, owned));
            }

            return status;
        }

        public static bool CanRefine(EquipmentInstance equipInst, PlayerInventory inventory, out string failReason)
        {
            if (equipInst == null || !(equipInst.Item is Equipment equipment))
            {
                failReason = "NO EQUIPMENT";
                return false;
            }

            if (equipInst.Refine >= MaxRefineLevel)
            {
                failReason = "MAX LEVEL REACHED";
                return false;
            }

            if (inventory == null)
            {
                failReason = "NO INVENTORY";
                return false;
            }

            int cost = CalculateCost(equipment, equipInst.Refine);
            if (!inventory.HasCredit(cost))
            {
                failReason = "INSUFFICIENT CREDITS";
                return false;
            }

            var matStatus = GetMaterialStatus(equipment, equipInst.Refine, inventory);
            foreach (var mat in matStatus)
            {
                if (mat.owned < mat.required)
                {
                    failReason = "INSUFFICIENT MATERIALS";
                    return false;
                }
            }

            failReason = string.Empty;
            return true;
        }

        public static RefinementResult ExecuteRefine(EquipmentInstance equipInst, PlayerInventory inventory)
        {
            var result = new RefinementResult
            {
                Success = false,
                RolledSuccess = false,
                PreviousLevel = equipInst != null ? equipInst.Refine : 0,
                NewLevel = equipInst != null ? equipInst.Refine : 0,
                Message = string.Empty
            };

            if (!CanRefine(equipInst, inventory, out string failReason))
            {
                result.Message = failReason;
                OnRefinementExecuted?.Invoke(result);
                return result;
            }

            Equipment equipment = (Equipment)equipInst.Item;
            int cost = CalculateCost(equipment, equipInst.Refine);
            inventory.SpendCredit(cost);

            var matStatus = GetMaterialStatus(equipment, equipInst.Refine, inventory);
            foreach (var mat in matStatus)
            {
                inventory.RemoveItem(mat.itemId, mat.required);
            }

            float rate = CalculateSuccessRate(equipInst.Refine);
            float roll = UnityEngine.Random.Range(0f, 100f);
            bool success = roll <= rate;

            result.Success = true;
            result.RolledSuccess = success;

            if (success)
            {
                equipInst.Refine++;
                result.NewLevel = equipInst.Refine;
                result.Message = $"SUCCESS! +{equipInst.Refine}";

                if (equipInst.IsEquipped && GameData.Player != null && GameData.Player.Equipment != null)
                {
                    GameData.Player.Equipment.SmartEquip(equipInst);
                }
            }
            else
            {
                result.NewLevel = equipInst.Refine;
                result.Message = "FAILED! Materials lost.";
            }

            OnRefinementExecuted?.Invoke(result);
            return result;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
```

---

### Task 3: Center Item Slot Drag-and-Drop Drop Target

**Files:**
- Create: `Assets/Script/UI/Refinement/RefinementCenterSlot.cs`

**Interfaces:**
- Consumes: `GlobalDragManager.Payload`, `ItemDragHandler.DraggedItem`, `CoreSystem.Data.Items.EquipmentInstance`
- Produces: `RefinementCenterSlot` triggering `RefinementWindow.SetTarget()`

- [ ] **Step 1: Implement `RefinementCenterSlot.cs`**
```csharp
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.UI.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoreSystem.UI.Refinement
{
    public class RefinementCenterSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        #region [Variables/Fields]
        [SerializeField] private RefinementWindow _window;
        [SerializeField] private Image _slotBorderImage;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Sprite _defaultSlotSprite;
        #endregion

        #region [Properties]
        public RefinementWindow Window
        {
            get => _window;
            set => _window = value;
        }
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_window == null)
            {
                _window = GetComponentInParent<RefinementWindow>();
            }
        }
        #endregion

        #region [Public Methods]
        public void OnDrop(PointerEventData eventData)
        {
            var payload = GlobalDragManager.Payload ?? ItemDragHandler.DraggedItem;
            if (payload is EquipmentInstance equipInst)
            {
                if (_window != null)
                {
                    _window.SetTarget(equipInst);
                }
            }
            else if (payload is ItemInstance itemInst && itemInst is EquipmentInstance eq)
            {
                if (_window != null)
                {
                    _window.SetTarget(eq);
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (_window != null)
                {
                    _window.SetTarget(null);
                }
            }
        }

        public void UpdateDisplay(EquipmentInstance equipment)
        {
            if (equipment == null || equipment.Item == null)
            {
                if (_itemIcon != null)
                {
                    _itemIcon.sprite = null;
                    _itemIcon.enabled = false;
                }
                return;
            }

            if (_itemIcon != null)
            {
                _itemIcon.sprite = equipment.Item.Icon;
                _itemIcon.enabled = _itemIcon.sprite != null;
            }
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
```

---

### Task 4: Virtualized Stat Container Row Component & Refinement Window Controller

**Files:**
- Create: `Assets/Script/UI/Refinement/RefinementStatRowData.cs`
- Create: `Assets/Script/UI/Refinement/RefinementStatRowView.cs`
- Modify: `Assets/Script/UI/Refinement/RefinementWindow.cs`

**Interfaces:**
- Consumes: `GlobalVisualizer.GlobalVisualizerView`, `RefinementSystem`, `RefinementRecipeDatabase`
- Produces: Complete, interactive `RefinementWindow` with 1-3 dynamic material slots and virtualized stats

- [ ] **Step 1: Create `RefinementStatRowData.cs` and `RefinementStatRowView.cs`**
```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Refinement
{
    public struct RefinementStatRowData
    {
        public string Label;
        public string ValueFormatted;
        public bool IsHighlighted;
    }

    public class RefinementStatRowView : MonoBehaviour
    {
        #region [Variables/Fields]
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text _value;
        [SerializeField] private Image _backgroundImage;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public void Bind(RefinementStatRowData data)
        {
            if (_label != null) _label.text = data.Label;
            if (_value != null) _value.text = data.ValueFormatted;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
```

- [ ] **Step 2: Update `RefinementWindow.cs` with `GlobalVisualizerView` and dynamic material slots**
Modify `RefinementWindow.cs` to integrate:
- `_statVisualizer` of type `GlobalVisualizerView` (`Packages/com.globalvisualizer.core`).
- `RefinementCenterSlot _centerSlot`.
- `Slider _rateSlider` for progress bar.
- Dynamic material slot handling (1 to 3 items, icon, count text, tooltip).
- Execute refine calling `RefinementSystem.ExecuteRefine()`.

---

### Task 5: Prefab Update & Generator Sync

**Files:**
- Modify: `Assets/Script/Editor/UI/RefinementAndSocketPrefabBuilder.cs`
- Modify / Rebuild: `Assets/Prefab/Game Page/Refinement Window.prefab`

- [ ] **Step 1: Update `RefinementAndSocketPrefabBuilder.cs` to set up `GlobalVisualizerView`, `RefinementCenterSlot`, `ScrollRect`, and serialize all window bindings**
- [ ] **Step 2: Rebuild `Refinement Window.prefab` via menu or trigger script**
- [ ] **Step 3: Trigger snapshot render using `Temp/trigger_snapshot.txt` and inspect output `Logs/UIDebug/refinement_unity_render.png`**

---

### Task 6: Visual and Functional Verification

**Files:**
- Inspect: `Logs/UIDebug/refinement_unity_render.png`

- [ ] **Step 1: Check snapshot render image for layout alignment and styling**
- [ ] **Step 2: Verify zero compiler errors or missing references in Unity**
- [ ] **Step 3: Verify all test cases (empty slot, 1-material recipe, 3-material recipe, sufficient vs insufficient materials)**
