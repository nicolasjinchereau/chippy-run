using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Util
{
    public static IEnumerator Blend(float duration, Action<float> tick)
    {
        tick(0.0f);

        float speed = 1.0f / duration;
        float t = 0;

        while(true)
        {
            yield return null;

            t += Time.deltaTime * speed;

            if(t >= 1.0f)
                break;

            tick(t);
        }

        tick(1.0f);
    }

    public static IEnumerator Blend(float duration, float from, float to, Action<float> tick) {
        yield return Blend(duration, t => tick(Mathf.Lerp(from, to, t)));
    }

    public static IEnumerator Blend(float duration, Action<float> tick, Action after)
    {
        yield return Blend(duration, tick);
        after();
    }

    public static Color32 HexRGBA(uint c) {
        byte r = (byte)(0xFF & (c >> 24));
        byte g = (byte)(0xFF & (c >> 16));
        byte b = (byte)(0xFF & (c >> 8));
        byte a = (byte)(0xFF & (c));
        return new Color32(r, g, b, a);
    }

    public static float InverseLerp(float val, float a, float b) {
        val = (val - a) / (b - a);
        return val < 0 ? 0 : (val > 1 ? 1 : val);
    }

    public static Rect RectRelativeTo(RectTransform from, Transform to)
    {
        Rect rect = from.rect;

        Vector3 p1 = new Vector2(rect.xMin, rect.yMin);
        Vector3 p2 = new Vector2(rect.xMax, rect.yMax);

        Matrix4x4 mtx = to.worldToLocalMatrix * from.localToWorldMatrix;
        p1 = mtx.MultiplyPoint(p1);
        p2 = mtx.MultiplyPoint(p2);

        rect.xMin = p1.x;
        rect.yMin = p1.y;
        rect.xMax = p2.x;
        rect.yMax = p2.y;

        return rect;
    }
}
