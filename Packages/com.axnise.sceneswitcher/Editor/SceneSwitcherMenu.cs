using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Axnise.SceneSwitcher.Editor
{
    public static class SceneSwitcherMenu
    {
        #region [Variables/Fields]
        private const string BuildHeader = "Build Profile";
        private const string RecentHeader = "Recent";
        private const string AllHeader = "All Scenes";
        private const string AdditiveHeader = "Open Additive";
        #endregion

        #region [Public Methods]
        public static GenericMenu Build()
        {
            var menu = new GenericMenu();
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                menu.AddDisabledItem(new GUIContent("Stop Play mode to switch scenes"));
                return menu;
            }

            string active = SceneSwitcherRegistry.ActiveScenePath;

            IReadOnlyList<string> build = SceneSwitcherRegistry.BuildScenes;
            if (build.Count > 0)
            {
                menu.AddDisabledItem(new GUIContent(BuildHeader));
                for (int i = 0; i < build.Count; i++)
                {
                    AddScene(menu, $"{i}  {SceneSwitcherRegistry.GetDisplayName(build[i])}", build[i], active);
                }
                menu.AddSeparator(string.Empty);
            }

            IReadOnlyList<string> recent = SceneSwitcherRegistry.RecentScenes;
            if (recent.Count > 0)
            {
                menu.AddDisabledItem(new GUIContent(RecentHeader));
                foreach (string path in recent)
                {
                    AddScene(menu, SceneSwitcherRegistry.GetDisplayName(path) + "  (" + SceneSwitcherRegistry.GetFolderPath(path) + ")", path, active);
                }
                menu.AddSeparator(string.Empty);
            }

            IReadOnlyList<string> all = SceneSwitcherRegistry.AllScenes;
            foreach (string path in all)
            {
                AddScene(menu, AllHeader + "/" + MenuPath(path), path, active);
                menu.AddItem(new GUIContent(AdditiveHeader + "/" + MenuPath(path)), false, () => SceneSwitcherRegistry.OpenScene(path, OpenSceneMode.Additive));
            }
            if (all.Count == 0) menu.AddDisabledItem(new GUIContent(AllHeader + "/No scenes found"));

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Ping Active Scene"), false, SceneSwitcherRegistry.PingActiveScene);
            menu.AddItem(new GUIContent("Open Scene Switcher Window"), false, SceneSwitcherWindow.ShowWindow);
            menu.AddItem(new GUIContent("Clear Recent"), false, SceneSwitcherRegistry.ClearRecent);
            menu.AddItem(new GUIContent("Refresh Scene List"), false, SceneSwitcherRegistry.Invalidate);
            return menu;
        }
        #endregion

        #region [Private Methods]
        private static void AddScene(GenericMenu menu, string label, string path, string active)
        {
            menu.AddItem(new GUIContent(label), path == active, () => SceneSwitcherRegistry.OpenScene(path));
        }

        private static string MenuPath(string path)
        {
            string folder = SceneSwitcherRegistry.GetFolderPath(path);
            string name = SceneSwitcherRegistry.GetDisplayName(path);
            return string.IsNullOrEmpty(folder) ? name : folder + "/" + name;
        }
        #endregion
    }
}
