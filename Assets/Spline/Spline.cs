using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct SplinePoint
{
    public Vector3 pos;
    public Quaternion rot;

    public Vector3 tangent {
        get { return rot * Vector3.forward; }
    }
}

public enum SplineAlgorithm
{
    CatmullRom,
    LaneRiesenfeld,
}

public class Spline
{
    public List<SplinePoint> points { get; private set; }
    public SplineAlgorithm algorithm { get; private set; }
    public int subdivisions { get; private set; }
    public bool looped { get; private set; }
    public bool alignPointsToPath { get; private set; }
    public float length { get; private set; }

    public Spline() {
        points = new List<SplinePoint>();
    }

    public Spline(List<Transform> waypoints, SplineAlgorithm algorithm, int subdivisions, bool loop, bool alignPointsToPath)
    {
        points = new List<SplinePoint>();
        UpdateSpline(waypoints, algorithm, subdivisions, loop, alignPointsToPath);
    }

    public void Clear()
    {
        points.Clear();
        algorithm = SplineAlgorithm.CatmullRom;
        subdivisions = 0;
        looped = false;
        alignPointsToPath = true;
        length = 0;
    }

    public void UpdateSpline(List<Transform> waypoints, SplineAlgorithm algorithm, int subdivisions, bool loop, bool alignPointsToPath) {
        UpdateSpline(waypoints, algorithm, subdivisions, loop, alignPointsToPath, new List<SplinePoint>());
    }

    public void UpdateSpline(List<Transform> waypoints, SplineAlgorithm algorithm, int subdivisions, bool loop, bool alignPointsToPath, List<SplinePoint> tempBuffer)
    {
        if(waypoints.Count < 3)
            throw new Exception("'waypoints' must contain at least 3 elements");
        
        int capacity = (waypoints.Count + 1) * subdivisions;

        points.Clear();
        tempBuffer.Clear();

        if(points.Capacity < capacity)
            points.Capacity = capacity;

        if(tempBuffer.Capacity < capacity)
            tempBuffer.Capacity = capacity;
        
        foreach(Transform wp in waypoints)
        {
            points.Add(new SplinePoint(){
                pos = wp.position,
                rot = wp.rotation
            });
        }

        if(algorithm == SplineAlgorithm.LaneRiesenfeld)
            SubdivideBSpline(subdivisions, loop, tempBuffer);
        else if(algorithm == SplineAlgorithm.CatmullRom)
            SubdivideCatmulRom(subdivisions, loop, tempBuffer);
        else
            throw new ArgumentException("Invalid spline algorithm", "algorithm");
        
        float totalLength = 0;

        for(int i = 0; i < points.Count - 1; ++i)
            totalLength += Vector3.Distance(points[i].pos, points[i + 1].pos);
        
        if(alignPointsToPath)
        {
            if(loop)
            {
                var pt = points[0];
                var tan0 = (points[0].pos - points[points.Count - 2].pos).normalized;
                var tan1 = (points[1].pos - points[0].pos).normalized;
                pt.rot = Quaternion.LookRotation((tan0 + tan1) * 0.5f);
                points[0] = pt;
            }
            else
            {
                var pt = points[0];
                pt.rot = Quaternion.LookRotation(points[1].pos - points[0].pos);
                points[0] = pt;
            }

            int i = 1;

            for( ; i < points.Count - 1; ++i)
            {
                var pt = points[i];
                var tan0 = (points[i].pos - points[i - 1].pos).normalized;
                var tan1 = (points[i + 1].pos - points[i].pos).normalized;
                pt.rot = Quaternion.LookRotation((tan0 + tan1) * 0.5f);
                points[i] = pt;
            }

            {
                var pt = points[i];

                if(loop)
                    pt.rot = points[0].rot;
                else
                    pt.rot = Quaternion.LookRotation(points[i].pos - points[i - 1].pos);
                
                points[i] = pt;
            }
        }

        this.algorithm = algorithm;
        this.subdivisions = subdivisions;
        this.alignPointsToPath = alignPointsToPath;
        this.looped = loop;
        this.length = totalLength;
    }

    void SubdivideCatmulRom(int subdivs, bool loop, List<SplinePoint> tempBuffer)
    {
        int pointCount = points.Count;

        int end = loop ? pointCount : pointCount - 1;

        for(int p = 0; p < end; ++p)
        {
            tempBuffer.Add(points[p]);

            float spacing = 1.0f / (subdivs + 1);
            float t = spacing;

            for(int s = 0; s < subdivs; ++s, t += spacing)
            {
                if(loop)
                {
                    var a = (p - 1 + pointCount) % pointCount;
                    var b = p;
                    var c = (p + 1) % pointCount;
                    var d = (p + 2) % pointCount;

                    var pt = CatmullRom(points[a].pos, points[b].pos, points[c].pos, points[d].pos, t);
                    tempBuffer.Add(new SplinePoint(){ pos = pt });
                }
                else
                {
                    var a = Mathf.Max(p - 1, 0);
                    var b = p;
                    var c = Mathf.Min(p + 1, pointCount - 1);
                    var d = Mathf.Min(p + 2, pointCount - 1);

                    var pt = CatmullRom(points[a].pos, points[b].pos, points[c].pos, points[d].pos, t);
                    tempBuffer.Add(new SplinePoint(){ pos = pt });
                }
            }
        }

        if(!loop)
            tempBuffer.Add(points[pointCount - 1]);
        else
            tempBuffer.Add(points[0]);
        
        points.Clear();
        points.AddRange(tempBuffer);
    }

    void SubdivideBSpline(int subdivs, bool loop, List<SplinePoint> tempBuffer)
    {
        for(int s = 0; s < subdivs; ++s)
        {
            for(int p = 0; p < points.Count; ++p)
            {
                tempBuffer.Add(points[p]);

                if(p < points.Count - 1)
                {
                    tempBuffer.Add(new SplinePoint(){
                        pos = Vector3.Lerp(points[p].pos, points[p + 1].pos, 0.5f),
                        rot = Quaternion.Slerp(points[p].rot, points[p + 1].rot, 0.5f)
                    });
                }

                if(p == points.Count - 1 && loop)
                {
                    tempBuffer.Add(new SplinePoint(){
                        pos = Vector3.Lerp(points[p].pos, points[0].pos, 0.5f),
                        rot = Quaternion.Slerp(points[p].rot, points[0].rot, 0.5f)
                    });
                }
            }

            int count = tempBuffer.Count;
            int start = loop ? 0 : 1;
            int end = loop ? count : count - 1;

            points.Clear();

            if(!loop)
                points.Add(tempBuffer[0]);

            for(int p = start; p < end; ++p)
            {
                int prev = p == 0 ? count - 1 : p - 1;
                int next = p == count - 1 ? 0 : p + 1;

                SplinePoint pt = new SplinePoint();

                pt.pos = (tempBuffer[prev].pos + 2.0f * tempBuffer[p].pos + tempBuffer[next].pos) / 4.0f;

                Quaternion q1 = Quaternion.Slerp(tempBuffer[prev].rot, tempBuffer[p].rot, 0.5f);
                Quaternion q2 = Quaternion.Slerp(tempBuffer[p].rot, tempBuffer[next].rot, 0.5f);
                pt.rot = Quaternion.Slerp(q1, q2, 0.5f);

                points.Add(pt);
            }

            if(!loop)
                points.Add(tempBuffer[tempBuffer.Count - 1]);

            tempBuffer.Clear();
        }

        if(loop)
            points.Add(points[0]);
    }

    public SplinePoint SampleFrac(float fractionOfPath) {
        return SampleDist(fractionOfPath * length);
    }

    public SplinePoint SampleDist(float distanceAlongPath)
    {
        float dist = Mathf.Clamp(distanceAlongPath, 0, length);

        float segStart = 0;

        int pointCount = points.Count;

        for(int i = 1; i < pointCount; ++i)
        {
            float segLength = Vector3.Distance(points[i].pos, points[i - 1].pos);
            float segEnd = segStart + segLength;

            if((dist >= segStart && dist < segEnd))
            {
                float lt = (dist - segStart) / segLength;
                var prev = points[i - 1];
                var curr = points[i];

                return new SplinePoint(){
                    pos = Vector3.Lerp(prev.pos, curr.pos, lt),
                    rot = Quaternion.Slerp(prev.rot, curr.rot, lt)
                };
            }

            segStart = segEnd;
        }

        return points[pointCount - 1];
    }

    public SplinePoint GetClosestPoint(Vector3 point) {
        float distanceAlongPath;
        float distanceToPath;
        return GetClosestPoint(point, out distanceAlongPath, out distanceToPath);
    }

    public SplinePoint GetClosestPoint(Vector3 point, out float distanceAlongPath) {
        float distanceToPath;
        return GetClosestPoint(point, out distanceAlongPath, out distanceToPath);
    }

    public SplinePoint GetClosestPoint(Vector3 point, out float distanceAlongPath, out float distToPath)
    {
        SplinePoint closestPoint = new SplinePoint();

        int closestIdx = 0;
        float closestDist = float.MaxValue;

        for(int i = 0; i < points.Count - 1; ++i)
        {
            var curr = points[i];
            var next = points[i + 1];

            var p = point - curr.pos;
            var v = next.pos - curr.pos;
            float t = Vector3.Dot(p, v) / Vector3.Dot(v, v);
            t = Mathf.Clamp01(t);

            Vector3 segPointPos = Vector3.Lerp(curr.pos, next.pos, t);
            Quaternion segPointRot = Quaternion.Slerp(curr.rot, next.rot, t);
            float distFromSeg = Vector3.Distance(point, segPointPos);

            if(distFromSeg < closestDist)
            {
                closestDist = distFromSeg;
                closestIdx = i;
                closestPoint.pos = segPointPos;
                closestPoint.rot = segPointRot;
            }
        }

        distToPath = closestDist;
        distanceAlongPath = 0;

        for(int i = 0; i < closestIdx; ++i)
            distanceAlongPath += Vector3.Distance(points[i].pos, points[i + 1].pos);

        distanceAlongPath += Vector3.Distance(points[closestIdx].pos, closestPoint.pos);
        
        return closestPoint;
    }

    public Vector3 GetCenter()
    {
        Vector3 center = Vector3.zero;

        foreach(var pt in points)
            center += pt.pos;
        
        return center / points.Count;
    }

    public static Vector3 CatmullRom(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        return (
            (b * 2) +
            (-a + c) * t +
            (a * 2 - b * 5 + c * 4 - d) * t * t +
            (-a + b * 3 - c * 3 + d) * t * t * t
        ) * 0.5f;
    }
}
