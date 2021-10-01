using UnityEngine;
using System.Collections;

public class SnakeEgg : MonoBehaviour, IPoolObject
{
    public Material collectedMaterial;
    public MeshRenderer meshRenderer;
    public Material originalMaterial;
    public CapsuleCollider capsuleCollider;

    float angle = 0;

    void Awake()
    {
        originalMaterial = meshRenderer.sharedMaterial;
    }

    void Update()
    {
        var up = Quaternion.Euler(0, Mathf.Repeat(angle * 360.0f, 360.0f), 0) * new Vector3(1, 4, 0).normalized;
        transform.localRotation = Quaternion.FromToRotation(Vector3.up, up);
        angle += Time.deltaTime * 0.5f;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag != "Player")
            return;
        
        meshRenderer.sharedMaterial = collectedMaterial;
        this.enabled = false;
        capsuleCollider.enabled = false;
        transform.parent = null;

        GameUI.CollectEgg(this);
    }

    public ObjectPool OwningPool { get; set; }
    public int PrefabIndex { get; set; }
    public GameObject GameObject { get; set; }

    void IPoolObject.Respawn(Vector3 position, Quaternion rotation)
    {
        this.transform.position = position;
        this.transform.rotation = rotation;

        meshRenderer.sharedMaterial = originalMaterial;
        this.enabled = true;
        capsuleCollider.enabled = true;
    }
}
