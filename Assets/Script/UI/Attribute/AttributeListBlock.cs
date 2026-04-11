using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttributeListBlock : MonoBehaviour
{
    public Image AttributeIcon;
    public TMP_Text AttributeNameText;
    public TMP_Text AttributeValueText;

    public void Setup(CoreSystem.Attributes.AttributeTemplate template, float baseValue, float refineBonus)
    {
        if (template == null) return;

        if (AttributeNameText != null) 
            AttributeNameText.text = template.Name;

        if (AttributeValueText != null)
        {
            string sign = baseValue >= 0 ? "+" : "";
            string percent = template.IsPercent ? "%" : "";
            
            string bonusText = "";
            if (refineBonus != 0)
            {
                string bSign = refineBonus > 0 ? "+" : "";
                bonusText = $" <color=#A0A0A0>({bSign}{refineBonus}{percent})</color>";
            }

            AttributeValueText.text = $"{sign}{baseValue}{percent}{bonusText}";
        }

        if (AttributeIcon != null)
        {
            // Assuming icons are in Resources
            // AttributeIcon.sprite = Resources.Load<Sprite>($"Icons/Attributes/{template.Icon}");
        }
    }

    public void Setup(CoreSystem.Attributes.AttributeTemplate template, float value)
    {
        Setup(template, value, 0);
    }

    public void Setup(CoreSystem.Attributes.AttributeTemplate template, CoreSystem.Attributes.Attribute attr)
    {
        if (template == null || attr == null) return;

        if (AttributeNameText != null) 
            AttributeNameText.text = template.Name;

        if (AttributeValueText != null)
        {
            string percent = template.IsPercent ? "%" : "";
            float finalValue = attr.CurrentValue;
            float baseValue = attr.BaseValue;
            float flatBonus = attr.FlatBonusSum;
            float percentBonus = attr.PercentBonusSum;

            // 1. Final Value (Bold and primary)
            string finalText = $"<b>{finalValue:F0}{percent}</b>";

            // 2. Breakdown (Base + Bonuses)
            // Show only if base differs from final
            string breakdownText = "";
            if (finalValue != baseValue)
            {
                string basePart = $"{baseValue:F0}{percent}";
                string flatPart = flatBonus != 0 ? $" {(flatBonus > 0 ? "+" : "")}{flatBonus:F0}{percent}" : "";
                string percentPart = percentBonus != 0 ? $" {(percentBonus > 0 ? "+" : "")}{percentBonus * 100:F0}%" : "";

                breakdownText = $" <size=85%><color=#A0A0A0>({basePart}<color=#25E3B7>{flatPart}{percentPart}</color>)</color></size>";
            }

            AttributeValueText.text = $"{finalText}{breakdownText}";
        }
    }

    public void Setup(string name, string valueText)
    {
        if (AttributeNameText != null) AttributeNameText.text = name;
        if (AttributeValueText != null) AttributeValueText.text = valueText;
    }
}
