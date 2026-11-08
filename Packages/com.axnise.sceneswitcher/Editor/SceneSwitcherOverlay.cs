using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace Axnise.SceneSwitcher.Editor
{
    [Overlay(typeof(SceneView), "Scene Switcher", false)]
    public class SceneSwitcherOverlay : ToolbarOverlay
    {
        #region [Variables/Fields]
        public const string DropdownId = "Axnise/SceneSwitcher/Dropdown";
        #endregion

        #region [Public Methods]
        public SceneSwitcherOverlay() : base(DropdownId)
        {
        }
        #endregion

        [EditorToolbarElement(DropdownId, typeof(SceneView))]
        private class SceneDropdown : EditorToolbarDropdown
        {
            #region [Unity Methods]
            public SceneDropdown()
            {
                tooltip = "Switch scene";
                icon = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;
                clicked += ShowMenu;
                RegisterCallback<AttachToPanelEvent>(_ =>
                {
                    UpdateText();
                    SceneSwitcherRegistry.OnChanged += UpdateText;
                });
                RegisterCallback<DetachFromPanelEvent>(_ => SceneSwitcherRegistry.OnChanged -= UpdateText);
            }
            #endregion

            #region [Private Methods]
            private void UpdateText()
            {
                text = SceneSwitcherRegistry.ActiveSceneName;
            }

            private void ShowMenu()
            {
                SceneSwitcherMenu.Build().DropDown(worldBound);
            }
            #endregion
        }
    }
}
