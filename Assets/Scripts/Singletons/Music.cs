using UnityEngine;
using System.Collections;

public enum AudioTrack
{
    None,
    NatureAmbience,
    GameMusic
}

public class Music : MonoBehaviour
{
	private static Music _that = null;

	public AudioClip[] clips;
	private float[] clipVolumes = new float[] {
		1.0f,
		1.0f
	};

	AudioTrack currentTrack = AudioTrack.None;
    public AudioSource source;
    bool stopping = false;

    public static Music that
    {
        get
        {
            if (_that == null)
            {
                var prefab = Resources.Load<GameObject>("Music");
                var go = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                DontDestroyOnLoad(go);
                _that = go.GetComponent<Music>();
            }
            
            return _that;
        }
    }

	public void PlayTrack(AudioTrack nextTrack)
	{
        if(nextTrack != currentTrack || stopping)
		{
            source.Stop();
            stopping = false;

			if(nextTrack == AudioTrack.None)
			{
                source.clip = null;
			}
			else
			{
				source.clip = clips[(int)nextTrack - 1];
				source.ignoreListenerVolume = true;
				source.volume = clipVolumes[(int)nextTrack - 1];
				source.Play();
			}

			currentTrack = nextTrack;
		}
	}

    public void Stop()
    {
        stopping = true;
    }

    void Update()
    {
        if(stopping)
        {
            float vol = Mathf.Max(source.volume - Time.deltaTime, 0);
            source.volume = vol;

            if(vol < 0.0001f)
            {
                stopping = false;
                source.Stop();
                source.clip = null;
                currentTrack = AudioTrack.None;
            }
        }
    }
}
