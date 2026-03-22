using UnityEngine;

namespace CoreSystem.Settings
{
    public static class GameSetting
    {
        public static string Language = "EN"; // Default to English

        public static void SetLanguage(string lang)
        {
            Language = lang.ToUpper();
            Debug.Log($"[GameSetting] Language set to: {Language}");
            // Optional: Save to PlayerPrefs
            PlayerPrefs.SetString("GameLanguage", Language);
            PlayerPrefs.Save();
        }

        public static void Load()
        {
            Language = PlayerPrefs.GetString("GameLanguage", "EN");
        }
    }
}
