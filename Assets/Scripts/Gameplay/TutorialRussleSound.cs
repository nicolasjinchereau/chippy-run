using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRussleSound : MonoBehaviour
{
    public AudioSource src;
    int triggers = 0;

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == Layer.Shrubs)
        {
            ++triggers;
            src.Play();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == Layer.Shrubs)
        {
            if(--triggers == 0) {
                src.Stop();
            }
        }
    }
}
