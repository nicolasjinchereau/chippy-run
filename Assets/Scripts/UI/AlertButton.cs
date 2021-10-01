using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class AlertButton : MonoBehaviour
{
    public RectTransform pivot;
    public GameObject contentRoot;
    public Button button;
    public TMP_Text label;
    public Material labelMaterial;
    public Action action;
    
    float speedScale = 5.0f;
    float state = 0;

    public void Show(string text = null, Action action = null, int numFlashes = 3)
    {
        if (text != null)
            this.label.text = text;

        if (action != null)
            this.action = action;

        StopAllCoroutines();
        StartCoroutine(MoveRight(numFlashes));
    }

    public void Hide(bool immediate = false)
    {
        labelMaterial.SetFloat(ShaderUtilities.ID_GlowInner, 0);
        labelMaterial.SetFloat(ShaderUtilities.ID_GlowOuter, 0);
        button.interactable = false;
        
        if (immediate)
        {
            state = 0;
            pivot.anchoredPosition = new Vector2(0, -270);
            contentRoot.SetActive(false);
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(MoveLeft());
        }
    }

    public void OnPressed()
    {
        action.Invoke();
    }

    IEnumerator MoveRight(int numFlashes)
    {
        contentRoot.SetActive(true);

        while (state < 1.0f - float.Epsilon)
        {
            state = Mathf.Min(state + Time.deltaTime * speedScale, 1.0f);
            pivot.anchoredPosition = new Vector2(-270 * (1.0f - state), 0);
            yield return null;
        }

        state = 1;
        pivot.anchoredPosition = new Vector2(0, 0);
        button.interactable = true;

        if (numFlashes > 0)
        {
            labelMaterial.SetFloat(ShaderUtilities.ID_GlowInner, 0);
            labelMaterial.SetFloat(ShaderUtilities.ID_GlowOuter, 0);
            labelMaterial.SetColor(ShaderUtilities.ID_GlowColor, new Color(1, 1, 1, 0));

            yield return StartCoroutine(Util.Blend(1.0f * ((float)numFlashes / 3.0f), t =>
            {
                var value = -Mathf.Cos(t * Mathf.PI * 2.0f * numFlashes) * 0.5f + 0.5f;
                labelMaterial.SetFloat(ShaderUtilities.ID_GlowInner, value);
                labelMaterial.SetFloat(ShaderUtilities.ID_GlowOuter, value);
            }));

            labelMaterial.SetFloat(ShaderUtilities.ID_GlowInner, 0);
            labelMaterial.SetFloat(ShaderUtilities.ID_GlowOuter, 0);
        }
    }
    
    IEnumerator MoveLeft()
    {
        labelMaterial.SetFloat(ShaderUtilities.ID_GlowInner, 0);
        labelMaterial.SetFloat(ShaderUtilities.ID_GlowOuter, 0);
        button.interactable = false;

        while (state > float.Epsilon)
        {
            state = Mathf.Max(state - Time.deltaTime * speedScale, 0.0f);
            pivot.anchoredPosition = new Vector2(-270 * (1.0f - state), 0);
            yield return null;
        }

        state = 0;
        pivot.anchoredPosition = new Vector2(0, -270);
        contentRoot.SetActive(false);
    }
}
