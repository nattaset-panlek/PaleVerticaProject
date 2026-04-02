using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CoreSystem.Data.Items
{
    public static class ItemIconLoader
    {
        private const string BaseFolder     = "Sprite/Item/";
        private const string FallbackResource = "Sprite/Item/UI_Null_Icon";
        private const string FallbackAddressable = "Assets/Addressable/Sprite/Item/UI_Null_Icon.png";

        private static Sprite _iconCacheFallback;

        /// <summary>
        /// Load a sprite using an Addressable key or path stored in the item database.
        /// Falls back to Resources if Addressable fails.
        /// </summary>
        public static Sprite Load(string iconPath)
        {
            Sprite sprite = null;

            if (!string.IsNullOrEmpty(iconPath))
            {
                // 1. Try Addressables first (Sync via WaitForCompletion)
                try
                {
                    AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(iconPath);
                    sprite = handle.WaitForCompletion();
                    
                    if (sprite == null)
                    {
                        // Try with default Addressable prefix if not present
                        AsyncOperationHandle<Sprite> h2 = Addressables.LoadAssetAsync<Sprite>(BaseFolder + iconPath);
                        sprite = h2.WaitForCompletion();
                    }
                }
                catch
                {
                    // Fallback to Resources logic
                }

                // 2. Try Resources fallback
                if (sprite == null)
                {
                    sprite = Resources.Load<Sprite>(iconPath);
                    if (sprite == null)
                        sprite = Resources.Load<Sprite>(BaseFolder + iconPath);
                }
            }

            if (sprite == null)
            {
                sprite = GetFallback();
                if (!string.IsNullOrEmpty(iconPath))
                    Debug.LogWarning($"[ItemIconLoader] Sprite not found: \"{iconPath}\". Using fallback UI_Null_Icon.");
            }

            return sprite;
        }

        private static Sprite GetFallback()
        {
            if (_iconCacheFallback != null) return _iconCacheFallback;

            // Try Addressable fallback first
            try
            {
                AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(FallbackAddressable);
                _iconCacheFallback = handle.WaitForCompletion();
            }
            catch {}

            // Try Resources fallback
            if (_iconCacheFallback == null)
                _iconCacheFallback = Resources.Load<Sprite>(FallbackResource);

            if (_iconCacheFallback == null)
                Debug.LogError("[ItemIconLoader] Fallback sprite UI_Null_Icon not found at: " + FallbackResource);

            return _iconCacheFallback;
        }
    }
}
