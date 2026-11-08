using CoreSystem.Pathfinding;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoreSystem.Editor.World
{
    public static class WalkableGridSceneInstaller
    {
        #region [Variables/Fields]
        public const string SandBoxScenePath = "Assets/Scenes/SandBox.unity";
        public const string SandBoxWalkablePath = "Assets/Scenes/SandBox_MapLab/Environment/Walkable.json";
        public const string ObjectName = "Walkable Grid";
        #endregion

        #region [Public Methods]
        public static void InstallSandBoxBatch()
        {
            int exitCode = 0;
            try
            {
                if (!Install(SandBoxScenePath, SandBoxWalkablePath)) exitCode = 1;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                exitCode = 1;
            }
            EditorApplication.Exit(exitCode);
        }

        public static bool Install(string scenePath, params string[] walkablePaths)
        {
            var data = new TextAsset[walkablePaths.Length];
            for (int i = 0; i < walkablePaths.Length; i++)
            {
                data[i] = AssetDatabase.LoadAssetAtPath<TextAsset>(walkablePaths[i]);
                if (data[i] != null) continue;
                Debug.LogError($"[WalkableGridSceneInstaller] Walkable data not found: {walkablePaths[i]}");
                return false;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var manager = FindManager(scene);
            bool created = manager == null;
            if (created) manager = new GameObject(ObjectName).AddComponent<WalkableGridManager>();

            var serialized = new SerializedObject(manager);
            var list = serialized.FindProperty("_walkableData");
            list.arraySize = data.Length;
            for (int i = 0; i < data.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = data[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                Debug.LogError($"[WalkableGridSceneInstaller] Could not save {scenePath}");
                return false;
            }

            Debug.Log($"[WalkableGridSceneInstaller] {scenePath}: {(created ? "added" : "updated")} '{manager.name}' with {string.Join(", ", walkablePaths)}");
            return true;
        }
        #endregion

        #region [Private Methods]
        private static WalkableGridManager FindManager(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var found = root.GetComponentInChildren<WalkableGridManager>(true);
                if (found != null) return found;
            }
            return null;
        }
        #endregion
    }
}
