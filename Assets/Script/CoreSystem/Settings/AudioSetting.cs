using System;
using UnityEngine;

namespace CoreSystem.Settings
{
    public static class AudioSetting
    {
        #region [Variables/Fields]
        public const float DefaultVolume = 0.8f;
        private const string KeyPrefix = "Setting.Audio.";
        #endregion

        #region [Properties]
        public static event Action<AudioChannel> OnChanged;
        #endregion

        #region [Public Methods]
        public static float GetVolume(AudioChannel channel)
        {
            return Mathf.Clamp01(PlayerPrefs.GetFloat(KeyPrefix + channel, DefaultVolume));
        }

        public static void SetVolume(AudioChannel channel, float volume)
        {
            float clamped = Mathf.Clamp01(volume);
            if (PlayerPrefs.HasKey(KeyPrefix + channel) && Mathf.Approximately(GetVolume(channel), clamped)) return;
            PlayerPrefs.SetFloat(KeyPrefix + channel, clamped);
            PlayerPrefs.Save();
            if (channel == AudioChannel.Master) Apply();
            OnChanged?.Invoke(channel);
        }

        public static float GetEffectiveVolume(AudioChannel channel)
        {
            if (IsMuted(AudioChannel.Master) || IsMuted(channel)) return 0f;
            return channel == AudioChannel.Master ? GetVolume(AudioChannel.Master) : GetVolume(AudioChannel.Master) * GetVolume(channel);
        }

        public static string MuteKey(AudioChannel channel)
        {
            switch (channel)
            {
                case AudioChannel.Music: return SettingKeys.MuteMusic;
                case AudioChannel.Effects: return SettingKeys.MuteEffects;
                case AudioChannel.Interface: return SettingKeys.MuteInterface;
                case AudioChannel.Voice: return SettingKeys.MuteVoice;
                default: return SettingKeys.MuteMaster;
            }
        }

        public static bool IsMuted(AudioChannel channel)
        {
            return SettingsStorage.GetBool(MuteKey(channel), false);
        }

        public static void Apply()
        {
            AudioListener.volume = IsMuted(AudioChannel.Master) ? 0f : GetVolume(AudioChannel.Master);
        }

        public static void ResetToDefaults()
        {
            foreach (AudioChannel channel in Enum.GetValues(typeof(AudioChannel))) PlayerPrefs.DeleteKey(KeyPrefix + channel);
            PlayerPrefs.Save();
            Apply();
            foreach (AudioChannel channel in Enum.GetValues(typeof(AudioChannel))) OnChanged?.Invoke(channel);
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ApplyOnStart()
        {
            SettingsEvents.OnChanged -= HandleSettingChanged;
            SettingsEvents.OnChanged += HandleSettingChanged;
            Apply();
        }

        private static void HandleSettingChanged(string key)
        {
            if (key == SettingKeys.MuteMaster) Apply();
            foreach (AudioChannel channel in Enum.GetValues(typeof(AudioChannel)))
            {
                if (key == MuteKey(channel)) OnChanged?.Invoke(channel);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OnChanged = null;
        }
        #endregion
    }
}
