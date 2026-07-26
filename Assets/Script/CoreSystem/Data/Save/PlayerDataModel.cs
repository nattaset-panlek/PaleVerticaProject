using System;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;

namespace CoreSystem.Data.Save
{
    [Serializable]
    public class CharacterStatsRecord
    {
        public int BaseLevel = 1;
        public int JobLevel = 1;
        public long CurrentBaseExp = 0;
        public long CurrentJobExp = 0;
        public float BaseSTR = 1;
        public float BaseAGI = 1;
        public float BaseVIT = 1;
        public float BaseINT = 1;
        public float BaseDEX = 1;
        public float BaseLUK = 1;
        public int StatPoints = 0;
        public int SkillPoints = 0;
        public int PassivePoints = 0;
    }

    [Serializable]
    public class InventoryItemRecord
    {
        public long InstanceId;
        public int ItemId;
        public int Quantity;
        public int Refine;
        public int Reforge;
        public List<int> SocketedFragmentIds = new List<int>();
        public int EquippedSlotIndex = -1;
        public int CostumeSlotIndex = -1;
    }

    [Serializable]
    public class SkillRecord
    {
        public int SkillId;
        public int EquippedSlotIndex = -1;
        public List<int> LinkedSupportSkillIds = new List<int>();
    }

    [Serializable]
    public class SaveMetaDataRecord
    {
        public int SlotIndex;
        public string SaveName = "New Save";
        public string SaveTimestamp = "";
        public string SaveVersion = "1.0.0";
        public float TotalPlayTimeSeconds = 0f;
    }

    [Serializable]
    public class HotbarSlotRecord
    {
        public int ActionType;
        public int SkillId = -1;
        public int ItemId = -1;
        public long ItemInstanceId = -1;
        public string CustomActionId = "";
        public string HotkeyText = "";
    }

    [Serializable]
    public class HotbarRowRecord
    {
        public int RowIndex;
        public bool IsLocked;
        public Vector2 AnchoredPosition;
        public int SlotCount;
        public List<HotbarSlotRecord> Slots = new List<HotbarSlotRecord>();
    }

    [Serializable]
    public class PlayerDataModel
    {
        #region [Variables/Fields]
        public SaveMetaDataRecord Meta = new SaveMetaDataRecord();
        public CharacterStatsRecord Stats = new CharacterStatsRecord();
        public List<InventoryItemRecord> InventoryItems = new List<InventoryItemRecord>();
        public List<SkillRecord> Skills = new List<SkillRecord>();
        public List<PassiveNodeRecord> PassiveNodes = new List<PassiveNodeRecord>();
        public List<string> ActiveUniquePassiveIds = new List<string>();
        public List<HotbarRowRecord> Hotbars = new List<HotbarRowRecord>();
        #endregion

        #region [Public Methods]
        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static PlayerDataModel FromJson(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString)) return new PlayerDataModel();
            return JsonUtility.FromJson<PlayerDataModel>(jsonString);
        }
        #endregion
    }
}
