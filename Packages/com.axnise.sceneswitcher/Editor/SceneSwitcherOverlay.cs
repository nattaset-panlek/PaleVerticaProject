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
    [Overlay(typeof(SceneView), "Scene Switcher", true)]
    public class SceneSwitcherOverlay : ToolbarOverlay
    {
        // Define the IDs for the toolbar elements
        public const string DropdownId = "SceneSwitcher/Dropdown";

        // Constructor to setup the toolbar items
        public SceneSwitcherOverlay() : base(DropdownId) { }

        [EditorToolbarElement(DropdownId, typeof(SceneView))]
        class SceneDropdown : EditorToolbarDropdown
        {
            public SceneDropdown()
            {
                text = "Select Scene...";
                tooltip = "Quickly switch between scenes in the project.";
                icon = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;
                
                // Update text to current scene
                this.RegisterCallback<AttachToPanelEvent>(evt => {
                    UpdateText();
                    EditorSceneManager.sceneOpened += (scene, mode) => UpdateText();
                });

                clicked += ShowSceneMenu;
            }

            private void UpdateText()
            {
                text = EditorSceneManager.GetActiveScene().name;
            }

            private void ShowSceneMenu()
            {
                GenericMenu menu = new GenericMenu();
                
                // 1. Get Build Scenes
                var buildScenes = EditorBuildSettings.scenes.Select(s => s.path).ToList();
                if (buildScenes.Count > 0)
                {
                    menu.AddDisabledItem(new GUIContent("--- Build Settings ---"));
                    foreach (var path in buildScenes)
                        AddPathToMenu(menu, path, "Build/");
                    menu.AddSeparator("");
                }

                // 2. Get All Scenes (Recursive Search)
                menu.AddDisabledItem(new GUIContent("--- All Scenes ---"));
                string[] guids = AssetDatabase.FindAssets("t:Scene");
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (buildScenes.Contains(path)) continue;

                    string folder = Path.GetDirectoryName(path).Replace("Assets/", "").Replace("\\", "/");
                    AddPathToMenu(menu, path, folder + "/");
                }

                menu.ShowAsContext();
            }

            private void AddPathToMenu(GenericMenu menu, string path, string prefix)
            {
                string sceneName = Path.GetFileNameWithoutExtension(path);
                bool isActive = EditorSceneManager.GetActiveScene().path == path;
                
                menu.AddItem(new GUIContent(prefix + sceneName), isActive, () => {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(path);
                    }
                });
            }
        }
    }
}
