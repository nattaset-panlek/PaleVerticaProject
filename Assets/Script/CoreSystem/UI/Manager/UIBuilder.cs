using System;
using UnityEngine;

namespace CoreSystem.UI
{
    public static class UIBuilder
    {
        public static UIGlobalModal BuildGlobalModal(string windowTitle, Action<UIBuilderContext> builderAction, Vector2? windowSize = null)
        {
            var modal = UIGlobalModal.GetOrCreate(windowTitle);
            if (modal == null)
            {
                Debug.LogError("[UIBuilder] Failed to get or create Global Modal.");
                return null;
            }

            modal.SetTitle(windowTitle);
            modal.gameObject.name = $"Global Modal [{windowTitle}]";

            if (windowSize.HasValue)
            {
                modal.SetWindowSize(windowSize.Value);
            }

            if (builderAction != null)
            {
                modal.Build(builderAction);
            }

            return modal;
        }
    }
}
