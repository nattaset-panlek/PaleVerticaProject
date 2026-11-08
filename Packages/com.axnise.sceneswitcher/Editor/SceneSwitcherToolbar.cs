using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace Axnise.SceneSwitcher.Editor
{
    [InitializeOnLoad]
    public static class SceneSwitcherToolbar
    {
        #region [Variables/Fields]
        public const string ElementPath = "Axnise/Scene Switcher";
        private const string Tooltip = "Switch scene: Build Profile, recent and all scenes";
        #endregion

        #region [Unity Methods]
        static SceneSwitcherToolbar()
        {
            SceneSwitcherRegistry.OnChanged += Refresh;
        }
        #endregion

        #region [Public Methods]
        [MainToolbarElement(ElementPath, defaultDockPosition = MainToolbarDockPosition.Middle)]
        public static MainToolbarElement CreateElement()
        {
            Texture2D icon = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;
            var content = new MainToolbarContent(SceneSwitcherRegistry.ActiveSceneName, icon, Tooltip);
            var dropdown = new MainToolbarDropdown(content, ShowMenu);
            dropdown.enabled = !EditorApplication.isPlayingOrWillChangePlaymode;
            return dropdown;
        }

        public static void Refresh()
        {
            MainToolbar.Refresh(ElementPath);
        }
        #endregion

        #region [Private Methods]
        private static void ShowMenu(Rect rect)
        {
            SceneSwitcherMenu.Build().DropDown(rect);
        }
        #endregion
    }
}
