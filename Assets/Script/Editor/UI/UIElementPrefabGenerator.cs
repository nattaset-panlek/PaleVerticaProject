using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CoreSystem.UI;

namespace CoreSystem.Editor.UI
{
    [InitializeOnLoad]
    public static class UIElementPrefabGenerator
    {
        private const string PrefabFolderPath = "Assets/Prefab/Game Page/Global/UIElement";
        private const string RegistryAssetPath = "Assets/Prefab/UIElementRegistry.asset";

        static UIElementPrefabGenerator()
        {
            EditorApplication.delayCall += GenerateMissingBasePrefabs;
        }

        [MenuItem("Realm of Hero/UI/Generate Base UIElement Prefabs")]
        public static void GenerateMissingBasePrefabs()
        {
            if (!Directory.Exists(PrefabFolderPath))
            {
                Directory.CreateDirectory(PrefabFolderPath);
                AssetDatabase.Refresh();
            }

            var registry = AssetDatabase.LoadAssetAtPath<UIElementRegistrySO>(RegistryAssetPath);
            bool registryModified = false;

            // 0: Container
            CreateAndRegister(registry, UIElementType.Container, "Container", "Container.prefab", () => 
            {
                GameObject go = new GameObject("Container", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
                var vlg = go.GetComponent<VerticalLayoutGroup>();
                vlg.spacing = 8f;
                vlg.padding = new RectOffset(8, 8, 8, 8);
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = false;
                vlg.childForceExpandHeight = false;

                var csf = go.GetComponent<ContentSizeFitter>();
                csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                return go;
            }, ref registryModified);

            // 1: Text
            CreateAndRegister(registry, UIElementType.Text, "Text", "Text.prefab", () => 
            {
                GameObject go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "New Text";
                tmp.fontSize = 16f;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.MidlineLeft;

                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(160f, 30f);
                return go;
            }, ref registryModified);

            // 3: Image
            CreateAndRegister(registry, UIElementType.Image, "Image", "Image.prefab", () => 
            {
                GameObject go = new GameObject("Image", typeof(RectTransform), typeof(Image));
                var img = go.GetComponent<Image>();
                img.color = Color.white;
                img.raycastTarget = true;

                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100f, 100f);
                return go;
            }, ref registryModified);

            // 5: Dropdown
            CreateAndRegister(registry, UIElementType.Dropdown, "Dropdown", "Dropdown.prefab", () => 
            {
                GameObject go = TMP_DefaultControls.CreateDropdown(new TMP_DefaultControls.Resources());
                go.name = "Dropdown";
                return go;
            }, ref registryModified);

            // 6: Toggle
            CreateAndRegister(registry, UIElementType.Toggle, "Toggle", "Toggle.prefab", () => 
            {
                GameObject go = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle), typeof(HorizontalLayoutGroup));
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(160f, 30f);

                var hlg = go.GetComponent<HorizontalLayoutGroup>();
                hlg.spacing = 8f;
                hlg.childControlWidth = false;
                hlg.childControlHeight = false;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
                hlg.childAlignment = TextAnchor.MiddleLeft;

                GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
                bg.transform.SetParent(go.transform, false);
                var bgRt = bg.GetComponent<RectTransform>();
                bgRt.sizeDelta = new Vector2(24f, 24f);
                var bgImg = bg.GetComponent<Image>();
                bgImg.color = new Color(0.18f, 0.18f, 0.22f, 1f);

                GameObject checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
                checkmark.transform.SetParent(bg.transform, false);
                var chkRt = checkmark.GetComponent<RectTransform>();
                chkRt.anchorMin = new Vector2(0.2f, 0.2f);
                chkRt.anchorMax = new Vector2(0.8f, 0.8f);
                chkRt.sizeDelta = Vector2.zero;
                var chkImg = checkmark.GetComponent<Image>();
                chkImg.color = new Color(0.2f, 0.8f, 0.4f, 1f);

                GameObject label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                label.transform.SetParent(go.transform, false);
                var lblRt = label.GetComponent<RectTransform>();
                lblRt.sizeDelta = new Vector2(120f, 24f);
                var lblTmp = label.GetComponent<TextMeshProUGUI>();
                lblTmp.text = "Toggle Label";
                lblTmp.fontSize = 15f;
                lblTmp.color = Color.white;
                lblTmp.alignment = TextAlignmentOptions.MidlineLeft;

                var toggle = go.GetComponent<Toggle>();
                toggle.targetGraphic = bgImg;
                toggle.graphic = chkImg;
                toggle.isOn = true;
                return go;
            }, ref registryModified);

            // 7: Slider
            CreateAndRegister(registry, UIElementType.Slider, "Slider", "Slider.prefab", () => 
            {
                GameObject go = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(200f, 20f);

                GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
                bg.transform.SetParent(go.transform, false);
                var bgRt = bg.GetComponent<RectTransform>();
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.sizeDelta = Vector2.zero;
                var bgImg = bg.GetComponent<Image>();
                bgImg.color = new Color(0.15f, 0.15f, 0.18f, 1f);

                GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
                fillArea.transform.SetParent(go.transform, false);
                var faRt = fillArea.GetComponent<RectTransform>();
                faRt.anchorMin = Vector2.zero;
                faRt.anchorMax = Vector2.one;
                faRt.sizeDelta = new Vector2(-10f, 0f);

                GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fill.transform.SetParent(fillArea.transform, false);
                var fillRt = fill.GetComponent<RectTransform>();
                fillRt.sizeDelta = Vector2.zero;
                var fillImg = fill.GetComponent<Image>();
                fillImg.color = new Color(0.25f, 0.65f, 1f, 1f);

                GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
                handleArea.transform.SetParent(go.transform, false);
                var haRt = handleArea.GetComponent<RectTransform>();
                haRt.anchorMin = Vector2.zero;
                haRt.anchorMax = Vector2.one;
                haRt.sizeDelta = new Vector2(-10f, 0f);

                GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
                handle.transform.SetParent(handleArea.transform, false);
                var hRt = handle.GetComponent<RectTransform>();
                hRt.sizeDelta = new Vector2(20f, 20f);
                var hImg = handle.GetComponent<Image>();
                hImg.color = Color.white;

                var slider = go.GetComponent<Slider>();
                slider.fillRect = fillRt;
                slider.handleRect = hRt;
                slider.targetGraphic = hImg;
                slider.value = 0.5f;
                return go;
            }, ref registryModified);

            // 8: Custom
            CreateAndRegister(registry, UIElementType.Custom, "Custom", "Custom.prefab", () => 
            {
                GameObject go = new GameObject("Custom", typeof(RectTransform), typeof(UICustomElement));
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100f, 100f);
                return go;
            }, ref registryModified);

            // 9: ScrollRect
            CreateAndRegister(registry, UIElementType.ScrollRect, "ScrollRect", "ScrollRect.prefab", () => 
            {
                GameObject go = new GameObject("ScrollRect", typeof(RectTransform), typeof(UnityEngine.UI.ScrollRect), typeof(Image));
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(300f, 200f);
                var img = go.GetComponent<Image>();
                img.color = new Color(0f, 0f, 0f, 0f);
                img.raycastTarget = true;

                GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
                viewport.transform.SetParent(go.transform, false);
                var vpRt = viewport.GetComponent<RectTransform>();
                vpRt.anchorMin = Vector2.zero;
                vpRt.anchorMax = Vector2.one;
                vpRt.sizeDelta = Vector2.zero;

                GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
                content.transform.SetParent(viewport.transform, false);
                var cRt = content.GetComponent<RectTransform>();
                cRt.anchorMin = new Vector2(0f, 1f);
                cRt.anchorMax = new Vector2(1f, 1f);
                cRt.pivot = new Vector2(0f, 1f);
                cRt.sizeDelta = Vector2.zero;

                var vlg = content.GetComponent<VerticalLayoutGroup>();
                vlg.spacing = 8f;
                vlg.childControlWidth = true;
                vlg.childControlHeight = false;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;

                var csf = content.GetComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var sr = go.GetComponent<UnityEngine.UI.ScrollRect>();
                sr.viewport = vpRt;
                sr.content = cRt;
                sr.horizontal = false;
                sr.vertical = true;
                return go;
            }, ref registryModified);

            // 10: Scrollbar
            CreateAndRegister(registry, UIElementType.Scrollbar, "Scrollbar", "Scrollbar.prefab", () => 
            {
                GameObject go = new GameObject("Scrollbar", typeof(RectTransform), typeof(Scrollbar), typeof(Image));
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(16f, 160f);
                var img = go.GetComponent<Image>();
                img.color = new Color(0.12f, 0.12f, 0.15f, 0.6f);

                GameObject slidingArea = new GameObject("Sliding Area", typeof(RectTransform));
                slidingArea.transform.SetParent(go.transform, false);
                var saRt = slidingArea.GetComponent<RectTransform>();
                saRt.anchorMin = Vector2.zero;
                saRt.anchorMax = Vector2.one;
                saRt.sizeDelta = Vector2.zero;

                GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
                handle.transform.SetParent(slidingArea.transform, false);
                var hRt = handle.GetComponent<RectTransform>();
                hRt.sizeDelta = new Vector2(16f, 30f);
                var hImg = handle.GetComponent<Image>();
                hImg.color = new Color(0.7f, 0.7f, 0.7f, 1f);

                var sb = go.GetComponent<Scrollbar>();
                sb.handleRect = hRt;
                sb.targetGraphic = hImg;
                sb.direction = Scrollbar.Direction.BottomToTop;
                return go;
            }, ref registryModified);

            // 11: RawImage
            CreateAndRegister(registry, UIElementType.RawImage, "RawImage", "RawImage.prefab", () => 
            {
                GameObject go = new GameObject("RawImage", typeof(RectTransform), typeof(RawImage));
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100f, 100f);
                return go;
            }, ref registryModified);

            // 12: ProgressBar
            CreateAndRegister(registry, UIElementType.ProgressBar, "ProgressBar", "ProgressBar.prefab", () => 
            {
                GameObject go = new GameObject("ProgressBar", typeof(RectTransform), typeof(Slider), typeof(Image));
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(200f, 18f);
                var bgImg = go.GetComponent<Image>();
                bgImg.color = new Color(0.12f, 0.12f, 0.15f, 0.9f);

                GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
                fillArea.transform.SetParent(go.transform, false);
                var faRt = fillArea.GetComponent<RectTransform>();
                faRt.anchorMin = Vector2.zero;
                faRt.anchorMax = Vector2.one;
                faRt.sizeDelta = Vector2.zero;

                GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fill.transform.SetParent(fillArea.transform, false);
                var fillRt = fill.GetComponent<RectTransform>();
                fillRt.sizeDelta = Vector2.zero;
                var fillImg = fill.GetComponent<Image>();
                fillImg.color = new Color(0.2f, 0.75f, 0.95f, 1f);

                var slider = go.GetComponent<Slider>();
                slider.fillRect = fillRt;
                slider.interactable = false;
                slider.transition = Selectable.Transition.None;
                slider.value = 1f;
                return go;
            }, ref registryModified);

            // 13: Divider
            CreateAndRegister(registry, UIElementType.Divider, "Divider", "Divider.prefab", () => 
            {
                GameObject go = new GameObject("Divider", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100f, 2f);
                var img = go.GetComponent<Image>();
                img.color = new Color(0.35f, 0.35f, 0.4f, 0.6f);

                var le = go.GetComponent<LayoutElement>();
                le.preferredHeight = 2f;
                le.flexibleWidth = 1f;
                return go;
            }, ref registryModified);

            // 14: Mask
            CreateAndRegister(registry, UIElementType.Mask, "Mask", "Mask.prefab", () => 
            {
                GameObject go = new GameObject("Mask", typeof(RectTransform), typeof(RectMask2D));
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100f, 100f);
                return go;
            }, ref registryModified);

            if (registryModified && registry != null)
            {
                EditorUtility.SetDirty(registry);
                AssetDatabase.SaveAssets();
            }

            AssetDatabase.Refresh();
        }

        private static void CreateAndRegister(UIElementRegistrySO registry, UIElementType type, string styleName, string prefabFileName, System.Func<GameObject> creator, ref bool registryModified)
        {
            string prefabPath = $"{PrefabFolderPath}/{prefabFileName}";
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefabAsset == null)
            {
                GameObject instance = creator();
                prefabAsset = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                UnityEngine.Object.DestroyImmediate(instance);
                Debug.Log($"[UIElementPrefabGenerator] Created template prefab: {prefabPath}");
            }

            if (registry != null && prefabAsset != null)
            {
                var existing = registry.GetEntry(type, styleName);
                if (existing == null)
                {
                    registry.Entries.Add(new UIElementEntry
                    {
                        StyleName = styleName,
                        Type = type,
                        Prefab = prefabAsset,
                        TargetComponentPath = ""
                    });
                    registryModified = true;
                    Debug.Log($"[UIElementPrefabGenerator] Registered '{styleName}' ({type}) in UIElementRegistry.");
                }
                else if (existing.Prefab == null)
                {
                    existing.Prefab = prefabAsset;
                    registryModified = true;
                }
            }
        }
    }
}
