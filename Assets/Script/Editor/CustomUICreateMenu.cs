using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UI.Components;

namespace UI.Editor
{
    public static class CustomUICreateMenu
    {
        // Add to GameObject -> UI -> Switch Toggle
        [MenuItem("GameObject/UI/Custom Components/Switch Toggle", false, 10)]
        public static void CreateSwitchToggle(MenuCommand menuCommand)
        {
            // Ensure Canvas exists
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = FindOrCreateCanvas();
            }

            // 1. Create Main Container
            GameObject switchObj = new GameObject("Switch Toggle", typeof(RectTransform), typeof(Image), typeof(Toggle), typeof(SwitchToggle));
            GameObjectUtility.SetParentAndAlign(switchObj, parent);
            
            RectTransform switchRT = switchObj.GetComponent<RectTransform>();
            switchRT.sizeDelta = new Vector2(100, 50); // Typical switch size (2:1)
            
            Image backgroundImage = switchObj.GetComponent<Image>();
            backgroundImage.raycastTarget = true;
            backgroundImage.color = new Color(0.913f, 0.913f, 0.921f); // Off Color
            
            // Set circle sprite if available, otherwise just use default round square
            // We'll use a standard white square for now (assuming users will swap out sprites)
            // But Unity UI default UIMask is a good start.
            Sprite roundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (roundSprite != null) backgroundImage.sprite = roundSprite;
            backgroundImage.type = Image.Type.Sliced;

            // 2. Create Handle
            GameObject handleObj = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            GameObjectUtility.SetParentAndAlign(handleObj, switchObj);
            
            RectTransform handleRT = handleObj.GetComponent<RectTransform>();
            float handleSize = 42; // margin 4 from top/bottom
            handleRT.sizeDelta = new Vector2(handleSize, handleSize);
            
            Image handleImage = handleObj.GetComponent<Image>();
            if (roundSprite != null) handleImage.sprite = roundSprite;
            handleImage.type = Image.Type.Sliced;
            handleImage.color = Color.white;

            // 3. Configure Toggle and SwitchToggle components
            Toggle toggle = switchObj.GetComponent<Toggle>();
            toggle.transition = Selectable.Transition.None; 
            toggle.targetGraphic = backgroundImage;
            toggle.graphic = null; // Don't link handle as graphic, otherwise Unity hides it when Off

            SwitchToggle customSwitch = switchObj.GetComponent<SwitchToggle>();
            
            // Link references
            SerializedObject so = new SerializedObject(customSwitch);
            so.FindProperty("handleTransform").objectReferenceValue = handleRT;
            so.FindProperty("background").objectReferenceValue = backgroundImage;
            so.ApplyModifiedProperties();

            // Register for Undo
            Undo.RegisterCreatedObjectUndo(switchObj, "Create Switch Toggle");
            Selection.activeGameObject = switchObj;
        }

        [MenuItem("GameObject/UI/Custom Components/Animated Button", false, 11)]
        public static void CreateAnimatedButton(MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = FindOrCreateCanvas();
            }

            // 1. Create Main Container
            GameObject btnObj = new GameObject("Animated Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(AnimatedButton));
            GameObjectUtility.SetParentAndAlign(btnObj, parent);
            
            RectTransform btnRT = btnObj.GetComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(160, 50); // Typical button size
            
            Image backgroundImage = btnObj.GetComponent<Image>();
            backgroundImage.raycastTarget = true;
            backgroundImage.color = new Color(0.0f, 0.478f, 1.0f); // #007AFF (iOS Blue)
            
            Sprite roundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (roundSprite != null) backgroundImage.sprite = roundSprite;
            backgroundImage.type = Image.Type.Sliced;

            // 2. Create Text child
            GameObject textObj = new GameObject("Text (TMP)", typeof(RectTransform));
            GameObjectUtility.SetParentAndAlign(textObj, btnObj);
            
            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero; // Full stretch
            
            // Try to use TMP_Text if available, otherwise Text
            // Since TMP is standard in modern Unity, we'll try that first via reflection or direct check if we know it exists.
            // But for simplicity and to avoid compilation errors if TMP is not ready, we use standard Text or just create the object.
            
            // Actually, we'll just use a simple Text component for now as a fallback.
            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = "BUTTON";
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.color = Color.white;
            textComponent.fontSize = 24;
            
            // Register for Undo
            Undo.RegisterCreatedObjectUndo(btnObj, "Create Animated Button");
            Selection.activeGameObject = btnObj;
        }

        [MenuItem("GameObject/UI/Custom Components/Draggable Zoom Panel", false, 12)]
        public static void CreateDraggableZoomPanel(MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
                parent = FindOrCreateCanvas();

            Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // 1. Container
            GameObject containerObj = new GameObject("Draggable Zoom Panel", typeof(RectTransform), typeof(Image), typeof(DraggableZoomPanel));
            GameObjectUtility.SetParentAndAlign(containerObj, parent);

            RectTransform containerRT = containerObj.GetComponent<RectTransform>();
            containerRT.sizeDelta = new Vector2(800, 600);

            Image containerImage = containerObj.GetComponent<Image>();
            containerImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            containerImage.raycastTarget = true;
            if (uiSprite != null) { containerImage.sprite = uiSprite; containerImage.type = Image.Type.Sliced; }

            // 2. Content
            GameObject contentObj = new GameObject("Content", typeof(RectTransform));
            GameObjectUtility.SetParentAndAlign(contentObj, containerObj);

            RectTransform contentRT = contentObj.GetComponent<RectTransform>();
            contentRT.anchorMin        = new Vector2(0.5f, 0.5f);
            contentRT.anchorMax        = new Vector2(0.5f, 0.5f);
            contentRT.pivot            = new Vector2(0.5f, 0.5f);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta        = new Vector2(2000, 2000);

            // 3. Link Content → DraggableZoomPanel
            DraggableZoomPanel panel = containerObj.GetComponent<DraggableZoomPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("_content").objectReferenceValue = contentRT;
            so.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(containerObj, "Create Draggable Zoom Panel");
            Selection.activeGameObject = containerObj;
        }

        [MenuItem("GameObject/UI/Custom Components/UI Line Renderer", false, 13)]
        public static void CreateUILineRenderer(MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
                parent = FindOrCreateCanvas();

            GameObject go = new GameObject("UI Line Renderer", typeof(RectTransform), typeof(UI.Components.UILineRenderer));
            GameObjectUtility.SetParentAndAlign(go, parent);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = Vector2.zero;
            rt.anchorMax        = Vector2.one;
            rt.sizeDelta        = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            Undo.RegisterCreatedObjectUndo(go, "Create UI Line Renderer");
            Selection.activeGameObject = go;
        }

        private static GameObject FindOrCreateCanvas()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null) return canvas.gameObject;

            GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObj.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            if (Object.FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            return canvasObj;
        }
    }
}
