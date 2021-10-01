using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolObject
{
    ObjectPool OwningPool { get; set; }
    int PrefabIndex { get; set; }
    GameObject GameObject { get; set; }
    void Respawn(Vector3 position, Quaternion rotation);
}

public static class IPoolObjectExtensions
{
    /// <summary>Return this object to the pool that owns it</summary>
    public static void ReturnToPool(this IPoolObject obj)
    {
        obj.OwningPool.Return(obj);
    }
}

public class ObjectPool : MonoBehaviour
{
    public GameObject[] prefabs;
    public int initialCount = 5;
    public bool allowGrowth = true;

    List<IPoolObject>[] pools;

	void Awake()
    {
        pools = new List<IPoolObject>[prefabs.Length];

        for(int p = 0; p < pools.Length; ++p)
        {
            pools[p] = new List<IPoolObject>();

            for(int i = 0; i < initialCount; ++i)
                pools[p].Add(MakeNew(p));
        }
	}

    IPoolObject MakeNew(int prefabIndex)
    {
        var obj = Instantiate(prefabs[prefabIndex]).GetComponent<IPoolObject>();
        if(obj == null)
            throw new System.Exception("Prefab does not have a script attached which implements IPoolObject");

        obj.OwningPool = this;
        obj.PrefabIndex = prefabIndex;
        obj.GameObject = (obj as MonoBehaviour).gameObject;
        obj.GameObject.SetActive(false);
        obj.GameObject.transform.SetParent(transform, false);
        obj.GameObject.transform.localPosition = Vector3.zero;

        return obj;
    }

    public IPoolObject Spawn(Vector3 position, Quaternion rotation, int prefabIndex = 0)
    {
        var pool = pools[prefabIndex];

        if(pools[prefabIndex].Count == 0)
        {
            if(!allowGrowth)
                return null;

            Debug.LogWarning("Growing Object Pool");
            pool.Add(MakeNew(prefabIndex));
        }

        var obj = pool[pool.Count - 1];
        pool.RemoveAt(pool.Count - 1);

        obj.GameObject.transform.SetParent(null, false);
        obj.Respawn(position, rotation);
        obj.GameObject.SetActive(true);

        return obj;
    }

    public T Spawn<T>(Vector3 position, Quaternion rotation, int prefabIndex = 0)
        where T : MonoBehaviour
    {
        return (T)Spawn(position, rotation, prefabIndex);
    }

    public void Return(IPoolObject obj)
    {
        if(obj.OwningPool != this)
            throw new System.Exception("The specified object is not owned by this pool");
        
        obj.GameObject.SetActive(false);
        obj.GameObject.transform.SetParent(transform, false);
        obj.GameObject.transform.localPosition = Vector3.zero;
        pools[obj.PrefabIndex].Add(obj);
    }

    public void Return(GameObject go)
    {
        var obj = go.GetComponent<IPoolObject>();
        if(obj == null)
            throw new System.Exception("GameObject does not have a script attached which implements IPoolObject");

        Return(obj);
    }
}
