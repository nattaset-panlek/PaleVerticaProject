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
        #region [Variables/Fields]
        public bool Success;
        public bool RolledSuccess;
        public int PreviousLevel;
        public int NewLevel;
        public string Message;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public RefinementResult(bool success, bool rolledSuccess, int previousLevel, int newLevel, string message)
        {
            Success = success;
            RolledSuccess = rolledSuccess;
            PreviousLevel = previousLevel;
            NewLevel = newLevel;
            Message = message;
        }
        #endregion

        #region [Private Methods]
        #endregion
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

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static float CalculateSuccessRate(int currentLevel)
        {
            if (currentLevel < 0 || currentLevel >= MaxRefineLevel) return 0f;
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
            float multiplier = recipe != null ? recipe.CostMultiplier : 1.0f;
            int level = Mathf.Max(0, currentLevel);
            return Mathf.RoundToInt((BaseCost + (level * CostPerLevel)) * multiplier);
        }

        public static List<(int itemId, int required, int owned)> GetMaterialStatus(Equipment equipment, int currentLevel, PlayerInventory inventory)
        {
            var status = new List<(int itemId, int required, int owned)>();
            if (equipment == null) return status;

            var recipe = RefinementRecipeDatabase.GetRecipe(equipment);
            if (recipe == null || recipe.Materials == null) return status;

            for (int i = 0; i < recipe.Materials.Count; i++)
            {
                var mat = recipe.Materials[i];
                if (mat == null) continue;

                int req = mat.GetRequiredQuantity(currentLevel);
                int owned = GetInventoryItemCount(inventory, mat.ItemId);
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
            for (int i = 0; i < matStatus.Count; i++)
            {
                if (matStatus[i].owned < matStatus[i].required)
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

            var matStatus = GetMaterialStatus(equipment, equipInst.Refine, inventory);

            inventory.BeginBatch();
            try
            {
                inventory.SpendCredit(cost);
                for (int i = 0; i < matStatus.Count; i++)
                {
                    RemoveInventoryItems(inventory, matStatus[i].itemId, matStatus[i].required);
                }
            }
            finally
            {
                inventory.EndBatch();
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
        private static int GetInventoryItemCount(PlayerInventory inventory, int itemId)
        {
            if (inventory == null) return 0;

            int count = 0;
            var instances = inventory.ItemInstances;
            for (int i = 0; i < instances.Count; i++)
            {
                var item = instances[i];
                if (item != null && item.ItemId == itemId)
                {
                    count += item.Quantity;
                }
            }
            return count;
        }

        private static void RemoveInventoryItems(PlayerInventory inventory, int itemId, int amount)
        {
            if (inventory == null || amount <= 0) return;

            int remaining = amount;
            var instances = inventory.ItemInstances;
            for (int i = 0; i < instances.Count; i++)
            {
                var item = instances[i];
                if (item != null && item.ItemId == itemId)
                {
                    int toRemove = Mathf.Min(remaining, item.Quantity);
                    inventory.RemoveItemByInstance(item.Id, toRemove);
                    remaining -= toRemove;
                    if (remaining <= 0) break;
                }
            }
        }
        #endregion
    }
}
