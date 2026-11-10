using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CoreSystem.Data.Save
{
    public static class SaveStorage
    {
        #region [Variables/Fields]
        public static event Action<string> OnProfileChanged;
        #endregion

        #region [Properties]
        public static string RootFolderPath => Path.Combine(Application.persistentDataPath, "Saves");
        public static string SaveFolderPath => SaveProfileFormula.FolderFor(RootFolderPath, Profile);
        public static string Profile { get; private set; } = string.Empty;
        public static bool IsDefaultProfile => Profile.Length == 0;
        #endregion

        #region [Public Methods]
        public static void UseProfile(string profile)
        {
            string clean = SaveProfileFormula.Sanitize(profile);
            if (clean == Profile) return;
            Profile = clean;
            Debug.Log($"[SaveStorage] Save profile: {(IsDefaultProfile ? "default" : Profile)} ({SaveFolderPath})");
            OnProfileChanged?.Invoke(Profile);
        }

        public static void UseDefaultProfile()
        {
            UseProfile(string.Empty);
        }

        public static bool DeleteProfile(string profile)
        {
            string clean = SaveProfileFormula.Sanitize(profile);
            if (clean.Length == 0) return false;

            string folder = SaveProfileFormula.FolderFor(RootFolderPath, clean);
            if (!Directory.Exists(folder)) return false;

            try
            {
                Directory.Delete(folder, true);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveStorage] Failed to delete profile {clean}: {ex.Message}");
                return false;
            }
        }

        public static List<string> GetProfiles()
        {
            var result = new List<string>();
            string folder = Path.Combine(RootFolderPath, SaveProfileFormula.ProfilesFolderName);
            if (!Directory.Exists(folder)) return result;
            foreach (string directory in Directory.GetDirectories(folder)) result.Add(Path.GetFileName(directory));
            return result;
        }

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

                Debug.Log($"[SaveStorage] Saved slot {slotIndex} to: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveStorage] Failed to save slot {slotIndex}: {ex.Message}");
                return false;
            }
        }

        public static PlayerDataModel LoadSlot(int slotIndex)
        {
            string filePath = GetSlotFilePath(slotIndex);
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[SaveStorage] Save slot file not found: {filePath}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return LoadFromJson(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveStorage] Failed to load slot {slotIndex}: {ex.Message}");
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
                    Debug.Log($"[SaveStorage] Deleted save slot {slotIndex}");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SaveStorage] Failed to delete slot {slotIndex}: {ex.Message}");
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
                    Debug.LogWarning($"[SaveStorage] Failed to read slot info from {file}: {ex.Message}");
                }
            }

            return result;
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Profile = string.Empty;
            OnProfileChanged = null;
        }
        #endregion
    }
}
