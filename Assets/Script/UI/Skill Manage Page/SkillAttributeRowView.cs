using TMPro;
using UnityEngine;

namespace CoreSystem.UI.SkillManagePage
{
    public class SkillAttributeRowView : MonoBehaviour
    {
        #region [Variables/Fields]
        private const float Epsilon = 0.005f;
        private const string Minus = "−";

        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text _value;
        #endregion

        #region [Public Methods]
        public void Setup(string label, string value)
        {
            if (_label != null) _label.text = label;
            if (_value != null) _value.text = value;
        }

        public static bool IsEmpty(float baseValue, float finalValue)
        {
            return Mathf.Abs(baseValue) < Epsilon && Mathf.Abs(finalValue) < Epsilon;
        }

        public static string FormatChange(float baseValue, float finalValue, string unit, Color changeColor)
        {
            float change = finalValue - baseValue;
            if (Mathf.Abs(change) < Epsilon) return $"{Number(baseValue)}{unit}";

            string sign = change > 0f ? "+" : Minus;
            return $"{Number(baseValue)}  {sign}  <color=#{ColorUtility.ToHtmlStringRGB(changeColor)}>{Number(Mathf.Abs(change))}</color>{unit}";
        }

        public static string Number(float value)
        {
            return Mathf.Approximately(value, Mathf.Round(value)) ? value.ToString("#,0") : value.ToString("#,0.##");
        }
        #endregion
    }
}
