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
                    sprite = AddressableAssetLoader.TryLoad<Sprite>(iconPath);

                    if (sprite == null)
                    {
                        sprite = AddressableAssetLoader.TryLoad<Sprite>(BaseFolder + iconPath);

                        if (sprite == null && !iconPath.EndsWith(".png"))
                        {
                            sprite = AddressableAssetLoader.TryLoad<Sprite>(BaseFolder + iconPath + ".png");
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
                _iconCacheFallback = AddressableAssetLoader.TryLoad<Sprite>(FallbackAddressable);
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
