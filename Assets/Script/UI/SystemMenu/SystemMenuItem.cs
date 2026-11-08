using System;

namespace CoreSystem.UI.SystemMenu
{
    public readonly struct SystemMenuItem
    {
        #region [Variables/Fields]
        public readonly string Label;
        public readonly Action Action;
        public readonly bool Enabled;
        public readonly bool IsSeparator;
        #endregion

        #region [Properties]
        public static SystemMenuItem Separator => new SystemMenuItem(null, null, false, true);
        #endregion

        #region [Public Methods]
        public SystemMenuItem(string label, Action action, bool enabled = true) : this(label, action, enabled, false)
        {
        }

        private SystemMenuItem(string label, Action action, bool enabled, bool isSeparator)
        {
            Label = label;
            Action = action;
            Enabled = enabled && !isSeparator && action != null;
            IsSeparator = isSeparator;
        }
        #endregion
    }
}
