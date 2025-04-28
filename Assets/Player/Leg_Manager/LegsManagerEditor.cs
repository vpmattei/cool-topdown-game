using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LegsManager))]
public class LegsManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw all fields _except_ our four backing fields
        DrawPropertiesExcluding(serializedObject,
            "_stepDistance", "_moveDuration", "_stepHeight", "_legInterval");

        // 2) Now draw your custom sliders (they’ll overwrite the ones drawn by DrawDefaultInspector)
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Custom Leg Settings", EditorStyles.boldLabel);

        var mgr = (LegsManager)target;
        mgr.StepDistance = EditorGUILayout.Slider("Step Distance", mgr.StepDistance, 1f, 4f);
        mgr.MoveDuration = EditorGUILayout.Slider("Move Duration", mgr.MoveDuration, 0.01f, 1f);
        mgr.StepHeight = EditorGUILayout.Slider("Step Height", mgr.StepHeight, 0.5f, 6f);
        mgr.LegInterval = EditorGUILayout.Slider("Leg Interval", mgr.LegInterval, 0.01f, 1f);

        if (GUI.changed)
            EditorUtility.SetDirty(target);

        serializedObject.ApplyModifiedProperties();
    }
}