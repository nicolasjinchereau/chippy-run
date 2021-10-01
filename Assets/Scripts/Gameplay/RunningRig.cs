using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunningRig : MonoBehaviour
{
    public float chippyRunSpeed = 2.0f;
    public float textureScrollSpeed = 1.5f;
    public Animation chippyAnim;
    public MeshRenderer trunkRenderer;

    Material mat;

    private void Awake() {
        mat = new Material(trunkRenderer.sharedMaterial);
        trunkRenderer.sharedMaterial = mat;
    }

    void Update()
    {
        chippyAnim["w_run"].speed = chippyRunSpeed;
        mat.mainTextureOffset = new Vector2(0, Time.time * textureScrollSpeed);
    }
}
