using System;
using UnityEngine;

namespace CoreSystem.Settings.Options
{
    public sealed class SliderSettingOption : SettingOption
    {
        #region [Variables/Fields]
        public readonly float Min;
        public readonly float Max;
        public readonly bool WholeNumbers;

        private readonly Func<float> _get;
        private readonly Action<float> _set;
        private readonly Func<float> _default;
        private readonly Func<float, string> _format;
        #endregion

        #region [Properties]
        public override SettingOptionKind Kind => SettingOptionKind.Slider;
        public override object DefaultValue => _default();
        #endregion

        #region [Public Methods]
        public SliderSettingOption(string key, float min, float max, float defaultValue, bool wholeNumbers = true, Func<float, string> format = null)
        {
            Key = key;
            Min = min;
            Max = max;
            WholeNumbers = wholeNumbers;
            _default = () => defaultValue;
            _get = () => SettingsStorage.GetFloat(key, defaultValue);
            _set = value => SettingsStorage.SetFloat(key, value);
            _format = format;
        }

        public SliderSettingOption(string key, float min, float max, Func<float> get, Action<float> set, Func<float> defaultValue, bool wholeNumbers = true, Func<float, string> format = null)
        {
            Key = key;
            Min = min;
            Max = max;
            WholeNumbers = wholeNumbers;
            _get = get;
            _set = set;
            _default = defaultValue;
            _format = format;
        }

        public override object Read() => Normalize(_get());

        public override void Write(object value) => _set((float)Normalize(value));

        public override object Normalize(object value)
        {
            float v = value is float f ? f : value is int i ? i : TryConvert(value, out float converted) ? converted : _default();
            v = Mathf.Clamp(v, Min, Max);
            return WholeNumbers ? Mathf.Round(v) : v;
        }

        public override bool ValuesEqual(object a, object b)
        {
            return Mathf.Approximately((float)Normalize(a), (float)Normalize(b));
        }

        public string Format(float value)
        {
            return _format != null ? _format(value) : Mathf.RoundToInt(value).ToString();
        }
        #endregion

        #region [Private Methods]
        private static bool TryConvert(object value, out float result)
        {
            result = 0f;
            if (value == null) return false;
            if (value is string text) return float.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
            try
            {
                result = Convert.ToSingle(value, System.Globalization.CultureInfo.InvariantCulture);
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
