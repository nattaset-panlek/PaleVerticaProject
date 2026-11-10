using System;
using UnityEngine;

namespace CoreSystem.Settings
{
    public static class GameSetting
    {
        #region [Variables/Fields]
        public static string Language = "EN";

        private const string LanguageKey = "GameLanguage";
        private const string ShowCastProgressKey = "Setting.ShowCastProgress";
        private const string WindowPositionKey = "Setting.WindowPosition.";
        #endregion

        #region [Properties]
        public static event Action OnChanged;

        public static bool ShowCastProgress
        {
            get => PlayerPrefs.GetInt(ShowCastProgressKey, 1) == 1;
            set
            {
                if (ShowCastProgress == value) return;
                PlayerPrefs.SetInt(ShowCastProgressKey, value ? 1 : 0);
                PlayerPrefs.Save();
                OnChanged?.Invoke();
            }
        }
        #endregion

        #region [Public Methods]
        public static void SetLanguage(string lang)
        {
            string language = CoreSystem.Localization.LocalizationFormula.Normalize(lang);
            bool changed = language != Language;
            Language = language;
            Debug.Log($"[GameSetting] Language set to: {Language}");
            PlayerPrefs.SetString(LanguageKey, Language);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
            if (changed) CoreSystem.Localization.LocalizationEvents.RaiseLanguageChanged(Language);
        }

        public static void Load()
        {
            Language = CoreSystem.Localization.LocalizationFormula.Normalize(PlayerPrefs.GetString(LanguageKey, CoreSystem.Localization.LocalizationFormula.DefaultLanguage));
        }

        public static bool TryGetWindowPosition(string windowName, out Vector2 position)
        {
            string key = WindowPositionKey + windowName;
            if (!PlayerPrefs.HasKey(key + ".x"))
            {
                position = Vector2.zero;
                return false;
            }
            position = new Vector2(PlayerPrefs.GetFloat(key + ".x"), PlayerPrefs.GetFloat(key + ".y"));
            return true;
        }

        public static void SetWindowPosition(string windowName, Vector2 position)
        {
            string key = WindowPositionKey + windowName;
            PlayerPrefs.SetFloat(key + ".x", position.x);
            PlayerPrefs.SetFloat(key + ".y", position.y);
            PlayerPrefs.Save();
        }

        public static void ClearWindowPosition(string windowName)
        {
            string key = WindowPositionKey + windowName;
            PlayerPrefs.DeleteKey(key + ".x");
            PlayerPrefs.DeleteKey(key + ".y");
            PlayerPrefs.Save();
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadOnStart()
        {
            Load();
        }
        #endregion
    }
}
