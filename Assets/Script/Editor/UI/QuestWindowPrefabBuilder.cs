using System.Collections.Generic;
using System.Linq;
using CoreSystem.UI;
using CoreSystem.UI.Quests;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.Editor.UI
{
    public static class QuestWindowPrefabBuilder
    {
        #region [Variables/Fields]
        public const string PrefabPath = "Assets/Prefab/Game Page/Quest Window.prefab";

        private const string RowPrefabPath = "Assets/Prefab/Game Page/Quest Row Slot.prefab";
        private const string ObjectivePrefabPath = "Assets/Prefab/Game Page/Quest Objective Row.prefab";
        private const string ItemSlotPrefabPath = "Assets/Prefab/UI/ItemSlot.prefab";
        private const string RegistryPath = "Assets/Prefab/UIWindowRegistry.asset";
        private const string ArtFolder = "Assets/Art/UI/New/Quest/";
        private const string DemiBold = "Assets/Art/Front/Novecento Sans/Novecentosanswide-DemiBold SDF.asset";
        private const string Normal = "Assets/Art/Front/Novecento Sans/Novecentosanswide-Normal SDF.asset";
        private const string KanitBold = "Assets/Art/Front/Kanit/Kanit-Bold SDF.asset";
        private const string KanitSemi = "Assets/Art/Front/Kanit/Kanit-SemiBold SDF.asset";
        private const string KanitMedium = "Assets/Art/Front/Kanit/Kanit-Medium SDF.asset";
        private const string KanitRegular = "Assets/Art/Front/Kanit/Kanit-Regular SDF.asset";
        private const string KanitLight = "Assets/Art/Front/Kanit/Kanit-Light SDF.asset";
        private const float RightWidth = 1099f;
        private const float RowWidth = 337f;
        private const float RowHeight = 62f;
        private const float ObjectiveHeight = 42f;
        private const float RowGap = 8f;
        private const float SectionGap = 10f;

        private static readonly Vector2 PanelSize = new Vector2(1539f, 705f);
        private static readonly Color Dark = new Color32(0x14, 0x13, 0x13, 0xFF);
        private static readonly Color Muted = new Color32(0x61, 0x61, 0x61, 0xFF);
        private static readonly Color Line = new Color32(0x35, 0x35, 0x35, 0xFF);
        private static readonly Color Track = new Color32(0xBD, 0xBD, 0xBD, 0xFF);
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/UI/Build Quest Window")]
        public static void Build()
        {
            ImportArt();
            QuestRowSlot row = BuildRowPrefab();
            QuestObjectiveRow objective = BuildObjectivePrefab();

            var root = new GameObject("Quest Window", typeof(RectTransform));
            Stretch(root.GetComponent<RectTransform>());
            var window = root.AddComponent<QuestWindow>();

            var panel = NewChild(root.transform, "Panel");
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = PanelSize;
            panel.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
            Sprite(panel, "Body", "ModalBody.png", new Rect(-3f, -3f, 1545f, 711f), false);

            BuildHeader(panel, out Button close);
            var list = BuildList(panel);
            var details = BuildDetails(panel);
            BuildFooter(panel, out Button reject, out Button track, out TMP_Text trackLabel);

            var serialized = new SerializedObject(window);
            Set(serialized, "_rowPrefab", row);
            Set(serialized, "_activeRoot", list.active);
            Set(serialized, "_completedRoot", list.completed);
            Set(serialized, "_completedHeading", list.completedHeading);
            Set(serialized, "_trackedCount", list.count);
            Set(serialized, "_details", details.scroll.gameObject);
            Set(serialized, "_emptyText", details.empty);
            Set(serialized, "_name", details.name);
            Set(serialized, "_giver", details.giver);
            Set(serialized, "_status", details.status);
            Set(serialized, "_objectivePrefab", objective);
            Set(serialized, "_objectiveRoot", details.objectives);
            Set(serialized, "_description", details.description);
            Set(serialized, "_itemSlotPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ItemSlotPrefabPath));
            Set(serialized, "_rewardRoot", details.rewards);
            Set(serialized, "_baseExp", details.baseExp);
            Set(serialized, "_jobExp", details.jobExp);
            Set(serialized, "_detailsScroll", details.scroll);
            Set(serialized, "_closeButton", close);
            Set(serialized, "_rejectButton", reject);
            Set(serialized, "_trackButton", track);
            Set(serialized, "_trackLabel", trackLabel);
            serialized.FindProperty("_isDraggable").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            Register(prefab);
            AssetDatabase.SaveAssets();
            Debug.Log("Quest Window built. Press L in Play mode.");
        }

        public static void BuildBatch()
        {
            Build();
        }
        #endregion

        #region [Private Methods]
        private static void BuildHeader(RectTransform panel, out Button close)
        {
            var tag = NewChild(panel, "Title Tag");
            Place(tag, new Rect(24f, 26f, 200f, 41f));
            var tagImage = tag.gameObject.AddComponent<Image>();
            tagImage.sprite = Art("Tag_Round.png");
            tagImage.type = Image.Type.Sliced;
            tagImage.pixelsPerUnitMultiplier = 2f;
            tagImage.raycastTarget = false;
            Sprite(tag, "Icon", "Icon_Quest.png", new Rect(10f, 6f, 30f, 30f), false);
            Label(tag, "Title", "Quest", KanitBold, 24f, Color.white, TextAlignmentOptions.MidlineLeft, new Rect(53f, 0f, 144f, 41f));

            var closeRect = Sprite(panel, "Close", "Icon_Close.png", new Rect(1477f, 31f, 30f, 30f), true);
            close = closeRect.gameObject.AddComponent<Button>();
        }

        private static (RectTransform active, RectTransform completed, GameObject completedHeading, TMP_Text count) BuildList(RectTransform panel)
        {
            RectTransform content = BuildScroll(panel, "List Scroll", new Rect(24f, 104f, 342f, 490f), new Rect(348f, 104f, 6f, 490f), out _);
            var column = content.gameObject.AddComponent<VerticalLayoutGroup>();
            ConfigureColumn(column, 6f);

            var activeHeading = LayoutChild(content, "Active Heading", 342f, 26f);
            Label(activeHeading, "Label", "Active", DemiBold, 16f, Dark, TextAlignmentOptions.MidlineLeft, new Rect(0f, 0f, 240f, 26f));
            TMP_Text count = Label(activeHeading, "Count", "0 / 5", KanitRegular, 15f, Muted, TextAlignmentOptions.MidlineRight, new Rect(236f, 0f, 100f, 26f));

            RectTransform active = Group(content, "Active Rows", RowGap);
            var completedHeading = LayoutChild(content, "Completed Heading", 342f, 26f);
            Label(completedHeading, "Label", "Completed", DemiBold, 16f, Dark, TextAlignmentOptions.MidlineLeft, new Rect(0f, 0f, 240f, 26f));
            RectTransform completed = Group(content, "Completed Rows", RowGap);
            return (active, completed, completedHeading.gameObject, count);
        }

        private static (ScrollRect scroll, GameObject empty, TMP_Text name, TMP_Text giver, TMP_Text status, RectTransform objectives, TMP_Text description, RectTransform rewards, TMP_Text baseExp, TMP_Text jobExp) BuildDetails(RectTransform panel)
        {
            RectTransform content = BuildScroll(panel, "Details Scroll", new Rect(410f, 104f, RightWidth, 511f), new Rect(1515f, 104f, 6f, 511f), out ScrollRect scroll);
            var column = content.gameObject.AddComponent<VerticalLayoutGroup>();
            ConfigureColumn(column, SectionGap);

            var head = LayoutChild(content, "Quest Head", RightWidth, 66f);
            TMP_Text name = Label(head, "Name", string.Empty, DemiBold, 30f, Dark, TextAlignmentOptions.MidlineLeft, new Rect(0f, 0f, 800f, 44f));
            TMP_Text giver = Label(head, "Giver", string.Empty, KanitRegular, 15f, Muted, TextAlignmentOptions.MidlineLeft, new Rect(0f, 42f, 800f, 24f));
            TMP_Text status = Label(head, "Status", string.Empty, KanitSemi, 14f, Dark, TextAlignmentOptions.MidlineRight, new Rect(RightWidth - 300f, 8f, 300f, 28f));

            Section(content, "Objectives", 118f);
            RectTransform objectives = Group(content, "Objective Rows", 4f);

            Section(content, "Description", 126f);
            TMP_Text description = Label(content, "Description", string.Empty, KanitLight, 18f, Dark, TextAlignmentOptions.TopLeft, new Rect(0f, 0f, RightWidth, 30f));
            description.textWrappingMode = TextWrappingModes.Normal;

            Section(content, "Rewards", 102f);
            var rewardRow = LayoutChild(content, "Reward Row", RightWidth, 96f);
            var slots = NewChild(rewardRow, "Item Slots");
            Place(slots, new Rect(0f, 0f, 520f, 96f));
            var slotLayout = slots.gameObject.AddComponent<HorizontalLayoutGroup>();
            slotLayout.spacing = 8f;
            slotLayout.childForceExpandWidth = false;
            slotLayout.childForceExpandHeight = false;
            Label(rewardRow, "Base Label", "Base EXP", KanitRegular, 18f, Muted, TextAlignmentOptions.MidlineLeft, new Rect(540f, 6f, 140f, 40f));
            TMP_Text baseExp = Label(rewardRow, "Base Value", "0", KanitSemi, 18f, Dark, TextAlignmentOptions.MidlineRight, new Rect(680f, 6f, 120f, 40f));
            Label(rewardRow, "Job Label", "Job EXP", KanitRegular, 18f, Muted, TextAlignmentOptions.MidlineLeft, new Rect(540f, 50f, 140f, 40f));
            TMP_Text jobExp = Label(rewardRow, "Job Value", "0", KanitSemi, 18f, Dark, TextAlignmentOptions.MidlineRight, new Rect(680f, 50f, 120f, 40f));

            TMP_Text empty = Label(panel, "Empty", "No quests.", KanitLight, 20f, Muted, TextAlignmentOptions.Center, new Rect(410f, 104f, RightWidth, 511f));
            return (scroll, empty.gameObject, name, giver, status, objectives, description, slots, baseExp, jobExp);
        }

        private static void BuildFooter(RectTransform panel, out Button reject, out Button track, out TMP_Text trackLabel)
        {
            reject = Action(panel, "Reject Quest", "Button_Outline.png", Dark, 1035f, out _);
            track = Action(panel, "Track Quest", "Button_Outline.png", Dark, 1284f, out trackLabel);
        }

        private static Button Action(RectTransform panel, string label, string art, Color color, float x, out TMP_Text text)
        {
            RectTransform rect = Sprite(panel, label, art, new Rect(x, 631f, 225f, 50f), true);
            var button = rect.gameObject.AddComponent<Button>();
            text = Label(rect, "Label", label, KanitSemi, 18f, color, TextAlignmentOptions.Center, new Rect(0f, 0f, 225f, 50f));
            return button;
        }

        private static QuestRowSlot BuildRowPrefab()
        {
            var root = new GameObject("Quest Row Slot", typeof(RectTransform));
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(RowWidth, RowHeight);
            var layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = RowWidth;
            layout.preferredHeight = RowHeight;
            var rect = root.GetComponent<RectTransform>();
            var hit = root.AddComponent<Image>();
            hit.color = new Color(1f, 1f, 1f, 0f);
            var group = root.AddComponent<CanvasGroup>();
            var button = root.AddComponent<Button>();
            button.transition = Selectable.Transition.None;

            RectTransform normal = Sprite(rect, "Normal", "Row_Normal.png", new Rect(0f, 0f, RowWidth, RowHeight), false);
            RectTransform selected = Sprite(rect, "Selected", "Row_Selected.png", new Rect(0f, 0f, RowWidth, RowHeight), false);
            RectTransform badge = Sprite(rect, "Tracked Badge", "Tracked_Badge.png", new Rect(150f, 3f, 184f, 56f), false);
            RectTransform dark = Sprite(rect, "Star Dark", "Star_Dark.png", new Rect(12f, 11f, 40f, 40f), false);
            RectTransform white = Sprite(rect, "Star White", "Star_White.png", new Rect(12f, 11f, 40f, 40f), false);
            TMP_Text label = Label(rect, "Label", string.Empty, Normal, 19f, Dark, TextAlignmentOptions.MidlineLeft, new Rect(60f, 0f, 270f, RowHeight));
            label.overflowMode = TextOverflowModes.Ellipsis;

            var slot = root.AddComponent<QuestRowSlot>();
            var serialized = new SerializedObject(slot);
            Set(serialized, "_button", button);
            Set(serialized, "_label", label);
            Set(serialized, "_normalBackground", normal.gameObject);
            Set(serialized, "_selectedBackground", selected.gameObject);
            Set(serialized, "_trackedBadge", badge.gameObject);
            Set(serialized, "_darkStar", dark.gameObject);
            Set(serialized, "_whiteStar", white.gameObject);
            Set(serialized, "_group", group);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, RowPrefabPath);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<QuestRowSlot>();
        }

        private static QuestObjectiveRow BuildObjectivePrefab()
        {
            var root = new GameObject("Quest Objective Row", typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(RightWidth, ObjectiveHeight);
            var layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = RightWidth;
            layout.preferredHeight = ObjectiveHeight;
            var image = root.AddComponent<Image>();
            image.sprite = Art("Objective_Row.png");
            image.raycastTarget = false;

            TMP_Text label = Label(rect, "Label", string.Empty, KanitMedium, 18f, Dark, TextAlignmentOptions.MidlineLeft, new Rect(20f, 0f, 860f, ObjectiveHeight));
            TMP_Text value = Label(rect, "Value", string.Empty, KanitSemi, 18f, Dark, TextAlignmentOptions.MidlineRight, new Rect(RightWidth - 220f, 0f, 200f, ObjectiveHeight));

            var row = root.AddComponent<QuestObjectiveRow>();
            var serialized = new SerializedObject(row);
            Set(serialized, "_label", label);
            Set(serialized, "_value", value);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, ObjectivePrefabPath);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<QuestObjectiveRow>();
        }

        private static RectTransform BuildScroll(RectTransform panel, string name, Rect area, Rect bar, out ScrollRect scroll)
        {
            var host = NewChild(panel, name);
            Place(host, area);
            scroll = host.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            var viewport = NewChild(host, "Viewport");
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            var hit = viewport.gameObject.AddComponent<Image>();
            hit.color = new Color(1f, 1f, 1f, 0f);

            var content = NewChild(viewport, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;
            scroll.verticalScrollbar = BuildScrollbar(panel, name + " Bar", bar);
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            return content;
        }

        private static Scrollbar BuildScrollbar(RectTransform panel, string name, Rect area)
        {
            var track = NewChild(panel, name);
            Place(track, area);
            Rounded(track, Track);

            var sliding = NewChild(track, "Sliding Area");
            Stretch(sliding);
            var handle = NewChild(sliding, "Handle");
            Stretch(handle);
            Image handleImage = Rounded(handle, Dark);

            var bar = track.gameObject.AddComponent<Scrollbar>();
            bar.direction = Scrollbar.Direction.BottomToTop;
            bar.handleRect = handle;
            bar.targetGraphic = handleImage;
            return bar;
        }

        private static Image Rounded(RectTransform rect, Color color)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = Art("Scroll_Round.png");
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 2f;
            image.color = color;
            return image;
        }

        private static void Section(RectTransform parent, string title, float pillWidth)
        {
            var section = LayoutChild(parent, "Section " + title, RightWidth, 28f);
            float lineWidth = (RightWidth - pillWidth) * 0.5f;

            Bar(section, "Line Left", new Rect(4f, 13f, lineWidth - 4f, 2f));
            Bar(section, "Line Right", new Rect(lineWidth + pillWidth, 13f, lineWidth - 4f, 2f));
            Sprite(section, "Dot Left", "Section_Dot.png", new Rect(0f, 10f, 8f, 8f), false);
            Sprite(section, "Dot Right", "Section_Dot.png", new Rect(RightWidth - 8f, 10f, 8f, 8f), false);

            var pill = NewChild(section, "Pill");
            Place(pill, new Rect(lineWidth, 0f, pillWidth, 28f));
            var pillImage = pill.gameObject.AddComponent<Image>();
            pillImage.sprite = Art("Section_Pill.png");
            pillImage.type = Image.Type.Sliced;
            pillImage.pixelsPerUnitMultiplier = 2f;
            pillImage.raycastTarget = false;
            Label(pill, "Label", title.ToUpperInvariant(), KanitSemi, 13f, Line, TextAlignmentOptions.Center, new Rect(0f, 0f, pillWidth, 28f)).characterSpacing = 8f;
        }

        private static void Bar(RectTransform parent, string name, Rect figma)
        {
            var rect = NewChild(parent, name);
            Place(rect, figma);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = Line;
            image.raycastTarget = false;
        }

        private static void ConfigureColumn(VerticalLayoutGroup column, float spacing)
        {
            column.spacing = spacing;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = false;
            column.childForceExpandHeight = false;
        }

        private static RectTransform Group(RectTransform parent, string name, float spacing)
        {
            var rect = NewChild(parent, name);
            var column = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            ConfigureColumn(column, spacing);
            return rect;
        }

        private static RectTransform LayoutChild(RectTransform parent, string name, float width, float height)
        {
            var rect = NewChild(parent, name);
            var layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;
            layout.flexibleWidth = 0f;
            layout.flexibleHeight = 0f;
            return rect;
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
                importer.spriteBorder = SlicedBorder(path);
                importer.SaveAndReimport();
            }
        }

        private static Vector4 SlicedBorder(string path)
        {
            if (path.EndsWith("Section_Pill.png")) return Vector4.one * 28f;
            if (path.EndsWith("Scroll_Round.png")) return Vector4.one * 5f;
            if (path.EndsWith("Tag_Round.png")) return Vector4.one * 9f;
            return Vector4.zero;
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

        private static RectTransform Sprite(RectTransform parent, string name, string file, Rect figma, bool raycast)
        {
            var rect = NewChild(parent, name);
            Place(rect, figma);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = Art(file);
            image.raycastTarget = raycast;
            return rect;
        }

        private static TextMeshProUGUI Label(RectTransform parent, string name, string value, string fontPath, float size, Color color, TextAlignmentOptions alignment, Rect figma)
        {
            var rect = NewChild(parent, name);
            Place(rect, figma);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.enableAutoSizing = false;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }

        private static void Set(SerializedObject serialized, string property, Object value)
        {
            serialized.FindProperty(property).objectReferenceValue = value;
        }

        private static void Register(GameObject prefab)
        {
            var registry = AssetDatabase.LoadAssetAtPath<UIWindowRegistrySO>(RegistryPath);
            UIWindowEntry entry = registry.Entries.FirstOrDefault(e => e.WindowName == QuestWindow.WindowName);
            if (entry == null)
            {
                entry = new UIWindowEntry { WindowName = QuestWindow.WindowName, Keys = new List<KeyCode> { KeyCode.L } };
                registry.Entries.Add(entry);
            }

            entry.Type = UIType.Window;
            entry.Prefab = prefab;
            EditorUtility.SetDirty(registry);
        }
        #endregion
    }
}
