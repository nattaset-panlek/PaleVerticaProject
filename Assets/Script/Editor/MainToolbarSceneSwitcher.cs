using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor.UIElements;

namespace RealmOfHero.Editor
{
    [InitializeOnLoad]
    public static class MainToolbarSceneSwitcher
    {
        private static ToolbarMenu _sceneMenu;

        static MainToolbarSceneSwitcher()
        {
            // Unity might not have initialized the toolbar yet, so we wait or retry
            EditorApplication.delayCall += () => {
                InjectToToolbar();
            };

            // Keep text updated when scene changes
            EditorSceneManager.sceneOpened += (scene, mode) => UpdateButtonText();
            EditorSceneManager.sceneClosed += (scene) => UpdateButtonText();
            EditorSceneManager.sceneSaved += (scene) => UpdateButtonText();
        }

        private static void InjectToToolbar()
        {
            // Reflection to find Unity's internal Toolbar root
            var toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
            var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
            if (toolbars.Length == 0) return;

            // In Unity 2022, the root visual element is usually stored in m_Root
            var rootField = toolbarType.GetField("m_Root", BindingFlags.Instance | BindingFlags.NonPublic);
            if (rootField == null) return;

            foreach (var toolbar in toolbars)
            {
                var root = rootField.GetValue(toolbar) as VisualElement;
                if (root == null) continue;

                // ToolbarZoneRight is usually near Play/Pause buttons
                var rightZone = root.Q("ToolbarZoneRight");
                if (rightZone == null) continue;

                // Prevent double injection
                if (rightZone.Q("MainSceneSwitcher") != null) continue;

                _sceneMenu = new ToolbarMenu { name = "MainSceneSwitcher" };
                _sceneMenu.style.marginLeft = 5;
                _sceneMenu.style.marginRight = 5;
                _sceneMenu.style.minWidth = 100;
                _sceneMenu.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f);
                _sceneMenu.style.borderTopLeftRadius = 3;
                _sceneMenu.style.borderTopRightRadius = 3;
                _sceneMenu.style.borderBottomLeftRadius = 3;
                _sceneMenu.style.borderBottomRightRadius = 3;
                
                UpdateButtonText();
                
                // Add scenes to the menu before it opens
                _sceneMenu.RegisterCallback<MouseEnterEvent>(evt => BuildMenu());

                // Insert at the beginning of the right zone (closest to the center/play buttons)
                rightZone.Insert(0, _sceneMenu);
            }
        }

        private static void UpdateButtonText()
        {
            if (_sceneMenu == null) return;
            _sceneMenu.text = "📂 " + EditorSceneManager.GetActiveScene().name;
        }

        private static void BuildMenu()
        {
            if (_sceneMenu == null) return;
            
            // Clear items (The easiest way is to re-build every time mouse enters or similar)
            // But DropdownMenu doesn't have a clear, so we rebuild the entire menu logic
            var menu = _sceneMenu.menu;
            
            // We can't easily clear the menu, so we check if we need to rebuild
            // Actually, for simplicity, we just add items if they aren't there or let it be
            // Better: use the action items and handle dynamic list
            
            // Note: In 2022, appending actions multiple times will duplicate them.
            // Since we can't clear, we'll use a hack to recreate the whole button if needed, 
            // OR just populate it once.
            
            // Re-injection is cleaner for "dynamic" content in internal toolbar
            if (menu.MenuItems().Count > 0) return; // Only build once for performance

            // 1. Get Build Scenes
            var buildScenes = EditorBuildSettings.scenes.Select(s => s.path).ToList();
            if (buildScenes.Count > 0)
            {
                menu.AppendAction("--- Build Settings ---", null, DropdownMenuAction.Status.Disabled);
                foreach (var path in buildScenes)
                    AddPathToAction(menu, path, "Build/");
                menu.AppendSeparator();
            }

            // 2. Get All Scenes
            menu.AppendAction("--- All Scenes ---", null, DropdownMenuAction.Status.Disabled);
            string[] guids = AssetDatabase.FindAssets("t:Scene");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (buildScenes.Contains(path)) continue;

                string folder = Path.GetDirectoryName(path).Replace("Assets/", "").Replace("\\", "/");
                AddPathToAction(menu, path, folder + "/");
            }
        }

        private static void AddPathToAction(DropdownMenu menu, string path, string prefix)
        {
            string sceneName = Path.GetFileNameWithoutExtension(path);
            
            menu.AppendAction(prefix + sceneName, (action) => {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(path);
                }
            }, (action) => {
                return EditorSceneManager.GetActiveScene().path == path ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
            });
        }
    }
}
