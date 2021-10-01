using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapAnimation : MonoBehaviour
{
    public RawImage tapDeviceGray;
    public RawImage tapDeviceGreen;
    public RawImage tapHand;
    public RawImage tapEmphasis;
    Coroutine routine = null;

	void OnEnable() {
        routine = StartCoroutine(Animate());
	}

    void OnDisable()
    {
        if(routine != null) {
            StopCoroutine(routine);
            routine = null;
        }
    }

    IEnumerator Animate()
    {
        tapDeviceGray.enabled = true;
        tapDeviceGreen.enabled = false;
        tapHand.enabled = false;
        tapEmphasis.enabled = false;

        yield return Util.Blend(0.25f, t => {
            transform.localScale = new Vector3(Curve.InElastic(t), 1, 1);
        });

        yield return new WaitForSeconds(0.5f);

        var finishPos = tapHand.transform.localPosition;
        var startPos = finishPos + new Vector3(0, -10, 0);
        var finishScale = 1.0f;
        var startScale = 1.1f;

        tapHand.enabled = true;
        tapHand.transform.localPosition = startPos;
        tapHand.transform.localScale = Vector3.one * startScale;

        while(true)
        {
            yield return Util.Blend(0.5f, t => {
                t = t * t;
                tapHand.transform.localPosition = Vector3.Lerp(startPos, finishPos, t);
                tapHand.transform.localScale = Vector3.one * Mathf.Lerp(startScale, finishScale, t);
            });

            tapEmphasis.enabled = true;
            tapDeviceGreen.enabled = true;
            tapDeviceGray.enabled = false;

            yield return new WaitForSeconds(0.5f);

            tapEmphasis.enabled = false;
            tapDeviceGreen.enabled = false;
            tapDeviceGray.enabled = true;
        }
	}
}
