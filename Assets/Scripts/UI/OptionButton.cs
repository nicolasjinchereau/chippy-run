using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class OptionButton : MonoBehaviour
{
    public RectTransform pivot;
    public GameObject contentRoot;
    public Button button;
    public TMP_Text label;
    public Action action;

    float slideRange = 200;
    float speedScale = 5.0f;
    float state = 0;

    public void Show(string text = null, Action action = null)
    {
        if (text != null)
            this.label.text = text;

        if (action != null)
            this.action = action;

        StopAllCoroutines();
        StartCoroutine(MoveUp());
    }

    public void Hide(bool immediate = false)
    {
        if (immediate)
        {
            state = 0;
            pivot.anchoredPosition = new Vector2(0, -slideRange);
            contentRoot.SetActive(false);
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(MoveDown());
        }
    }

    public void OnPressed() {
        action.Invoke();
    }

    IEnumerator MoveUp()
    {
        contentRoot.SetActive(true);

        while (state < 1.0f - float.Epsilon) {
            state = Mathf.Min(state + Time.deltaTime * speedScale, 1.0f);
            pivot.anchoredPosition = new Vector2(0, -slideRange * (1.0f - state));
            yield return null;
        }

        state = 1;
        pivot.anchoredPosition = new Vector2(0, 0);
    }

    IEnumerator MoveDown()
    {
        while (state > float.Epsilon)
        {
            state = Mathf.Max(state - Time.deltaTime * speedScale, 0.0f);
            pivot.anchoredPosition = new Vector2(0, -slideRange * (1.0f - state));
            yield return null;
        }

        state = 0;
        pivot.anchoredPosition = new Vector2(0, -slideRange);
        contentRoot.SetActive(false);
    }
}
