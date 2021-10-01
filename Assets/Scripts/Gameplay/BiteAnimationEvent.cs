using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiteAnimationEvent : MonoBehaviour
{
    public void OnBite()
    {
        SharedSounds.snap.Play();
    }
}
