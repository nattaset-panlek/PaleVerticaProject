using System.Linq;
using CoreSystem.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace CoreSystem.Editor.World
{
    [InitializeOnLoad]
    public static class ScenePreviewCamera
    {
        #region [Variables/Fields]
        private const string CameraName = "[Preview Camera]";
        private const string MenuPath = "Pale Vertica/World/Preview Camera";
        private const string EnabledKey = "PaleVertica.ScenePreviewCamera";
        private const float CameraDepth = -10f;
        #endregion

        #region [Properties]
        private static bool IsEnabled
        {
            get => EditorPrefs.GetBool(EnabledKey, true);
            set => EditorPrefs.SetBool(EnabledKey, value);
        }
        #endregion

        #region [Unity Methods]
        static ScenePreviewCamera()
        {
            EditorSceneManager.sceneOpened += (scene, mode) => Refresh();
            EditorSceneManager.newSceneCreated += (scene, setup, mode) => Refresh();
            EditorApplication.playModeStateChanged += HandlePlayMode;
            EditorApplication.delayCall += Refresh;
        }
        #endregion

        #region [Private Methods]
        [SettingsProvider]
        private static SettingsProvider CreateSettings()
        {
            return new SettingsProvider("Preferences/Pale Vertica", SettingsScope.User)
            {
                guiHandler = _ =>
                {
                    bool enabled = EditorGUILayout.Toggle("Preview Camera In Maps", IsEnabled);
                    if (enabled == IsEnabled) return;
                    IsEnabled = enabled;
                    Refresh();
                }
            };
        }

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            IsEnabled = !IsEnabled;
            Refresh();
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, IsEnabled);
            return true;
        }

        private static void HandlePlayMode(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode) Remove();
            else if (change == PlayModeStateChange.EnteredEditMode) Refresh();
        }

        private static void Refresh()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            Remove();
            if (!IsEnabled) return;

            Camera source = CoreCamera();
            if (source == null || SceneHasCamera()) return;
            Create(source);
        }

        private static Camera CoreCamera()
        {
            var settings = Resources.Load<GameCoreSettings>(GameCoreSettings.ResourcePath);
            return settings != null && settings.CorePrefab != null ? settings.CorePrefab.GetComponentInChildren<Camera>(true) : null;
        }

        private static bool SceneHasCamera()
        {
            Scene scene = SceneManager.GetActiveScene();
            return scene.IsValid() && scene.GetRootGameObjects().Any(root => root.GetComponentInChildren<Camera>(true) != null);
        }

        private static void Create(Camera source)
        {
            var go = new GameObject(CameraName) { hideFlags = HideFlags.DontSave };
            go.transform.position = StartPosition();

            var camera = go.AddComponent<Camera>();
            EditorUtility.CopySerialized(source, camera);

            var sourceData = source.GetComponent<UniversalAdditionalCameraData>();
            if (sourceData != null) EditorUtility.CopySerialized(sourceData, camera.GetUniversalAdditionalCameraData());
        }

        private static Vector3 StartPosition()
        {
            MapSpawnPoint start = Object.FindObjectsByType<MapSpawnPoint>(FindObjectsSortMode.None)
                .FirstOrDefault(point => new SerializedObject(point).FindProperty("_id").stringValue == GameCoreLoader.StartSpawnId);
            Vector3 position = start != null ? start.transform.position : Vector3.zero;
            position.z = CameraDepth;
            return position;
        }

        private static void Remove()
        {
            foreach (Camera camera in Resources.FindObjectsOfTypeAll<Camera>().Where(camera => camera.name == CameraName))
            {
                Object.DestroyImmediate(camera.gameObject);
            }
        }
        #endregion
    }
}
