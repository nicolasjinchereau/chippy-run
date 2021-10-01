using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloatingPoints : MonoBehaviour, IPoolObject
{
    public TMP_Text label;
    public Vector3 velocity = Vector3.zero;
    public float start = 0.0f;
    public float life = 1.0f;

    void Update()
    {
        transform.position = transform.position + velocity * Time.deltaTime;

        float t = (Time.time - start) / life;

        label.canvasRenderer.SetAlpha(1.0f - t * t);

        if(Time.time >= start + life)
            this.ReturnToPool();
    }

    public ObjectPool OwningPool { get; set; }
    public int PrefabIndex { get; set; }
    public GameObject GameObject { get; set; }

    void IPoolObject.Respawn(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}
