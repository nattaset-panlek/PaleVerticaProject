using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreSystem.Entity;
using CoreSystem.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoreSystem.Editor.World
{
    public static class GameCoreSceneConverter
    {
        #region [Variables/Fields]
        private const string PrefabPath = "Assets/Prefab/Game Core/[Game Core].prefab";
        private const string SettingsPath = "Assets/Resources/GameCoreSettings.asset";
        private const string SourceScene = "Assets/Scenes/SandBox.unity";
        private const string TitleSceneName = "Title";
        private static readonly string[] SystemRoots = { "Player", "Canvas", "Main Camera", "Manager", "EventSystem" };
        private static readonly string[] Scenes = { SourceScene, "Assets/Scenes/Town.unity", "Assets/Scenes/Field.unity" };
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/World/Convert Scenes To Game Core")]
        public static void Convert()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            {
                Debug.LogError("[Game Core].prefab already exists. Delete it first to convert again.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            GameObject prefab = BuildPrefab();
            if (prefab == null) return;

            foreach (string path in Scenes) StripScene(path);
            CreateSettings(prefab);
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(SourceScene);
            Debug.Log("Scenes converted. Press Play from any scene.");
        }
        #endregion

        #region [Private Methods]
        private static GameObject BuildPrefab()
        {
            Scene scene = EditorSceneManager.OpenScene(SourceScene);
            List<GameObject> roots = FindSystemRoots(scene);
            if (roots.Count != SystemRoots.Length)
            {
                Debug.LogError($"SandBox needs root objects: {string.Join(", ", SystemRoots)}. Found {roots.Count}. Nothing was changed.");
                return null;
            }

            EnsureFolder(PrefabPath);
            var core = new GameObject("[Game Core]");
            foreach (GameObject root in roots) root.transform.SetParent(core.transform, true);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(core, PrefabPath);
            EditorSceneManager.OpenScene(SourceScene);
            return prefab;
        }

        private static void StripScene(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) return;

            Scene scene = EditorSceneManager.OpenScene(path);
            List<GameObject> roots = FindSystemRoots(scene);
            CreateStartPoint(roots);
            foreach (GameObject root in roots) Object.DestroyImmediate(root);
            EditorSceneManager.SaveScene(scene);
        }

        private static List<GameObject> FindSystemRoots(Scene scene)
        {
            return scene.GetRootGameObjects().Where(root => SystemRoots.Contains(root.name)).ToList();
        }

        private static void CreateStartPoint(List<GameObject> roots)
        {
            GameObject playerRoot = roots.FirstOrDefault(root => root.GetComponentInChildren<Character>(true) != null);
            var go = new GameObject("Start Point");
            go.transform.position = playerRoot != null ? playerRoot.transform.position : Vector3.zero;

            var serialized = new SerializedObject(go.AddComponent<MapSpawnPoint>());
            serialized.FindProperty("_id").stringValue = GameCoreLoader.StartSpawnId;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateSettings(GameObject prefab)
        {
            EnsureFolder(SettingsPath);
            var settings = ScriptableObject.CreateInstance<GameCoreSettings>();
            settings.CorePrefab = prefab;
            settings.ScenesWithoutCore.Add(TitleSceneName);
            AssetDatabase.CreateAsset(settings, SettingsPath);
        }

        private static void EnsureFolder(string assetPath)
        {
            string folder = Path.GetDirectoryName(Path.GetFullPath(assetPath));
            if (Directory.Exists(folder)) return;
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }
        #endregion
    }
}
