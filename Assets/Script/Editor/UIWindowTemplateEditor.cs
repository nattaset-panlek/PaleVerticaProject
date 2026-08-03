using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CoreSystem.UI;

namespace CoreSystem.EditorTools
{
    public static class UIWindowTemplateEditor
    {
        #region [Public Methods]
        [MenuItem("GameObject/UI/Realm UI/Create UI Window Template", false, 10)]
        [MenuItem("Tools/Realm of Hero/UI/Create UI Window Template", false, 10)]
        public static void CreateUIWindowTemplate(MenuCommand menuCommand)
        {
            GameObject template = CreateBaseUITemplate("UIWindow_Template", menuCommand);
            if (template != null)
            {
                template.AddComponent<UIWindow>();
                Selection.activeGameObject = template;
            }
        }

        [MenuItem("GameObject/UI/Realm UI/Create UI Modal Template", false, 11)]
        [MenuItem("Tools/Realm of Hero/UI/Create UI Modal Template", false, 11)]
        public static void CreateUIModalTemplate(MenuCommand menuCommand)
        {
            GameObject template = CreateBaseUITemplate("UIModal_Template", menuCommand);
            if (template != null)
            {
                template.AddComponent<UIModal>();
                Selection.activeGameObject = template;
            }
        }

        [MenuItem("Tools/Realm of Hero/UI/Create UI Manager Hierarchy Structure", false, 0)]
        public static void CreateUIManagerHierarchy(MenuCommand menuCommand)
        {
            Canvas canvas = GetOrCreateCanvas();
            
            GameObject managerObj = new GameObject("UIManager", typeof(RectTransform), typeof(UIManager));
            GameObjectUtility.SetParentAndAlign(managerObj, canvas.gameObject);

            RectTransform managerRect = managerObj.GetComponent<RectTransform>();
            StretchToParent(managerRect);

            GameObject windowContainer = new GameObject("WindowContainer", typeof(RectTransform));
            GameObjectUtility.SetParentAndAlign(windowContainer, managerObj);
            StretchToParent(windowContainer.GetComponent<RectTransform>());

            GameObject modalContainer = new GameObject("ModalContainer", typeof(RectTransform));
            GameObjectUtility.SetParentAndAlign(modalContainer, managerObj);
            StretchToParent(modalContainer.GetComponent<RectTransform>());

            GameObject modalOverlay = new GameObject("ModalOverlay", typeof(RectTransform), typeof(Image), typeof(Button));
            GameObjectUtility.SetParentAndAlign(modalOverlay, modalContainer);
            StretchToParent(modalOverlay.GetComponent<RectTransform>());
            
            Image overlayImg = modalOverlay.GetComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.6f);
            modalOverlay.SetActive(false);

            SerializedObject serializedManager = new SerializedObject(managerObj.GetComponent<UIManager>());
            serializedManager.FindProperty("_windowContainer").objectReferenceValue = windowContainer.GetComponent<RectTransform>();
            serializedManager.FindProperty("_modalContainer").objectReferenceValue = modalContainer.GetComponent<RectTransform>();
            serializedManager.FindProperty("_modalOverlay").objectReferenceValue = modalOverlay;
            serializedManager.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(managerObj, "Create UIManager Hierarchy");
            Selection.activeGameObject = managerObj;
        }
        #endregion

        #region [Private Methods]
        private static GameObject CreateBaseUITemplate(string name, MenuCommand menuCommand)
        {
            Canvas canvas = GetOrCreateCanvas();
            Transform parentTransform = menuCommand.context as GameObject != null ? ((GameObject)menuCommand.context).transform : canvas.transform;

            GameObject mainPanel = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            GameObjectUtility.SetParentAndAlign(mainPanel, parentTransform.gameObject);

            RectTransform rect = mainPanel.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600f, 400f);

            Image bgImage = mainPanel.GetComponent<Image>();
            bgImage.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);

            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
            GameObjectUtility.SetParentAndAlign(titleObj, mainPanel);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -10f);
            titleRect.sizeDelta = new Vector2(-40f, 40f);

            Text titleText = titleObj.GetComponent<Text>();
            titleText.text = name;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontSize = 22;
            titleText.color = Color.white;

            GameObject closeButtonObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            GameObjectUtility.SetParentAndAlign(closeButtonObj, mainPanel);
            RectTransform closeRect = closeButtonObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-10f, -10f);
            closeRect.sizeDelta = new Vector2(30f, 30f);

            Image closeImg = closeButtonObj.GetComponent<Image>();
            closeImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);

            Undo.RegisterCreatedObjectUndo(mainPanel, "Create " + name);
            return mainPanel;
        }

        private static Canvas GetOrCreateCanvas()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            }

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            }

            return canvas;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }
        #endregion
    }
}
