using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectableType
{
    Egg,
    Medallion,
}

public class TutorialCollectable : MonoBehaviour
{
    public Collider col;
    public ParticleSystem particles;
    public CollectableType type;

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(type == CollectableType.Egg)
                SharedSounds.eggCollected.Play();
            else if(type == CollectableType.Medallion)
                SharedSounds.medallionPieceCollected.Play();
            
            col.enabled = false;
            particles.transform.SetParent(null, true);
            particles.transform.localScale = Vector3.one;
            particles.Play();
            Destroy(gameObject);
            Destroy(particles.gameObject, 1.0f);
        }
    }
}
