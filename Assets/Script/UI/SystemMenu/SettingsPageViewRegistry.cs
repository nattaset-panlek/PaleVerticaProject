using System;
using System.Collections.Generic;
using CoreSystem.Settings;
using CoreSystem.UI;

namespace CoreSystem.UI.SystemMenu
{
    public static class SettingsPageViewRegistry
    {
        #region [Variables/Fields]
        private static readonly Dictionary<string, Action<UIBuilderContext, SettingsWindow>> _views = new Dictionary<string, Action<UIBuilderContext, SettingsWindow>>();
        #endregion

        #region [Public Methods]
        public static void Register(string viewId, Action<UIBuilderContext, SettingsWindow> build)
        {
            if (string.IsNullOrEmpty(viewId) || build == null) return;
            _views[viewId] = build;
        }

        public static bool TryGet(string viewId, out Action<UIBuilderContext, SettingsWindow> build)
        {
            EnsureDefaults();
            build = null;
            return !string.IsNullOrEmpty(viewId) && _views.TryGetValue(viewId, out build);
        }
        #endregion

        #region [Private Methods]
        private static void EnsureDefaults()
        {
            if (!_views.ContainsKey(SettingKeys.KeybindView)) _views[SettingKeys.KeybindView] = KeybindPageView.Build;
        }
        #endregion
    }
}
