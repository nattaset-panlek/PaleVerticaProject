using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.Editor.UI
{
    public static class EditorUIFactory
    {
        #region [Variables/Fields]
        public const string NovecentoPath = "Assets/Art/Front/Novecento Sans/Novecentosanswide-DemiBold SDF.asset";
        public const string KanitRegularPath = "Assets/Art/Front/Kanit/Kanit-Regular SDF.asset";
        public const string KanitMediumPath = "Assets/Art/Front/Kanit/Kanit-Medium SDF.asset";
        public const string KanitSemiBoldPath = "Assets/Art/Front/Kanit/Kanit-SemiBold SDF.asset";
        public const string KanitBoldPath = "Assets/Art/Front/Kanit/Kanit-Bold SDF.asset";
        #endregion

        #region [Public Methods]
        public static RectTransform NewChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent != null ? parent.gameObject.layer : 5;
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void SetTopLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        public static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static Image AddImage(Transform parent, string name, Sprite sprite, Color color, float x, float y, float width, float height)
        {
            RectTransform rect = NewChild(parent, name);
            SetTopLeft(rect, x, y, width, height);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static TextMeshProUGUI AddText(Transform parent, string name, string content, TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment, float x, float y, float width, float height)
        {
            RectTransform rect = NewChild(parent, name);
            SetTopLeft(rect, x, y, width, height);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.text = content;
            text.fontSize = size;
            text.enableAutoSizing = false;
            text.color = color;
            text.alignment = alignment;
            text.richText = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.margin = Vector4.zero;
            text.raycastTarget = false;
            return text;
        }

        public static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--) Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }

        public static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);

        public static Sprite LoadSprite(string path)
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite) return sprite;
            }
            return null;
        }

        public static void PrepareSprite(string path, Vector4 border)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            bool dirty = importer.textureType != TextureImporterType.Sprite
                || importer.spriteImportMode != SpriteImportMode.Single
                || importer.mipmapEnabled
                || !importer.alphaIsTransparency
                || importer.textureCompression != TextureImporterCompression.Uncompressed
                || importer.spriteBorder != border;
            if (!dirty) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.spriteBorder = border;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        public static void SetObject(Object target, string property, Object value)
        {
            var so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(property);
            if (prop == null) throw new System.InvalidOperationException($"{target.GetType().Name} has no field '{property}'.");
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetObjects(Object target, string property, Object[] values)
        {
            var so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(property);
            if (prop == null) throw new System.InvalidOperationException($"{target.GetType().Name} has no field '{property}'.");
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetBool(Object target, string property, bool value)
        {
            var so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(property);
            if (prop == null) throw new System.InvalidOperationException($"{target.GetType().Name} has no field '{property}'.");
            prop.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString("#" + hex, out Color color) ? color : Color.magenta;
        }
        #endregion
    }
}
