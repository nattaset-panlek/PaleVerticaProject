using System;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Settings.Options;

namespace CoreSystem.Settings
{
    public static class SettingsCatalog
    {
        #region [Variables/Fields]
        public static readonly string[] Languages = CoreSystem.Localization.LocalizationFormula.Languages;
        public const float DefaultBrightness = 50f;
        public const float DefaultZoomSpeed = 50f;
        public const float DefaultHudScale = 100f;
        #endregion

        #region [Public Methods]
        public static void RegisterDefaults()
        {
            SettingsRegistry.AddPage(new SettingsPage(SettingKeys.PageGraphics, "Graphics", 0));
            SettingsRegistry.AddPage(new SettingsPage(SettingKeys.PageAudio, "Audio", 1));
            SettingsRegistry.AddPage(new SettingsPage(SettingKeys.PageGameplay, "Gameplay", 2));
            SettingsRegistry.AddPage(new SettingsPage(SettingKeys.PageControls, "Controls", 3, SettingKeys.KeybindView) { ResetLabel = "Reset All" });

            RegisterGraphics();
            RegisterAudio();
            RegisterGameplay();
        }
        #endregion

        #region [Private Methods]
        private static void RegisterGraphics()
        {
            string page = SettingKeys.PageGraphics;
            SettingsRegistry.AddOption(page, null, "Resolution", new ChoiceSettingOption(SettingKeys.Resolution,
                () => DisplaySetting.GetResolutions().ConvertAll(DisplaySetting.FormatResolution),
                () => DisplaySetting.GetResolutions().IndexOf(DisplaySetting.Resolution),
                i => { var list = DisplaySetting.GetResolutions(); if (i >= 0 && i < list.Count) DisplaySetting.Resolution = list[i]; },
                () => Mathf.Max(0, DisplaySetting.GetResolutions().IndexOf(DisplaySetting.DefaultResolution))));
            SettingsRegistry.AddOption(page, null, "Display Mode", new ChoiceSettingOption(SettingKeys.DisplayMode,
                () => DisplaySetting.DisplayModeNames,
                () => Array.IndexOf(DisplaySetting.DisplayModes, DisplaySetting.Mode),
                i => DisplaySetting.Mode = DisplaySetting.DisplayModes[Mathf.Clamp(i, 0, DisplaySetting.DisplayModes.Length - 1)],
                () => Array.IndexOf(DisplaySetting.DisplayModes, DisplaySetting.DefaultMode)));
            SettingsRegistry.AddOption(page, null, "Vertical Sync", new ToggleSettingOption(SettingKeys.VSync,
                () => DisplaySetting.VSync, v => DisplaySetting.VSync = v, () => DisplaySetting.DefaultVSync));
            SettingsRegistry.AddOption(page, null, "Frame Rate Limit", new ChoiceSettingOption(SettingKeys.FrameLimit,
                () => Array.ConvertAll(DisplaySetting.FrameLimits, DisplaySetting.FormatFrameLimit),
                () => Array.IndexOf(DisplaySetting.FrameLimits, DisplaySetting.FrameLimit),
                i => DisplaySetting.FrameLimit = DisplaySetting.FrameLimits[Mathf.Clamp(i, 0, DisplaySetting.FrameLimits.Length - 1)],
                () => Array.IndexOf(DisplaySetting.FrameLimits, DisplaySetting.DefaultFrameLimit)));
            SettingsRegistry.AddOption(page, null, "Brightness", new SliderSettingOption(SettingKeys.Brightness, 0f, 100f, DefaultBrightness),
                tooltip: "Stored for the post-processing brightness control.");
            SettingsRegistry.AddOption(page, null, "Quality Preset", new ChoiceSettingOption(SettingKeys.Quality,
                () => DisplaySetting.QualityNames,
                () => DisplaySetting.QualityLevel,
                i => DisplaySetting.QualityLevel = i,
                () => DisplaySetting.DefaultQualityLevel));
        }

        private static void RegisterAudio()
        {
            string page = SettingKeys.PageAudio;
            Volume(page, "Master Volume", SettingKeys.VolumeMaster, AudioChannel.Master);
            Volume(page, "Music", SettingKeys.VolumeMusic, AudioChannel.Music);
            Volume(page, "Sound Effects", SettingKeys.VolumeEffects, AudioChannel.Effects);
            Volume(page, "Voice", SettingKeys.VolumeVoice, AudioChannel.Voice);
            Volume(page, "Interface", SettingKeys.VolumeInterface, AudioChannel.Interface);
            SettingsRegistry.AddOption(page, null, "Mute When In Background", new ToggleSettingOption(SettingKeys.MuteInBackground, false));
        }

        private static void Volume(string page, string label, string key, AudioChannel channel)
        {
            SettingsRegistry.AddOption(page, null, label, new SliderSettingOption(key, 0f, 100f,
                () => Mathf.Round(AudioSetting.GetVolume(channel) * 100f),
                v => AudioSetting.SetVolume(channel, v / 100f),
                () => AudioSetting.DefaultVolume * 100f));
            var mute = SettingsRegistry.AddOption(page, null, "Mute", new ToggleSettingOption(AudioSetting.MuteKey(channel), false, true));
            mute.InlineWithKey = key;
        }

        private static void RegisterGameplay()
        {
            string page = SettingKeys.PageGameplay;
            SettingsRegistry.AddOption(page, SettingKeys.GroupCamera, "Camera Shake", new ToggleSettingOption(SettingKeys.CameraShake, true));
            SettingsRegistry.AddOption(page, SettingKeys.GroupCamera, "Zoom Speed", new SliderSettingOption(SettingKeys.ZoomSpeed, 0f, 100f, DefaultZoomSpeed));
            SettingsRegistry.AddOption(page, SettingKeys.GroupInterface, "Show Damage Numbers", new ToggleSettingOption(SettingKeys.ShowDamageNumbers, true, true));
            SettingsRegistry.AddOption(page, SettingKeys.GroupInterface, "Show Minimap", new ToggleSettingOption(SettingKeys.ShowMinimap, true, true),
                tooltip: "Shows the minimap in the top right corner.");
            SettingsRegistry.AddOption(page, SettingKeys.GroupInterface, "HUD Scale", new SliderSettingOption(SettingKeys.HudScale, 50f, 150f, DefaultHudScale, true, v => $"{Mathf.RoundToInt(v)}%"));
            SettingsRegistry.AddOption(page, SettingKeys.GroupInterface, "Show Cast Progress", new ToggleSettingOption(SettingKeys.ShowCastProgress,
                () => GameSetting.ShowCastProgress, v => GameSetting.ShowCastProgress = v, () => true));
            SettingsRegistry.AddOption(page, SettingKeys.GroupLanguage, "Language", new ChoiceSettingOption(SettingKeys.Language,
                () => Languages,
                () => Mathf.Max(0, Array.IndexOf(Languages, GameSetting.Language)),
                i => { string lang = Languages[Mathf.Clamp(i, 0, Languages.Length - 1)]; if (GameSetting.Language != lang) GameSetting.SetLanguage(lang); },
                () => 0));
        }
        #endregion
    }
}
