using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI
{
    public static class UIShapes
    {
        #region [Variables/Fields]
        public const float RoundedRadius = 32f;
        private const int TextureSize = 66;
        private static Sprite _roundedRect;
        #endregion

        #region [Properties]
        public static Sprite RoundedRect => _roundedRect != null ? _roundedRect : (_roundedRect = CreateRoundedRect());
        #endregion

        #region [Public Methods]
        public static Image GetShapeImage(GameObject go)
        {
            if (go == null) return null;
            var selectable = go.GetComponent<Selectable>();
            if (selectable != null && selectable.targetGraphic is Image target) return target;
            return go.GetComponent<Image>();
        }

        public static void ApplyCornerRadius(Image image, float radius)
        {
            if (image == null) return;

            if (radius <= 0f)
            {
                if (image.sprite == RoundedRect)
                {
                    image.sprite = null;
                    image.type = Image.Type.Simple;
                }
                return;
            }

            Sprite sprite = image.sprite;
            float corner = sprite != null ? Mathf.Max(Mathf.Max(sprite.border.x, sprite.border.y), Mathf.Max(sprite.border.z, sprite.border.w)) : 0f;
            if (sprite == null || corner <= 0f)
            {
                sprite = RoundedRect;
                image.sprite = sprite;
                corner = RoundedRadius;
            }

            float referencePixelsPerUnit = image.canvas != null ? image.canvas.referencePixelsPerUnit : 100f;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = Mathf.Max(0.01f, corner * referencePixelsPerUnit / (sprite.pixelsPerUnit * radius));
        }
        #endregion

        #region [Private Methods]
        private static Sprite CreateRoundedRect()
        {
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "UIRoundedRect"
            };

            var pixels = new Color32[TextureSize * TextureSize];
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float cx = Mathf.Clamp(px, RoundedRadius, TextureSize - RoundedRadius);
                    float cy = Mathf.Clamp(py, RoundedRadius, TextureSize - RoundedRadius);
                    float distance = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));
                    byte alpha = (byte)(Mathf.Clamp01(RoundedRadius - distance + 0.5f) * 255f);
                    pixels[y * TextureSize + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            var border = new Vector4(RoundedRadius, RoundedRadius, RoundedRadius, RoundedRadius);
            return Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _roundedRect = null;
        }
        #endregion
    }
}
