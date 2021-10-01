using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound2D : MonoBehaviour
{
    public AudioClip clip;
    public AudioSource source;

    bool _blocking = false;
    float _defaultVolume = 1.0f;
    float _defaultPitch = 1.0f;

    private void Awake()
    {
        source.playOnAwake = false;
        source.spatialBlend = 0;
        source.bypassEffects = true;
        source.bypassListenerEffects = true;
        source.bypassReverbZones = true;
        source.ignoreListenerVolume = true;
        source.loop = false;
        source.spatialize = false;
        source.volume = _defaultVolume;
        source.pitch = _defaultPitch;
    }

    public void Play() {
        Play(_defaultVolume);
    }

    public void PlayDelayed(float delay) {
        Play(_defaultVolume, delay);
    }

    public void Play(float volume, float delay = 0)
    {
        if (_blocking)
        {
            if (!source.isPlaying)
            {
                source.volume = volume;

                if (delay > 0)
                    source.PlayDelayed(delay);
                else
                    source.Play();
            }
        }
        else
        {
            if (source.isPlaying)
                source.Stop();

            source.volume = volume;

            if (delay > 0.00001f)
                source.PlayDelayed(delay);
            else
                source.Play();
        }
    }

    public void Stop()
    {
        if (source != null)
            source.Stop();
    }
}
