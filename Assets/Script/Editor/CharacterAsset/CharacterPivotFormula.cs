using UnityEngine;

namespace CoreSystem.Editor.CharacterAsset
{
    public static class CharacterPivotFormula
    {
        #region [Variables/Fields]
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static Vector2 AnchorPivot(Vector2 anchor, Vector2 canvasPosition, Vector2 size)
        {
            if (size.x <= 0f || size.y <= 0f) return new Vector2(0.5f, 0.5f);
            return new Vector2((anchor.x - canvasPosition.x) / size.x, (anchor.y - canvasPosition.y) / size.y);
        }

        public static Vector2 EffectivePivot(SpriteAlignment alignment, Vector2 customPivot)
        {
            switch (alignment)
            {
                case SpriteAlignment.BottomLeft: return new Vector2(0f, 0f);
                case SpriteAlignment.BottomCenter: return new Vector2(0.5f, 0f);
                case SpriteAlignment.BottomRight: return new Vector2(1f, 0f);
                case SpriteAlignment.LeftCenter: return new Vector2(0f, 0.5f);
                case SpriteAlignment.RightCenter: return new Vector2(1f, 0.5f);
                case SpriteAlignment.TopLeft: return new Vector2(0f, 1f);
                case SpriteAlignment.TopCenter: return new Vector2(0.5f, 1f);
                case SpriteAlignment.TopRight: return new Vector2(1f, 1f);
                case SpriteAlignment.Custom: return customPivot;
                default: return new Vector2(0.5f, 0.5f);
            }
        }

        public static Vector2 PivotOnCanvas(CharacterSpriteInfo sprite)
        {
            Vector2 pivot = EffectivePivot(sprite.Alignment, sprite.Pivot);
            return sprite.CanvasPosition + Vector2.Scale(pivot, sprite.Rect.size);
        }

        public static bool TryCanvasToAtlas(Vector2 canvasPoint, CharacterSpriteInfo sprite, out Vector2Int atlasPixel)
        {
            Vector2 local = canvasPoint - sprite.CanvasPosition;
            atlasPixel = new Vector2Int(Mathf.FloorToInt(sprite.Rect.x + local.x), Mathf.FloorToInt(sprite.Rect.y + local.y));
            return local.x >= 0f && local.y >= 0f && local.x < sprite.Rect.width && local.y < sprite.Rect.height;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
