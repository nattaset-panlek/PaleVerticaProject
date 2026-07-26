using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CoreSystem.Data.Save
{
    public static class SaveManager
    {
        #region [Properties]
        public static string SaveFolderPath => Path.Combine(Application.persistentDataPath, "Saves");
        #endregion

        #region [Public Methods]
        public static string GetSlotFilePath(int slotIndex)
        {
            return Path.Combine(SaveFolderPath, $"save_slot_{slotIndex}.json");
        }

        public static bool SaveSlot(int slotIndex, PlayerDataModel data)
        {
            if (data == null) return false;

            try
            {
                if (!Directory.Exists(SaveFolderPath))
                {
                    Directory.CreateDirectory(SaveFolderPath);
                }

                data.Meta.SlotIndex = slotIndex;
                data.Meta.SaveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                string json = data.ToJson(true);
                string filePath = GetSlotFilePath(slotIndex);
                File.WriteAllText(filePath, json);

                Debug.Log($"[SaveManager] Saved slot {slotIndex} to: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Failed to save slot {slotIndex}: {ex.Message}");
                return false;
            }
        }

        public static PlayerDataModel LoadSlot(int slotIndex)
        {
            string filePath = GetSlotFilePath(slotIndex);
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[SaveManager] Save slot file not found: {filePath}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return LoadFromJson(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Failed to load slot {slotIndex}: {ex.Message}");
                return null;
            }
        }

        public static PlayerDataModel LoadFromJson(string json)
        {
            return PlayerDataModel.FromJson(json);
        }

        public static bool DeleteSlot(int slotIndex)
        {
            string filePath = GetSlotFilePath(slotIndex);
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    Debug.Log($"[SaveManager] Deleted save slot {slotIndex}");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SaveManager] Failed to delete slot {slotIndex}: {ex.Message}");
                    return false;
                }
            }
            return false;
        }

        public static bool SlotExists(int slotIndex)
        {
            return File.Exists(GetSlotFilePath(slotIndex));
        }

        public static List<SaveMetaDataRecord> GetAllSlotInfo()
        {
            List<SaveMetaDataRecord> result = new List<SaveMetaDataRecord>();
            if (!Directory.Exists(SaveFolderPath)) return result;

            string[] files = Directory.GetFiles(SaveFolderPath, "save_slot_*.json");
            foreach (var file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var model = PlayerDataModel.FromJson(json);
                    if (model != null && model.Meta != null)
                    {
                        result.Add(model.Meta);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SaveManager] Failed to read slot info from {file}: {ex.Message}");
                }
            }

            return result;
        }
        #endregion
    }
}
