namespace CoreSystem.Settings.Options
{
    public abstract class SettingOption
    {
        #region [Variables/Fields]
        public string Key;
        public string PageId;
        public string Group;
        public string Label;
        public string Tooltip;
        public int Order;
        public string InlineWithKey;
        #endregion

        #region [Properties]
        public abstract SettingOptionKind Kind { get; }
        public abstract object DefaultValue { get; }
        public bool IsInline => !string.IsNullOrEmpty(InlineWithKey);
        #endregion

        #region [Public Methods]
        public abstract object Read();

        public abstract void Write(object value);

        public virtual bool ValuesEqual(object a, object b)
        {
            return Equals(a, b);
        }

        public virtual object Normalize(object value)
        {
            return value;
        }
        #endregion
    }
}
