using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlowdownTutorial : MonoBehaviour
{
    public GameObject chippy;
    public int textIndex;
    public bool runChippy = true;

    void Update()
    {
        if(runChippy) {
            chippy.transform.position += Vector3.up * Time.deltaTime * 2.0f;
        }
    }

    public IEnumerator PlayTutorial()
    {
        var shouldRunChippy = runChippy;
        runChippy = false;

        yield return Util.Blend(1.0f, t => GameUI.that.overlay.color = new Color(0, 0, 0, 1.0f - t));
        GameUI.that.overlay.gameObject.SetActive(false);

        TMP_Text text = GameUI.that.tutorialTextSlowdown[textIndex];

        text.gameObject.SetActive(true);
        yield return Util.Blend(0.25f, t => text.transform.localScale = new Vector3(Curve.InElastic(t), 1, 1));

        if (shouldRunChippy)
        {
            yield return new WaitForSeconds(3);
            runChippy = true;
        }

        yield return new WaitForSeconds(5);

        yield return Util.Blend(0.25f, t => text.transform.localScale = new Vector3(1.0f - t, 1, 1));
        text.gameObject.SetActive(false);

        GameUI.that.overlay.gameObject.SetActive(true);
        yield return Util.Blend(1.0f, t => GameUI.that.overlay.color = new Color(0, 0, 0, t));
    }
}
