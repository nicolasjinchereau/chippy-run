using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplinePath : MonoBehaviour
{
    Spline _spline = new Spline();

    public SplineAlgorithm algorithm = SplineAlgorithm.CatmullRom;
    [Range(0, 20)]
    public int subdivisions = 5;
    public bool looped = false;
    public bool alignPointsToPath = true;

    public Spline spline {
        get { return _spline; }
    }

    void Awake()
    {
        if(transform.childCount >= 3)
        {
            var tempPoints = new List<Transform>();
            
            for(int i = 0; i < transform.childCount; ++i)
                tempPoints.Add(transform.GetChild(i));

            _spline.UpdateSpline(tempPoints, algorithm, subdivisions, looped, alignPointsToPath);
        }
    }
}
