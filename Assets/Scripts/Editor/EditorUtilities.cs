using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Globalization;
using JsonFx;

public class EditorUtilities : MonoBehaviour
{
    [MenuItem("Utilities/Test")]
    public static void Test()
    {

    }

    [MenuItem("Utilities/Set Lives to Zero")]
    public static void SetLivesToZero()
    {
        if (Application.isPlaying)
        {
            PlayerPrefs.SetInt("lives_remaing", 0);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogError("Must be in play mode");
        }
    }
}
