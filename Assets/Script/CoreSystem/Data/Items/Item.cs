using CoreSystem;
using CoreSystem.Data;
using UnityEngine;

namespace CoreSystem.Data.Items
{
    public abstract class Item
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public LocalizedString Description { get; set; }
        public ItemType Type { get; set; }
        public bool IsStackable { get; set; }
        public ItemRarity Rarity { get; set; }

        /// <summary>Relative path inside Resources (e.g. "Sprite/Item/UI_Helmet_Icon")</summary>
        public string IconPath { get; set; }

        private Sprite _iconCache;
        /// <summary>Loads the sprite from Resources on first access, returns null-icon fallback.</summary>
        public Sprite Icon
        {
            get
            {
                if (_iconCache == null)
                    _iconCache = ItemIconLoader.Load(IconPath);
                return _iconCache;
            }
        }
        
        public virtual void OnUse()
        {
        }

        public abstract Item Clone();
    }
}
