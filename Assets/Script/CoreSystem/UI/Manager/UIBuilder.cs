using System;
using UnityEngine;

namespace CoreSystem.UI
{
    public static class UIBuilder
    {
        /// <summary>
        /// Creates or opens a Global Modal using the UIManager, clears its existing content,
        /// and provides a UIBuilderContext to construct the layout procedurally.
        /// </summary>
        public static UIGlobalModal BuildGlobalModal(string windowTitle, Action<UIBuilderContext> builderAction, Vector2? windowSize = null)
        {
            var uiManager = UIManager.Instance;
            if (uiManager == null)
            {
                Debug.LogError("[UIBuilder] UIManager Instance is null.");
                return null;
            }

            var registry = uiManager.ElementRegistry;
            if (registry == null)
            {
                Debug.LogError("[UIBuilder] UIElementRegistry is not assigned in UIManager.");
                return null;
            }

            // Open the global modal template
            UIGlobalModal modal = uiManager.OpenGlobalModal();
            if (modal == null)
            {
                Debug.LogError("[UIBuilder] Failed to open Global Modal. Is the template assigned in UIManager?");
                return null;
            }

            modal.SetTitle(windowTitle);
            modal.gameObject.name = $"Global Modal [{windowTitle}]";
            modal.ClearContent();
            
            if (windowSize.HasValue)
            {
                modal.SetWindowSize(windowSize.Value);
            }

            if (builderAction != null)
            {
                var context = new UIBuilderContext(modal.ContentContainer, registry);
                builderAction(context);
            }

            return modal;
        }
    }
}
