#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System;
using CoreSystem.Data.Items.Model;
using CoreSystem.UI.Hotbar;
using CoreSystem.UI.Inventory.Utility;
using CoreSystem.UI.GameInterface;
using CoreSystem.UI.SkillManagePage;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ScenarioTesting
{
    public static class ScenarioUITarget
    {
        #region [Variables/Fields]
        public const string Usage = "{\"label\":\"Equip\"} | {\"item\":4001} | {\"skill\":1001} | {\"hotbar\":1,\"row\":0} | {\"equip\":\"Helm\"} | {\"name\":\"Close Button\"}";
        #endregion

        #region [Public Methods]
        public static bool TryFind(JToken spec, out RectTransform rect, out string description)
        {
            rect = null;
            description = spec?.ToString(Newtonsoft.Json.Formatting.None) ?? "nothing";
            if (!(spec is JObject target)) return false;

            if (target["label"] != null)
            {
                Button button = ScenarioInput.FindButton((string)target["label"]);
                rect = button != null ? button.transform as RectTransform : null;
            }
            else if (target["item"] != null) rect = FindItemSlot((int)target["item"]);
            else if (target["skill"] != null) rect = FindSkillRow((int)target["skill"]);
            else if (target["hotbar"] != null) rect = FindHotbarSlot((int?)target["row"] ?? 0, (int)target["hotbar"] - 1);
            else if (target["equip"] != null) rect = FindEquipmentSlot((string)target["equip"]);
            else if (target["name"] != null) rect = FindByName((string)target["name"]);
            return rect != null;
        }

        public static Vector2 ScreenOf(RectTransform rect)
        {
            return ScenarioInput.ScreenOf(rect);
        }
        #endregion

        #region [Private Methods]
        private static RectTransform FindItemSlot(int itemId)
        {
            foreach (ItemSlotGlobal slot in UnityEngine.Object.FindObjectsByType<ItemSlotGlobal>(FindObjectsSortMode.None))
            {
                if (slot.isActiveAndEnabled && slot.itemInstance != null && slot.itemInstance.ItemId == itemId) return slot.transform as RectTransform;
            }
            return null;
        }

        private static RectTransform FindSkillRow(int skillId)
        {
            foreach (SkillItemListUI row in UnityEngine.Object.FindObjectsByType<SkillItemListUI>(FindObjectsSortMode.None))
            {
                if (row.isActiveAndEnabled && row.SkillInstance != null && row.SkillInstance.SkillData != null && row.SkillInstance.SkillData.Id == skillId) return row.transform as RectTransform;
            }
            return null;
        }

        private static RectTransform FindHotbarSlot(int rowIndex, int slotIndex)
        {
            foreach (HotbarSlotView slot in UnityEngine.Object.FindObjectsByType<HotbarSlotView>(FindObjectsSortMode.None))
            {
                if (slot.isActiveAndEnabled && slot.RowIndex == rowIndex && slot.SlotIndex == slotIndex) return slot.transform as RectTransform;
            }
            return null;
        }

        private static RectTransform FindEquipmentSlot(string slotName)
        {
            if (!Enum.TryParse(slotName, true, out EquipmentSlot wanted)) return null;
            foreach (EquipmentSlotUI slot in UnityEngine.Object.FindObjectsByType<EquipmentSlotUI>(FindObjectsSortMode.None))
            {
                if (slot.isActiveAndEnabled && slot.TargetSlot == wanted) return slot.transform as RectTransform;
            }
            return null;
        }

        private static RectTransform FindByName(string objectName)
        {
            foreach (RectTransform rect in UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsSortMode.None))
            {
                if (rect.gameObject.activeInHierarchy && rect.name == objectName) return rect;
            }
            return null;
        }
        #endregion
    }
}
#endif
