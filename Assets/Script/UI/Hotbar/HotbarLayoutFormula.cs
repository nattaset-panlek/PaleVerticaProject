using System.Collections.Generic;
using CoreSystem.Data.Save;

namespace CoreSystem.UI.Hotbar
{
    public static class HotbarLayoutFormula
    {
        #region [Variables/Fields]
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static string GetSlotKeyLabel(int slotIndex)
        {
            return ((slotIndex + 1) % 10).ToString();
        }

        public static string GetHotkeyLabel(int rowIndex, int slotIndex)
        {
            string slotLabel = GetSlotKeyLabel(slotIndex);
            switch (rowIndex)
            {
                case 0: return slotLabel;
                case 1: return $"Alt+{slotLabel}";
                case 2: return $"Ctrl+{slotLabel}";
                case 3: return $"Shift+{slotLabel}";
                default: return $"R{rowIndex + 1}-{slotLabel}";
            }
        }

        public static HotbarSlotRecord ToRecord(HotbarSlotData data, int rowIndex, int slotIndex)
        {
            var record = new HotbarSlotRecord { HotkeyText = GetHotkeyLabel(rowIndex, slotIndex) };
            if (data == null) return record;

            record.ActionType = (int)data.ActionType;
            record.SkillId = data.SkillId;
            record.ItemId = data.ItemId;
            record.ItemInstanceId = data.ItemInstanceId;
            record.CustomActionId = data.CustomActionId;
            return record;
        }

        public static HotbarSlotData FromRecord(HotbarSlotRecord record)
        {
            if (record == null) return new HotbarSlotData();

            return new HotbarSlotData
            {
                ActionType = (HotbarActionType)record.ActionType,
                SkillId = record.SkillId,
                ItemId = record.ItemId,
                ItemInstanceId = record.ItemInstanceId,
                CustomActionId = record.CustomActionId ?? string.Empty
            };
        }

        public static List<HotbarSlotRecord> ToRecords(IList<HotbarSlotData> slots, int rowIndex)
        {
            var records = new List<HotbarSlotRecord>(slots != null ? slots.Count : 0);
            if (slots == null) return records;

            for (int s = 0; s < slots.Count; s++)
            {
                records.Add(ToRecord(slots[s], rowIndex, s));
            }
            return records;
        }

        public static HotbarSlotData[] FromRecords(IList<HotbarSlotRecord> records, int slotCount)
        {
            var slots = new HotbarSlotData[slotCount];
            for (int s = 0; s < slotCount; s++)
            {
                HotbarSlotRecord record = (records != null && s < records.Count) ? records[s] : null;
                slots[s] = FromRecord(record);
            }
            return slots;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
