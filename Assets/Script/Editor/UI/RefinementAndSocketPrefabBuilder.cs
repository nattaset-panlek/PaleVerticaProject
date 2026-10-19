using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CoreSystem.UI;
using CoreSystem.UI.Refinement;
using CoreSystem.UI.Socket;
using GlobalVisualizer;

namespace CoreSystem.Editor.UI
{
    public static class RefinementAndSocketPrefabBuilder
    {
        #region [Variables/Fields]
        public const string RefinementPrefabPath = "Assets/Prefab/Game Page/Refinement Window.prefab";
        public const string SocketPrefabPath = "Assets/Prefab/Game Page/Socket Window.prefab";
        public const string RegistryAssetPath = "Assets/Prefab/UIWindowRegistry.asset";

        private const string KanitBoldPath = "Assets/Art/Front/Kanit/Kanit-Bold SDF.asset";
        private const string KanitMediumPath = "Assets/Art/Front/Kanit/Kanit-Medium SDF.asset";
        private const string NovecentoPath = "Assets/Art/Front/Novecento Sans/Novecentosanswide-DemiBold SDF.asset";
        private const string NovecentoNormalPath = "Assets/Art/Front/Novecento Sans/Novecentosanswide-Normal SDF.asset";
        private const string NotoSemiBoldPath = "Assets/Art/Front/Noto Sans Thai/NotoSansThai-SemiBold SDF.asset";
        private const string RoundedPath = "Assets/Art/UI/New/Rect/rounded.png";
        private const string CloseIconPath = "Assets/Art/UI/New/Icon/UI_Close_Secon_Icon.png";
        private const string StarIconPath = "Assets/Art/UI/New/Icon/UI_StarGylp_Icon.png";

        private const string RefineBgPath = "Assets/Art/UI/New/Refinement/UI_Refinement_Background.png";
        private const string RefineSunburstPath = "Assets/Art/UI/New/Refinement/UI_Refinement_Sunburst.png";
        private const string RefineLevelBadgePath = "Assets/Art/UI/New/Refinement/UI_Refinement_LevelBadge.png";
        private const string RefineItemSlotPath = "Assets/Art/UI/New/Refinement/UI_Item_Slot_Center.png";
        private const string RefineBtnPath = "Assets/Art/UI/New/Refinement/UI_Primary_Button_Refinement.png";
        private const string MaterialSlotRedPath = "Assets/Art/UI/New/Refinement/UI_Material_Slot_Red.png";
        private const string MaterialSlotBluePath = "Assets/Art/UI/New/Refinement/UI_Material_Slot_Blue.png";
        private const string StatBlockPath = "Assets/Art/UI/New/Frame/UI_Stat_Block.png";

        private const string SocketBgPath = "Assets/Art/UI/New/Socket/UI_Socket_Background.png";
        private const string SocketMandalaPath = "Assets/Art/UI/New/Socket/UI_Socket_Mandala.png";
        private const string SocketItemSlotPath = "Assets/Art/UI/New/Socket/UI_Item_Slot_Center.png";
        private const string SocketBtnPath = "Assets/Art/UI/New/Socket/UI_Primary_Button_Socket.png";
        private const string FragmentSlotPath = "Assets/Art/UI/New/Page/Skill Manager Page/UI_Fragment_Slot.png";

        private const string HeaderTagPillPath = "Assets/Art/UI/Refinement/tag_header_pill.png";
        private const string HeaderTagStarPath = "Assets/Art/UI/Refinement/tag_star_icon.png";
        private const string CleanBtnPath = "Assets/Art/UI/Refinement/btn_primary_clean_290x50.png";
        private const string CircleSpritePath = "Assets/ThirdParty/Le Tai's Asset/TrueShadow/Demo/Inspiration/Sprites/Circle.png";
        private const string FragGemIconPath = "Assets/Art/UI/New/Icon/UI_Fragment_Gem_Icon.png";
        private const string FragLinkIconPath = "Assets/Art/UI/New/Icon/UI_Fragment_Link_Icon.png";
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/UI/Build Refinement and Socket Windows")]
        public static void BuildAll()
        {
            BuildRefinementWindow();
            BuildSocketWindow();
            RegisterWindows();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[RefinementAndSocketPrefabBuilder] Successfully built Refinement & Socket windows.");
        }

        public static void BuildAndSnapshotBatch()
        {
            int exitCode = 0;
            try
            {
                BuildAll();
                TakeSnapshots();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                exitCode = 1;
            }
            EditorApplication.Exit(exitCode);
        }

        public static void BuildSocketAndSnapshotBatch()
        {
            int exitCode = 0;
            try
            {
                BuildSocketWindow();
                TakeSnapshots();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                exitCode = 1;
            }
            EditorApplication.Exit(exitCode);
        }

        public static void TakeSnapshotsBatch()
        {
            int exitCode = 0;
            try
            {
                TakeSnapshots();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                exitCode = 1;
            }
            EditorApplication.Exit(exitCode);
        }

        [MenuItem("Pale Vertica/UI/Take Snapshots of Refinement and Socket")]
        public static void TakeSnapshots()
        {
            AssetDatabase.ImportAsset(RefinementPrefabPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(SocketPrefabPath, ImportAssetOptions.ForceUpdate);
            var refinePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RefinementPrefabPath);
            var socketPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SocketPrefabPath);

            string artifactDir = @"C:\Users\User\.gemini\antigravity\brain\01cbee92-e5dc-4e78-a17f-5a8b93624323";
            string logDir = Path.Combine(Application.dataPath, "..", "Logs", "UIDebug");
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

            if (refinePrefab != null)
            {
                byte[] png = RenderPrefabToPng(refinePrefab);
                File.WriteAllBytes(Path.Combine(logDir, "refinement_unity_render.png"), png);
                if (Directory.Exists(artifactDir))
                {
                    File.WriteAllBytes(Path.Combine(artifactDir, "refinement_unity_render.png"), png);
                }
                Debug.Log("[Snapshot] Saved refinement_unity_render.png");
            }

            if (socketPrefab != null)
            {
                byte[] png = RenderPrefabToPng(socketPrefab);
                File.WriteAllBytes(Path.Combine(logDir, "socket_unity_render.png"), png);
                if (Directory.Exists(artifactDir))
                {
                    File.WriteAllBytes(Path.Combine(artifactDir, "socket_unity_render.png"), png);
                }
                Debug.Log("[Snapshot] Saved socket_unity_render.png");
            }
        }

        public static GameObject BuildRefinementWindow()
        {
            var root = CreateRootWindow("Refinement Window");
            var canvasGroup = root.GetComponent<CanvasGroup>();
            var windowComp = root.AddComponent<RefinementWindow>();

            var fontNovecento = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NovecentoPath);
            var fontKanitBold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KanitBoldPath);
            var fontKanitMed = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KanitMediumPath);
            var sprRounded = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);
            var sprClose = AssetDatabase.LoadAssetAtPath<Sprite>(CloseIconPath);
            var sprStar = AssetDatabase.LoadAssetAtPath<Sprite>(StarIconPath);
            var sprBg = AssetDatabase.LoadAssetAtPath<Sprite>(RefineBgPath);
            var sprSunburst = AssetDatabase.LoadAssetAtPath<Sprite>(RefineSunburstPath);
            var sprBadge = AssetDatabase.LoadAssetAtPath<Sprite>(RefineLevelBadgePath);
            var sprCenterSlot = AssetDatabase.LoadAssetAtPath<Sprite>(RefineItemSlotPath);
            var sprBtn = AssetDatabase.LoadAssetAtPath<Sprite>(RefineBtnPath);
            var sprSlotRed = AssetDatabase.LoadAssetAtPath<Sprite>(MaterialSlotRedPath);
            var sprSlotBlue = AssetDatabase.LoadAssetAtPath<Sprite>(MaterialSlotBluePath);
            var sprStatBlock = AssetDatabase.LoadAssetAtPath<Sprite>(StatBlockPath);

            var bgObj = CreateChild(root.transform, "Background");
            SetStretch(bgObj.GetComponent<RectTransform>());
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.sprite = sprBg;
            bgImg.type = Image.Type.Simple;
            bgImg.raycastTarget = false;

            var header = CreateHeader(root.transform, "FORGE MASTERY", sprRounded, sprStar, sprClose, fontKanitBold, out var closeBtn, out var headerTitle);

            var leftContainer = CreateChild(root.transform, "Left Container");
            var leftRect = leftContainer.GetComponent<RectTransform>();
            SetRect(leftRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(275f, -25f), new Vector2(450f, 600f));

            var dialObj = CreateChild(leftContainer.transform, "Sunburst Dial");
            var dialRect = dialObj.GetComponent<RectTransform>();
            SetRect(dialRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(390f, 390f));
            var dialImg = dialObj.AddComponent<Image>();
            dialImg.sprite = sprSunburst;
            dialImg.preserveAspect = true;
            dialImg.raycastTarget = false;

            var centerSlot = CreateChild(dialObj.transform, "Center Item Slot");
            var centerSlotRect = centerSlot.GetComponent<RectTransform>();
            SetRect(centerSlotRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(85f, 85f));
            var centerSlotImg = centerSlot.AddComponent<Image>();
            centerSlotImg.sprite = sprCenterSlot;
            centerSlotImg.preserveAspect = true;
            centerSlotImg.raycastTarget = true;

            var itemIconObj = CreateChild(centerSlot.transform, "Item Icon");
            var itemIconRect = itemIconObj.GetComponent<RectTransform>();
            SetRect(itemIconRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(64f, 64f));
            var itemIconImg = itemIconObj.AddComponent<Image>();
            itemIconImg.preserveAspect = true;
            itemIconImg.enabled = false;

            var centerSlotComp = centerSlot.AddComponent<RefinementCenterSlot>();
            var soCenter = new SerializedObject(centerSlotComp);
            soCenter.FindProperty("_window").objectReferenceValue = windowComp;
            soCenter.FindProperty("_slotBorderImage").objectReferenceValue = centerSlotImg;
            soCenter.FindProperty("_itemIcon").objectReferenceValue = itemIconImg;
            soCenter.FindProperty("_defaultSlotSprite").objectReferenceValue = sprCenterSlot;
            soCenter.ApplyModifiedPropertiesWithoutUndo();

            var levelBadgeObj = CreateChild(leftContainer.transform, "Level Badge");
            var badgeRect = levelBadgeObj.GetComponent<RectTransform>();
            SetRect(badgeRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-70f, -40f), new Vector2(200f, 44f));
            var badgeImg = levelBadgeObj.AddComponent<Image>();
            badgeImg.sprite = sprBadge;
            badgeImg.preserveAspect = true;

            var levelTextObj = CreateText(levelBadgeObj.transform, "Level Text", "Lv.12/20", fontNovecento, 26f, Color.white, TextAlignmentOptions.Center);
            SetStretch(levelTextObj.rectTransform);

            var matArea = CreateChild(leftContainer.transform, "Materials Area");
            var matRect = matArea.GetComponent<RectTransform>();
            SetRect(matRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(270f, 80f));
            var matLayout = matArea.AddComponent<HorizontalLayoutGroup>();
            matLayout.spacing = 15f;
            matLayout.childAlignment = TextAnchor.MiddleCenter;
            matLayout.childControlWidth = false;
            matLayout.childControlHeight = false;

            var matSlotObjs = new List<GameObject>();
            var matIcons = new List<Image>();
            var matCounts = new List<TMP_Text>();
            Sprite[] matSprites = { sprSlotRed, sprSlotBlue, sprSlotRed };
            for (int i = 0; i < 3; i++)
            {
                var slotObj = CreateChild(matArea.transform, $"Material Slot {i + 1}");
                var sRect = slotObj.GetComponent<RectTransform>();
                sRect.sizeDelta = new Vector2(78f, 78f);
                var sImg = slotObj.AddComponent<Image>();
                sImg.sprite = matSprites[i];
                sImg.preserveAspect = true;
                matSlotObjs.Add(slotObj);
                matIcons.Add(sImg);

                var countTxt = CreateText(slotObj.transform, "Count", "17", fontKanitBold, 16f, Color.black, TextAlignmentOptions.Center);
                SetRect(countTxt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 2f), new Vector2(60f, 25f));
                matCounts.Add(countTxt);
            }

            var rightContainer = CreateChild(root.transform, "Right Container");
            var rightRect = rightContainer.GetComponent<RectTransform>();
            SetRect(rightRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-40f, -20f), new Vector2(450f, 600f));

            var itemInfo = CreateChild(rightContainer.transform, "Item Info");
            var infoRect = itemInfo.GetComponent<RectTransform>();
            SetRect(infoRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(0f, 100f));

            var itemNameTxt = CreateText(itemInfo.transform, "Item Name", "DRAGON TOOTH PENDANT", fontNovecento, 28f, new Color(0.96f, 0.96f, 0.96f), TextAlignmentOptions.TopLeft);
            SetRect(itemNameTxt.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 36f));

            var itemTypeTxt = CreateText(itemInfo.transform, "Item Type", "TALISMAN", fontNovecento, 15f, new Color(0.45f, 0.45f, 0.45f), TextAlignmentOptions.TopLeft);
            SetRect(itemTypeTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 1f), new Vector2(0f, -40f), new Vector2(0f, 24f));

            var rarityObj = CreateChild(itemInfo.transform, "Rarity Badge");
            var rarityRect = rarityObj.GetComponent<RectTransform>();
            SetRect(rarityRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0f, -38f), new Vector2(110f, 28f));
            var rarityImg = rarityObj.AddComponent<Image>();
            rarityImg.sprite = sprRounded;
            rarityImg.type = Image.Type.Sliced;
            rarityImg.color = new Color(0.2f, 0.2f, 0.22f, 1f);

            var rarityTxt = CreateText(rarityObj.transform, "Rarity Text", "COMMON", fontNovecento, 16f, Color.white, TextAlignmentOptions.Center);
            SetStretch(rarityTxt.rectTransform);

            var statsSection = CreateChild(rightContainer.transform, "Stats Section");
            var statsRect = statsSection.GetComponent<RectTransform>();
            SetRect(statsRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -115f), new Vector2(0f, 260f));

            var attrHeader = CreateText(statsSection.transform, "Attribute Bonus Label", "ATTRIBUTE BONUS", fontNovecento, 13f, new Color(0.8f, 0.8f, 0.8f), TextAlignmentOptions.TopLeft);
            SetRect(attrHeader.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(0f, 20f));

            var statContainer = CreateChild(statsSection.transform, "Stat Container");
            var statContainerRect = statContainer.GetComponent<RectTransform>();
            SetRect(statContainerRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(0f, 230f));
            statContainer.AddComponent<RectMask2D>();

            var scrollRect = statContainer.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 25f;
            scrollRect.viewport = statContainerRect;

            var statListObj = CreateChild(statContainer.transform, "Stat Rows Container");
            var statListRect = statListObj.GetComponent<RectTransform>();
            SetRect(statListRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(-12f, 0f));
            scrollRect.content = statListRect;

            var scrollTrack = CreateChild(statContainer.transform, "Scroll Track");
            var trackRect = scrollTrack.GetComponent<RectTransform>();
            SetRect(trackRect, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(6f, 0f));
            var trackImg = scrollTrack.AddComponent<Image>();
            trackImg.color = new Color(0.12f, 0.12f, 0.14f, 0.5f);
            var scrollbar = scrollTrack.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            var scrollThumb = CreateChild(scrollTrack.transform, "Scroll Thumb");
            var thumbRect = scrollThumb.GetComponent<RectTransform>();
            SetRect(thumbRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 40f));
            var thumbImg = scrollThumb.AddComponent<Image>();
            thumbImg.sprite = sprRounded;
            thumbImg.type = Image.Type.Sliced;
            thumbImg.color = new Color(0.4f, 0.91f, 0.98f, 0.6f);

            scrollbar.handleRect = thumbRect;
            scrollbar.targetGraphic = thumbImg;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.verticalScrollbarSpacing = 4f;

            var rowTemplateObj = CreateChild(statContainer.transform, "Stat Row Template");
            var rowTemplateRt = rowTemplateObj.GetComponent<RectTransform>();
            SetRect(rowTemplateRt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 48f));
            var rowTemplateImg = rowTemplateObj.AddComponent<Image>();
            rowTemplateImg.sprite = sprStatBlock;
            rowTemplateImg.type = Image.Type.Sliced;
            rowTemplateImg.raycastTarget = false;

            var lTxt = CreateText(rowTemplateObj.transform, "Label", "PHYSICAL POWER", fontNovecento, 18f, new Color(0.92f, 0.92f, 0.92f, 1f), TextAlignmentOptions.MidlineLeft);
            SetRect(lTxt.rectTransform, new Vector2(0f, 0f), new Vector2(0.55f, 1f), new Vector2(0f, 0.5f), new Vector2(15f, 0f), Vector2.zero);

            var vTxt = CreateText(rowTemplateObj.transform, "Value", "100 <color=#38bdf8>+(500)</color>", fontNovecento, 20f, Color.white, TextAlignmentOptions.MidlineRight);
            SetRect(vTxt.rectTransform, new Vector2(0.55f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-15f, 0f), Vector2.zero);

            var statRowView = rowTemplateObj.AddComponent<RefinementStatRowView>();
            var soRow = new SerializedObject(statRowView);
            soRow.FindProperty("_label").objectReferenceValue = lTxt;
            soRow.FindProperty("_value").objectReferenceValue = vTxt;
            soRow.FindProperty("_backgroundImage").objectReferenceValue = rowTemplateImg;
            soRow.ApplyModifiedPropertiesWithoutUndo();
            rowTemplateObj.SetActive(false);

            var visualizer = statContainer.AddComponent<GlobalVisualizerView>();
            visualizer.FocusOnClick = false;
            var layoutSettings = VisualLayoutSettings.Vertical(48f, 8f);
            layoutSettings.ItemSize = new Vector2(400f, 48f);
            layoutSettings.CrossAlignment = VisualAlignment.Center;
            visualizer.SetLayout(layoutSettings);
            visualizer.ItemPrefab = rowTemplateRt;

            var rateArea = CreateChild(rightContainer.transform, "Rate & Cost Section");
            var rateRect = rateArea.GetComponent<RectTransform>();
            SetRect(rateRect, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(0f, 100f));

            var probLabel = CreateText(rateArea.transform, "Rate Label", "Success Probability", fontNovecento, 16f, new Color(0.45f, 0.45f, 0.45f), TextAlignmentOptions.TopLeft);
            SetRect(probLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0.7f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(0f, 22f));

            var probVal = CreateText(rateArea.transform, "Rate Value", "65%", fontNovecento, 28f, new Color(0.4f, 0.91f, 0.98f), TextAlignmentOptions.TopRight);
            SetRect(probVal.rectTransform, new Vector2(0.7f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 32f));

            var barBg = CreateChild(rateArea.transform, "Rate Bar Background");
            var barBgRect = barBg.GetComponent<RectTransform>();
            SetRect(barBgRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -32f), new Vector2(0f, 8f));
            var barBgImg = barBg.AddComponent<Image>();
            barBgImg.color = new Color(0.12f, 0.12f, 0.14f);

            var barFill = CreateChild(barBg.transform, "Rate Bar Fill");
            var barFillRect = barFill.GetComponent<RectTransform>();
            barFillRect.anchorMin = new Vector2(0f, 0f);
            barFillRect.anchorMax = new Vector2(0.65f, 1f);
            barFillRect.pivot = new Vector2(0.5f, 0.5f);
            barFillRect.anchoredPosition = Vector2.zero;
            barFillRect.sizeDelta = Vector2.zero;
            var barFillImg = barFill.AddComponent<Image>();
            barFillImg.color = new Color(0.4f, 0.91f, 0.98f);

            var rateSlider = barBg.AddComponent<Slider>();
            rateSlider.interactable = false;
            rateSlider.direction = Slider.Direction.LeftToRight;
            rateSlider.minValue = 0f;
            rateSlider.maxValue = 1f;
            rateSlider.value = 0.65f;
            rateSlider.fillRect = barFillRect;
            rateSlider.targetGraphic = barFillImg;

            var costLabel = CreateText(rateArea.transform, "Cost Label", "Consumption", fontNovecento, 16f, new Color(0.45f, 0.45f, 0.45f), TextAlignmentOptions.BottomLeft);
            SetRect(costLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f), Vector2.zero, new Vector2(0f, 24f));

            var costVal = CreateText(rateArea.transform, "Cost Value", "5,000 CREDITS", fontNovecento, 22f, new Color(0.96f, 0.96f, 0.96f), TextAlignmentOptions.BottomRight);
            SetRect(costVal.rectTransform, new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 26f));

            var btnArea = CreateChild(rightContainer.transform, "Bottom Button Area");
            var btnAreaRect = btnArea.GetComponent<RectTransform>();
            SetRect(btnAreaRect, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 15f), new Vector2(0f, 55f));

            var refineBtnObj = CreateChild(btnArea.transform, "Refine Button");
            var btnRect = refineBtnObj.GetComponent<RectTransform>();
            SetRect(btnRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(225f, 50f));
            var btnImg = refineBtnObj.AddComponent<Image>();
            btnImg.sprite = sprBtn;
            btnImg.preserveAspect = true;
            var refineBtn = refineBtnObj.AddComponent<Button>();
            refineBtn.targetGraphic = btnImg;

            var btnTxt = CreateText(refineBtnObj.transform, "Button Text", string.Empty, fontNovecento, 20f, new Color(0.22f, 0.17f, 0.17f), TextAlignmentOptions.Center);
            SetStretch(btnTxt.rectTransform);
            btnTxt.gameObject.SetActive(false);

            var so = new SerializedObject(windowComp);
            so.FindProperty("_headerRect").objectReferenceValue = header.GetComponent<RectTransform>();
            so.FindProperty("_headerTitle").objectReferenceValue = headerTitle;
            so.FindProperty("_itemNameText").objectReferenceValue = itemNameTxt;
            so.FindProperty("_itemTypeText").objectReferenceValue = itemTypeTxt;
            so.FindProperty("_rarityText").objectReferenceValue = rarityTxt;
            so.FindProperty("_rarityBackground").objectReferenceValue = rarityImg;
            so.FindProperty("_levelText").objectReferenceValue = levelTextObj;
            so.FindProperty("_centerSlot").objectReferenceValue = centerSlotComp;
            so.FindProperty("_sunburstDial").objectReferenceValue = dialImg;
            so.FindProperty("_itemIcon").objectReferenceValue = itemIconImg;
            so.FindProperty("_successRateText").objectReferenceValue = probVal;
            so.FindProperty("_successRateSlider").objectReferenceValue = rateSlider;
            so.FindProperty("_successRateBar").objectReferenceValue = barFillImg;
            so.FindProperty("_costText").objectReferenceValue = costVal;
            so.FindProperty("_refineButton").objectReferenceValue = refineBtn;
            so.FindProperty("_buttonText").objectReferenceValue = btnTxt;
            so.FindProperty("_closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("_statVisualizer").objectReferenceValue = visualizer;
            so.FindProperty("_statRowPrefab").objectReferenceValue = rowTemplateRt;
            so.FindProperty("_statContainer").objectReferenceValue = statListObj.transform;

            var propSlots = so.FindProperty("_materialSlotObjects");
            propSlots.ClearArray();
            for (int i = 0; i < matSlotObjs.Count; i++)
            {
                propSlots.InsertArrayElementAtIndex(i);
                propSlots.GetArrayElementAtIndex(i).objectReferenceValue = matSlotObjs[i];
            }

            var propIcons = so.FindProperty("_materialIcons");
            propIcons.ClearArray();
            for (int i = 0; i < matIcons.Count; i++)
            {
                propIcons.InsertArrayElementAtIndex(i);
                propIcons.GetArrayElementAtIndex(i).objectReferenceValue = matIcons[i];
            }

            var propCounts = so.FindProperty("_materialCountTexts");
            propCounts.ClearArray();
            for (int i = 0; i < matCounts.Count; i++)
            {
                propCounts.InsertArrayElementAtIndex(i);
                propCounts.GetArrayElementAtIndex(i).objectReferenceValue = matCounts[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, RefinementPrefabPath);
            Object.DestroyImmediate(root);
            return AssetDatabase.LoadAssetAtPath<GameObject>(RefinementPrefabPath);
        }

        public static GameObject BuildSocketWindow()
        {
            var root = CreateRootWindow("Socket Window");
            var canvasGroup = root.GetComponent<CanvasGroup>();
            var windowComp = root.AddComponent<SocketWindow>();

            var fontNovecentoDemi = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NovecentoPath);
            var fontNovecentoNormal = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NovecentoNormalPath);
            var fontKanitBold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KanitBoldPath);
            var fontNotoSemiBold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NotoSemiBoldPath);

            var sprRounded = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);
            var sprClose = AssetDatabase.LoadAssetAtPath<Sprite>(CloseIconPath);
            var sprHeaderPill = AssetDatabase.LoadAssetAtPath<Sprite>(HeaderTagPillPath);
            var sprHeaderStar = AssetDatabase.LoadAssetAtPath<Sprite>(HeaderTagStarPath);
            var sprBg = AssetDatabase.LoadAssetAtPath<Sprite>(SocketBgPath);
            var sprMandala = AssetDatabase.LoadAssetAtPath<Sprite>(SocketMandalaPath);
            var sprCenterSlot = AssetDatabase.LoadAssetAtPath<Sprite>(SocketItemSlotPath);
            var sprCleanBtn = AssetDatabase.LoadAssetAtPath<Sprite>(CleanBtnPath);
            var sprFragSlot = AssetDatabase.LoadAssetAtPath<Sprite>(FragmentSlotPath);
            var sprCircle = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
            var sprFragGem = AssetDatabase.LoadAssetAtPath<Sprite>(FragGemIconPath);
            var sprFragLink = AssetDatabase.LoadAssetAtPath<Sprite>(FragLinkIconPath);

            var bgObj = CreateChild(root.transform, "Background");
            SetStretch(bgObj.GetComponent<RectTransform>());
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.sprite = sprBg;
            bgImg.type = Image.Type.Simple;
            bgImg.raycastTarget = false;

            var header = CreateChild(root.transform, "Header");
            SetStretch(header.GetComponent<RectTransform>());

            var tagObj = CreateChild(header.transform, "Title Tag");
            var tagRt = tagObj.GetComponent<RectTransform>();
            SetRect(tagRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(17f, -18f), new Vector2(266f, 40f));
            var tagImg = tagObj.AddComponent<Image>();
            tagImg.sprite = sprHeaderPill;
            tagImg.type = Image.Type.Sliced;
            tagImg.color = Color.white;

            var starObj = CreateChild(tagObj.transform, "Star Icon");
            var starRt = starObj.GetComponent<RectTransform>();
            SetRect(starRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -4f), new Vector2(32f, 32f));
            var starImg = starObj.AddComponent<Image>();
            starImg.sprite = sprHeaderStar;
            starImg.preserveAspect = true;
            starImg.color = Color.white;

            var headerTitle = CreateText(tagObj.transform, "Title Text", "Socket Mastery", fontKanitBold, 24f, Color.black, TextAlignmentOptions.MidlineLeft);
            headerTitle.fontStyle = FontStyles.Bold;
            headerTitle.enableWordWrapping = false;
            headerTitle.overflowMode = TextOverflowModes.Overflow;
            var titleRt = headerTitle.rectTransform;
            SetRect(titleRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(56f, -9f), new Vector2(210f, 26f));

            var closeObj = CreateChild(header.transform, "Close Button");
            var closeRt = closeObj.GetComponent<RectTransform>();
            SetRect(closeRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(1022f, -27f), new Vector2(24f, 24f));
            var closeImg = closeObj.AddComponent<Image>();
            closeImg.sprite = sprClose;
            closeImg.preserveAspect = true;
            var closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeImg;

            var dialObj = CreateChild(root.transform, "Mandala Dial");
            var dialRt = dialObj.GetComponent<RectTransform>();
            SetRect(dialRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(56f, -123f), new Vector2(460f, 460f));
            var dialImg = dialObj.AddComponent<Image>();
            dialImg.sprite = sprMandala;
            dialImg.preserveAspect = true;
            dialImg.raycastTarget = false;

            var centerSlot = CreateChild(root.transform, "Center Item Slot");
            var centerSlotRt = centerSlot.GetComponent<RectTransform>();
            SetRect(centerSlotRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(247f, -311f), new Vector2(79f, 79f));
            var centerSlotImg = centerSlot.AddComponent<Image>();
            centerSlotImg.sprite = sprCenterSlot;
            centerSlotImg.preserveAspect = true;

            var itemIconObj = CreateChild(centerSlot.transform, "Item Icon");
            var itemIconRt = itemIconObj.GetComponent<RectTransform>();
            SetRect(itemIconRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60f, 60f));
            var itemIconImg = itemIconObj.AddComponent<Image>();
            itemIconImg.preserveAspect = true;
            itemIconImg.enabled = false;

            var rarityObj = CreateChild(root.transform, "Rarity Badge");
            var rarityRt = rarityObj.GetComponent<RectTransform>();
            SetRect(rarityRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(622f, -33f), new Vector2(131f, 35f));
            var rarityImg = rarityObj.AddComponent<Image>();
            rarityImg.sprite = sprRounded;
            rarityImg.type = Image.Type.Sliced;
            rarityImg.color = new Color(0f, 0.28235f, 1f, 1f);

            var rarityTxt = CreateText(rarityObj.transform, "Rarity Text", "COMMON", fontNovecentoDemi, 18f, Color.white, TextAlignmentOptions.Center);
            var rarityTxtRt = rarityTxt.rectTransform;
            SetRect(rarityTxtRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110f, 24f));

            var itemNameTxt = CreateText(root.transform, "Item Name", "Dragon Tooth Pendant", fontNovecentoDemi, 32f, new Color(0.96f, 0.96f, 0.96f), TextAlignmentOptions.TopLeft);
            itemNameTxt.characterSpacing = 2f;
            var itemNameRt = itemNameTxt.rectTransform;
            SetRect(itemNameRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(622f, -82f), new Vector2(404f, 32f));

            var itemTypeTxt = CreateText(root.transform, "Item Type", "Talisman", fontNovecentoNormal, 16f, new Color(0.45f, 0.45f, 0.45f), TextAlignmentOptions.TopLeft);
            var itemTypeRt = itemTypeTxt.rectTransform;
            SetRect(itemTypeRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(622f, -128f), new Vector2(215f, 24f));

            var slotButtons = new List<Button>();
            var slotBgs = new List<Image>();
            var slotIcons = new List<Image>();
            var slotIndicators = new List<GameObject>();
            var slotHighlights = new List<GameObject>();

            Vector2[] socketPositions = {
                new Vector2(703f, -208f),
                new Vector2(862f, -208f),
                new Vector2(703f, -353f),
                new Vector2(862f, -353f)
            };
            bool[] isFilled = { true, false, false, true };
            Sprite[] socketSprites = { sprFragGem, null, null, sprFragLink };

            for (int i = 0; i < 4; i++)
            {
                var sObj = CreateChild(root.transform, $"Socket Slot {i + 1}");
                var sRt = sObj.GetComponent<RectTransform>();
                SetRect(sRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), socketPositions[i], new Vector2(90f, 90f));

                var sBtn = sObj.AddComponent<Button>();
                var sImg = sObj.AddComponent<Image>();
                sImg.sprite = sprFragSlot;
                sImg.preserveAspect = true;
                sBtn.targetGraphic = sImg;

                var circObj = CreateChild(sObj.transform, "Backing Circle");
                var circRt = circObj.GetComponent<RectTransform>();
                SetRect(circRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(64f, 64f));
                var circImg = circObj.AddComponent<Image>();
                circImg.sprite = sprCircle;
                circImg.preserveAspect = true;
                circImg.color = new Color(0.851f, 0.851f, 0.851f, 1f);
                circObj.SetActive(isFilled[i]);

                var iconObj = CreateChild(sObj.transform, "Fragment Icon");
                var iconRt = iconObj.GetComponent<RectTransform>();
                SetRect(iconRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 2f), new Vector2(48f, 48f));
                var iconImg = iconObj.AddComponent<Image>();
                iconImg.sprite = socketSprites[i];
                iconImg.preserveAspect = true;
                iconImg.enabled = isFilled[i];
                iconObj.SetActive(isFilled[i]);

                var emptyObj = CreateChild(sObj.transform, "Empty Indicator");
                var emptyRt = emptyObj.GetComponent<RectTransform>();
                SetRect(emptyRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24f, 24f));
                emptyObj.SetActive(!isFilled[i]);

                var hlObj = CreateChild(sObj.transform, "Active Highlight");
                var hlRt = hlObj.GetComponent<RectTransform>();
                SetRect(hlRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(96f, 96f));
                var hlImg = hlObj.AddComponent<Image>();
                hlImg.sprite = sprRounded;
                hlImg.type = Image.Type.Sliced;
                hlImg.color = new Color(0.4f, 0.91f, 0.98f, 0.35f);
                hlObj.SetActive(false);

                slotButtons.Add(sBtn);
                slotBgs.Add(sImg);
                slotIcons.Add(iconImg);
                slotIndicators.Add(emptyObj);
                slotHighlights.Add(hlObj);
            }

            var costLabel = CreateText(root.transform, "Cost Label", "Consumption", fontNovecentoNormal, 18f, new Color(0.45f, 0.45f, 0.45f), TextAlignmentOptions.TopLeft);
            var costLabelRt = costLabel.rectTransform;
            SetRect(costLabelRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(622f, -545f), new Vector2(200f, 18f));

            var costVal = CreateText(root.transform, "Cost Value", "5,000 credits", fontNovecentoNormal, 24f, new Color(0.96f, 0.96f, 0.96f), TextAlignmentOptions.MidlineRight);
            var costValRt = costVal.rectTransform;
            SetRect(costValRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1020f, -540f), new Vector2(180f, 24f));

            var infuseBtnObj = CreateChild(root.transform, "Infuse Button");
            var btnRt = infuseBtnObj.GetComponent<RectTransform>();
            SetRect(btnRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(732f, -605f), new Vector2(290f, 50f));
            var btnImg = infuseBtnObj.AddComponent<Image>();
            btnImg.sprite = sprCleanBtn;
            btnImg.preserveAspect = false;
            var infuseBtn = infuseBtnObj.AddComponent<Button>();
            infuseBtn.targetGraphic = btnImg;

            var btnTxt = CreateText(infuseBtnObj.transform, "Button Text", "Infuse Socket", fontNotoSemiBold, 20f, new Color(0.22f, 0.173f, 0.173f, 1f), TextAlignmentOptions.Center);
            var btnTxtRt = btnTxt.rectTransform;
            SetRect(btnTxtRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(26f, 1f), new Vector2(140f, 30f));

            var so = new SerializedObject(windowComp);
            so.FindProperty("_headerRect").objectReferenceValue = tagRt;
            so.FindProperty("_headerTitle").objectReferenceValue = headerTitle;
            so.FindProperty("_itemNameText").objectReferenceValue = itemNameTxt;
            so.FindProperty("_itemTypeText").objectReferenceValue = itemTypeTxt;
            so.FindProperty("_rarityText").objectReferenceValue = rarityTxt;
            so.FindProperty("_itemIcon").objectReferenceValue = itemIconImg;
            so.FindProperty("_costText").objectReferenceValue = costVal;
            so.FindProperty("_infuseButton").objectReferenceValue = infuseBtn;
            so.FindProperty("_buttonText").objectReferenceValue = btnTxt;
            so.FindProperty("_closeButton").objectReferenceValue = closeBtn;

            var propBtns = so.FindProperty("_socketSlotButtons");
            propBtns.ClearArray();
            for (int i = 0; i < slotButtons.Count; i++)
            {
                propBtns.InsertArrayElementAtIndex(i);
                propBtns.GetArrayElementAtIndex(i).objectReferenceValue = slotButtons[i];
            }

            var propBgs = so.FindProperty("_socketSlotBackgrounds");
            propBgs.ClearArray();
            for (int i = 0; i < slotBgs.Count; i++)
            {
                propBgs.InsertArrayElementAtIndex(i);
                propBgs.GetArrayElementAtIndex(i).objectReferenceValue = slotBgs[i];
            }

            var propIcons = so.FindProperty("_socketSlotIcons");
            propIcons.ClearArray();
            for (int i = 0; i < slotIcons.Count; i++)
            {
                propIcons.InsertArrayElementAtIndex(i);
                propIcons.GetArrayElementAtIndex(i).objectReferenceValue = slotIcons[i];
            }

            var propInds = so.FindProperty("_socketEmptyIndicators");
            propInds.ClearArray();
            for (int i = 0; i < slotIndicators.Count; i++)
            {
                propInds.InsertArrayElementAtIndex(i);
                propInds.GetArrayElementAtIndex(i).objectReferenceValue = slotIndicators[i];
            }

            var propHls = so.FindProperty("_socketActiveHighlights");
            propHls.ClearArray();
            for (int i = 0; i < slotHighlights.Count; i++)
            {
                propHls.InsertArrayElementAtIndex(i);
                propHls.GetArrayElementAtIndex(i).objectReferenceValue = slotHighlights[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, SocketPrefabPath);
            Object.DestroyImmediate(root);
            return AssetDatabase.LoadAssetAtPath<GameObject>(SocketPrefabPath);
        }

        public static void RegisterWindows()
        {
            var registry = AssetDatabase.LoadAssetAtPath<UIWindowRegistrySO>(RegistryAssetPath);
            if (registry == null) return;

            var refinePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RefinementPrefabPath);
            var socketPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SocketPrefabPath);

            var so = new SerializedObject(registry);
            var entries = so.FindProperty("_entries");

            EnsureEntry(entries, "Refinement", refinePrefab);
            EnsureEntry(entries, "Socket", socketPrefab);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);
        }
        #endregion

        #region [Private Methods]
        [InitializeOnLoadMethod]
        private static void AutoSnapshotOnLoad()
        {
            EditorApplication.delayCall += () =>
            {
                TakeSnapshots();
            };
            EditorApplication.update += CheckSnapshotTrigger;
        }

        private static void CheckSnapshotTrigger()
        {
            string trigger = Path.Combine(Application.dataPath, "..", "Temp", "trigger_snapshot.txt");
            if (File.Exists(trigger))
            {
                try { File.Delete(trigger); } catch { }
                BuildAll();
                TakeSnapshots();
            }
        }

        private static void EnsureEntry(SerializedProperty entries, string windowName, GameObject prefab)
        {
            for (int i = 0; i < entries.arraySize; i++)
            {
                var elem = entries.GetArrayElementAtIndex(i);
                var nameProp = elem.FindPropertyRelative("WindowName");
                if (string.Equals(nameProp.stringValue, windowName, System.StringComparison.OrdinalIgnoreCase))
                {
                    elem.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
                    return;
                }
            }

            int index = entries.arraySize;
            entries.InsertArrayElementAtIndex(index);
            var newElem = entries.GetArrayElementAtIndex(index);
            newElem.FindPropertyRelative("WindowName").stringValue = windowName;
            newElem.FindPropertyRelative("Type").enumValueIndex = 0;
            newElem.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
            newElem.FindPropertyRelative("DestroyOnClose").boolValue = false;
        }

        private static GameObject CreateRootWindow(string name)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(CanvasGroup));
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(1083f, 705f);

            var cg = root.GetComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;

            return root;
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreateHeader(Transform parent, string titleText, Sprite sprRounded, Sprite sprStar, Sprite sprClose, TMP_FontAsset font, out Button closeBtn, out TMP_Text headerTitle)
        {
            var header = CreateChild(parent, "Header");
            var hRect = header.GetComponent<RectTransform>();
            SetRect(hRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 65f));

            var tagObj = CreateChild(header.transform, "Title Tag");
            var tagRect = tagObj.GetComponent<RectTransform>();
            SetRect(tagRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(25f, -15f), new Vector2(250f, 40f));
            var tagImg = tagObj.AddComponent<Image>();
            tagImg.sprite = sprRounded;
            tagImg.type = Image.Type.Sliced;
            tagImg.color = Color.black;

            var iconObj = CreateChild(tagObj.transform, "Icon");
            var iconRect = iconObj.GetComponent<RectTransform>();
            SetRect(iconRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(24f, 24f));
            var iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = sprStar;
            iconImg.preserveAspect = true;

            headerTitle = CreateText(tagObj.transform, "Title", titleText, font, 20f, Color.white, TextAlignmentOptions.MidlineLeft);
            headerTitle.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            SetRect(headerTitle.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(42f, 0f), new Vector2(0f, 0f));

            var closeObj = CreateChild(header.transform, "Close Button");
            var cRect = closeObj.GetComponent<RectTransform>();
            SetRect(cRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-25f, -22f), new Vector2(24f, 24f));
            var cImg = closeObj.AddComponent<Image>();
            cImg.sprite = sprClose;
            cImg.preserveAspect = true;
            closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = cImg;

            return header;
        }

        private static TMP_Text CreateText(Transform parent, string name, string content, TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment)
        {
            var go = CreateChild(parent, name);
            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = content;
            if (font != null) txt.font = font;
            txt.fontSize = size;
            txt.color = color;
            txt.alignment = alignment;
            txt.raycastTarget = false;
            return txt;
        }

        private static void SetRect(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        private static void SetStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        private static byte[] RenderPrefabToPng(GameObject prefab)
        {
            Scene scene = EditorSceneManager.NewPreviewScene();
            var root = new GameObject("[SnapshotRoot]");
            SceneManager.MoveGameObjectToScene(root, scene);
            var rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);

            try
            {
                var camGo = new GameObject("Camera", typeof(Camera));
                camGo.transform.SetParent(root.transform, false);
                camGo.transform.position = new Vector3(0f, 0f, -10f);
                var cam = camGo.GetComponent<Camera>();
                cam.scene = scene;
                cam.orthographic = true;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.263f, 0.263f, 0.263f, 1f);
                cam.targetTexture = rt;
                cam.enabled = false;

                var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
                canvasGo.transform.SetParent(root.transform, false);
                var canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 5f;

                var scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 1f;

                var inst = Object.Instantiate(prefab, canvas.transform, false);
                var windowRt = (RectTransform)inst.transform;
                windowRt.anchorMin = new Vector2(0.5f, 0.5f);
                windowRt.anchorMax = new Vector2(0.5f, 0.5f);
                windowRt.pivot = new Vector2(0.5f, 0.5f);
                windowRt.anchoredPosition = Vector2.zero;

                var visualizer = inst.GetComponentInChildren<GlobalVisualizerView>();
                if (visualizer != null && visualizer.ItemPrefab != null)
                {
                    visualizer.SetCount(4, (go, index) =>
                    {
                        var row = go.GetComponent<RefinementStatRowView>();
                        if (row != null)
                        {
                            string[] names = { "PHYSICAL POWER", "MAGIC POWER", "MAX HEALTH", "DEFENSE" };
                            row.Bind(new RefinementStatRowData(names[index % 4], "100 <color=#38bdf8>+(500)</color>", false));
                        }
                    });
                }

                Canvas.ForceUpdateCanvases();
                foreach (var group in canvas.GetComponentsInChildren<LayoutGroup>(false))
                {
                    LayoutRebuilder.MarkLayoutForRebuild((RectTransform)group.transform);
                }
                Canvas.ForceUpdateCanvases();

                cam.Render();
                var prevActive = RenderTexture.active;
                RenderTexture.active = rt;
                var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
                tex.Apply();
                RenderTexture.active = prevActive;

                byte[] bytes = tex.EncodeToPNG();
                Object.DestroyImmediate(tex);
                return bytes;
            }
            finally
            {
                Object.DestroyImmediate(root);
                rt.Release();
                Object.DestroyImmediate(rt);
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
        #endregion
    }
}
