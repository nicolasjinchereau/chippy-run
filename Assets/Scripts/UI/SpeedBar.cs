using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedBar : MonoBehaviour
{
    public Image borderImage;
    public Image fillImage;
    public Gradient fillColor;
    public Color fillTint = Color.white;
    public float minFill = 0.1f;

    public float fillAmount = 0;
    
    private void OnValidate()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        fillAmount = Mathf.Clamp01(fillAmount);

        var fill = minFill + fillAmount * (1.0f - minFill);

        var sz = fillImage.rectTransform.sizeDelta;
        sz.x = (borderImage.rectTransform.sizeDelta.x - 4) * fill;
        fillImage.rectTransform.sizeDelta = sz;

        fillImage.color = fillColor.Evaluate(fillAmount) * fillTint;
    }

    private void Update()
    {
        UpdateUI();
    }
}
