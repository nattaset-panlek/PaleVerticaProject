using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using CoreSystem.UI.Components;
using CoreSystem.UI.SkillManagePage;

namespace CoreSystem.Editor.UI
{
    public static class SkillManagePrefabBuilder
    {
        #region [Variables/Fields]
        public const string WindowPrefabPath = "Assets/Prefab/Game Page/Character SkillManager Window.prefab";
        public const string RowPrefabPath = "Assets/Prefab/Page UI/Skill Manage/Skill Attribute Row.prefab";
        public const string SectionPrefabPath = "Assets/Prefab/Page UI/Skill Manage/Skill Attribute Section.prefab";
        private const string ModifierFieldPrefabPath = "Assets/Prefab/Page UI/Skill Manage/Modifier Container.prefab";
        private const float ModifierFieldWidth = 466f;
        private const float ModifierFieldSpacing = 6f;
        private const string ListItemPrefabPath = "Assets/Prefab/Page UI/Skill Manage/Item Skill List.prefab";
        private const string TagPrefabPath = "Assets/Prefab/Page UI/Skill Manage/Skill Tag Element.prefab";
        private const string RowSpritePath = "Assets/Art/UI/New/Page/Skill Manager Page/UI_Skill_Attribute_Row.png";
        private const string RoundedSpritePath = "Assets/Art/UI/New/Rect/rounded.png";
        private const string StarIconPath = "Assets/Art/UI/New/Icon/UI_StarGylp_Icon.png";
        private const string KanitRegularPath = "Assets/Art/Front/Kanit/Kanit-Regular SDF.asset";
        private const string KanitMediumPath = "Assets/Art/Front/Kanit/Kanit-Medium SDF.asset";
        private const string KanitBoldPath = "Assets/Art/Front/Kanit/Kanit-Bold SDF.asset";
        private const string NovecentoPath = "Assets/Art/Front/Novecento Sans/Novecentosanswide-DemiBold SDF.asset";

        private const string EffectPageName = "Effect Page";
        private const string BasicPageName = "Basic Page";
        private const string AdvancePageName = "Advance Page";
        private const string PlaceholderName = "Placeholder";
        private const string PlaceholderTitle = "No Support Skill";
        private const string PlaceholderText = "Place a support gem in an open slot above to change how this skill works. Its effect will be listed here.";
        private const string TitleTagName = "Title Tag";
        private const float ArtInset = 3f;
        private const float RoundedBorder = 21f;
        private const float HeaderHeight = 50f;
        private const int EffectTabIndex = 1;
        private const int BasicTabIndex = 2;
        private const int AdvanceTabIndex = 3;
        private static readonly Color SectionInk = new Color(0.55f, 0.55f, 0.55f, 1f);

        private static readonly Color Ink = new Color(0.149f, 0.149f, 0.149f, 1f);
        private static readonly Color RowInk = new Color(0.212f, 0.212f, 0.212f, 1f);
        private static readonly Color TrackColor = new Color(0.62f, 0.62f, 0.62f, 1f);
        private static readonly Color HandleColor = new Color(0.263f, 0.263f, 0.263f, 1f);
        private static readonly Color NameInk = new Color(0.259f, 0.259f, 0.259f, 1f);
        private static readonly Color DamageInk = new Color(0.38f, 0.38f, 0.38f, 1f);
        private static readonly Color LevelLabelInk = new Color(0.624f, 0.624f, 0.624f, 1f);
        private static readonly Color LevelValueInk = new Color(0.89f, 0.89f, 0.89f, 1f);
        private static readonly Vector2 TagSize = new Vector2(80f, 30f);
        private const float TagSpacing = 13f;
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/UI/Build Skill Manage Pages")]
        public static void BuildFromMenu()
        {
            if (!EditorUtility.DisplayDialog("Build Skill Manage Pages", $"Rebuilds the Effect and Basic pages, the title tag and the tab wiring in\n{WindowPrefabPath}\nand the row prefab. Other parts of the window are not touched.", "Build", "Cancel")) return;
            Debug.Log(Build());
        }

        public static void BuildBatch()
        {
            int exitCode = 0;
            try
            {
                Debug.Log(Build());
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                exitCode = 1;
            }
            EditorApplication.Exit(exitCode);
        }

        public static string Build()
        {
            var log = new StringBuilder("[SkillManagePrefabBuilder]\n");
            PrepareRowSprite();
            var rowPrefab = BuildRowPrefab();
            var sectionPrefab = BuildSectionPrefab();
            log.AppendLine($"  Row prefab: {RowPrefabPath}");
            log.AppendLine(TuneListItem());
            log.AppendLine(TuneTagElement());
            log.AppendLine(TuneModifierField());

            var root = PrefabUtility.LoadPrefabContents(WindowPrefabPath);
            try
            {
                var window = (RectTransform)root.transform;
                var skillWindow = root.GetComponent<SkillManageWindow>();
                var container = FindPath(root.transform, "Right Container/Container");
                var modifier = FindPath(root.transform, "Right Container/Container/Modifier");
                var header = FindPath(root.transform, "Header");
                var headerTemplate = FindPath(root.transform, "Right Container/Container/Modifier/SupportSkill Header");
                var scrollTemplate = FindPath(root.transform, "Right Container/Container/Modifier/Modifier Effect Container/Scroll View");
                if (skillWindow == null || container == null || modifier == null || header == null || headerTemplate == null || scrollTemplate == null)
                {
                    return log.AppendLine("  Window hierarchy changed: SkillManageWindow, Container, Modifier, Header, SupportSkill Header or Modifier Scroll View not found.").ToString();
                }

                BuildTitleTag(window, header);
                TuneTagRow(root.transform);
                var effectPage = BuildEffectPage(window, container, skillWindow, headerTemplate, scrollTemplate);
                var basicPage = BuildBasicPage(window, container, skillWindow, headerTemplate, scrollTemplate, rowPrefab);
                var advancePage = BuildAdvancePage(window, container, skillWindow, headerTemplate, scrollTemplate, rowPrefab, sectionPrefab);
                WireTabs(root, modifier.gameObject, effectPage, basicPage, advancePage);
                TuneModifierContent(root.transform);
                BuildModifierPlaceholder(root.transform, skillWindow);
                WireFilter(root.transform, skillWindow);

                PrefabUtility.SaveAsPrefabAsset(root, WindowPrefabPath);
                log.AppendLine($"  Window: title tag, '{EffectPageName}', '{BasicPageName}', '{AdvancePageName}', modifier placeholder, filter menu and tab pages updated.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            return log.ToString();
        }
        #endregion

        #region [Private Methods]
        private static void BuildTitleTag(RectTransform window, Transform header)
        {
            Remove(header, TitleTagName);
            var tag = NewChild(header, TitleTagName);
            Place(tag, window, 31f, 25f, 136f, 40f);
            var tagImage = tag.gameObject.AddComponent<Image>();
            tagImage.sprite = LoadSprite(RoundedSpritePath);
            tagImage.type = Image.Type.Sliced;
            tagImage.pixelsPerUnitMultiplier = RoundedBorder / 5f;
            tagImage.color = Color.black;
            tagImage.raycastTarget = false;

            var icon = NewChild(tag, "Icon");
            SetTopLeft(icon, 10f, 4f, 32f, 32f);
            var iconImage = icon.gameObject.AddComponent<Image>();
            iconImage.sprite = LoadSprite(StarIconPath);
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            var title = AddText(tag, "Title", "Skill", Load<TMP_FontAsset>(KanitBoldPath), 24f, Color.white, TextAlignmentOptions.MidlineLeft);
            title.fontStyle = FontStyles.UpperCase;
            SetTopLeft(title.rectTransform, 56f, 0f, 76f, 40f);
            tag.SetAsLastSibling();
        }

        private static string TuneListItem()
        {
            var root = PrefabUtility.LoadPrefabContents(ListItemPrefabPath);
            try
            {
                var name = Text(root.transform, "Skill Summry/Skill Name");
                if (name != null)
                {
                    name.color = NameInk;
                    name.characterSpacing = 5f;
                    name.enableAutoSizing = false;
                    name.fontSize = 20f;
                    name.textWrappingMode = TextWrappingModes.NoWrap;
                    name.overflowMode = TextOverflowModes.Ellipsis;
                    name.rectTransform.sizeDelta = new Vector2(name.rectTransform.sizeDelta.x, 24f);
                }

                var damage = Text(root.transform, "Skill Summry/Damage");
                if (damage != null)
                {
                    damage.color = DamageInk;
                    damage.enableAutoSizing = false;
                    damage.fontSize = 16f;
                    damage.textWrappingMode = TextWrappingModes.NoWrap;
                    damage.overflowMode = TextOverflowModes.Ellipsis;
                    damage.alignment = TextAlignmentOptions.BottomLeft;
                    damage.fontStyle = FontStyles.UpperCase;
                    damage.rectTransform.sizeDelta = new Vector2(damage.rectTransform.sizeDelta.x, 24f);
                }

                var label = Text(root.transform, "Skill Level/Level");
                if (label != null)
                {
                    label.color = LevelLabelInk;
                    label.characterSpacing = 20f;
                    label.enableAutoSizing = false;
                }

                var value = Text(root.transform, "Skill Level/Value");
                if (value != null)
                {
                    value.color = LevelValueInk;
                    value.characterSpacing = 8f;
                    value.enableAutoSizing = false;
                    value.fontSize = 32f;
                    value.overflowMode = TextOverflowModes.Overflow;
                }

                PrefabUtility.SaveAsPrefabAsset(root, ListItemPrefabPath);
                return $"  List item tuned: {ListItemPrefabPath}";
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static string TuneTagElement()
        {
            var root = PrefabUtility.LoadPrefabContents(TagPrefabPath);
            try
            {
                var fitter = root.GetComponent<ContentSizeFitter>();
                if (fitter != null) Object.DestroyImmediate(fitter);

                var group = root.GetComponent<HorizontalOrVerticalLayoutGroup>();
                if (group != null)
                {
                    group.padding = new RectOffset(10, 10, 0, 0);
                    group.childAlignment = TextAnchor.MiddleCenter;
                    group.childControlWidth = true;
                    group.childControlHeight = true;
                    group.childForceExpandWidth = true;
                    group.childForceExpandHeight = true;
                }

                var le = root.GetComponent<LayoutElement>();
                if (le == null) le = root.AddComponent<LayoutElement>();
                le.minWidth = TagSize.x;
                le.minHeight = TagSize.y;
                le.preferredHeight = TagSize.y;
                le.flexibleWidth = 0f;
                le.flexibleHeight = 0f;
                ((RectTransform)root.transform).sizeDelta = TagSize;

                var text = root.GetComponentInChildren<TMP_Text>(true);
                if (text != null)
                {
                    text.font = Load<TMP_FontAsset>(KanitRegularPath);
                    text.fontSize = 16f;
                    text.enableAutoSizing = false;
                    text.alignment = TextAlignmentOptions.Center;
                    text.textWrappingMode = TextWrappingModes.NoWrap;
                    text.overflowMode = TextOverflowModes.Overflow;
                }

                PrefabUtility.SaveAsPrefabAsset(root, TagPrefabPath);
                return $"  Tag tuned: {TagPrefabPath}";
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void TuneTagRow(Transform root)
        {
            var scrollRoot = FindPath(root, "Right Container/Header/Skill Header/Type Tag/Scroll View");
            var scroll = scrollRoot != null ? scrollRoot.GetComponent<ScrollRect>() : null;
            if (scroll == null) return;

            if (scroll.horizontalScrollbar != null)
            {
                scroll.horizontalScrollbar.gameObject.SetActive(false);
                scroll.horizontalScrollbar = null;
            }
            scroll.horizontalScrollbarSpacing = 0f;
            if (scroll.viewport != null)
            {
                scroll.viewport.anchorMin = Vector2.zero;
                scroll.viewport.anchorMax = Vector2.one;
                scroll.viewport.offsetMin = Vector2.zero;
                scroll.viewport.offsetMax = Vector2.zero;
            }

            var group = scroll.content != null ? scroll.content.GetComponent<HorizontalLayoutGroup>() : null;
            if (group == null) return;
            group.spacing = TagSpacing;
            group.childAlignment = TextAnchor.MiddleLeft;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
        }

        private static TMP_Text Text(Transform root, string path)
        {
            var target = root.Find(path);
            return target != null ? target.GetComponent<TMP_Text>() : null;
        }

        private static GameObject BuildEffectPage(RectTransform window, Transform container, SkillManageWindow skillWindow, Transform headerTemplate, Transform scrollTemplate)
        {
            var page = NewPage(container, EffectPageName);
            var body = NewBody(page, window, 535f, 293.5f, 512f, 404f);

            AddHeader(body, headerTemplate, "Effect Header", "Effect", 112f);
            var effectText = AddText(body, "Effect Text", "Effect", Load<TMP_FontAsset>(KanitRegularPath), 18f, Ink, TextAlignmentOptions.TopLeft);
            effectText.textWrappingMode = TextWrappingModes.Normal;
            effectText.rectTransform.sizeDelta = new Vector2(498f, 81f);
            AddSpacer(body, 18f);
            AddHeader(body, headerTemplate, "Description Header", "Description", 170f);

            var scroll = AddScroll(body, scrollTemplate, "Description Scroll", 512f, 40f, 0, 0f, true);
            var description = AddText(scroll.content, "Description Text", "Description", Load<TMP_FontAsset>(KanitRegularPath), 18f, Ink, TextAlignmentOptions.TopLeft);
            description.textWrappingMode = TextWrappingModes.Normal;

            var view = page.gameObject.AddComponent<SkillEffectPageView>();
            var so = new SerializedObject(view);
            so.FindProperty("_window").objectReferenceValue = skillWindow;
            so.FindProperty("_effectText").objectReferenceValue = effectText;
            so.FindProperty("_descriptionText").objectReferenceValue = description;
            so.FindProperty("_descriptionScroll").objectReferenceValue = scroll;
            so.ApplyModifiedPropertiesWithoutUndo();

            page.gameObject.SetActive(false);
            return page.gameObject;
        }

        private static GameObject BuildBasicPage(RectTransform window, Transform container, SkillManageWindow skillWindow, Transform headerTemplate, Transform scrollTemplate, GameObject rowPrefab)
        {
            var page = NewPage(container, BasicPageName);
            var body = NewBody(page, window, 535f, 293.5f, 518f, 403.5f);

            AddHeader(body, headerTemplate, "Attributes Header", "Attributes", 162f);
            AddSpacer(body, 13.5f);
            var scroll = AddScroll(body, scrollTemplate, "Attributes Scroll", 518f, 22f, 4, 7f, false);

            var view = page.gameObject.AddComponent<SkillBasicPageView>();
            var so = new SerializedObject(view);
            so.FindProperty("_window").objectReferenceValue = skillWindow;
            so.FindProperty("_rowPrefab").objectReferenceValue = rowPrefab.GetComponent<SkillAttributeRowView>();
            so.FindProperty("_rowContainer").objectReferenceValue = scroll.content;
            so.FindProperty("_scroll").objectReferenceValue = scroll;
            so.ApplyModifiedPropertiesWithoutUndo();

            page.gameObject.SetActive(false);
            return page.gameObject;
        }

        private static RectTransform NewPage(Transform container, string name)
        {
            Remove(container, name);
            var page = NewChild(container, name);
            page.anchorMin = Vector2.zero;
            page.anchorMax = Vector2.one;
            page.offsetMin = Vector2.zero;
            page.offsetMax = Vector2.zero;
            return page;
        }

        private static RectTransform NewBody(RectTransform page, RectTransform window, float x, float y, float width, float height)
        {
            var body = NewChild(page, "Body");
            Place(body, window, x, y, width, height);
            var group = body.gameObject.AddComponent<VerticalLayoutGroup>();
            group.childAlignment = TextAnchor.UpperLeft;
            group.childControlWidth = false;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            group.spacing = 0f;
            return body;
        }

        private static void AddHeader(RectTransform body, Transform template, string name, string title, float pillWidth)
        {
            var header = (RectTransform)Object.Instantiate(template.gameObject, body, false).transform;
            header.name = name;
            header.sizeDelta = new Vector2(500f, HeaderHeight);
            var le = header.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = HeaderHeight;
            le.minHeight = HeaderHeight;

            if (header.childCount > 1)
            {
                var pill = (RectTransform)header.GetChild(1);
                pill.sizeDelta = new Vector2(pillWidth, pill.sizeDelta.y);
            }
            var text = header.GetComponentInChildren<TMP_Text>(true);
            if (text != null) text.text = title;
        }

        private static ScrollRect AddScroll(RectTransform body, Transform template, string name, float width, float scrollbarReserve, int topPadding, float spacing, bool controlWidth)
        {
            var go = Object.Instantiate(template.gameObject, body, false);
            go.name = name;
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(width, 100f);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 60f;
            le.flexibleHeight = 1f;

            var scroll = go.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            scroll.scrollSensitivity = Mathf.Max(scroll.scrollSensitivity, 25f);

            var viewport = scroll.viewport;
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.pivot = new Vector2(0f, 1f);
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = new Vector2(-scrollbarReserve, 0f);

            var content = scroll.content;
            for (int i = content.childCount - 1; i >= 0; i--) Object.DestroyImmediate(content.GetChild(i).gameObject);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var group = content.GetComponent<VerticalLayoutGroup>();
            if (group == null) group = content.gameObject.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(0, 0, topPadding, topPadding);
            group.spacing = spacing;
            group.childAlignment = TextAnchor.UpperLeft;
            group.childControlWidth = controlWidth;
            group.childControlHeight = true;
            group.childForceExpandWidth = controlWidth;
            group.childForceExpandHeight = false;
            var fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var bar = scroll.verticalScrollbar;
            if (bar != null)
            {
                var barRect = (RectTransform)bar.transform;
                barRect.anchorMin = new Vector2(1f, 0f);
                barRect.anchorMax = new Vector2(1f, 1f);
                barRect.pivot = new Vector2(1f, 1f);
                barRect.anchoredPosition = Vector2.zero;
                barRect.sizeDelta = new Vector2(4f, 0f);
                var track = bar.GetComponent<Image>();
                if (track != null) track.color = TrackColor;
                if (bar.handleRect != null)
                {
                    var handle = bar.handleRect.GetComponent<Image>();
                    if (handle != null) handle.color = HandleColor;
                }
            }
            scroll.verticalScrollbarSpacing = 0f;
            return scroll;
        }

        private static void AddSpacer(RectTransform body, float height)
        {
            var spacer = NewChild(body, "Spacer");
            spacer.sizeDelta = new Vector2(10f, height);
            var le = spacer.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
        }

        private static GameObject BuildAdvancePage(RectTransform window, Transform container, SkillManageWindow skillWindow, Transform headerTemplate, Transform scrollTemplate, GameObject rowPrefab, GameObject sectionPrefab)
        {
            var page = NewPage(container, AdvancePageName);
            var body = NewBody(page, window, 535f, 293.5f, 518f, 403.5f);

            AddHeader(body, headerTemplate, "Details Header", "Details", 140f);
            AddSpacer(body, 13.5f);
            var scroll = AddScroll(body, scrollTemplate, "Details Scroll", 518f, 22f, 4, 7f, false);

            var view = page.gameObject.AddComponent<SkillAdvancePageView>();
            var so = new SerializedObject(view);
            so.FindProperty("_window").objectReferenceValue = skillWindow;
            so.FindProperty("_rowPrefab").objectReferenceValue = rowPrefab.GetComponent<SkillAttributeRowView>();
            so.FindProperty("_sectionPrefab").objectReferenceValue = sectionPrefab.GetComponent<SkillAttributeRowView>();
            so.FindProperty("_rowContainer").objectReferenceValue = scroll.content;
            so.FindProperty("_scroll").objectReferenceValue = scroll;
            so.ApplyModifiedPropertiesWithoutUndo();

            page.gameObject.SetActive(false);
            return page.gameObject;
        }

        private static string TuneModifierField()
        {
            var root = PrefabUtility.LoadPrefabContents(ModifierFieldPrefabPath);
            try
            {
                var fitter = root.GetComponent<ContentSizeFitter>();
                if (fitter != null) Object.DestroyImmediate(fitter);
                var le = root.GetComponent<LayoutElement>();
                if (le == null) le = root.AddComponent<LayoutElement>();
                le.preferredWidth = ModifierFieldWidth;
                le.flexibleHeight = 0f;
                PrefabUtility.SaveAsPrefabAsset(root, ModifierFieldPrefabPath);
                return $"  Modifier field tuned: {ModifierFieldPrefabPath}";
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void TuneModifierContent(Transform root)
        {
            var content = FindPath(root, "Right Container/Container/Modifier/Modifier Effect Container/Scroll View/Viewport/Content");
            var group = content != null ? content.GetComponent<VerticalLayoutGroup>() : null;
            if (group == null) return;
            group.spacing = ModifierFieldSpacing;
            group.childControlWidth = false;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
        }

        private static void BuildModifierPlaceholder(Transform root, SkillManageWindow skillWindow)
        {
            var content = FindPath(root, "Right Container/Container/Modifier/Modifier Effect Container/Scroll View/Viewport/Content");
            if (content == null) return;
            Remove(content, PlaceholderName);

            var windowSo = new SerializedObject(skillWindow);
            var fieldPrefab = windowSo.FindProperty("_modifierFieldPrefab").objectReferenceValue as GameObject;
            if (fieldPrefab == null) return;

            var placeholder = (GameObject)PrefabUtility.InstantiatePrefab(fieldPrefab, content);
            placeholder.name = PlaceholderName;
            placeholder.transform.SetAsFirstSibling();
            var field = placeholder.GetComponent<ModelModifierFieldContainerComponent>();
            if (field != null) field.Setup(PlaceholderTitle.ToUpperInvariant(), PlaceholderText);
            var group = placeholder.GetComponent<CanvasGroup>();
            if (group == null) group = placeholder.AddComponent<CanvasGroup>();
            group.alpha = 0.7f;
            group.blocksRaycasts = false;

            windowSo.FindProperty("_modifierPlaceholder").objectReferenceValue = placeholder;
            windowSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireFilter(Transform root, SkillManageWindow skillWindow)
        {
            var button = FindPath(root, "Left Container/Item Field Search Container/Filter Button");
            if (button == null) return;

            var image = button.GetComponent<Image>();
            if (image != null) image.raycastTarget = true;
            var filter = button.GetComponent<SkillListFilterView>();
            if (filter == null) filter = button.gameObject.AddComponent<SkillListFilterView>();
            var filterSo = new SerializedObject(filter);
            filterSo.FindProperty("_window").objectReferenceValue = skillWindow;
            filterSo.ApplyModifiedPropertiesWithoutUndo();

            var windowSo = new SerializedObject(skillWindow);
            windowSo.FindProperty("_filter").objectReferenceValue = filter;
            windowSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject BuildSectionPrefab()
        {
            var root = new GameObject("Skill Attribute Section", typeof(RectTransform));
            ((RectTransform)root.transform).sizeDelta = new Vector2(496f, 30f);
            var le = root.AddComponent<LayoutElement>();
            le.minHeight = 30f;
            le.preferredHeight = 30f;
            le.preferredWidth = 496f;

            var label = AddText(root.transform, "Label", "Section", Load<TMP_FontAsset>(NovecentoPath), 15f, SectionInk, TextAlignmentOptions.BottomLeft);
            label.fontStyle = FontStyles.UpperCase;
            label.characterSpacing = 8f;
            Stretch(label.rectTransform, 8f, 8f);

            var view = root.AddComponent<SkillAttributeRowView>();
            var so = new SerializedObject(view);
            so.FindProperty("_label").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, SectionPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void WireTabs(GameObject root, GameObject modifier, GameObject effect, GameObject basic, GameObject advance)
        {
            var tabs = root.GetComponentInChildren<TabGroup>(true);
            if (tabs == null) return;

            var so = new SerializedObject(tabs);
            var list = so.FindProperty("tabs");
            if (list.arraySize > 0) list.GetArrayElementAtIndex(0).FindPropertyRelative("Page").objectReferenceValue = modifier;
            if (list.arraySize > EffectTabIndex) list.GetArrayElementAtIndex(EffectTabIndex).FindPropertyRelative("Page").objectReferenceValue = effect;
            if (list.arraySize > BasicTabIndex) list.GetArrayElementAtIndex(BasicTabIndex).FindPropertyRelative("Page").objectReferenceValue = basic;
            if (list.arraySize > AdvanceTabIndex) list.GetArrayElementAtIndex(AdvanceTabIndex).FindPropertyRelative("Page").objectReferenceValue = advance;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject BuildRowPrefab()
        {
            var root = new GameObject("Skill Attribute Row", typeof(RectTransform));
            ((RectTransform)root.transform).sizeDelta = new Vector2(496f, 45f);
            var image = root.AddComponent<Image>();
            image.sprite = LoadSprite(RowSpritePath);
            image.type = Image.Type.Sliced;
            image.raycastTarget = false;
            var le = root.AddComponent<LayoutElement>();
            le.minHeight = 45f;
            le.preferredHeight = 45f;
            le.preferredWidth = 496f;

            var label = AddText(root.transform, "Label", "Damage", Load<TMP_FontAsset>(KanitMediumPath), 20f, RowInk, TextAlignmentOptions.MidlineLeft);
            Stretch(label.rectTransform, 25f, 230f);
            var value = AddText(root.transform, "Value", "0", Load<TMP_FontAsset>(KanitMediumPath), 20f, RowInk, TextAlignmentOptions.MidlineRight);
            value.fontStyle = FontStyles.Normal;
            Stretch(value.rectTransform, 200f, 22f);

            var view = root.AddComponent<SkillAttributeRowView>();
            var so = new SerializedObject(view);
            so.FindProperty("_label").objectReferenceValue = label;
            so.FindProperty("_value").objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, RowPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void PrepareRowSprite()
        {
            AssetDatabase.ImportAsset(RowSpritePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(RowSpritePath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.spriteBorder = new Vector4(24f, 0f, 24f, 0f);
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static void Place(RectTransform rect, RectTransform window, float x, float y, float width, float height)
        {
            Rect area = window.rect;
            Vector3 world = window.TransformPoint(new Vector3(area.xMin + x + ArtInset, area.yMax - (y + ArtInset), 0f));
            var parent = (RectTransform)rect.parent;
            Vector3 local = parent.InverseTransformPoint(world);
            Rect parentArea = parent.rect;

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(local.x - parentArea.xMin, local.y - parentArea.yMax);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetTopLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void Stretch(RectTransform rect, float left, float right)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, 0f);
            rect.offsetMax = new Vector2(-right, 0f);
        }

        private static TextMeshProUGUI AddText(Transform parent, string name, string content, TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment)
        {
            var rect = NewChild(parent, name);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.richText = true;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform NewChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Remove(Transform parent, string name)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i);
                if (child.name == name) Object.DestroyImmediate(child.gameObject);
            }
        }

        private static Transform FindPath(Transform root, string path) => root.Find(path);

        private static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);

        private static Sprite LoadSprite(string path)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite) return sprite;
            }
            return null;
        }
        #endregion
    }
}
