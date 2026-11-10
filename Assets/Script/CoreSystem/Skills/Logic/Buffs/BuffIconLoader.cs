using CoreSystem.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CoreSystem.Skills.Logic
{
    public static class BuffIconLoader
    {
        private const string BaseFolder = "Assets/Addressable/Sprite/Buff/";
        private const string LegacyFolder = "Sprite/Buff/";
        private const string FallbackAddressable = "Assets/Addressable/Sprite/Buff/Icon_Default_Buff.png";
        private const string FallbackResource = "Sprite/Buff/Icon_Default_Buff";

        private static Sprite _iconCacheFallback;

        /// <summary>
        /// Global load for buff icons.
        /// Handles Addressables first, then falls back to Resources.
        /// </summary>
        public static Sprite Load(string iconNameOrPath)
        {
            if (string.IsNullOrEmpty(iconNameOrPath)) return GetFallback();

            Sprite sprite = null;

            // 1. Try Addressables by Path/Key
            try
            {
                // Try Full Path first
                sprite = AddressableAssetLoader.TryLoad<Sprite>(iconNameOrPath);

                if (sprite == null)
                {
                    // Try Addressable Prefix
                    sprite = AddressableAssetLoader.TryLoad<Sprite>(BaseFolder + iconNameOrPath);
                    
                    if (sprite == null && !iconNameOrPath.EndsWith(".png"))
                    {
                         sprite = AddressableAssetLoader.TryLoad<Sprite>(BaseFolder + iconNameOrPath + ".png");
                    }
                }
            }
            catch { /* Not Addressable */ }

            // 2. Try Resources
            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>(iconNameOrPath);
                if (sprite == null)
                    sprite = Resources.Load<Sprite>(LegacyFolder + iconNameOrPath);
            }

            return sprite ?? GetFallback();
        }

        public static Sprite GetFallback()
        {
            if (_iconCacheFallback != null) return _iconCacheFallback;

            // Addressable fallback
            try
            {
                _iconCacheFallback = AddressableAssetLoader.TryLoad<Sprite>(FallbackAddressable);
            }
            catch { }

            // Resource fallback
            if (_iconCacheFallback == null)
                _iconCacheFallback = Resources.Load<Sprite>(FallbackResource);

            if (_iconCacheFallback == null)
                Debug.LogWarning("[BuffIconLoader] CRITICAL: Could not find any fallback icon at " + FallbackAddressable + " or " + FallbackResource);

            return _iconCacheFallback;
        }
    }
}
