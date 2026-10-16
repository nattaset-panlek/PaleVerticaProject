using System;
using UnityEngine;

namespace CoreSystem.UI
{
    [Serializable]
    public class UIModalSkin
    {
        #region [Variables/Fields]
        [Tooltip("Window background sprite. Empty = keep the prefab sprite (tinted dark in Dark mode).")]
        public Sprite WindowSprite;
        public Color WindowColor = Color.white;
        public Color TitleColor = new Color(0.27f, 0.27f, 0.27f, 1f);
        public Color IconColor = new Color(0.28f, 0.28f, 0.28f, 1f);
        #endregion

        #region [Public Methods]
        public static UIModalSkin CreateDark()
        {
            return new UIModalSkin
            {
                TitleColor = new Color(0.90f, 0.91f, 0.94f, 1f),
                IconColor = new Color(0.85f, 0.86f, 0.90f, 1f)
            };
        }
        #endregion
    }
}
