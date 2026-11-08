using System.Linq;
using CoreSystem.UI.TitleMenu;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CoreSystem.Editor.World
{
    public static class TitleSceneBuilder
    {
        #region [Variables/Fields]
        private const string ScenePath = "Assets/Scenes/Title.unity";
        private const string BoldFontPath = "Assets/Art/Front/Kanit/Kanit-Bold SDF.asset";
        private const string FontPath = "Assets/Art/Front/Kanit/Kanit-SemiBold SDF.asset";
        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        private static readonly Vector2 ButtonSize = new Vector2(320f, 56f);
        private static readonly Color BackgroundColor = new Color(0.07f, 0.07f, 0.09f);
        private static readonly Color ButtonColor = new Color(1f, 1f, 1f, 0.1f);
        private const float FirstButtonY = -60f;
        private const float ButtonStep = 72f;
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/World/Build Title Scene")]
        public static void Build()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                Debug.LogError("Title.unity already exists. Delete it first to rebuild.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            CreateEventSystem();
            CreateMenu();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings();
            Debug.Log("Title scene built and set as the first scene.");
        }
        #endregion

        #region [Private Methods]
        private static void CreateCamera()
        {
            var go = new GameObject("Title Camera");
            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        private static void CreateMenu()
        {
            var go = new GameObject("Title Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();

            TextMeshProUGUI title = CreateText("Game Title", go.transform, "PALE VERTICA", BoldFontPath, 84f);
            Place(title.rectTransform, new Vector2(0f, 260f), new Vector2(1200f, 120f));
            TextMeshProUGUI subtitle = CreateText("Subtitle", go.transform, "Whispers of the Pale Throne", FontPath, 28f);
            Place(subtitle.rectTransform, new Vector2(0f, 180f), new Vector2(1200f, 48f));

            Button continueButton = CreateButton(go.transform, "Continue", 0);
            Button newGameButton = CreateButton(go.transform, "New Game", 1);
            Button exitButton = CreateButton(go.transform, "Exit", 2);

            var serialized = new SerializedObject(go.AddComponent<TitleMenuView>());
            serialized.FindProperty("_continueButton").objectReferenceValue = continueButton;
            serialized.FindProperty("_newGameButton").objectReferenceValue = newGameButton;
            serialized.FindProperty("_exitButton").objectReferenceValue = exitButton;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Button CreateButton(Transform parent, string label, int index)
        {
            var go = new GameObject(label + " Button", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = ButtonColor;
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            Place(image.rectTransform, new Vector2(0f, FirstButtonY - ButtonStep * index), ButtonSize);

            TextMeshProUGUI text = CreateText("Label", go.transform, label, FontPath, 24f);
            text.raycastTarget = false;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string content, string fontPath, float size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<TextMeshProUGUI>();
            text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            text.text = content;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            return text;
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void AddToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        #endregion
    }
}
