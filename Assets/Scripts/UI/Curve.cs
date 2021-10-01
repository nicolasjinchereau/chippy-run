using UnityEngine;
using System.Collections;

public class Curve : ScriptableObject
{
	public delegate float Function(float t);

	public static float clamp(float x)
	{
		return (x > 1.0f)? 1.0f : ((x < 0)? 0 : x);
	}

	public static float Constant(float t)
	{
		return 1.0f;
	}

	public static float Linear(float t)
	{
		t = clamp(t);
		return t;
	}

	public static float InvLinear(float t)
	{
		t = clamp(t);
		return 1.0f - t;
	}

	public static float SmoothStep(float t)
	{
		t = clamp(t);
		return t * t * (3 - (2 * t));
	}

	public static float InvSmoothStep(float t)
	{
		t = clamp(t);
		return 1 - (t * t * (3 - (2 * t)));
	}

	public static float SmoothStepSteep(float t)
	{
		t = clamp(t);
		return t * t * t * (t * (6 * t - 15) + 10);
	}

	public static float InvSmoothStepSteep(float t)
	{
		t = clamp(t);
		return 1 - (t * t * t * (t * (6 * t - 15) + 10));
	}

	public static float OutQuad(float t)
	{
		t = clamp(t);
		return t * t;
	}

	public static float InvOutQuad(float t)
	{
		t = clamp(t);
		return 1 - (t * t);
	}

	public static float InQuad(float t)
	{
		t = clamp(t);
		return t * (2 - t);
	}

	public static float InvInQuad(float t)
	{
		t = clamp(t);
		float inv = 1 - t;
		return inv * inv;
	}

	public static float OutCube(float t)
	{
		t = clamp(t);
		return t * t * t;
	}

	public static float InvOutCube(float t)
	{
		t = clamp(t);
		return 1 - (t * t * t);
	}

	public static float InCube(float t)
	{
		t = clamp(t);
		float inv = 1 - t;
		return 1 - (inv * inv * inv);
	}

	public static float InvInCube(float t)
	{
		t = clamp(t);
		float inv = 1 - t;
		return (inv * inv * inv);
	}

	public static float ArcLinear(float t)
	{
		t = clamp(t);
		return 1 - (2 * Mathf.Abs(t - 0.5f));
	}

	public static float InvArcLinear(float t)
	{
		t = clamp(t);
		return (2 * Mathf.Abs(t - 0.5f));
	}

	public static float ArcQuad(float t)
	{
		t = clamp(t);
		float x = (2 * t - 1);
		return 1 - (x * x);
	}

	public static float InvArcQuad(float t)
	{
		t = clamp(t);
		float x = (2 * t - 1);
		return (x * x);
	}

	public static float ArcCube(float t)
	{
		t = clamp(t);
		float x = (2 * t - 1);
		float xsq = x * x;
		return 1 - ((0.5f * xsq) + (0.5f * (xsq * xsq)));
	}

	public static float InvArcCube(float t)
	{
		t = clamp(t);
		float x = (2 * t - 1);
		float xsq = x * x;
		return (0.5f * xsq) + (0.5f * (xsq * xsq));
	}

	public static float ArcQuart(float t)
	{
		t = clamp(t);
		float x = (2 * t - 1);
		float xsq = x * x;
		return 1 - (xsq * xsq);
	}

	public static float InvArcQuart(float t)
	{
		t = clamp(t);
		float x = (2 * t - 1);
		float xsq = x * x;
		return (xsq * xsq);
	}

	public static float ArcOct(float t)
	{
		t = clamp(t);
		float x = (2 * t - 1);
		float xsq = x * x;
		float xc = xsq * xsq;
		return 1 - (xc * xc);
	}

	public static float InvArcOct(float t)
	{
		t = clamp(t);
		float x = (2 * t - 1);
		float xsq = x * x;
		float xc = xsq * xsq;
		return (xc * xc);
	}

	public static float ArcQuadOutSharp(float t)
	{
		t = clamp(t);
		float t1 = t - 1;
		float x = (2 * t1 * t1 - 1);
		return 1 - (x * x);
	}

	public static float ArcQuadInSharp(float t)
	{
		t = clamp(t);
		float x = (2 * t * t - 1);
		return 1 - (x * x);
	}

	public static float ArcCubeOutSharp(float t)
	{
		t = clamp(t);
		float t1 = t - 1;
		float x = (2 * t1 * t1 - 1);
		float xsq = x * x;
		return 1 - ((0.5f * xsq) + (0.5f * (xsq * xsq)));
	}

	public static float ArcCubeInSharp(float t)
	{
		t = clamp(t);
		float x = (2 * t * t - 1);
		float xsq = x * x;
		return 1 - ((0.5f * xsq) + (0.5f * (xsq * xsq)));
	}

	public static float ArcQuartOutSharp(float t)
	{
		t = clamp(t);
		float t1 = t - 1;
		float x = (2 * t1 * t1 - 1);
		float xsq = x * x;
		return 1 - (xsq * xsq);
	}

	public static float ArcQuartInSharp(float t)
	{
		t = clamp(t);
		float x = (2 * t * t - 1);
		float xsq = x * x;
		return 1 - (xsq * xsq);
	}

	public static float InElastic(float t)
	{
		t = clamp(t);
		float ts = t * t;
		float tc = t * t * t;
		return (33 * tc * ts) - (106 * ts * ts) + (126 * tc) - (67 * ts) + (15 * t);
	}

    public static float InBack(float t)
    {
        t = clamp(t);
        //return t * t - t * Mathf.Sin(Mathf.PI * Mathf.Pow(t, 0.05f)) * 5.0f;
        return t * t - t * Mathf.Sin(Mathf.PI * t);
    }
};
