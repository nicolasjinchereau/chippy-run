using UnityEngine;
using System.Collections;

public class MedallionPiece : MonoBehaviour, IPoolObject
{
    public MeshRenderer meshRenderer;
    public SphereCollider sphereCollider;
    public ParticleSystem particles;
    public Material collectedMaterial;
    Material originalMaterial;

    void Awake() {
        originalMaterial = meshRenderer.sharedMaterial;
    }

    void Update()
    {
        float turnsPerSecond = 0.2f;
        transform.Rotate(0.0f, turnsPerSecond * 360.0f * Time.deltaTime, 0.0f, Space.Self);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag != "Player")
            return;
        
        particles.Stop();
        this.enabled = false;
        sphereCollider.enabled = false;
        meshRenderer.sharedMaterial = collectedMaterial;

        GameUI.CollectMedallionPiece(this);
    }

    public ObjectPool OwningPool { get; set; }
    public int PrefabIndex { get; set; }
    public GameObject GameObject { get; set; }

    void IPoolObject.Respawn(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;

        meshRenderer.sharedMaterial = originalMaterial;
        this.enabled = true;
        sphereCollider.enabled = true;
        particles.Play();
    }
}
