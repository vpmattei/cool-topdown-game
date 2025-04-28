using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LegsManager))]
public class LegsManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Grab the target object
        var mgr = (LegsManager)target;

        // Start listening for changes
        EditorGUI.BeginChangeCheck();

        // Now draw your sliders, binding them to the PROPERTIES, not the raw fields:
        mgr.StepDistance = EditorGUILayout.Slider(
            new GUIContent("Step Distance", "The threshold after which movement should start"),
            mgr.StepDistance, 1f, 4f);

        mgr.MoveDuration = EditorGUILayout.Slider(
            new GUIContent("Move Duration", "How long it takes for each leg to move"),
            mgr.MoveDuration, 0.01f, 1f);

        mgr.StepHeight = EditorGUILayout.Slider(
            new GUIContent("Step Height", "How high each leg goes"),
            mgr.StepHeight, 0.5f, 6f);

        mgr.LegInterval = EditorGUILayout.Slider(
            new GUIContent("Leg Interval", "Time interval between legs starting movement"),
            mgr.LegInterval, 0.01f, 1f);

        // Draw all default fields except the four sliders (we handle those ourselves)
        DrawPropertiesExcluding(serializedObject,
            "_stepDistance", "_moveDuration", "_stepHeight", "_legInterval");

        // If anything changed, mark dirty so Unity serializes it
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(mgr);
        }
    }
}