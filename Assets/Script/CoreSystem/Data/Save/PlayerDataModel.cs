using System;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;

namespace CoreSystem.Data.Save
{
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
