using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Settings.Options
{
    public sealed class ChoiceSettingOption : SettingOption
    {
        #region [Variables/Fields]
        private readonly Func<IList<string>> _labels;
        private readonly Func<int> _get;
        private readonly Action<int> _set;
        private readonly Func<int> _default;
        #endregion

        #region [Properties]
        public override SettingOptionKind Kind => SettingOptionKind.Choice;
        public override object DefaultValue => Normalize(_default());
        public IList<string> Labels => _labels() ?? new List<string>();
        #endregion

        #region [Public Methods]
        public ChoiceSettingOption(string key, IList<string> labels, int defaultIndex)
        {
            Key = key;
            _labels = () => labels;
            _default = () => defaultIndex;
            _get = () => SettingsStorage.GetInt(key, defaultIndex);
            _set = value => SettingsStorage.SetInt(key, value);
        }

        public ChoiceSettingOption(string key, Func<IList<string>> labels, Func<int> get, Action<int> set, Func<int> defaultIndex)
        {
            Key = key;
            _labels = labels;
            _get = get;
            _set = set;
            _default = defaultIndex;
        }

        public override object Read() => Normalize(_get());

        public override void Write(object value) => _set((int)Normalize(value));

        public override object Normalize(object value)
        {
            int index = value is int i ? i : TryConvert(value, out int converted) ? converted : _default();
            int count = Labels.Count;
            return count == 0 ? 0 : Mathf.Clamp(index, 0, count - 1);
        }
        #endregion

        #region [Private Methods]
        private static bool TryConvert(object value, out int result)
        {
            result = 0;
            if (value == null) return false;
            if (value is string text) return int.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out result);
            try
            {
                result = Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }
}
