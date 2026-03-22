using UnityEngine;

namespace CoreSystem.Data.Items
{
    public static class ItemIconLoader
    {
        private const string BaseFolder     = "Sprite/Item/";
        private const string FallbackSprite = "Sprite/Item/UI_Null_Icon";

        private static Sprite _fallback;

        /// <summary>
        /// Load a sprite using a path stored in the item database.
        /// <para>
        /// The path can be:
        ///   • A full Resources-relative path  – "Sprite/Item/UI_Helmet_Icon"
        ///   • Just a filename (no folder)     – "UI_Helmet_Icon"
        ///   • Null / empty                    – returns the fallback icon
        /// </para>
        /// </summary>
        public static Sprite Load(string iconPath)
        {
            Sprite sprite = null;

            if (!string.IsNullOrEmpty(iconPath))
            {
                sprite = Resources.Load<Sprite>(iconPath);

                if (sprite == null)
                    sprite = Resources.Load<Sprite>(BaseFolder + iconPath);
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
            if (_fallback == null)
                _fallback = Resources.Load<Sprite>(FallbackSprite);

            if (_fallback == null)
                Debug.LogError("[ItemIconLoader] Fallback sprite UI_Null_Icon not found at: " + FallbackSprite);

            return _fallback;
        }
    }
}
