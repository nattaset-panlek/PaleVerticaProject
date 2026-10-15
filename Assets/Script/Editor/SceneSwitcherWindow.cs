using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Toolbars;

namespace RealmOfHero.Editor
{
    // --- traditional Window version for easy access ---
    public class SceneSwitcherWindow : EditorWindow
    {
        private Vector2 scrollPos;

        [MenuItem("Window/Realm of Hero/Scene Switcher")]
        public static void ShowWindow()
        {
            GetWindow<SceneSwitcherWindow>("Scene Switcher");
        }

        private void OnGUI()
        {
            GUILayout.Label("Quick Scene Switcher", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // Build Scenes
            var buildScenes = EditorBuildSettings.scenes.Select(s => s.path).ToList();
            if (buildScenes.Count > 0)
            {
                GUILayout.Label("Build Settings", EditorStyles.miniBoldLabel);
                foreach (var path in buildScenes)
                {
                    DrawSceneButton(path, "Build/");
                }
                EditorGUILayout.Space(10);
            }

            // All Scenes
            GUILayout.Label("All Scenes", EditorStyles.miniBoldLabel);
            string[] guids = AssetDatabase.FindAssets("t:Scene");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (buildScenes.Contains(path)) continue;
                DrawSceneButton(path, "");
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSceneButton(string path, string prefix)
        {
            string sceneName = Path.GetFileNameWithoutExtension(path);
            bool isActive = EditorSceneManager.GetActiveScene().path == path;

            GUI.enabled = !isActive;
            if (isActive) GUI.backgroundColor = Color.green;

            if (GUILayout.Button(prefix + sceneName, GUILayout.Height(25)))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(path);
                }
            }

            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
        }
    }
}
