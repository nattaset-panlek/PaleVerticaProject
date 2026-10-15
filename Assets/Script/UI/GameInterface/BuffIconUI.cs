using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CoreSystem.Skills.Logic;
using CoreSystem.UI.Utility;

namespace CoreSystem.UI.GameInterface
{
    public class BuffIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image Icon;
        public Image ReminingValue;

        private ActiveBuff _buff;

        public void SetBuffIcon(ActiveBuff buff)
        {
            _buff = buff;
            if (Icon != null)
            {
                Icon.sprite = buff.Icon;
            }
            if (ReminingValue != null)
            {
                float fillAmount = Mathf.Clamp01(buff.RemainingDuration / buff.TotalDuration);
                ReminingValue.fillAmount = fillAmount;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_buff == null || HoverTextManager.Instance == null) return;

            string title = _buff.BuffName;
            if (_buff.MaxStacks > 1)
            {
                title += $" ({_buff.Stacks}/{_buff.MaxStacks})";
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (var mod in _buff.Modifiers)
            {
                string sign = mod.Value >= 0 ? "+" : "";
                string typeStr = (mod.Type == CoreSystem.ModifierType.PercentAdd || mod.Type == CoreSystem.ModifierType.PercentMult) ? "%" : "";
                sb.AppendLine($"{sign}{mod.Value}{typeStr} {_buff.TargetAttribute}");
            }

            sb.Append($"Remaining: {_buff.RemainingDuration:F1}s");

            HoverTextManager.Instance.Show(title, sb.ToString());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (HoverTextManager.Instance != null)
            {
                HoverTextManager.Instance.Hide();
            }
        }

        private void OnDisable()
        {
            if (HoverTextManager.Instance != null)
            {
                HoverTextManager.Instance.Hide();
            }
        }
    }
}
