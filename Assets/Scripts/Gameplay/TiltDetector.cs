using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiltDetector : MonoBehaviour
{
    Material mat;
    public bool triggered = false;

	void Awake() {
        mat = GetComponent<Renderer>().material;
	}
	
	void Update()
    {
        if(!triggered) {
            float s = Mathf.Sin(Time.time * 2.0f * Mathf.PI) * 0.5f + 0.5f;
            mat.SetFloat("_Glow", Mathf.Lerp(0.0f, 0.5f, s));
        }

        if(Game.that.player) {
            var pos = transform.position;
            pos.y = Game.that.player.transform.position.y;
            transform.position = pos;
        }
	}

    IEnumerator DoTrigger()
    {
        triggered = true;

        SharedSounds.medallionCollected.Play();

        yield return Util.Blend(1.0f, t => {
            mat.SetFloat("_Glow", 0.5f * (1.0f - t));
            mat.SetColor("_Color", new Color(0, 1, 0, 0.5f * (1.0f - t)));
        });

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if(!triggered && other.tag == "Player")
            StartCoroutine(DoTrigger());
    }
}
