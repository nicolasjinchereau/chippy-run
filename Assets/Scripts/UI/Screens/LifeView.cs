using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LifeView : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TMP_Text livesLabel;

    public void Show()
    {
        int lives = PlayerPrefs.GetInt(PrefKeys.LivesRemaining);
        livesLabel.text = lives.ToString();
        canvasGroup.alpha = 0;
        gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(Util.Blend(1.0f, t => canvasGroup.alpha = t));
    }

    public void Hide()
    {
        StopAllCoroutines();

        StartCoroutine(
            Util.Blend(
                1.0f,
                t => canvasGroup.alpha = 1.0f - t,
                () => gameObject.SetActive(false)
            )
        );
    }

    public void AddLives(int count)
    {
        int lives = PlayerPrefs.GetInt(PrefKeys.LivesRemaining) + count;
        livesLabel.text = lives.ToString();

        var normal = Util.HexRGBA(0xDEB800FF);
        var glow = Util.HexRGBA(0x46DB00FF);

        StopAllCoroutines();

        StartCoroutine(Util.Blend(1.0f, t => {
            livesLabel.color = Color.Lerp(normal, glow, (1.0f - t) * (1.0f - t));
        }));
    }

    public void SubtractLives(int count)
    {
        int lives = PlayerPrefs.GetInt(PrefKeys.LivesRemaining) - count;
        livesLabel.text = lives.ToString();

        var normal = Util.HexRGBA(0xDEB800FF);
        var glow = Util.HexRGBA(0xDE0009FF);

        StopAllCoroutines();

        StartCoroutine(Util.Blend(1.0f, t => {
            livesLabel.color = Color.Lerp(normal, glow, (1.0f - t) * (1.0f - t));
        }));
    }
}
