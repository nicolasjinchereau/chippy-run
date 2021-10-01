using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(SplinePath))]
public class SplinePathEditor : Editor
{
    static List<Transform> tempTransforms = new List<Transform>();
    static List<SplinePoint> tempSplinePoints = new List<SplinePoint>();
    static Vector3[] tempLinePoints = new Vector3[0];

    static GUIStyle labelStyle;
    static bool sceneChangedSinceMouseDown;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        var path = target as SplinePath;

        if(GUILayout.Button("Add Point"))
        {
            Vector3 childPos = Vector3.zero;

            if(path.transform.childCount == 0)
            {
                childPos = path.transform.position;
            }
            else if(path.transform.childCount == 1)
            {
                var child = path.transform.GetChild(0);
                childPos = child.position + Vector3.right * HandleSize(child.position);
            }
            else
            {
                var child0 = path.transform.GetChild(path.transform.childCount - 2);
                var child1 = path.transform.GetChild(path.transform.childCount - 1);

                var offset = (child1.position - child0.position);
                childPos = child1.position + offset;
            }

            var c = new GameObject("Point");
            c.transform.parent = path.transform;
            c.transform.position = childPos;
            c.hideFlags = HideFlags.NotEditable;

            Undo.RegisterCreatedObjectUndo(c, "Added Spline Point");
            UpdateSpline();
            MarkActiveSceneDirty();
        }

        if(GUILayout.Button("Remove Point"))
        {
            if(path.transform.childCount > 0)
            {
                var child = path.transform.GetChild(path.transform.childCount - 1);
                Undo.DestroyObjectImmediate(child.gameObject);

                UpdateSpline();
                MarkActiveSceneDirty();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Make Points Planar");

        GUILayout.BeginHorizontal();
        {
            if(GUILayout.Button("X"))
                MakePlanar(0);

            if(GUILayout.Button("Y"))
                MakePlanar(1);
            
            if(GUILayout.Button("Z"))
                MakePlanar(2);
        }
        GUILayout.EndHorizontal();

        if(path.subdivisions != path.spline.subdivisions ||
           path.looped != path.spline.looped ||
           path.alignPointsToPath != path.spline.alignPointsToPath)
        {
            UpdateSpline();
        }
    }

    void OnEnable()
    {
        UpdateSpline();
        AlignPivotToMin();
    }

    void UpdateSpline() {
        UpdateSpline(target as SplinePath);
    }

    static void UpdateSpline(SplinePath path)
    {
        if(path.transform.childCount >= 3)
        {
            tempTransforms.Clear();

            for(int i = 0; i < path.transform.childCount; ++i)
                tempTransforms.Add(path.transform.GetChild(i));

            path.spline.UpdateSpline(
                tempTransforms, path.algorithm, path.subdivisions,
                path.looped, path.alignPointsToPath, tempSplinePoints);
        }
        else
        {
            path.spline.Clear();
        }
    }

    public void AlignPivotToMin()
    {
        var path = target as SplinePath;

        if (path.transform.childCount == 0)
            return;

        Vector3 min = path.transform.GetChild(0).position;
        Vector3 max = path.transform.GetChild(0).position;

        for(int i = 1; i < path.transform.childCount; ++i)
        {
            var pos = path.transform.GetChild(i).position;
            min = Vector3.Min(min, pos);
            max = Vector3.Max(max, pos);
        }

        min -= (max - min) * 0.1f;

        if((path.transform.position - min).magnitude > 0.001f)
        {
            tempTransforms.Clear();
            if(tempTransforms.Capacity < path.transform.childCount)
                tempTransforms.Capacity = path.transform.childCount;
            
            Undo.RecordObject(path.transform, "Adjust Spline Pivot");

            foreach(Transform t in path.transform)
            {
                Undo.RecordObject(t, "Adjust Spline Pivot");
                tempTransforms.Add(t);
            }

            foreach(Transform t in tempTransforms)
                t.parent = null;
            
            path.transform.position = min;

            foreach(Transform t in tempTransforms)
                t.parent = path.transform;
        }
    }

    static readonly string[] axisNames = { "X", "Y", "Z" };

    void MakePlanar(int axis)
    {
        var path = target as SplinePath;

        if(path.transform.childCount == 0)
            return;
        
        string undoTitle = "Make Spline Planar (" + axisNames[axis] + " Axis)";

        float avg = 0;

        foreach(Transform child in path.transform)
            avg += child.position[axis];

        avg /= path.transform.childCount;

        bool dirty = false;

        foreach(Transform child in path.transform)
        {
            var pos = child.position;

            if(Mathf.Abs(pos[axis] - avg) > float.Epsilon)
            {
                Undo.RecordObject(child, undoTitle);

                dirty = true;
                pos[axis] = avg;
                child.position = pos;
            }
        }

        if(dirty)
        {
            UpdateSpline();
            AlignPivotToMin();
            MarkActiveSceneDirty();
        }
    }

    static float HandleSize(Vector3 at) {
        return HandleUtility.GetHandleSize(at) * 0.08f;
    }

    static float LineWidth(Vector3 at) {
        return HandleUtility.GetHandleSize(at);
    }

    static void MarkActiveSceneDirty() {
        if(!Application.isPlaying)
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    void OnSceneGUI()
    {
        if(Event.current.type == EventType.MouseDown)
            sceneChangedSinceMouseDown = false;
        
        bool mouseUp = (Event.current.type == EventType.MouseUp);

        var path = target as SplinePath;

        bool sceneChanged = false;

        for(int i = 0; i < path.transform.childCount; ++i)
        {
            var child = path.transform.GetChild(i);
            var newPos = Handles.PositionHandle(child.position, Quaternion.identity);
            if(Vector3.Distance(child.position, newPos) > 0)
            {
                Undo.RecordObject(child, "Moved Spline Point");
                child.position = newPos;
                sceneChanged = true;
                sceneChangedSinceMouseDown = true;
            }
        }

        UpdateSpline();

        if(sceneChanged)
            MarkActiveSceneDirty();
        
        if(mouseUp && sceneChangedSinceMouseDown)
        {
            sceneChangedSinceMouseDown = false;
            AlignPivotToMin();
        }
    }

    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    static void DrawSplineGizmo(SplinePath path, GizmoType gizmoType)
    {
        if(path.spline.points.Count != path.transform.childCount)
            UpdateSpline(path);
        
        bool selected = (gizmoType & GizmoType.Selected) != 0;
        Color greenColor = selected ? new Color(0, 1, 0, 1) : new Color(0, 0.6f, 0, 1);
        Color yellowColor = selected ? new Color(1, 1, 0, 1) : new Color(0.6f, 0.6f, 0, 1);

        // draw spline
        var old = Handles.color;
        Handles.color = greenColor;

        if(path.spline.points.Count > 0)
        {
            var pointCount = path.spline.points.Count;

            if(tempLinePoints.Length < pointCount || pointCount < tempLinePoints.Length / 2)
                tempLinePoints = new Vector3[pointCount];

            Vector3 center = Vector3.zero;

            for(int i = 0; i < path.spline.points.Count; ++i) {
                tempLinePoints[i] = path.spline.points[i].pos;
                center += tempLinePoints[i];
            }

            center /= pointCount;

            Handles.DrawAAPolyLine(3, pointCount, tempLinePoints);
        }

        Handles.color = old;

        // draw control points
        old = Gizmos.color;
        Gizmos.color = yellowColor;

        if(selected && (labelStyle == null || labelStyle.fontSize != 16))
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 16;
        }

        for(int i = 0; i < path.transform.childCount; ++i)
        {
            var tf = path.transform.GetChild(i);
            float handleSize = HandleSize(tf.position);
            Gizmos.DrawSphere(tf.position, handleSize);

            if(selected)
                Handles.Label(tf.position - Camera.current.transform.up * handleSize, i.ToString(), labelStyle);
        }

        Gizmos.color = old;
    }
}
