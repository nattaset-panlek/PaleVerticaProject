using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CoreSystem.UI.Editor
{
    public class GlobalModalTemplateGenerator
    {
        [MenuItem("Realm of Hero/UI/Create Global Modal Template")]
        public static void CreateTemplate()
        {
            // 1. Create Root
            GameObject root = new GameObject("Global Modal Template");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            
            CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
            UIGlobalModal modal = root.AddComponent<UIGlobalModal>();

            // 2. Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(root.transform, false);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);

            // 3. Modal Container (Centers the content)
            GameObject window = new GameObject("Window");
            window.transform.SetParent(root.transform, false);
            RectTransform windowRect = window.AddComponent<RectTransform>();
            windowRect.anchorMin = new Vector2(0.5f, 0.5f);
            windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.sizeDelta = new Vector2(800, 600);
            Image windowImage = window.AddComponent<Image>();
            windowImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            // 4. Top Navigator
            GameObject topNav = new GameObject("Top Navigator");
            topNav.transform.SetParent(window.transform, false);
            RectTransform navRect = topNav.AddComponent<RectTransform>();
            navRect.anchorMin = new Vector2(0, 1);
            navRect.anchorMax = new Vector2(1, 1);
            navRect.pivot = new Vector2(0.5f, 1);
            navRect.anchoredPosition = Vector2.zero;
            navRect.sizeDelta = new Vector2(0, 60);
            Image navImage = topNav.AddComponent<Image>();
            navImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Title Text
            GameObject titleTextGo = new GameObject("Title Text (TMP)");
            titleTextGo.transform.SetParent(topNav.transform, false);
            RectTransform titleRect = titleTextGo.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(20, 0);
            titleRect.offsetMax = new Vector2(-60, 0); // Leave room for close button
            TextMeshProUGUI titleText = titleTextGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "Global Modal Title";
            titleText.fontSize = 24;
            titleText.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Capline;

            // Close Button
            GameObject closeBtnGo = new GameObject("Close Button");
            closeBtnGo.transform.SetParent(topNav.transform, false);
            RectTransform closeRect = closeBtnGo.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 0.5f);
            closeRect.anchorMax = new Vector2(1, 0.5f);
            closeRect.pivot = new Vector2(1, 0.5f);
            closeRect.anchoredPosition = new Vector2(-10, 0);
            closeRect.sizeDelta = new Vector2(40, 40);
            Image closeImage = closeBtnGo.AddComponent<Image>();
            closeImage.color = Color.red;
            Button closeButton = closeBtnGo.AddComponent<Button>();

            // 5. Content Container
            GameObject content = new GameObject("Content Container");
            content.transform.SetParent(window.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(20, 20);
            contentRect.offsetMax = new Vector2(-20, -60); // Below top nav

            // Setup UIGlobalModal links
            SerializedObject so = new SerializedObject(modal);
            so.FindProperty("_contentContainer").objectReferenceValue = contentRect;
            so.FindProperty("_titleText").objectReferenceValue = titleText;
            so.FindProperty("_closeButton").objectReferenceValue = closeButton;
            so.ApplyModifiedProperties();

            // Select it in editor
            Selection.activeGameObject = root;

            Debug.Log("Global Modal Template generated! Please assign it to the UIManager in your scene and save it as a prefab.");
        }
    }
}
