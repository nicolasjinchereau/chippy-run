using UnityEngine;
using UnityEditor;
using System.Collections;

public class StumpFixer : MonoBehaviour
{
    public static Transform transf;

    [MenuItem("Custom Scripts/Fix SnakeEgg")]
    static void FixEggs()
    {
        int count = 0;

        var trunks = Resources.FindObjectsOfTypeAll<TrunkPiece>();

        foreach(var trunk in trunks)
        {
            if(EditorUtility.IsPersistent(trunk.gameObject))
            {
                foreach(Transform child in trunk.transform)
                {
                    if(child.gameObject.name == "SnakeEggSpawn")
                    {
                        ++count;
                    }
                }
            }
        }

        Debug.Log("egg spawn count: " + count);
    }

    [MenuItem("Custom Scripts/Copy Transform %#c")]
    static void CopyTransform()
    {
        if (Selection.activeTransform != null)
        {
            transf = Selection.activeTransform;
        }
    }

    [MenuItem("Custom Scripts/Paste Transform %#v")]
    static void PasteTransform()
    {
        if (Selection.activeTransform != null && transf != null)
        {
            Selection.activeTransform.position = transf.position;
            Selection.activeTransform.rotation = transf.rotation;
        }
    }

	[MenuItem("Custom Scripts/Align Collectables")]
	static void AlignCollectables()
	{
		foreach (Transform tr in Selection.transforms)
		{
			foreach(Transform child in tr)
			{
				if (child.gameObject.name == "SnakeEgg")
				{
					Vector3 core = new Vector3(0.0f, child.transform.position.y, 0.0f);
					Vector3 up = (child.transform.position - core);
					Vector3 upNormalized = up.normalized;

					RaycastHit hit;
					if (Physics.Raycast(new Ray(core + upNormalized * 20.0f, -upNormalized), out hit, 20.0f, 1 << Layer.Stumps | 1 << Layer.Ramps))
					{
						child.transform.position = hit.point + (hit.point - core).normalized * 0.2f;
					}

					child.transform.rotation = Quaternion.LookRotation(Vector3.down, upNormalized);
				}
				else if (child.gameObject.name == "MedallionSpawnPoint")
				{
					Vector3 core = new Vector3(0.0f, child.transform.position.y, 0.0f);
					Vector3 up = (child.transform.position - core);
					Vector3 upNormalized = up.normalized;
					
					RaycastHit hit;
					if (Physics.Raycast(new Ray(core + upNormalized * 20.0f, -upNormalized), out hit, 20.0f, 1 << Layer.Stumps | 1 << Layer.Ramps))
					{
						child.transform.position = hit.point + upNormalized * 1.3f;
					}

					child.transform.rotation = Quaternion.LookRotation(Vector3.down, upNormalized);
				}
			}

			PrefabUtility.ReplacePrefab(tr.gameObject, PrefabUtility.GetCorrespondingObjectFromSource(tr), ReplacePrefabOptions.ConnectToPrefab);
		}
	}

	[MenuItem("Custom Scripts/Make Ramps Convex")]
	static void MakeRampsConvex()
	{
		foreach (Transform tr in Selection.transforms)
		{
			foreach(Transform child in tr)
			{
				if (child.gameObject.name == "Shrooms01")
				{
					MeshCollider mc = child.GetComponent<MeshCollider>();
					mc.convex = true;
				}
			}

			PrefabUtility.ReplacePrefab(tr.gameObject, PrefabUtility.GetCorrespondingObjectFromSource(tr), ReplacePrefabOptions.ConnectToPrefab);
		}
	}

    static string[] stumps = new string[]
	{
		"Stump01",
		"Stump02",
		"Stump03",
		"Stump04",
		"Stump05",
		"Stump06",
		"Stump07",
		"Stump08",
		"Stump09",
		"Stump10",
		"Stump11",
		"Stump12",
		"Stump13",
		"Stump14",
		"Stump15",
		"Stump16",
		"Stump17",
		"Stump18",
		"Stump19",
		"Stump20",
		"Stump21",
		"Stump22",
		"Stump23",
		"Stump24",
		"Stump25",
		"Stump26",
		"Stump27",
		"Stump28",
		"Stump29"
	};

	[MenuItem("Custom Scripts/Print Type")]
    static void PrintType()
    {
		Debug.Log(Selection.objects[0].GetType());
    }

    [MenuItem("Custom Scripts/Build Tree")]
    static void BuildTree()
    {
        float stumpY = 0;
		
        foreach (string stumpName in stumps)
        {
            GameObject newStump = PrefabUtility.InstantiatePrefab(Resources.Load(stumpName)) as GameObject;
            newStump.transform.position = new Vector3(0.0f, stumpY, 0.0f);
            stumpY += 20.0f;
        }
    }

    [MenuItem("Custom Scripts/Deactivate Branches")]
    static void DeactivateBranches()
    {
        SetBranchState(false);
    }
    
    [MenuItem("Custom Scripts/Activate Branches")]
    static void ActivateBranches()
    {
         SetBranchState(true);
    }

    static void SetBranchState(bool active)
    {
        int Branches = 9;

        foreach (Transform tr in Selection.transforms)
		{
            bool madeChanges = false;

            foreach (Transform t in tr)
            {
                if (t.gameObject.layer == Branches)
                {
                    if (t.gameObject.activeSelf != active)
                    {
                        //t.gameObject.SetActiveRecursively(active);
                        DestroyImmediate(t.gameObject);
                        madeChanges = true;
                    }
                }
            }

            if (madeChanges)
            {
                PrefabUtility.ReplacePrefab(tr.gameObject, PrefabUtility.GetCorrespondingObjectFromSource(tr), ReplacePrefabOptions.ConnectToPrefab);
            }
        }
    }

    [MenuItem("Custom Scripts/Delete Evil Chippy Spawn Points")]
    static void DeleteEvilChippySpawnPoints()
    {
        foreach (Transform tr in Selection.transforms)
		{
            bool madeChanges = false;
			
			Transform t;
			while ((t = tr.Find("EvilChippySpawnPoint")) != null)
			{
				DestroyImmediate(t.gameObject);
				madeChanges = true;
			}

            if (madeChanges)
            {
                PrefabUtility.ReplacePrefab(tr.gameObject, PrefabUtility.GetCorrespondingObjectFromSource(tr), ReplacePrefabOptions.ConnectToPrefab);
            }
        }
    }
}
