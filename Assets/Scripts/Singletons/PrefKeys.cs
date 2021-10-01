using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PrefKeys
{
    public const string UserID = "userID";
    public const string TutorialComplete = "tutorial_complete";
    public const string LivesRemaining = "lives_remaing";
    public const string Immortal = "immortal";

    public static bool IsImmortal
    {
        get { return PlayerPrefs.GetInt(Immortal, 0) != 0; }
        set { PlayerPrefs.SetInt(Immortal, value ? 1 : 0); PlayerPrefs.Save(); }
    }
}
