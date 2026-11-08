using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CoreSystem.Settings.Options;

namespace CoreSystem.Settings
{
    public static class SettingsRegistry
    {
        #region [Variables/Fields]
        private static readonly List<SettingsPage> _pages = new List<SettingsPage>();
        private static readonly List<SettingOption> _options = new List<SettingOption>();
        private static bool _defaultsRegistered;

        public static event Action OnChanged;
        #endregion

        #region [Properties]
        public static IReadOnlyList<SettingsPage> Pages
        {
            get
            {
                EnsureDefaults();
                return _pages.OrderBy(p => p.Order).ToList();
            }
        }

        public static IReadOnlyList<SettingOption> Options
        {
            get
            {
                EnsureDefaults();
                return _options;
            }
        }
        #endregion

        #region [Public Methods]
        public static SettingsPage AddPage(SettingsPage page)
        {
            if (page == null || string.IsNullOrEmpty(page.Id)) return null;
            _pages.RemoveAll(p => p.Id == page.Id);
            _pages.Add(page);
            OnChanged?.Invoke();
            return page;
        }

        public static T AddOption<T>(string pageId, string group, string label, T option, int order = -1, string tooltip = null) where T : SettingOption
        {
            if (option == null || string.IsNullOrEmpty(option.Key)) return option;
            option.PageId = pageId;
            option.Group = group;
            option.Label = label;
            option.Tooltip = tooltip;
            option.Order = order >= 0 ? order : _options.Count(o => o.PageId == pageId);
            _options.RemoveAll(o => o.Key == option.Key);
            _options.Add(option);
            OnChanged?.Invoke();
            return option;
        }

        public static bool RemovePage(string pageId)
        {
            bool removed = _pages.RemoveAll(p => p.Id == pageId) > 0;
            _options.RemoveAll(o => o.PageId == pageId);
            if (removed) OnChanged?.Invoke();
            return removed;
        }

        public static bool RemoveOption(string key)
        {
            bool removed = _options.RemoveAll(o => o.Key == key) > 0;
            if (removed) OnChanged?.Invoke();
            return removed;
        }

        public static SettingsPage FindPage(string pageId)
        {
            EnsureDefaults();
            return _pages.FirstOrDefault(p => p.Id == pageId);
        }

        public static SettingOption Find(string key)
        {
            EnsureDefaults();
            return _options.FirstOrDefault(o => o.Key == key);
        }

        public static List<SettingOption> GetOptions(string pageId)
        {
            EnsureDefaults();
            return _options.Where(o => o.PageId == pageId).OrderBy(o => o.Order).ToList();
        }

        public static List<string> GetGroups(string pageId)
        {
            var groups = new List<string>();
            foreach (var option in GetOptions(pageId))
            {
                string group = option.Group ?? string.Empty;
                if (!groups.Contains(group)) groups.Add(group);
            }
            return groups;
        }

        public static List<SettingOption> GetInline(string key)
        {
            EnsureDefaults();
            return _options.Where(o => o.InlineWithKey == key).OrderBy(o => o.Order).ToList();
        }

        public static void EnsureDefaults()
        {
            if (_defaultsRegistered) return;
            _defaultsRegistered = true;
            SettingsCatalog.RegisterDefaults();
        }

        public static void ResetToCatalog()
        {
            _pages.Clear();
            _options.Clear();
            _defaultsRegistered = false;
            EnsureDefaults();
            OnChanged?.Invoke();
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _pages.Clear();
            _options.Clear();
            _defaultsRegistered = false;
            OnChanged = null;
        }
        #endregion
    }
}
