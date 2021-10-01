using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineTest : MonoBehaviour {

    public int subdivisions = 4;
    public List<Transform> path = new List<Transform>();
    public List<Transform> loop = new List<Transform>();
    public float pathSamplePos = 0.0f;
    public float loopSamplePos = 0.0f;

    Spline pathSpline;
    Spline loopSpline;
    GameObject pathSampleSphere;
    GameObject loopSampleSphere;
    GameObject dragPoint;
    GameObject closestPoint;

	void Start()
    {
        Material red = new Material(Shader.Find("Diffuse"));
        red.SetColor("_Color", Color.red);

        Material green = new Material(Shader.Find("Diffuse"));
        green.SetColor("_Color", Color.green);

        Material blue = new Material(Shader.Find("Diffuse"));
        blue.SetColor("_Color", Color.blue);

        pathSpline = new Spline(path, SplineAlgorithm.CatmullRom, subdivisions, false, true);
        foreach(var pt in pathSpline.points)
        {
            var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.transform.position = pt.pos;
            s.transform.rotation = pt.rot;
            s.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            s.GetComponent<MeshRenderer>().sharedMaterial = red;
        }

        loopSpline = new Spline(loop, SplineAlgorithm.CatmullRom, subdivisions, true, true);
        foreach(var pt in loopSpline.points)
        {
            var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.transform.position = pt.pos;
            s.transform.rotation = pt.rot;
            s.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            s.GetComponent<MeshRenderer>().sharedMaterial = red;
        }

        var p1 = pathSpline.SampleFrac(0);
        pathSampleSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pathSampleSphere.transform.position = p1.pos;
        pathSampleSphere.transform.rotation = p1.rot;
        pathSampleSphere.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
        pathSampleSphere.GetComponent<MeshRenderer>().sharedMaterial = green;

        var p2 = loopSpline.SampleFrac(0);
        loopSampleSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        loopSampleSphere.transform.position = p2.pos;
        loopSampleSphere.transform.rotation = p2.rot;
        loopSampleSphere.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
        loopSampleSphere.GetComponent<MeshRenderer>().sharedMaterial = green;

        dragPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dragPoint.transform.position = pathSpline.GetCenter() + Vector3.forward * 1.25f;
        dragPoint.transform.rotation = Quaternion.identity;
        dragPoint.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
        dragPoint.GetComponent<MeshRenderer>().sharedMaterial = blue;
        dragPoint.name = "DRAG ME";

        closestPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        closestPoint.transform.position = Vector3.zero;
        closestPoint.transform.rotation = Quaternion.identity;
        closestPoint.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
        closestPoint.GetComponent<MeshRenderer>().sharedMaterial = blue;
        closestPoint.name = "closest to dragged point";
	}

    void Update()
    {
        pathSamplePos = Mathf.Clamp01(pathSamplePos);
        loopSamplePos = Mathf.Clamp01(loopSamplePos);

        var p1 = pathSpline.SampleFrac(pathSamplePos);
        var p2 = loopSpline.SampleFrac(loopSamplePos);

        pathSampleSphere.transform.position = p1.pos;
        pathSampleSphere.transform.rotation = p1.rot;

        loopSampleSphere.transform.position = p2.pos;
        loopSampleSphere.transform.rotation = p2.rot;

        float distAlongPath;
        var pt = pathSpline.GetClosestPoint(dragPoint.transform.position, out distAlongPath);
        closestPoint.transform.position = pt.pos;
        // OR
        // closestPoint.transform.position = pathSpline.SampleDist(distAlongPath).pos;
    }
}
