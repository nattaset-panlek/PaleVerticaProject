using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoreSystem.Editor.CharacterAsset
{
    public static class CharacterSortingSceneInstaller
    {
        #region [Variables/Fields]
        public const string SandBoxScenePath = "Assets/Scenes/Dev/SandBox.unity";
        public const string SandBoxPlayerName = "Player";
        private const string Tag = "[CharacterSortingSceneInstaller]";
        #endregion

        #region [Public Methods]
        public static void InstallSandBoxBatch()
        {
            int exitCode = 0;
            try
            {
                if (!Install(SandBoxScenePath, SandBoxPlayerName)) exitCode = 1;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                exitCode = 1;
            }
            EditorApplication.Exit(exitCode);
        }

        public static bool Install(string scenePath, string rootName)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject root = FindRoot(scene, rootName);
            if (root == null)
            {
                Debug.LogError($"{Tag} {scenePath} has no root object named '{rootName}'.");
                return false;
            }

            var log = new List<string>();
            CharacterAssetSettings settings = CharacterAssetSettings.Instance;
            CharacterPrefabGenerator.ApplySortingGroups(root.transform, settings, log);
            foreach (string line in log) Debug.LogError($"{Tag} {rootName}: {line}");
            if (log.Count > 0) return false;

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                Debug.LogError($"{Tag} Could not save {scenePath}");
                return false;
            }

            Debug.Log($"{Tag} {scenePath}: '{rootName}/{settings.ModelPath}' sorting group order {settings.ModelSortingOrder}, '{rootName}/{settings.ShadowPath}' order {settings.ShadowSortingOrder} sorted at root");
            return true;
        }
        #endregion

        #region [Private Methods]
        private static GameObject FindRoot(Scene scene, string rootName)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == rootName) return root;
            }
            return null;
        }
        #endregion
    }
}
