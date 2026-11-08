using System;

namespace CoreSystem.Settings.Options
{
    public sealed class ToggleSettingOption : SettingOption
    {
        #region [Variables/Fields]
        private readonly Func<bool> _get;
        private readonly Action<bool> _set;
        private readonly Func<bool> _default;
        private readonly bool _checkbox;
        #endregion

        #region [Properties]
        public override SettingOptionKind Kind => _checkbox ? SettingOptionKind.Checkbox : SettingOptionKind.Toggle;
        public override object DefaultValue => _default();
        #endregion

        #region [Public Methods]
        public ToggleSettingOption(string key, bool defaultValue, bool checkbox = false)
        {
            Key = key;
            _default = () => defaultValue;
            _get = () => SettingsStorage.GetBool(key, defaultValue);
            _set = value => SettingsStorage.SetBool(key, value);
            _checkbox = checkbox;
        }

        public ToggleSettingOption(string key, Func<bool> get, Action<bool> set, Func<bool> defaultValue, bool checkbox = false)
        {
            Key = key;
            _get = get;
            _set = set;
            _default = defaultValue;
            _checkbox = checkbox;
        }

        public override object Read() => _get();

        public override void Write(object value) => _set(value is bool b && b);

        public override object Normalize(object value) => value is bool b && b;
        #endregion
    }
}
