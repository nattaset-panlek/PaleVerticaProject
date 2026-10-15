using UnityEngine;

namespace CoreSystem
{
    public static class ItemRarityExtensions
    {
        public static Color GetColor(this ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Legendary: return HexToColor("FF0606");
                case ItemRarity.Crafting: return HexToColor("FF006E");
                case ItemRarity.Unique: return HexToColor("FFBB00");
                case ItemRarity.Epic: return HexToColor("8800FF");
                case ItemRarity.Rare: return HexToColor("00FFAE");
                case ItemRarity.Uncommon: return HexToColor("0048FF");
                case ItemRarity.Common: return HexToColor("D9D9D9");
                default: return Color.white;
            }
        }

        public static string GetHexColor(this ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Legendary: return "FF0606";
                case ItemRarity.Crafting: return "FF006E";
                case ItemRarity.Unique: return "FFBB00";
                case ItemRarity.Epic: return "8800FF";
                case ItemRarity.Rare: return "00FFAE";
                case ItemRarity.Uncommon: return "0048FF";
                case ItemRarity.Common: return "D9D9D9";
                default: return "FFFFFF";
            }
        }

        private static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString("#" + hex, out Color color))
            {
                return color;
            }
            return Color.white;
        }
    }
}
