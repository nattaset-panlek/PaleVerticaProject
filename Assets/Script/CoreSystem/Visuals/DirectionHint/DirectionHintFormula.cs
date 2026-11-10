using UnityEngine;

namespace CoreSystem.Visuals
{
    public static class DirectionHintFormula
    {
        #region [Variables/Fields]
        public const float ArtWidth = 338f;
        public const float ArtHeight = 344f;
        public const float CenterX = 169.5f;
        public const float CenterY = 183f;
        public static readonly Vector2 Pivot = new Vector2(CenterX / ArtWidth, 1f - CenterY / ArtHeight);
        #endregion

        #region [Public Methods]
        public static float Diameter(Vector2 colliderSize, float multiplier, float minimum)
        {
            float side = Mathf.Max(Mathf.Abs(colliderSize.x), Mathf.Abs(colliderSize.y));
            return Mathf.Max(minimum, side * Mathf.Max(0f, multiplier));
        }

        public static float AngleDegrees(Vector2 facing)
        {
            if (facing.sqrMagnitude < 1e-6f) return 0f;
            return Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f;
        }

        public static bool ShouldShow(bool playerUsesFacing, bool isPlayer, bool isDead, bool hostileToPlayer)
        {
            return playerUsesFacing && !isPlayer && !isDead && hostileToPlayer;
        }
        #endregion
    }
}
