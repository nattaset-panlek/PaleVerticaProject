using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Axnise.SceneSwitcher.Editor
{
    public class SceneSwitcherWindow : EditorWindow
    {
        #region [Variables/Fields]
        private Vector2 _scroll;
        private string _search = string.Empty;
        #endregion

        #region [Unity Methods]
        private void OnEnable()
        {
            SceneSwitcherRegistry.OnChanged += Repaint;
        }

        private void OnDisable()
        {
            SceneSwitcherRegistry.OnChanged -= Repaint;
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60))) SceneSwitcherRegistry.Invalidate();
            }

            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                DrawSection("Build Profile", SceneSwitcherRegistry.BuildScenes);
                DrawSection("Recent", SceneSwitcherRegistry.RecentScenes);
                DrawSection("All Scenes", SceneSwitcherRegistry.AllScenes);
                EditorGUILayout.EndScrollView();
            }
        }
        #endregion

        #region [Public Methods]
        [MenuItem("Window/Axnise/Scene Switcher")]
        public static void ShowWindow()
        {
            GetWindow<SceneSwitcherWindow>("Scene Switcher");
        }
        #endregion

        #region [Private Methods]
        private void DrawSection(string title, IReadOnlyList<string> paths)
        {
            if (paths.Count == 0) return;
            EditorGUILayout.Space(4);
            GUILayout.Label(title, EditorStyles.miniBoldLabel);

            string active = SceneSwitcherRegistry.ActiveScenePath;
            foreach (string path in paths)
            {
                string name = SceneSwitcherRegistry.GetDisplayName(path);
                string folder = SceneSwitcherRegistry.GetFolderPath(path);
                if (!string.IsNullOrEmpty(_search) && name.IndexOf(_search, System.StringComparison.OrdinalIgnoreCase) < 0
                    && folder.IndexOf(_search, System.StringComparison.OrdinalIgnoreCase) < 0) continue;

                bool isActive = path == active;
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(isActive))
                    {
                        if (GUILayout.Button(new GUIContent(name, path), GUILayout.Height(22))) SceneSwitcherRegistry.OpenScene(path);
                    }
                    if (GUILayout.Button(new GUIContent("+", "Open additive"), GUILayout.Width(24), GUILayout.Height(22)))
                    {
                        SceneSwitcherRegistry.OpenScene(path, OpenSceneMode.Additive);
                    }
                }
                if (!string.IsNullOrEmpty(folder)) EditorGUILayout.LabelField(folder, EditorStyles.centeredGreyMiniLabel);
            }
        }
        #endregion
    }
}
