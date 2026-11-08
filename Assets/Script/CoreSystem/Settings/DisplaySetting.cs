using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Settings
{
    public static class DisplaySetting
    {
        #region [Variables/Fields]
        public static readonly int[] FrameLimits = { 30, 60, 120, 144, 240, 0 };
        public static readonly FullScreenMode[] DisplayModes = { FullScreenMode.ExclusiveFullScreen, FullScreenMode.FullScreenWindow, FullScreenMode.Windowed };
        public static readonly string[] DisplayModeNames = { "Fullscreen", "Borderless", "Windowed" };

        private const string WidthKey = "Setting.Display.Width";
        private const string HeightKey = "Setting.Display.Height";
        private const string ModeKey = "Setting.Display.Mode";
        private const string VSyncKey = "Setting.Display.VSync";
        private const string FrameLimitKey = "Setting.Display.FrameLimit";
        private const string QualityKey = "Setting.Display.Quality";

        public const int DefaultFrameLimit = 60;
        public const bool DefaultVSync = true;
        public const FullScreenMode DefaultMode = FullScreenMode.FullScreenWindow;

        private static int _projectQualityLevel = -1;
        #endregion

        #region [Properties]
        public static event Action OnChanged;

        public static Vector2Int Resolution
        {
            get
            {
                Vector2Int current = CurrentScreenResolution();
                return new Vector2Int(PlayerPrefs.GetInt(WidthKey, current.x), PlayerPrefs.GetInt(HeightKey, current.y));
            }
            set
            {
                if (value.x <= 0 || value.y <= 0 || Resolution == value) return;
                PlayerPrefs.SetInt(WidthKey, value.x);
                PlayerPrefs.SetInt(HeightKey, value.y);
                Changed();
            }
        }

        public static FullScreenMode Mode
        {
            get
            {
                int stored = PlayerPrefs.GetInt(ModeKey, (int)DefaultMode);
                return Array.IndexOf(DisplayModes, (FullScreenMode)stored) >= 0 ? (FullScreenMode)stored : DefaultMode;
            }
            set
            {
                if (Array.IndexOf(DisplayModes, value) < 0 || Mode == value) return;
                PlayerPrefs.SetInt(ModeKey, (int)value);
                Changed();
            }
        }

        public static bool VSync
        {
            get => PlayerPrefs.GetInt(VSyncKey, DefaultVSync ? 1 : 0) == 1;
            set
            {
                if (VSync == value) return;
                PlayerPrefs.SetInt(VSyncKey, value ? 1 : 0);
                Changed();
            }
        }

        public static int FrameLimit
        {
            get
            {
                int stored = PlayerPrefs.GetInt(FrameLimitKey, DefaultFrameLimit);
                return Array.IndexOf(FrameLimits, stored) >= 0 ? stored : DefaultFrameLimit;
            }
            set
            {
                if (Array.IndexOf(FrameLimits, value) < 0 || FrameLimit == value) return;
                PlayerPrefs.SetInt(FrameLimitKey, value);
                Changed();
            }
        }

        public static int QualityLevel
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel()), 0, Mathf.Max(0, QualitySettings.names.Length - 1));
            set
            {
                int clamped = Mathf.Clamp(value, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
                if (QualityLevel == clamped && PlayerPrefs.HasKey(QualityKey)) return;
                PlayerPrefs.SetInt(QualityKey, clamped);
                Changed();
            }
        }

        public static string[] QualityNames => QualitySettings.names;

        public static Vector2Int DefaultResolution => CurrentScreenResolution();

        public static int DefaultQualityLevel => _projectQualityLevel >= 0 ? _projectQualityLevel : QualitySettings.GetQualityLevel();
        #endregion

        #region [Public Methods]
        public static List<Vector2Int> GetResolutions()
        {
            var list = new List<Vector2Int>();
            foreach (var r in Screen.resolutions)
            {
                var size = new Vector2Int(r.width, r.height);
                if (!list.Contains(size)) list.Add(size);
            }
            Vector2Int current = Resolution;
            if (!list.Contains(current)) list.Add(current);
            list.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
            return list;
        }

        public static string FormatResolution(Vector2Int size) => $"{size.x} x {size.y}";

        public static string FormatFrameLimit(int limit) => limit <= 0 ? "Unlimited" : limit.ToString();

        public static string FormatMode(FullScreenMode mode)
        {
            int index = Array.IndexOf(DisplayModes, mode);
            return index >= 0 ? DisplayModeNames[index] : mode.ToString();
        }

        public static void Apply()
        {
            QualitySettings.SetQualityLevel(QualityLevel, true);
            QualitySettings.vSyncCount = VSync ? 1 : 0;
            Application.targetFrameRate = FrameLimit > 0 ? FrameLimit : -1;
            Vector2Int size = Resolution;
            if (!Application.isEditor) Screen.SetResolution(size.x, size.y, Mode);
        }

        public static void ResetToDefaults()
        {
            foreach (string key in new[] { WidthKey, HeightKey, ModeKey, VSyncKey, FrameLimitKey, QualityKey }) PlayerPrefs.DeleteKey(key);
            Changed();
        }
        #endregion

        #region [Private Methods]
        private static Vector2Int CurrentScreenResolution()
        {
            Resolution current = Screen.currentResolution;
            return current.width > 0 ? new Vector2Int(current.width, current.height) : new Vector2Int(1920, 1080);
        }

        private static void Changed()
        {
            PlayerPrefs.Save();
            Apply();
            OnChanged?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ApplyOnStart()
        {
            if (_projectQualityLevel < 0) _projectQualityLevel = QualitySettings.GetQualityLevel();
            Apply();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OnChanged = null;
            _projectQualityLevel = QualitySettings.GetQualityLevel();
        }
        #endregion
    }
}
