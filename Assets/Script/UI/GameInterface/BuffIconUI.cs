using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BuffIconUI : MonoBehaviour
{
    public Image Icon;
    public Image ReminingValue;

    public void SetBuffIcon(Sprite icon, float remainingTime, float totalDuration)
    {
        if (Icon != null)
        {
            Icon.sprite = icon;
        }
        if (ReminingValue != null)
        {
            float fillAmount = Mathf.Clamp01(remainingTime / totalDuration);
            ReminingValue.fillAmount = fillAmount;
        }
    }
}
