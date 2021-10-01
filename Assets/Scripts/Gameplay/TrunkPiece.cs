using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

class MaterialSwapInfo
{
    public GameObject go;
    public MeshRenderer renderer;
    public Material[] normal;
    public Material[] blended;
}

public class TrunkPiece : MonoBehaviour
{
    public bool isBeaverStump = false;
    public GameObject snakeEggPrefab;
    public List<Transform> eggSpawns = new List<Transform>();
    public List<Collider> branchColliders = new List<Collider>();
    List<MaterialSwapInfo> swapInfo = new List<MaterialSwapInfo>();

    const float EggScale = 1.4f;

    void Start()
    {
        foreach(Transform child in transform)
        {
            if(child.gameObject.name == "SnakeEggSpawn")
                eggSpawns.Add(child);
        }

        foreach(var rend in GetComponentsInChildren<MeshRenderer>())
        {
            var normal = new List<Material>();
            var blended = new List<Material>();

            foreach (var mat in rend.sharedMaterials)
            {
                if (rend.enabled && (!mat.HasProperty("_Opacity") || !mat.HasProperty("_SrcBlend") || !mat.HasProperty("_DstBlend")))
                    throw new System.Exception("Material does not support needed properties: " + mat.name);

                mat.SetInt("_SrcBlend", (int)BlendMode.One);
                mat.SetInt("_DstBlend", (int)BlendMode.Zero);
                normal.Add(mat);

                Material blendedMat = new Material(mat);
                blendedMat.name = blendedMat.name + "_Blended";
                blendedMat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                blendedMat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                blended.Add(blendedMat);
            }

            var info = new MaterialSwapInfo() {
                go = rend.gameObject,
                renderer = rend,
                normal = normal.ToArray(),
                blended = blended.ToArray()
            };

            swapInfo.Add(info);
        }
	}

    public void SetBlended(bool blended)
    {
        foreach(var info in swapInfo)
        {
            if (!info.renderer)
                Debug.LogError("Renderer destroyed: " + info.go.name);

            info.renderer.sharedMaterials = blended ? info.blended : info.normal;
        }
    }

    public void SetOpacity(float opacity)
    {
        foreach(var info in swapInfo)
        {
            for(int i = 0; i < info.blended.Length; ++i)
                info.blended[i].SetFloat("_Opacity", opacity);
        }
    }

    public void SpawnEggs(ObjectPool eggPool)
    {
        for(int i = 0; i < eggSpawns.Count; ++i)
        {
            if(eggSpawns[i].childCount == 0)
            {
                var egg = eggPool.Spawn<SnakeEgg>(eggSpawns[i].position, eggSpawns[i].rotation);
                egg.transform.parent = eggSpawns[i];
                egg.transform.localScale = Vector3.one * EggScale;
            }
        }
    }

    public void FadeIn(float fadeInTime)
    {
        StartCoroutine(DoFadeIn(fadeInTime));
    }

    IEnumerator DoFadeIn(float fadeInTime)
    {
        SetBlended(true);

        float start = Time.time;
        float finish = start + fadeInTime;

        while(Time.time <= finish)
        {
            float opacity = (Time.time - start) / fadeInTime;
            SetOpacity(opacity);
            yield return null;
        }

        SetOpacity(1.0f);
        SetBlended(false);
    }

    public void EnableBranchCollision(bool enable)
    {
        foreach(var c in branchColliders)
        {
            var mc = c as MeshCollider;

            if(enable) {
                c.isTrigger = false;
                mc.convex = false;
            }
            else {
                mc.convex = true;
                c.isTrigger = true;
            }
        }
    }
}
