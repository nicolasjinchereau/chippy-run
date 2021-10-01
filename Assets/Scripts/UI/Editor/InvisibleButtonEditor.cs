using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(InvisibleButton), false), CanEditMultipleObjects]
public class InvisibleButtonEditor : GraphicEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_Script);
        RaycastControlsGUI();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("onClick"));
        serializedObject.ApplyModifiedProperties();
    }
}
