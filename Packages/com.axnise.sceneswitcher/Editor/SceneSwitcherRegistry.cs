using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Axnise.SceneSwitcher.Editor
{
    [InitializeOnLoad]
    public static class SceneSwitcherRegistry
    {
        #region [Variables/Fields]
        public const int MaxRecentScenes = 6;

        private const string RecentKeyPrefix = "Axnise.SceneSwitcher.Recent.";
        private const char RecentSeparator = '|';

        private static List<string> _buildScenes;
        private static List<string> _allScenes;

        public static event Action OnChanged;
        #endregion

        #region [Properties]
        public static IReadOnlyList<string> BuildScenes
        {
            get
            {
                EnsureCache();
                return _buildScenes;
            }
        }

        public static IReadOnlyList<string> AllScenes
        {
            get
            {
                EnsureCache();
                return _allScenes;
            }
        }

        public static IReadOnlyList<string> RecentScenes => LoadRecent().Where(p => File.Exists(p)).ToList();

        public static string ActiveScenePath => SceneManager.GetActiveScene().path;

        public static string ActiveSceneName
        {
            get
            {
                Scene scene = SceneManager.GetActiveScene();
                return string.IsNullOrEmpty(scene.name) ? "Untitled" : scene.name;
            }
        }

        private static string RecentKey => RecentKeyPrefix + Application.dataPath.GetHashCode();
        #endregion

        #region [Unity Methods]
        static SceneSwitcherRegistry()
        {
            EditorApplication.projectChanged += Invalidate;
            EditorBuildSettings.sceneListChanged += Invalidate;
            EditorSceneManager.activeSceneChangedInEditMode += HandleActiveSceneChanged;
            EditorSceneManager.sceneOpened += HandleSceneOpened;
            EditorSceneManager.sceneSaved += HandleSceneSaved;
            EditorApplication.playModeStateChanged += HandlePlayModeChanged;
        }
        #endregion

        #region [Public Methods]
        public static void Invalidate()
        {
            _buildScenes = null;
            _allScenes = null;
            OnChanged?.Invoke();
        }

        public static void OpenScene(string path)
        {
            OpenScene(path, OpenSceneMode.Single);
        }

        public static void OpenScene(string path, OpenSceneMode mode)
        {
            if (string.IsNullOrEmpty(path) || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (mode == OpenSceneMode.Single && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(path, mode);
        }

        public static void PingActiveScene()
        {
            string path = ActiveScenePath;
            if (string.IsNullOrEmpty(path)) return;
            SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (asset == null) return;
            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
        }

        public static string GetDisplayName(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public static string GetFolderPath(string path)
        {
            string folder = (Path.GetDirectoryName(path) ?? string.Empty).Replace("\\", "/");
            if (folder.StartsWith("Assets/", StringComparison.Ordinal)) folder = folder.Substring("Assets/".Length);
            else if (folder == "Assets") folder = string.Empty;
            return folder;
        }

        public static void ClearRecent()
        {
            EditorPrefs.DeleteKey(RecentKey);
            OnChanged?.Invoke();
        }
        #endregion

        #region [Private Methods]
        private static void EnsureCache()
        {
            if (_buildScenes != null && _allScenes != null) return;

            _buildScenes = EditorBuildSettings.scenes
                .Where(s => s != null && !string.IsNullOrEmpty(s.path))
                .Select(s => s.path)
                .ToList();

            _allScenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<string> LoadRecent()
        {
            string raw = EditorPrefs.GetString(RecentKey, string.Empty);
            return raw.Split(new[] { RecentSeparator }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private static void PushRecent(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            List<string> recent = LoadRecent();
            recent.Remove(path);
            recent.Insert(0, path);
            if (recent.Count > MaxRecentScenes) recent.RemoveRange(MaxRecentScenes, recent.Count - MaxRecentScenes);
            EditorPrefs.SetString(RecentKey, string.Join(RecentSeparator.ToString(), recent));
        }

        private static void HandleActiveSceneChanged(Scene previous, Scene next)
        {
            PushRecent(next.path);
            OnChanged?.Invoke();
        }

        private static void HandleSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (mode == OpenSceneMode.Single) PushRecent(scene.path);
            OnChanged?.Invoke();
        }

        private static void HandleSceneSaved(Scene scene)
        {
            OnChanged?.Invoke();
        }

        private static void HandlePlayModeChanged(PlayModeStateChange change)
        {
            OnChanged?.Invoke();
        }
        #endregion
    }
}
