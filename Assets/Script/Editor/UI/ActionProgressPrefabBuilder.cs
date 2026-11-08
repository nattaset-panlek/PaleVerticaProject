using System.IO;
using CoreSystem.UI;
using CoreSystem.UI.ActionProgress;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace CoreSystem.Editor.UI
{
    public static class ActionProgressPrefabBuilder
    {
        #region [Variables/Fields]
        public const string PrefabPath = "Assets/Prefab/Game Page/Action Progress Window.prefab";
        public const string RegistryAssetPath = "Assets/Prefab/UIWindowRegistry.asset";

        private const string ArtFolder = "Assets/Art/UI/New/Gameplay/Global/ActionProgress";
        private const string BackgroundPath = ArtFolder + "/UI_ActionProgress_Background.png";
        private const string BackgroundNoIconPath = ArtFolder + "/UI_ActionProgress_Background_NoIcon.png";
        public const string DefaultIconPath = ArtFolder + "/UI_ActionProgress_DefaultIcon.png";
        private const string RoundedPath = "Assets/Art/UI/New/Rect/rounded.png";
        private const string NotoMediumPath = "Assets/Art/Front/Noto Sans Thai/NotoSansThai-Medium SDF.asset";
        private const string NotoSemiBoldPath = "Assets/Art/Front/Noto Sans Thai/NotoSansThai-SemiBold SDF.asset";
        private const string NovecentoNormalFontPath = "Assets/Art/Front/Novecento-Font/Novecento Normal.ttf";
        private const string NovecentoNormalSdfPath = "Assets/Art/Front/Novecento-Font/Novecento Normal SDF.asset";

        private const float WindowWidth = 380f;
        private const float WindowHeight = 86f;
        private const float RoundedBorderPx = 21f;
        private const float DefaultBottomOffset = 230f;

        private static readonly Color TitleInk = Hex(0xF5F5F5);
        private static readonly Color TypeInk = Hex(0x737373);
        private static readonly Color TimeInk = Hex(0x818181);
        private static readonly Color TrackInk = Hex(0x262626);
        private static readonly Color TrackBorderInk = Hex(0x000000);
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/UI/Build Action Progress Window")]
        public static void BuildFromMenu()
        {
            Build();
            Debug.Log("[ActionProgressPrefabBuilder] Built " + PrefabPath);
        }

        public static void BuildBatch()
        {
            int exitCode = 0;
            try
            {
                Build();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                exitCode = 1;
            }
            EditorApplication.Exit(exitCode);
        }

        public static GameObject Build()
        {
            Sprite background = PrepareSprite(BackgroundPath, 200f);
            Sprite backgroundNoIcon = PrepareSprite(BackgroundNoIconPath, 200f);
            Sprite defaultIcon = PrepareSprite(DefaultIconPath, 200f);
            Sprite rounded = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);
            TMP_FontAsset notoMedium = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NotoMediumPath);
            TMP_FontAsset notoSemiBold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NotoSemiBoldPath);
            TMP_FontAsset novecentoNormal = EnsureFontAsset(NovecentoNormalFontPath, NovecentoNormalSdfPath);

            var root = new GameObject("Action Progress Window", typeof(RectTransform));
            try
            {
                var rect = (RectTransform)root.transform;
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(WindowWidth, WindowHeight);
                rect.anchoredPosition = new Vector2(0f, DefaultBottomOffset);

                var bg = root.AddComponent<Image>();
                bg.sprite = background;
                bg.raycastTarget = true;

                var iconRect = NewChild(root.transform, "Icon");
                Place(iconRect, 14f, 11f, 64f, 64f);
                var icon = iconRect.gameObject.AddComponent<Image>();
                icon.sprite = defaultIcon;
                icon.preserveAspect = true;
                icon.raycastTarget = false;

                var body = NewChild(root.transform, "Body");
                body.anchorMin = Vector2.zero;
                body.anchorMax = Vector2.one;
                body.pivot = new Vector2(0f, 1f);
                body.offsetMin = new Vector2(96f, 0f);
                body.offsetMax = new Vector2(-(WindowWidth - 364f), 0f);

                var actionName = AddText(body, "Action Name", "Action Name", notoMedium, 16f, TitleInk, TextAlignmentOptions.TopLeft, 0f, FontStyles.Normal);
                PlaceInBody(actionName.rectTransform, 5f, 9f, 240f, 24f);
                actionName.overflowMode = TextOverflowModes.Overflow;

                var trackBorder = NewChild(body, "Track Border");
                StretchRow(trackBorder, 37f, 11f, -2f, -2f);
                AddRounded(trackBorder.gameObject, rounded, TrackBorderInk, RoundedBorderPx / 5.5f);

                var track = NewChild(body, "Track");
                StretchRow(track, 39f, 7f, 0f, 0f);
                AddRounded(track.gameObject, rounded, TrackInk, RoundedBorderPx / 3.5f);

                var fill = NewChild(track, "Fill");
                fill.anchorMin = Vector2.zero;
                fill.anchorMax = new Vector2(120f / 268f, 1f);
                fill.offsetMin = Vector2.zero;
                fill.offsetMax = Vector2.zero;
                var fillImage = AddRounded(fill.gameObject, rounded, ActionProgressPalette.Cyan, RoundedBorderPx / 3.5f);

                var actionType = AddText(body, "Action Type", "Action", notoSemiBold, 16f, TypeInk, TextAlignmentOptions.TopLeft, 1.5f, FontStyles.UpperCase);
                PlaceInBody(actionType.rectTransform, 5f, 52f, 160f, 27f);
                actionType.overflowMode = TextOverflowModes.Overflow;

                var time = AddText(body, "Time", "99.99<size=66%>s</size>", novecentoNormal, 32f, TimeInk, TextAlignmentOptions.TopRight, 2f, FontStyles.Normal);
                var timeRect = time.rectTransform;
                timeRect.anchorMin = new Vector2(1f, 1f);
                timeRect.anchorMax = new Vector2(1f, 1f);
                timeRect.pivot = new Vector2(1f, 1f);
                timeRect.anchoredPosition = new Vector2(0f, -44f);
                timeRect.sizeDelta = new Vector2(109f, 38f);
                time.richText = true;
                time.overflowMode = TextOverflowModes.Overflow;

                var window = root.AddComponent<ActionProgressWindow>();
                var so = new SerializedObject(window);
                so.FindProperty("_background").objectReferenceValue = bg;
                so.FindProperty("_backgroundWithIcon").objectReferenceValue = background;
                so.FindProperty("_backgroundNoIcon").objectReferenceValue = backgroundNoIcon;
                so.FindProperty("_iconRoot").objectReferenceValue = iconRect.gameObject;
                so.FindProperty("_icon").objectReferenceValue = icon;
                so.FindProperty("_body").objectReferenceValue = body;
                so.FindProperty("_bodyLeftWithIcon").floatValue = 96f;
                so.FindProperty("_bodyLeftNoIcon").floatValue = 14f;
                so.FindProperty("_actionName").objectReferenceValue = actionName;
                so.FindProperty("_actionType").objectReferenceValue = actionType;
                so.FindProperty("_time").objectReferenceValue = time;
                so.FindProperty("_fill").objectReferenceValue = fill;
                so.FindProperty("_fillImage").objectReferenceValue = fillImage;
                so.FindProperty("_isDraggable").boolValue = true;
                so.FindProperty("_clampToCanvas").boolValue = true;
                so.ApplyModifiedPropertiesWithoutUndo();

                Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                RegisterWindow(prefab);
                AssetDatabase.SaveAssets();
                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        public static void RegisterWindow(GameObject prefab)
        {
            var registry = AssetDatabase.LoadAssetAtPath<UIWindowRegistrySO>(RegistryAssetPath);
            if (registry == null || prefab == null) return;

            var so = new SerializedObject(registry);
            var entries = so.FindProperty("_entries");
            SerializedProperty entry = null;
            for (int i = 0; i < entries.arraySize; i++)
            {
                var element = entries.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("WindowName").stringValue == ActionProgressWindow.DefaultWindowName)
                {
                    entry = element;
                    break;
                }
            }
            if (entry == null)
            {
                entries.InsertArrayElementAtIndex(entries.arraySize);
                entry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                entry.FindPropertyRelative("Keys").ClearArray();
            }
            entry.FindPropertyRelative("WindowName").stringValue = ActionProgressWindow.DefaultWindowName;
            entry.FindPropertyRelative("Type").enumValueIndex = (int)UIType.Window;
            entry.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
            entry.FindPropertyRelative("DestroyOnClose").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(registry);
        }
        #endregion

        #region [Private Methods]
        private static Sprite PrepareSprite(string path, float pixelsPerUnit)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool dirty = importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single
                             || !Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit) || importer.mipmapEnabled
                             || importer.textureCompression != TextureImporterCompression.Uncompressed;
                if (dirty)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = pixelsPerUnit;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static TMP_FontAsset EnsureFontAsset(string fontPath, string assetPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null) return existing;

            var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (font == null) return null;

            var asset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
            asset.name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(asset, assetPath);
            asset.material.name = asset.name + " Material";
            AssetDatabase.AddObjectToAsset(asset.material, asset);
            asset.atlasTextures[0].name = asset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
            asset.TryAddCharacters("0123456789.s");
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static RectTransform NewChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void PlaceInBody(RectTransform rect, float x, float y, float width, float height)
        {
            Place(rect, x, y, width, height);
        }

        private static void StretchRow(RectTransform rect, float y, float height, float left, float right)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(left, -y - height);
            rect.offsetMax = new Vector2(-right, -y);
        }

        private static Image AddRounded(GameObject go, Sprite rounded, Color color, float pixelsPerUnitMultiplier)
        {
            var image = go.AddComponent<Image>();
            image.sprite = rounded;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TextMeshProUGUI AddText(Transform parent, string name, string content, TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment, float letterSpacingPx, FontStyles style)
        {
            var rect = NewChild(parent, name);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.text = content;
            text.fontSize = size;
            text.enableAutoSizing = false;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.characterSpacing = letterSpacingPx / size * 100f;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            return text;
        }

        private static Color Hex(int rgb)
        {
            return new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
        }
        #endregion
    }
}
