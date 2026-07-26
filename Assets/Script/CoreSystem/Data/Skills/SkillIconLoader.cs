using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CoreSystem.Data.Skills
{
    public static class SkillIconLoader
    {
        #region [Variables/Fields]
        private const string BaseFolder = "Assets/Addressable/Sprite/Skill/Icon/";
        private const string FallbackAddressable = "Assets/Addressable/Sprite/Skill/Icon/Icon_Default.png";

        private static Sprite _iconCacheFallback;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static Sprite Load(string iconPath)
        {
            Sprite sprite = null;

            if (!string.IsNullOrEmpty(iconPath))
            {
                try
                {
                    AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(iconPath);
                    sprite = handle.WaitForCompletion();

                    if (sprite == null)
                    {
                        AsyncOperationHandle<Sprite> h2 = Addressables.LoadAssetAsync<Sprite>(BaseFolder + iconPath);
                        sprite = h2.WaitForCompletion();

                        if (sprite == null && !iconPath.EndsWith(".png"))
                        {
                            AsyncOperationHandle<Sprite> h3 = Addressables.LoadAssetAsync<Sprite>(BaseFolder + iconPath + ".png");
                            sprite = h3.WaitForCompletion();
                        }
                    }
                }
                catch
                {
                }
            }

            if (sprite == null)
            {
                sprite = GetFallback();
                if (!string.IsNullOrEmpty(iconPath))
                {
                    Debug.LogWarning($"[SkillIconLoader] Sprite not found in Addressables: \"{iconPath}\". Using fallback Icon_Default.");
                }
            }

            return sprite;
        }

        public static Sprite GetFallback()
        {
            if (_iconCacheFallback != null) return _iconCacheFallback;

            try
            {
                AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(FallbackAddressable);
                _iconCacheFallback = handle.WaitForCompletion();
            }
            catch {}

            if (_iconCacheFallback == null)
            {
                Debug.LogError("[SkillIconLoader] Fallback Addressable sprite Icon_Default not found at: " + FallbackAddressable);
            }

            return _iconCacheFallback;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
