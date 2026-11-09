using System.Collections.Generic;
using System.Linq;
using CoreSystem.UI;
using CoreSystem.UI.ClassSelect;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.Editor.UI
{
    public static class ClassSelectPrefabBuilder
    {
        #region [Variables/Fields]
        public const string PrefabPath = "Assets/Prefab/Game Page/Class Select Modal.prefab";
        public static readonly Vector2 FigmaPosition = new Vector2(-23.5f, 6f);

        private const string ArtFolder = "Assets/Art/UI/New/ClassSelect/";
        private const string FlowMaterialPath = "Assets/Art/UI/New/ClassSelect/PrismaticFlow.mat";
        private const string FlowShaderName = "Custom/UI/PrismaticFlow";
        private const string RegistryPath = "Assets/Prefab/UIWindowRegistry.asset";
        private const string HeadingFontPath = "Assets/Art/Front/Novecento Sans/Novecentosanswide-Normal SDF.asset";
        private const string BodyFontPath = "Assets/Art/Front/Noto Sans Thai/NotoSansThai-Medium SDF.asset";
        private const string Description = "Declare your path to survival... Choose a starting profession to pave the way to strength, and remember that 'profession' is just the beginning. You can freely transcend the boundaries of power and develop every skill through the Soul Blueprint.";
        private const float CardWidth = 261f;
        private const float CardHeight = 586f;
        private const float CardY = 127f;
        private const float IconScale = 0.5f;
        private const float FrameFadeDuration = 0.15f;

        private static readonly Vector2 PanelSize = new Vector2(1539f, 840f);
        private static readonly Vector2 BackdropSize = new Vector2(8000f, 8000f);
        private static readonly Color CaptionColor = new Color32(0x87, 0x87, 0x87, 0xFF);
        private static readonly Color BackdropColor = new Color(0f, 0f, 0f, 0.5f);

        private static readonly (string label, string icon, float x, Vector2 iconCentre)[] Cards =
        {
            ("Warrior", "Icon_Warrior.png", 127f, new Vector2(130.17f, 264f)),
            ("Mage", "Icon_Mage.png", 467.93f, new Vector2(131.22f, 263.15f)),
            ("Archer", "Icon_Archer.png", 808.85f, new Vector2(129.5f, 249.3f)),
            ("Assassin", "Icon_Assassin.png", 1149.78f, new Vector2(131.25f, 256.75f))
        };
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/UI/Build Class Select")]
        public static void Build()
        {
            ImportArt();
            var root = new GameObject("Class Select Modal", typeof(RectTransform));
            Stretch(root.GetComponent<RectTransform>());
            var modal = root.AddComponent<ClassSelectModal>();

            var backdrop = NewChild(root.transform, "Backdrop");
            backdrop.anchorMin = backdrop.anchorMax = new Vector2(0.5f, 0.5f);
            backdrop.sizeDelta = BackdropSize;
            backdrop.gameObject.AddComponent<Image>().color = BackdropColor;

            var panel = NewChild(root.transform, "Panel");
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = PanelSize;
            panel.anchoredPosition = Vector2.zero;
            AddImage(panel, "ModalBody.png", true);

            AddText(panel, "Title", "CLASS MASTERY", HeadingFontPath, 20f, Color.white, TextAlignmentOptions.Center, new Rect(520f, 30f, 499f, 54f), 61f);
            AddSprite(panel, "Divider", "Divider.png", new Rect(532f, 89f, 473f, 26f));

            var slots = new List<ClassCardSlot>();
            foreach (var card in Cards) slots.Add(BuildCard(panel, card.label, card.icon, card.x, card.iconCentre));

            AddSpriteAt(panel, "Help Icon", "Icon_Help.png", new Vector2(61f, 776f));
            var description = AddText(panel, "Description", Description, BodyFontPath, 16f, Color.white, TextAlignmentOptions.TopLeft, new Rect(86f, 760f, 1397f, 48f), 0f);
            description.textWrappingMode = TextWrappingModes.Normal;

            var close = AddSpriteAt(panel, "Close", "Icon_Close.png", new Vector2(1509f, 31f));
            close.GetComponent<Image>().raycastTarget = true;
            var closeButton = close.gameObject.AddComponent<Button>();

            var serialized = new SerializedObject(modal);
            var cards = serialized.FindProperty("_cards");
            cards.arraySize = slots.Count;
            for (int i = 0; i < slots.Count; i++) cards.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
            serialized.FindProperty("_closeButton").objectReferenceValue = closeButton;
            serialized.FindProperty("_closeOnOverlayClick").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            Register(prefab);
            AssetDatabase.SaveAssets();
            Debug.Log("Class Select Modal built.");
        }

        public static void BuildBatch()
        {
            Build();
        }
        #endregion

        #region [Private Methods]
        private static ClassCardSlot BuildCard(RectTransform panel, string label, string icon, float x, Vector2 iconCentre)
        {
            var card = NewChild(panel, $"Card {label}");
            Place(card, new Rect(x, CardY, CardWidth, CardHeight));
            AddImage(card, "CardBase.png", true);

            AddSprite(card, "Prismatic Frame", "CardFrame.png", new Rect(0f, 0f, CardWidth, CardHeight)).GetComponent<Image>().material = FlowMaterial();
            RectTransform frame = AddSprite(card, "Frame", "CardFrame.png", new Rect(0f, 0f, CardWidth, CardHeight));
            Button button = AddCardButton(card, frame.GetComponent<Image>());
            AddSpriteAt(card, "Icon", icon, iconCentre);
            AddText(card, "Caption", "CLASS", HeadingFontPath, 12f, CaptionColor, TextAlignmentOptions.Center, new Rect(79f, 412f, 103f, 30f), 0f);
            AddText(card, "Name", label, HeadingFontPath, 32f, Color.white, TextAlignmentOptions.Center, new Rect(60f, 427f, 141f, 63f), 0f);

            var slot = card.gameObject.AddComponent<ClassCardSlot>();
            var serialized = new SerializedObject(slot);
            serialized.FindProperty("_classLabel").stringValue = label;
            serialized.FindProperty("_button").objectReferenceValue = button;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return slot;
        }

        private static Material FlowMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(FlowMaterialPath);
            if (material != null) return material;
            material = new Material(Shader.Find(FlowShaderName));
            AssetDatabase.CreateAsset(material, FlowMaterialPath);
            return material;
        }

        private static Button AddCardButton(RectTransform card, Image frame)
        {
            Color hidden = new Color(1f, 1f, 1f, 0f);
            var button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = frame;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.disabledColor = Color.white;
            colors.highlightedColor = hidden;
            colors.pressedColor = hidden;
            colors.selectedColor = hidden;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = FrameFadeDuration;
            button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            return button;
        }

        private static void ImportArt()
        {
            AssetDatabase.Refresh();
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { ArtFolder.TrimEnd('/') }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
            }
        }

        private static Sprite Art(string file)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + file);
        }

        private static RectTransform NewChild(Transform parent, string name)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child.GetComponent<RectTransform>();
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Place(RectTransform rect, Rect figma)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(figma.x, -figma.y);
            rect.sizeDelta = figma.size;
        }

        private static Image AddImage(RectTransform rect, string file, bool raycast)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = Art(file);
            image.raycastTarget = raycast;
            return image;
        }

        private static RectTransform AddSprite(RectTransform parent, string name, string file, Rect figma)
        {
            var rect = NewChild(parent, name);
            Place(rect, figma);
            AddImage(rect, file, false);
            return rect;
        }

        private static RectTransform AddSpriteAt(RectTransform parent, string name, string file, Vector2 centre)
        {
            Vector2 size = Art(file).rect.size * IconScale;
            return AddSprite(parent, name, file, new Rect(centre - size * 0.5f, size));
        }

        private static TextMeshProUGUI AddText(RectTransform parent, string name, string value, string fontPath, float size, Color color, TextAlignmentOptions alignment, Rect figma, float spacing)
        {
            var rect = NewChild(parent, name);
            Place(rect, figma);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.characterSpacing = spacing;
            text.enableAutoSizing = false;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }

        private static void Register(GameObject prefab)
        {
            var registry = AssetDatabase.LoadAssetAtPath<UIWindowRegistrySO>(RegistryPath);
            UIWindowEntry entry = registry.Entries.FirstOrDefault(e => e.WindowName == ClassSelectModal.WindowName);
            if (entry == null)
            {
                entry = new UIWindowEntry { WindowName = ClassSelectModal.WindowName, Type = UIType.Modal, Keys = new List<KeyCode>() };
                registry.Entries.Add(entry);
            }

            entry.Prefab = prefab;
            EditorUtility.SetDirty(registry);
        }
        #endregion
    }
}
